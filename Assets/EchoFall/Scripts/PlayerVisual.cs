using UnityEngine;

namespace EchoFall.Movement
{
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerVisual : MonoBehaviour
    {
        public PlayerMotor motor;
        public Sprite[] frames;
        SpriteRenderer spriteRenderer;
        float animationClock;

        void Awake() => spriteRenderer = GetComponent<SpriteRenderer>();
        void LateUpdate()
        {
            if (motor == null || frames == null || frames.Length < 22) return;
            animationClock += Time.deltaTime;
            int frame = !motor.Grounded || motor.Dashing ? 20 : Mathf.Abs(motor.Velocity.x) > .25f
                ? 4 + Mathf.FloorToInt(animationClock * 36f) % 16
                : Mathf.FloorToInt(animationClock * 4f) % 4;
            spriteRenderer.sprite = frames[frame];
            spriteRenderer.flipX = motor.Facing < 0;
            transform.position = motor.RenderPosition;
        }
    }
}
