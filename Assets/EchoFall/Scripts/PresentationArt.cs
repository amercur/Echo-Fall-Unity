using System;
using System.Collections.Generic;
using UnityEngine;
namespace EchoFall.Movement
{
    public enum FootMaterial { Stone,Metal,Wood,Moss }
    public sealed class FootSurface : MonoBehaviour { public FootMaterial material; }
    public static class PresentationArt
    {
        static readonly Dictionary<string,Sprite[]> clips=new Dictionary<string,Sprite[]>();
        public static Sprite Frame(string state,float progress,bool loop=false)
        {
            if(!clips.TryGetValue(state,out var frames)){frames=Resources.LoadAll<Sprite>("Presentation/Player/"+state);Array.Sort(frames,(a,b)=>string.CompareOrdinal(a.name,b.name));clips[state]=frames;}
            return frames.Length==0?null:frames[Mathf.Clamp((int)((loop?Mathf.Repeat(progress,1):Mathf.Clamp01(progress))*frames.Length),0,frames.Length-1)];
        }
        static SpriteRenderer Prop(string asset,Transform parent,Vector2 position,float scale,int order,Color tint)
        {
            var sprite=Resources.Load<Sprite>("Presentation/Props/"+asset);if(sprite==null)return null;
            var go=new GameObject("Authored "+asset);go.transform.SetParent(parent,false);go.transform.position=position;go.transform.localScale=Vector3.one*scale;
            var r=go.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sortingOrder=order;r.color=tint;return r;
        }
        public static void Apply(SliceRoom room,SliceSession session)
        {
            if(room.transform.Find("Finished presentation")!=null)return;
            var root=new GameObject("Finished presentation").transform;root.SetParent(room.transform,false);
            bool garden=room.id=="garden"||room.id=="rootvault",engine=room.id=="cradle"||room.id=="lungs"||room.id=="mother";
            foreach(var collider in room.GetComponentsInChildren<Collider2D>(true))
            {
                if(collider.isTrigger)continue;
                var surface=collider.gameObject.AddComponent<FootSurface>();
                surface.material=garden?(collider.gameObject.layer==LayerMask.NameToLayer("EchoPlatform")?FootMaterial.Wood:FootMaterial.Moss):engine?FootMaterial.Metal:FootMaterial.Stone;
            }
            foreach(var npc in room.GetComponentsInChildren<SliceInteraction>(true))
            {
                string asset=npc.id=="keeper"?"keeper":npc.kind=="child"?"child":npc.kind=="creature"?"creature":null;
                if(asset==null)continue;
                foreach(var r in npc.GetComponentsInChildren<Renderer>(true))r.enabled=false;
                var figure=Prop(asset,npc.transform,(Vector2)npc.transform.position-Vector2.up*.35f,asset=="creature"?.38f:.32f,8,Color.white);
                if(figure!=null){var motion=figure.gameObject.AddComponent<WakeAtmosphere>();motion.sway=0;motion.drift=.004f;motion.speed=.9f;}
            }
            if(room.returnedCreature!=null)
            {
                foreach(var r in room.returnedCreature.GetComponentsInChildren<Renderer>(true))r.enabled=false;
                Prop("creature",room.returnedCreature,new Vector2(9,.05f),.38f,8,new Color(.65f,1,.85f));
            }
            if(garden||engine||room.id=="observatory")
            {
                foreach(var r in room.GetComponentsInChildren<Renderer>(true))
                    if(r.name=="Old branching trunk"||r.name=="Leafing branch"||r.name=="Suspended lung vessel"||r.name=="Copper manifold"||r.name=="Observatory plinth")r.enabled=false;
                var far=new GameObject("Finished regional depth").transform;far.SetParent(root,false);far.gameObject.AddComponent<SliceParallax>().factor=.12f;
                int count=garden?7:engine?6:0;
                for(int i=0;i<count;i++)
                {
                    var r=Prop(garden?"tree":"vessel",far,new Vector2(i*room.bounds.width/Mathf.Max(1,count-1)-.5f,garden?-.1f:1),garden?1.5f:1.25f,-54,garden?new Color(.24f,.36f,.29f):new Color(.38f,.28f,.30f));
                    if(r!=null){var motion=r.gameObject.AddComponent<WakeAtmosphere>();motion.sway=garden?.006f:.002f;motion.drift=engine?.02f:0;motion.phase=i*.7f;}
                }
                if(room.id=="observatory")Prop("plinth",root,new Vector2(5.2f,0),1.0f,-25,Color.white);
            }
            if(room.id=="cradle")
            {
                bool open=session.Archive.Remembers("sacrifice")&&!session.Archive.Remembers("betrayal");
                foreach(var line in room.GetComponentsInChildren<LineRenderer>(true))if(line.name=="An empty cage"||line.name=="A borrowed heartbeat")line.enabled=false;
                for(int i=0;i<4;i++)Prop(open?"cage-open":"cage",root,new Vector2(2+i*2.5f,2.7f),.55f,-15,new Color(.65f,.65f,.7f));
            }
        }
    }
}
