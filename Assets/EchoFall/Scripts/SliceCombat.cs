using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoFall.Movement
{
    public sealed class SliceCombat : MonoBehaviour
    {
        public PlayerMotor motor;
        public int Integrity { get; private set; } = 6;
        public int Resonance { get; private set; }
        public float Fracture { get; private set; }
        public float AttackVisual { get; private set; }
        public bool Guarding => guardTime > 0;
        public bool ChargingGuard => holdingGuard;
        public float MendProgress => mendTime;
        public int Combo { get; private set; }
        public float ImprintTime => markTime;
        public SliceEnemy Marked => marked;
        public bool Busy => recovery > 0 || Guarding || holdingAttack || holdingGuard ||
            (mend != null && mend.IsPressed()) || (ability != null && ability.IsPressed()) ||
            (imprint != null && imprint.IsPressed());
        InputAction attack, guard, imprint, mend, ability, aim;
        float recovery, comboWindow, attackHold, guardHold, guardTime, perfectTime, counterTime, guardCooldown, invulnerable, buffer, markTime, mendTime, abilityCooldown;
        bool holdingAttack, holdingGuard;
        bool attackPressed, attackReleased, guardPressed, guardReleased, imprintPressed, abilityPressed;
        SliceEnemy marked;
        int charge, lastReset;
        bool previousDash;
        PlayerVisual presentation;
        SliceSession Session => SliceSession.Instance;
        void Awake()
        {
            motor = GetComponent<PlayerMotor>();
            presentation = GetComponentInChildren<PlayerVisual>();
            attack = Button("Attack", "<Keyboard>/j", "<Gamepad>/buttonWest"); attack.AddBinding("<Keyboard>/x");
            guard = Button("Guard", "<Keyboard>/f", "<Gamepad>/leftShoulder");
            imprint = Button("Imprint", "<Keyboard>/q", "<Gamepad>/rightShoulder");
            mend = Button("Mend", "<Keyboard>/h", "<Gamepad>/rightTrigger");
            ability = Button("Memory", "<Keyboard>/c", "<Gamepad>/leftTrigger"); ability.AddBinding("<Keyboard>/l");
            aim = new InputAction("Aim", InputActionType.Value);
            aim.AddCompositeBinding("1DAxis").With("Negative", "<Keyboard>/s").With("Positive", "<Keyboard>/w");
            aim.AddBinding("<Gamepad>/leftStick/y"); aim.Enable();
            attack.performed += _ => attackPressed = true; attack.canceled += _ => attackReleased = true;
            guard.performed += _ => guardPressed = true; guard.canceled += _ => guardReleased = true;
            imprint.performed += _ => imprintPressed = true; ability.performed += _ => abilityPressed = true;
        }
        static InputAction Button(string name, string key, string pad)
        { var a = new InputAction(name, InputActionType.Button); a.AddBinding(key); a.AddBinding(pad); a.Enable(); return a; }
        void OnDestroy() { foreach (var a in new[] { attack, guard, imprint, mend, ability, aim }) a?.Dispose(); }
        void Update()
        {
            bool attackEdge=attackPressed, attackEnd=attackReleased, guardEdge=guardPressed, guardEnd=guardReleased, imprintEdge=imprintPressed, abilityEdge=abilityPressed;
            attackPressed=attackReleased=guardPressed=guardReleased=imprintPressed=abilityPressed=false;
            if (Session == null || !Session.Playing) { holdingAttack = holdingGuard = false; return; }
            if (attackEdge || guardEdge || imprintEdge || abilityEdge || mend.IsPressed()) Session.CancelRest();
            float dt = Time.deltaTime;
            recovery -= dt; comboWindow -= dt; guardTime -= dt; perfectTime -= dt; counterTime -= dt; guardCooldown -= dt; invulnerable -= dt; abilityCooldown -= dt;
            AttackVisual = Mathf.Max(0, AttackVisual - dt);
            if (motor.ResetCount != lastReset) { lastReset = motor.ResetCount; Hurt(1, motor.Position + Vector2.right, false, null, true); }
            if (motor.Dashing && !previousDash) recovery = Mathf.Min(recovery, .04f);
            previousDash = motor.Dashing;
            if (attackEdge) { buffer = .14f; holdingAttack = true; attackHold = 0; }
            if (holdingAttack) attackHold += dt;
            if (attackEnd) { if (holdingAttack && attackHold >= .55f) Strike(true, aim.ReadValue<float>()); holdingAttack = false; }
            if (buffer > 0 && recovery <= 0) { buffer = 0; Strike(false, aim.ReadValue<float>()); }
            buffer -= dt;
            if (guardEdge) { holdingGuard = true; guardHold = 0; BeginGuard(); }
            if (holdingGuard) guardHold += dt;
            if (guardEnd)
            { if (holdingGuard && guardHold >= .48f) { counterTime = perfectTime = guardTime = .21f; guardCooldown = .36f; SliceEffects.Ring(motor.Position + Vector2.up * .25f, new Color(1,.5f,.35f), .5f); } holdingGuard = false; }
            if (imprintEdge) Imprint();
            if (marked != null) { markTime += dt; if (markTime >= 2) Detonate(true); }
            if (mend.IsPressed() && motor.Grounded && Mathf.Abs(motor.Velocity.x) < .1f && Resonance > 0 && Integrity < 6)
            { mendTime += dt; if (mendTime >= 1) { Resonance--; Heal(1); mendTime = 0; } }
            else mendTime = 0;
            if (abilityEdge && abilityCooldown <= 0) MemoryAbility();
        }
        public void BeginGuard()
        {
            if (guardCooldown > 0) return;
            guardTime = .30f; perfectTime = Session != null && Session.Archive.active == "return" ? .21f : .145f; guardCooldown = .32f;
            SliceEffects.Ring(motor.Position + Vector2.up * .22f, Color.cyan, .3f);
        }
        public void Strike(bool charged, float direction)
        {
            if (Session == null || !Session.Playing) return;
            Session.CancelRest();
            if (!charged && recovery > 0) { buffer = .14f; return; }
            Combo = comboWindow > 0 ? Combo % 3 + 1 : 1;
            comboWindow = .72f; recovery = charged ? .46f : Combo == 3 ? .34f : .22f; AttackVisual = .22f;
            if(presentation!=null)presentation.PlayAttack(charged,direction,Combo,recovery);
            float damage = (charged ? 5.5f : Combo == 3 ? 3.5f : 2) + (Session.Decision == "fire" ? 1 : 0);
            bool down = direction < -.4f && !motor.Grounded;
            Vector2 dir = down ? Vector2.down : direction > .4f ? Vector2.up : Vector2.right * motor.Facing;
            if (!charged && !down && Session.Archive.active == "fire")
            { SliceProjectile.Spawn(motor.Position + Vector2.up * .25f, dir * 6, null, damage); return; }
            Vector2 center = motor.Position + Vector2.up * .24f + dir * .48f;
            Vector2 size = dir.y == 0 ? new Vector2(charged ? 1.25f : .95f, .8f) : new Vector2(.8f, 1.1f);
            var area = new Rect(center - size * .5f, size);
            SliceEffects.Slash(motor.Position + Vector2.up * .25f, dir, charged ? new Color(1,.75f,.42f) : new Color(.6f,1,1));
            bool hit = false;
            foreach (var enemy in FindObjectsByType<SliceEnemy>())
                if (enemy.Alive && area.Overlaps(enemy.HitRect)) { enemy.Damage(damage, charged); hit = true; }
            if(hit && (charged || Combo==3) && presentation!=null)presentation.Impact();
            if (down)
            {
                foreach (var point in FindObjectsByType<SlicePogo>()) if (area.Contains(point.transform.position)) hit = true;
                if (hit) motor.Pogo();
            }
        }
        public bool Hurt(int amount, Vector2 source, bool red, SliceEnemy attacker, bool bypass = false)
        {
            if (Session == null || !Session.Playing || Integrity <= 0) return false;
            if (!bypass && (invulnerable > 0 || (!red && motor.Dashing))) return false;
            Session.CancelRest();
            bool faces = !motor.Grounded || (source.x - motor.Position.x) * motor.Facing >= 0;
            if (!bypass && faces && ((red && counterTime > 0) || (!red && perfectTime > 0)))
            {
                Fracture = 0; Resonance = Mathf.Min(3, Resonance + 1); motor.RefreshAirOptions();
                guardTime = perfectTime = counterTime = 0; invulnerable = .10f; guardCooldown = .055f;
                if(presentation!=null)presentation.PlayDeflect(red);
                SliceEffects.Dust(motor.Position+Vector2.up*.3f,Vector2.up*.5f,6);
                if (attacker != null) { attacker.strain++; attacker.Deflected(red); }
                SliceEffects.Ring(motor.Position + Vector2.up * .2f, Color.white, .85f);
                Session.Notify(red ? "RED COUNTER / the opening is yours" : "PERFECT DEFLECT / +1 Resonance"); return true;
            }
            if (!bypass && !red && faces && guardTime > 0)
            { Fracture = Mathf.Min(Integrity - 1, Fracture + .5f); guardTime = perfectTime = 0; invulnerable = .2f; Session.Notify("LATE BLOCK / fracture debt — a clean deflect clears it"); return false; }
            int total = amount + Mathf.CeilToInt(Fracture); Fracture = 0;
            Integrity = attacker != null && attacker.training ? Mathf.Max(1, Integrity - total) : Mathf.Max(0, Integrity - total);
            mendTime = 0; invulnerable = .75f;
            if(presentation!=null)presentation.PlayHurt(total);
            SliceEffects.Ring(motor.Position + Vector2.up * .2f, new Color(1,.35f,.4f), .6f);
            if (Integrity <= 0) Session.EndRun(false);
            return false;
        }
        public void PayIntegrity(int cost) { Integrity = Mathf.Max(0, Integrity - cost); if(presentation!=null)presentation.PlayHurt(cost); if (Integrity == 0) Session.EndRun(false); }
        public void Heal(int amount) => Integrity = Mathf.Min(6, Integrity + amount);
        public void Rest() { Integrity = 6; Resonance = 3; Fracture = 0; ClearTransient(); }
        public void RestoreBench(int resonance)
        { Integrity = 6; Resonance = Mathf.Clamp(resonance, 1, 3); Fracture = 0; ClearTransient(); }
        public void DiscardInput()
        { attackPressed=attackReleased=guardPressed=guardReleased=imprintPressed=abilityPressed=holdingAttack=holdingGuard=false; }
        public void ClearTransient()
        {
            if(presentation!=null)presentation.ResetTransient();
            recovery = buffer = guardTime = perfectTime = counterTime = mendTime = AttackVisual = attackHold = guardHold = markTime = 0;
            guardCooldown = invulnerable = abilityCooldown = comboWindow = 0;
            Combo = charge = 0; previousDash = false; lastReset = motor.ResetCount;
            holdingAttack = holdingGuard = false; marked = null;
            DiscardInput();
            foreach (var projectile in FindObjectsByType<SliceProjectile>()) Destroy(projectile.gameObject);
        }
        SliceEnemy Closest(float radius)
        {
            SliceEnemy nearest = null;
            foreach (var e in FindObjectsByType<SliceEnemy>())
            { float d = Vector2.Distance(motor.Position, e.transform.position); if (e.Alive && d <= radius) { radius = d; nearest = e; } }
            return nearest;
        }
        public void Imprint()
        {
            if (marked != null) { Detonate(false); return; }
            var target = Closest(1.32f);
            if (target == null || Resonance < 1) { Session.Notify("Imprint needs Resonance and a nearby enemy."); return; }
            marked = target; charge = Resonance; Resonance = 0; markTime = 0;
            SliceEffects.Ring(target.transform.position + Vector3.up * .3f, Color.cyan, .6f);
        }
        void Detonate(bool mature)
        {
            if (marked != null) { marked.Damage(3 + 2 * charge + marked.strain + (mature ? 2 : 0), true); marked.strain = 0; if(presentation!=null)presentation.Impact(.05f,.055f); }
            marked = null; markTime = 0;
        }
        void MemoryAbility()
        {
            if (Session.Archive.active == "return") { BeginGuard(); return; }
            var target = Closest(3);
            if (target == null) return;
            target.Damage(Session.Archive.active == "mercy" ? 4 : 5, true); abilityCooldown = 3;
            SliceEffects.Ring(target.transform.position + Vector3.up * .3f, Session.Archive.active == "mercy" ? Color.cyan : new Color(1,.5f,.2f), 1);
        }
    }
}
