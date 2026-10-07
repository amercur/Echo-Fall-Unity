using UnityEngine;

namespace EchoFall.Movement
{
    [RequireComponent(typeof(Camera))]
    public sealed class WakeCameraPresentation : MonoBehaviour
    {
        // The authored reference framing is 960 x 540. Avoid exposing empty space
        // outside short rooms on ultrawide displays; do not change motor/camera tuning.
        Camera view;
        void Awake() => view=GetComponent<Camera>();
        void OnPreCull()
        {
            float screenAspect=(float)Screen.width/Mathf.Max(1,Screen.height);
            float width=Mathf.Min(1,(16f/9)/screenAspect);
            view.rect=new Rect((1-width)*.5f,0,width,1);
        }
        void OnDisable() { if(view!=null)view.rect=new Rect(0,0,1,1); }
    }
}
