using UnityEngine;

namespace EchoFall.Movement
{
    public sealed class SliceEffects : MonoBehaviour
    {
        static Material material;
        LineRenderer line;
        float age;
        Color color;
        public static LineRenderer Line(GameObject go, Color color, float width)
        {
            if (material == null) material = new Material(Shader.Find("Sprites/Default"));
            var line = go.AddComponent<LineRenderer>(); line.sharedMaterial = material; line.startColor = line.endColor = color;
            line.startWidth = line.endWidth = width; line.sortingOrder = 30; line.numCapVertices = 3;
            return line;
        }
        public static void Ring(Vector2 center, Color color, float radius)
        {
            var go = new GameObject("Signal pulse"); var line = Line(go, color, .018f); line.positionCount = 33;
            for (int i = 0; i <= 32; i++) { float a = i * Mathf.PI / 16; line.SetPosition(i, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius); }
            var fx = go.AddComponent<SliceEffects>(); fx.line = line; fx.color = color;
        }
        public static void Slash(Vector2 center, Vector2 direction, Color color)
        {
            var go = new GameObject("Blade arc"); var line = Line(go, color, .045f); line.positionCount = 17;
            float start = Mathf.Atan2(direction.y, direction.x) - 1.1f;
            for (int i = 0; i < 17; i++) { float a = start + i * 2.2f / 16; line.SetPosition(i, center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * .75f); }
            var fx = go.AddComponent<SliceEffects>(); fx.line = line; fx.color = color;
        }
        void Update()
        {
            if (SliceSession.Instance != null && !SliceSession.Instance.Playing) return;
            age += Time.deltaTime; var c = color; c.a *= 1 - age / .25f;
            line.startColor = line.endColor = c;
            if (age >= .25f) Destroy(gameObject);
        }
    }
}
