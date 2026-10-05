using UnityEngine;

namespace EchoFall.Movement
{
    [RequireComponent(typeof(TextMesh))]
    public sealed class LabMarker : MonoBehaviour
    {
        void Awake()
        {
            var text = GetComponent<TextMesh>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            GetComponent<MeshRenderer>().sharedMaterial = text.font.material;
        }
    }
}
