using UnityEngine;

namespace EchoFall.Movement
{
    public sealed class SliceEnemy : MonoBehaviour
    {
        public string id, kind;
        public bool training;
        public float hp = 7, left, right;
        public int strain;
        public Sprite[] frames;
        public SpriteRenderer visual;
        public bool Alive => hp > 0;
        public Rect HitRect => new Rect((Vector2)transform.position + new Vector2(-.3f, 0), new Vector2(.6f,.75f));
        public string State { get; private set; } = "patrol";
        public bool Red => kind == "lancer";
        float timer, clock;
        int facing = -1, chain;
        void Update()
        {
            var s = SliceSession.Instance;
            if (s == null || !s.Playing || !Alive) return;
            float dt = Time.deltaTime; timer -= dt; clock += dt;
            Vector2 delta = s.motor.Position - (Vector2)transform.position;
            float reach = kind == "drone" ? 5 : Red ? 1.7f : 1.05f;
            if (State == "patrol")
            {
                if (Mathf.Abs(delta.x) < reach && Mathf.Abs(delta.y) < (kind == "drone" ? 4 : .7f))
                { facing = delta.x < 0 ? -1 : 1; State = "tell"; timer = Red ? .95f : kind == "drone" ? .8f : .62f; }
                else
                {
                    var p = transform.position; p.x += facing * dt * (training ? .22f : .45f);
                    if (p.x <= left) { p.x = left; facing = 1; } if (p.x >= right) { p.x = right; facing = -1; }
                    transform.position = p;
                }
            }
            else if (State == "tell" && timer <= 0)
            {
                State = "strike"; timer = .16f;
                if (kind == "drone") SliceProjectile.Spawn(transform.position + Vector3.up * .25f, (delta + Vector2.up * .2f).normalized * 2.7f, this, 1);
                else
                {
                    SliceEffects.Slash(transform.position + Vector3.up * .3f, Vector2.right * facing, Red ? new Color(1,.2f,.3f) : Color.white);
                    if (Mathf.Abs(delta.x) < reach && Mathf.Abs(delta.y) < .65f && delta.x * facing >= -.1f)
                        s.combat.Hurt(Red ? 2 : 1, transform.position, Red, this);
                }
            }
            else if (State == "strike" && timer <= 0)
            {
                if (!Red && kind != "drone" && chain == 0) { chain = 1; State = "tell"; timer = .38f; }
                else { chain = 0; State = "recover"; timer = Red ? 1.5f : .8f; }
            }
            else if (State == "recover" && timer <= 0) State = "patrol";
            if (visual != null)
            {
                int frame = State == "tell" ? 4 + (int)(clock * 8) % 2 : State == "strike" ? 6 : State == "recover" ? 7 : (int)(clock * 6) % 4;
                if (frames != null && frames.Length >= 8) visual.sprite = frames[frame];
                visual.flipX = facing < 0;
                visual.color = State == "tell" ? (Red ? new Color(1,.38f,.4f) : new Color(1,1,.85f)) : Color.white;
            }
        }
        public void Deflected(bool red)
        {
            if (training) { hp = 0; Die(); return; }
            State = "recover"; timer = red ? 1.5f : .8f;
        }
        public void Damage(float amount, bool stagger)
        {
            if (!Alive) return;
            hp = training ? Mathf.Max(1, hp - amount) : hp - amount;
            SliceEffects.Ring(transform.position + Vector3.up * .25f, new Color(.65f,1,1), .45f);
            if (hp <= 0) Die();
            else if (stagger) { State = "recover"; timer = .7f; }
        }
        void Die()
        {
            var s = SliceSession.Instance;
            if (s != null) { s.Defeated.Add(id); if (training) s.Notify("The warden bows. You have learned the white deflect."); }
            gameObject.SetActive(false);
        }
    }
}
