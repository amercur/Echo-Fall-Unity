using UnityEngine;
namespace EchoFall.Movement
{
    [DefaultExecutionOrder(200)]
    public sealed class SlicePlayerPresentation : MonoBehaviour
    {
        public SliceCombat combat;
        public PlayerVisual visual;
        SpriteRenderer rendererComponent;
        void Awake() => rendererComponent = visual.GetComponent<SpriteRenderer>();
        void LateUpdate()
        {
            if (combat.AttackVisual > 0 && visual.frames.Length >= 30) rendererComponent.sprite = visual.frames[22 + Mathf.Clamp((int)((.22f - combat.AttackVisual) * 36),0,7)];
            else if (combat.Guarding && visual.frames.Length >= 32) rendererComponent.sprite = visual.frames[30 + (Time.frameCount / 8) % 2];
            rendererComponent.color = combat.MendProgress > 0 ? new Color(.65f,1,.9f) : Color.white;
        }
    }
}
