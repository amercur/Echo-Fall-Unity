using UnityEngine;
using UnityEngine.Rendering;

namespace EchoFall.Movement
{
    // Render-only offset: never writes the follow camera transform or movement values.
    [RequireComponent(typeof(Camera))]
    public sealed class PlayerCameraImpulse : MonoBehaviour
    {
        Camera view;
        RoomCamera follow;
        float remaining, strength, phase;
        public Vector2 Offset { get; private set; }
        void Awake() {view=GetComponent<Camera>();follow=GetComponent<RoomCamera>();}
        void OnEnable() {RenderPipelineManager.beginCameraRendering+=Begin;RenderPipelineManager.endCameraRendering+=End;}
        public void Kick(float amount) {strength=Mathf.Max(strength,Mathf.Clamp(amount,0,.065f));remaining=.16f;phase=0;}
        public void Clear() {remaining=strength=0;Offset=Vector2.zero;if(view!=null)view.ResetWorldToCameraMatrix();}
        void LateUpdate()
        {
            var s=SliceSession.Instance;
            if(s!=null && !s.Playing && s.Screen!=SliceScreen.Transfer) {Clear();return;}
            remaining=Mathf.Max(0,remaining-Time.unscaledDeltaTime);phase+=Time.unscaledDeltaTime;
            float envelope=remaining/.16f;
            Offset=new Vector2(Mathf.Sin(phase*93),Mathf.Cos(phase*111)*.65f)*strength*envelope*envelope;
            if(remaining<=0)strength=0;
        }
        void Begin(ScriptableRenderContext context,Camera camera)
        {
            if(camera!=view)return;
            view.ResetWorldToCameraMatrix();Vector2 offset=Offset;
            if(follow!=null)offset=follow.ClampCenter((Vector2)transform.position+offset,view.orthographicSize,view.aspect)-(Vector2)transform.position;
            view.worldToCameraMatrix=Matrix4x4.Translate(new Vector3(-offset.x,-offset.y,0))*view.worldToCameraMatrix;
        }
        void End(ScriptableRenderContext context,Camera camera) {if(camera==view)view.ResetWorldToCameraMatrix();}
        void OnDisable() {RenderPipelineManager.beginCameraRendering-=Begin;RenderPipelineManager.endCameraRendering-=End;Clear();}
    }
}
