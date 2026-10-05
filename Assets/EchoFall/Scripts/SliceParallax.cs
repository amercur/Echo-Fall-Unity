using UnityEngine;
namespace EchoFall.Movement
{
    public sealed class SliceParallax : MonoBehaviour
    {
        public float factor = .18f;
        Vector3 origin;
        void Awake() => origin = transform.position;
        void LateUpdate()
        {
            if (Camera.main == null) return;
            var p = Camera.main.transform.position;
            transform.position = origin + new Vector3(p.x * factor, p.y * factor * .35f, 0);
        }
    }
}
