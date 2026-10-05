using UnityEngine;

namespace EchoFall.Movement
{
    [CreateAssetMenu(menuName = "Echo Fall/Movement tuning")]
    public sealed class MovementTuning : ScriptableObject
    {
        // Original game.js values, converted from pixels to units at 100 px/unit.
        public const float Step = 1f / 120f;
        public Vector2 size = new Vector2(.22f, .40f);
        public float speed = 2.65f;
        public float groundAcceleration = 22f;
        public float airAcceleration = 12.5f;
        public float wallSteerAcceleration = 24f;
        public float gravity = 14.5f;
        public float terminalSpeed = 8.3f;
        public float jumpSpeed = 6f;
        public float releasedJumpSpeed = 2.2f;
        public float jumpBuffer = .14f;
        public float coyoteTime = .105f;
        public float wallSlideSpeed = .95f;
        public float wallJumpSpeed = 5.7f;
        public float wallPushSpeed = 2.45f;
        public float wallGrace = .11f;
        public float wallLock = .055f;
        public float wallSteerTime = .35f;
        public float dashSpeed = 7.2f;
        public float dashDuration = .15f;
        public float dashCooldown = .58f;
        public float dashBuffer = .12f;
        public float dropDuration = .23f;
        public float dropNudge = .03f;
        [Min(.0001f)] public float skin = .001f;
    }
}
