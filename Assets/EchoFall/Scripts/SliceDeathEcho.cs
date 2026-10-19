using UnityEngine;
namespace EchoFall.Movement
{
    public sealed class SliceDeathEcho : MonoBehaviour
    {
        SpriteRenderer sprite;float age;Vector3 scale;
        static Material dissolve;
        MaterialPropertyBlock properties;
        void Awake(){properties=new MaterialPropertyBlock();}
        public static void Spawn(SpriteRenderer source)
        {
            var go=new GameObject("Fading combat remnant");go.transform.SetPositionAndRotation(source.transform.position,source.transform.rotation);go.transform.localScale=source.transform.lossyScale;
            var fx=go.AddComponent<SliceDeathEcho>();fx.sprite=go.AddComponent<SpriteRenderer>();fx.sprite.sprite=source.sprite;fx.sprite.flipX=source.flipX;fx.sprite.sharedMaterial=source.sharedMaterial;fx.sprite.sortingOrder=source.sortingOrder;fx.scale=go.transform.localScale;
            if(dissolve==null){var shader=Resources.Load<Shader>("Presentation/EchoDissolve");if(shader!=null)dissolve=new Material(shader);}
            if(dissolve!=null)fx.sprite.sharedMaterial=dissolve;
        }
        void Update(){age+=Time.unscaledDeltaTime;float t=Mathf.Clamp01(age/.65f);sprite.color=new Color(.6f,.9f,1,1-t);properties.SetFloat("_Dissolve",t);sprite.SetPropertyBlock(properties);transform.localScale=Vector3.Scale(scale,new Vector3(1+t*.1f,1-t*.25f,1));if(t>=1)Destroy(gameObject);}
    }
}
