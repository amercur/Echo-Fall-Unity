using UnityEngine;

namespace EchoFall.Movement
{
    public sealed class SliceProjectile : MonoBehaviour
    {
        Vector2 velocity;
        SliceEnemy source;
        float damage, life = 4;
        bool friendly;
        public static void Spawn(Vector2 position, Vector2 velocity, SliceEnemy source, float damage)
        {
            var go = new GameObject(source == null ? "Ember bolt" : "White bolt"); go.transform.position = position;
            var p = go.AddComponent<SliceProjectile>(); p.velocity = velocity; p.source = source; p.damage = damage; p.friendly = source == null;
            var line = SliceEffects.Line(go, source == null ? new Color(1,.5f,.2f) : Color.white, .04f);
            line.useWorldSpace = false; line.positionCount = 2; line.SetPosition(0, Vector3.zero); line.SetPosition(1, -(Vector3)velocity.normalized * .18f);
        }
        void Update()
        {
            var s = SliceSession.Instance; if (s == null || !s.Playing) return;
            float dt = Time.deltaTime; life -= dt;
            if (life <= 0) { Destroy(gameObject); return; }
            Vector2 old = transform.position, next = old + velocity * dt;
            if (Physics2D.Linecast(old, next, s.motor.solids)) { Destroy(gameObject); return; }
            transform.position = next;
            if (!friendly && Vector2.Distance(next, s.motor.Position + Vector2.up * .25f) < .25f)
            {
                if (s.combat.Hurt(1, old, false, source))
                { friendly = true; velocity = source != null ? ((Vector2)source.transform.position + Vector2.up * .25f - next).normalized * 5 : -velocity; damage = 3; }
                else Destroy(gameObject);
            }
            if (friendly) foreach (var e in FindObjectsByType<SliceEnemy>())
                if (e.Alive && e.HitRect.Contains(next)) { e.Damage(damage, true); Destroy(gameObject); break; }
        }
    }
}
