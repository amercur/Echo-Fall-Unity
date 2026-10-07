using UnityEngine;

namespace EchoFall.Movement
{
    [RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
    public sealed class PlayerMotor : MonoBehaviour
    {
        public MovementTuning tuning;
        public LayerMask solids, oneWayPlatforms, hazards;
        public Rect roomBounds = new Rect(0, -1, 32, 12);
        public bool automaticSimulation = true;

        public Vector2 Position { get; private set; } // Foot center, Y up.
        public Vector2 Velocity { get; private set; }
        public Vector2 SafePosition { get; private set; }
        public bool Grounded { get; private set; }
        public bool OnPlatform { get; private set; }
        public bool AirDashUsed { get; private set; }
        public int Facing { get; private set; } = 1;
        public int Wall { get; private set; }
        public bool Dashing => dashTime > 0;
        public float DashCooldown => dashCooldown;
        public int ResetCount { get; private set; }
        // Presentation notifications only; animation never owns motor timing.
        public int WallJumpSequence { get; private set; }
        public int TeleportSequence { get; private set; }
        public Vector2 RenderPosition => Vector2.Lerp(previousPosition, Position,
            Application.isPlaying ? Mathf.Clamp01((Time.time - Time.fixedTime) / Time.fixedDeltaTime) : 1f);

        Rigidbody2D body;
        MovementInput input;
        Vector2 spawn, previousPosition;
        float jumpBuffer, coyote, wallGrace, wallLock, wallSteer, dashTime, dashCooldown, dashBuffer, dropTime;
        int lastWall;
        bool initialized, cutBufferedJump;
        readonly RaycastHit2D[] hits = new RaycastHit2D[32];
        readonly Collider2D[] overlaps = new Collider2D[16];

        void Awake() => Initialize();
        void Initialize()
        {
            if (initialized || tuning == null) return;
            body = GetComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            body.gravityScale = 0;
            body.constraints = RigidbodyConstraints2D.FreezeRotation;
            // We resolve movement explicitly and interpolate only presentation.
            body.interpolation = RigidbodyInterpolation2D.None;
            var box = GetComponent<BoxCollider2D>();
            box.size = tuning.size;
            box.offset = new Vector2(0, tuning.size.y * .5f);
            input = GetComponent<MovementInput>();
            Position = previousPosition = SafePosition = spawn = body.position;
            initialized = true;
        }

        void FixedUpdate()
        {
            if (automaticSimulation) Simulate(Time.fixedDeltaTime, input != null ? input.Consume() : default);
        }

        public void Teleport(Vector2 feet)
        {
            TeleportSequence++;
            Initialize();
            Position = previousPosition = feet;
            Velocity = Vector2.zero;
            Grounded = OnPlatform = AirDashUsed = false;
            Wall = lastWall = 0;
            jumpBuffer = coyote = wallGrace = wallLock = wallSteer = dashTime = dashCooldown = dashBuffer = dropTime = 0;
            cutBufferedJump = false;
            body.position = feet;
        }

        public void EnterRoom(Vector2 feet)
        {
            Teleport(feet);
            SafePosition = spawn = feet;
        }

        public void Pogo()
        {
            Velocity = new Vector2(Velocity.x, 5.35f);
            AirDashUsed = false;
            Grounded = false;
            dashTime = 0;
            dashCooldown = 0;
        }

        public void CombatKnockback(float sourceX)
        {
            Velocity=new Vector2(Position.x<sourceX?-2.7f:2.7f,1.7f);
            dashTime=0;wallLock=.12f;Grounded=false;
        }
        public void DeflectLift()
        { RefreshAirOptions();if(!Grounded)Velocity=new Vector2(Velocity.x,Mathf.Max(Velocity.y,2.1f)); }

        public void RefreshAirOptions() => AirDashUsed = false;

        public void Simulate(float dt, MovementCommand command)
        {
            Initialize();
            if (!initialized || dt <= 0) return;
            if (command.resetPressed)
            {
                Teleport(spawn);
                SafePosition = spawn;
                ResetCount++;
                return;
            }
            previousPosition = Position;
            var v = Velocity;
            float move = Mathf.Clamp(command.move, -1, 1);
            wallGrace = Decrease(wallGrace, dt);
            wallLock = Decrease(wallLock, dt);
            wallSteer = Decrease(wallSteer, dt);
            dashTime = Decrease(dashTime, dt);
            dashCooldown = Decrease(dashCooldown, dt);
            dropTime = Decrease(dropTime, dt);

            if (move != 0 && wallLock <= 0) Facing = move > 0 ? 1 : -1;
            if (command.jumpPressed) { jumpBuffer = tuning.jumpBuffer; cutBufferedJump = false; }
            if (command.jumpReleased)
            {
                v.y = Mathf.Min(v.y, tuning.releasedJumpSpeed);
                // A tap entirely between fixed ticks still produces a short jump.
                if (jumpBuffer > 0) cutBufferedJump = true;
            }
            jumpBuffer = Decrease(jumpBuffer, dt);
            coyote = Grounded ? tuning.coyoteTime : Decrease(coyote, dt);
            if (Wall != 0 && !Grounded) { wallGrace = tuning.wallGrace; lastWall = Wall; }

            if (jumpBuffer > 0 && command.down && Grounded && OnPlatform)
            {
                dropTime = tuning.dropDuration;
                Position += Vector2.down * tuning.dropNudge;
                Grounded = false;
                jumpBuffer = coyote = 0;
            }
            if (jumpBuffer > 0)
            {
                if (!Grounded && wallGrace > 0)
                {
                    WallJumpSequence++;
                    v = new Vector2(-lastWall * tuning.wallPushSpeed, tuning.wallJumpSpeed);
                    Facing = v.x > 0 ? 1 : -1;
                    wallLock = tuning.wallLock;
                    wallSteer = tuning.wallSteerTime;
                    AirDashUsed = false;
                    Wall = 0;
                    wallGrace = jumpBuffer = coyote = 0;
                    if (cutBufferedJump) v.y = Mathf.Min(v.y, tuning.releasedJumpSpeed);
                }
                else if (coyote > 0)
                {
                    v.y = cutBufferedJump ? tuning.releasedJumpSpeed : tuning.jumpSpeed;
                    Grounded = false;
                    jumpBuffer = coyote = 0;
                }
            }

            dashBuffer = Decrease(dashBuffer, dt);
            if (command.dashPressed) dashBuffer = tuning.dashBuffer;
            if (dashBuffer > 0 && dashCooldown <= 0 && (Grounded || !AirDashUsed))
            {
                dashBuffer = 0;
                AirDashUsed = !Grounded;
                // The source retains the endpoint tick at 120 Hz (.15 in JS doubles).
                // Keep that boundary inclusive despite Unity's float subtraction rounding.
                dashTime = tuning.dashDuration + 1e-7f;
                dashCooldown = tuning.dashCooldown;
                v.y = 0;
            }
            if (Dashing) v = new Vector2(Facing * tuning.dashSpeed, 0);
            else
            {
                float acceleration = Grounded ? tuning.groundAcceleration : wallSteer > 0 ? tuning.wallSteerAcceleration : tuning.airAcceleration;
                if (wallLock <= 0) v.x = Mathf.MoveTowards(v.x, move * tuning.speed, acceleration * dt);
                v.y = Mathf.Max(-tuning.terminalSpeed, v.y - tuning.gravity * dt);
                if (Wall != 0 && move == Wall && !Grounded && v.y < -tuning.wallSlideSpeed) v.y = -tuning.wallSlideSpeed;
            }

            Wall = 0;
            float dx = v.x * dt;
            if (Mathf.Abs(dx) > 0)
            {
                float travel = Sweep(Position, Vector2.right * Mathf.Sign(dx), Mathf.Abs(dx), false, out var hit);
                Position += Vector2.right * (Mathf.Sign(dx) * travel);
                if (hit != null) { Wall = dx > 0 ? 1 : -1; v.x = 0; dashTime = 0; }
            }

            Grounded = OnPlatform = false;
            float dy = v.y * dt;
            if (Mathf.Abs(dy) > 0)
            {
                float travel = Sweep(Position, Vector2.up * Mathf.Sign(dy), Mathf.Abs(dy), dy < 0, out var hit);
                Position += Vector2.up * (Mathf.Sign(dy) * travel);
                if (hit != null)
                {
                    if (dy < 0) { Grounded = true; OnPlatform = InMask(hit.gameObject.layer, oneWayPlatforms); }
                    v.y = 0;
                }
            }
            // A level dash keeps floor contact, but never attaches to a ledge while rising.
            else if (Sweep(Position, Vector2.down, tuning.skin * 2, true, out var support) <= tuning.skin * 2 && support != null)
            {
                Grounded = true;
                OnPlatform = InMask(support.gameObject.layer, oneWayPlatforms);
            }

            Position = new Vector2(Mathf.Clamp(Position.x, roomBounds.xMin + tuning.size.x * .5f,
                roomBounds.xMax - tuning.size.x * .5f), Mathf.Min(Position.y, roomBounds.yMax + .6f - tuning.size.y));
            bool inHazard = TouchingHazard();
            if (Grounded)
            {
                AirDashUsed = false;
                Wall = 0;
                // Require full foot support before replacing the recovery point near a pit.
                if (Mathf.Abs(v.x) < 2.8f && !inHazard && FullySupported()) SafePosition = Position;
            }
            Velocity = v;
            body.position = Position;
            // Source bounds compare the top-left player Y; Position is the foot center.
            if (inHazard || Position.y < roomBounds.yMin - .6f - tuning.size.y)
            {
                Teleport(SafePosition);
                ResetCount++;
            }
        }

        float Sweep(Vector2 feet, Vector2 direction, float distance, bool includePlatforms, out Collider2D obstacle)
        {
            obstacle = null;
            float allowed = distance;
            int mask = solids.value | (includePlatforms && dropTime <= 0 ? oneWayPlatforms.value : 0);
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = mask, useTriggers = true };
            int count = Physics2D.BoxCast(feet + Vector2.up * (tuning.size.y * .5f),
                tuning.size - Vector2.one * (2 * tuning.skin), 0, direction, filter, hits, distance + tuning.skin);
            for (int i = 0; i < count; i++)
            {
                var hit = hits[i];
                if (hit.collider == null || hit.rigidbody == body || Vector2.Dot(hit.normal, direction) > -.5f) continue;
                if (InMask(hit.collider.gameObject.layer, oneWayPlatforms) &&
                    (direction.y >= 0 || feet.y < hit.collider.bounds.max.y - tuning.skin * 2)) continue;
                // Box2D casts include a contact radius. Resolve authored axis-aligned
                // boxes against their actual faces, matching the source AABB controller
                // instead of stopping a few millimetres above floors or beside walls.
                float travel;
                if (hit.collider is BoxCollider2D && Mathf.Abs(hit.collider.transform.eulerAngles.z) < .001f)
                {
                    var bounds = hit.collider.bounds;
                    float halfWidth = tuning.size.x * .5f;
                    if (direction.x != 0)
                    {
                        if (feet.y + tuning.size.y <= bounds.min.y || feet.y >= bounds.max.y) continue;
                        travel = direction.x > 0 ? bounds.min.x - feet.x - halfWidth : feet.x - halfWidth - bounds.max.x;
                    }
                    else
                    {
                        if (feet.x + halfWidth <= bounds.min.x || feet.x - halfWidth >= bounds.max.x) continue;
                        travel = direction.y > 0 ? bounds.min.y - feet.y - tuning.size.y : feet.y - bounds.max.y;
                    }
                    if (travel < -tuning.skin * 2) continue;
                    travel = Mathf.Max(0, travel);
                }
                else travel = Mathf.Max(0, hit.distance - tuning.skin);
                if (travel <= allowed) { allowed = travel; obstacle = hit.collider; }
            }
            return allowed;
        }

        bool TouchingHazard()
        {
            var filter = new ContactFilter2D { useLayerMask = true, layerMask = hazards, useTriggers = true };
            return Physics2D.OverlapBox(Position + Vector2.up * (tuning.size.y * .5f),
                tuning.size, 0, filter, overlaps) > 0;
        }

        bool FullySupported()
        {
            float inset = tuning.size.x * .45f;
            int mask = solids.value | (dropTime <= 0 ? oneWayPlatforms.value : 0);
            return Physics2D.Raycast(Position + new Vector2(-inset, tuning.skin), Vector2.down, tuning.skin * 4, mask).collider != null
                && Physics2D.Raycast(Position + new Vector2(inset, tuning.skin), Vector2.down, tuning.skin * 4, mask).collider != null;
        }

        static bool InMask(int layer, LayerMask mask) => (mask.value & (1 << layer)) != 0;
        static float Decrease(float value, float dt) => Mathf.Max(0, value - dt);
    }
}
