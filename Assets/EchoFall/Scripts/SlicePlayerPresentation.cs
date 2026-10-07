using UnityEngine;
namespace EchoFall.Movement
{
    // Retains scene references. PlayerVisual is the sole sprite/pose owner.
    public sealed class SlicePlayerPresentation : MonoBehaviour
    {
        public SliceCombat combat;
        public PlayerVisual visual;
        void Start() { if (visual != null) visual.Bind(combat); }
    }
}
