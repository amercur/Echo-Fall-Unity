using UnityEngine;
namespace EchoFall.Movement
{
    // Composition and narrative dressing only. No collision or progression ownership.
    public static class OpeningScenery
    {
        static readonly Color memory=new Color(.47f,.78f,.76f),ivory=new Color(.72f,.74f,.65f),rose=new Color(.62f,.34f,.42f);
        static LineRenderer Stroke(Transform parent,string name,Color color,float width,int order,params Vector3[] points)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);var line=SliceEffects.Line(go,color,width);line.useWorldSpace=false;line.positionCount=points.Length;line.SetPositions(points);line.sortingOrder=order;return line;
        }
        static void Arc(Transform parent,Vector2 center,float radius,Color color,int order)
        {
            var points=new Vector3[49];for(int i=0;i<points.Length;i++){float a=(35+i*290f/48)*Mathf.Deg2Rad;points[i]=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius;}
            Stroke(parent,"Incomplete signal",color,.025f,order,points);
        }
        public static void Apply(SliceRoom room,SliceSession s)
        {
            if(room.transform.Find("Opening identity")!=null)return;
            foreach(var item in room.GetComponentsInChildren<SliceInteraction>(true))
            {
                if(item.id=="bell-secret")item.story="The bell was rung for every body sent below.\nSomeone stopped recording the names. The strokes in the margin kept going.";
                if(item.id=="pogo-secret")item.story="The engineer wrote: a bridge remembers the weight that crossed it.\nIn the margin, another hand: even when that weight was carried.";
                if(item.id=="first-transfer")item.label="THE LISTENING GLASS";
            }
            if(room.id!="wake")return;
            var root=new GameObject("Opening identity").transform;root.SetParent(room.transform,false);
            // Subdue repeated distant shapes; leave collision rims and interaction arches intact.
            foreach(var sprite in room.GetComponentsInChildren<SpriteRenderer>())
                if(sprite.name=="Distant spire"||sprite.name=="Distant tower")sprite.color*=new Color(.64f,.68f,.74f,1);
            foreach(var line in room.GetComponentsInChildren<LineRenderer>())
                if(line.name=="Pointed vault"||line.name=="Vault mullion"){line.startColor*=.58f;line.endColor=line.startColor;}
            var dial=new GameObject("The broken signal dial").transform;dial.SetParent(root,false);dial.gameObject.AddComponent<SliceParallax>().factor=.035f;
            Arc(dial,new Vector2(3.4f,2.45f),1.08f,new Color(.20f,.35f,.38f,.85f),-12);
            Arc(dial,new Vector2(3.4f,2.45f),.91f,new Color(.13f,.24f,.28f,.65f),-12);
            for(int i=0;i<8;i++)
            {
                float a=(70+i*35)*Mathf.Deg2Rad;Vector2 normal=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
                Stroke(dial,"Filed hour",new Color(.28f,.39f,.39f,.7f),.016f,-11,(Vector2)new Vector2(3.4f,2.45f)+normal*.96f,new Vector2(3.4f,2.45f)+normal*1.05f);
            }
            Stroke(root,"The warden's abandoned standard",ivory*.42f,.025f,-4,new Vector3(5.3f,.05f),new Vector3(5.3f,1.75f),new Vector3(5.72f,1.75f));
            Stroke(root,"Rose vow",rose*.7f,.09f,-3,new Vector3(5.44f,1.7f),new Vector3(5.44f,1.2f),new Vector3(5.53f,1.27f));
            Stroke(root,"A promise split",ivory,.013f,-2,new Vector3(5.41f,1.58f),new Vector3(5.41f,1.37f));
            Stroke(root,"A promise split",ivory,.013f,-2,new Vector3(5.48f,1.58f),new Vector3(5.48f,1.43f));
            // Low shelter frames the creature; it does not resemble a walkable ledge.
            Stroke(root,"Shelter fracture",new Color(.19f,.27f,.29f),.055f,-4,new Vector3(8.6f,.06f),new Vector3(8.48f,.64f),new Vector3(8.7f,.8f));
            var trace=new GameObject("Remembered hand trail").transform;trace.SetParent(root,false);
            bool scar=s.Archive.Remembers("fire");bool returned=s.Archive.RootsOpen;
            trace.gameObject.SetActive(scar||returned);
            Color tint=scar?new Color(.55f,.26f,.15f):memory*.7f;
            Stroke(trace,"A root follows the hand",tint,.018f,3,new Vector3(8.85f,.03f),new Vector3(8.4f,.06f),new Vector3(8.1f,.02f),new Vector3(7.65f,.08f),new Vector3(7.3f,.04f));
            if(returned)for(int i=0;i<3;i++)Stroke(trace,"Living tips",tint,.014f,3,new Vector3(7.6f+i*.35f,.06f),new Vector3(7.5f+i*.35f,.17f));
            // Existing actor atlases stay intact. The training actor gains a named, quiet landmark.
            foreach(var enemy in room.GetComponentsInChildren<SliceEnemy>())if(enemy.training)enemy.gameObject.name="The last oath / practice warden";
            PresentDecision(room,s);
        }
        public static void PresentDecision(SliceRoom room,SliceSession s)
        {
            if(room==null||room.id!="wake"||s.Decision==null)return;
            foreach(var item in room.GetComponentsInChildren<SliceInteraction>(true))if(item.kind=="creature")
            {
                foreach(var renderer in item.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
                item.label=s.Decision=="fire"?"THE WARM STONE":"WHERE IT RESTED";
            }
            if(room.emberScar!=null)room.emberScar.gameObject.SetActive(s.Decision=="fire"||s.Archive.Remembers("fire"));
        }
    }
}
