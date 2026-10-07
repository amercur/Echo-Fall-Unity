using UnityEngine;

namespace EchoFall.Movement
{
    // Presentation only. All displacement is confined to decoration without colliders.
    public sealed class WakeAtmosphere : MonoBehaviour
    {
        public float sway = .015f, drift = .025f, speed = .4f, phase;
        public bool breathe;
        Vector3 origin;
        Quaternion rotation;
        SpriteRenderer sprite;
        Color color;
        void Awake() { origin=transform.localPosition; rotation=transform.localRotation; sprite=GetComponent<SpriteRenderer>(); if(sprite!=null)color=sprite.color; }
        void Update()
        {
            var s=SliceSession.Instance;
            if(s!=null && !s.Playing)return;
            float t=Time.time*speed+phase;
            transform.localPosition=origin+new Vector3(Mathf.Sin(t)*sway,Mathf.Sin(t*.7f)*drift,0);
            transform.localRotation=rotation*Quaternion.Euler(0,0,Mathf.Sin(t)*sway*12);
            if(breathe && sprite!=null) { var c=color; c.a*=.8f+Mathf.Sin(t)*.2f; sprite.color=c; }
        }
    }
}
