using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoFall.Movement.Editor
{
    public static partial class WakeSliceBuilder
    {
        [MenuItem("Echo Fall/Build Phase 4 World")]
        public static void BuildWorld()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            AssetDatabase.Refresh();
            block=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Block.png");
            lit=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WakeLit.mat");unlit=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WakeUnlit.mat");
            arch=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/ruin-arch.png");spire=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/choir-spire.png");ledge=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/ruin-ledge.png");
            var source=JsonUtility.FromJson<Source>(File.ReadAllText(Root+"/Data/WorldExpansion.json"));
            foreach(var data in source.rooms)
            {
                BuildRoom(data);
                var room=UnityEngine.Object.FindAnyObjectByType<SliceRoom>();
                if(room.id=="rootvault")
                {
                    var bridge=Group("MERCY / roots over the hollow",room.transform);room.rootBridge=bridge;
                    Geometry(new RectData{x=255,y=360,w=345,h=15},"EchoPlatform",bridge,bridge,true);
                    Stroke("Remembered living roots",new[]{new Vector2(2.5f,.91f),new Vector2(4.2f,.80f),new Vector2(6,.95f)},new Color(.4f,.65f,.31f),.06f,4,bridge);
                }
                RegionArt(room);
                EditorSceneManager.SaveScene(room.gameObject.scene);
            }
            // Extend the existing threshold without removing the Phase 3 victory glass.
            EditorSceneManager.OpenScene(Root+"/Scenes/Wake_king.unity");
            var court=UnityEngine.Object.FindAnyObjectByType<SliceRoom>();
            var glass=court.GetComponentsInChildren<SliceInteraction>(true).First(i=>i.kind=="victory");
            glass.id="court-transfer";glass.transform.position=new Vector2(11.9f,.35f);glass.story="The Court releases you. Transfer here or continue to the Cradle.";
            if(!court.GetComponentsInChildren<SliceInteraction>().Any(i=>i.id=="king-east"))
                CreateInteraction(new Item{id="king-east",kind="gate",x=1260,y=417,target="cradle",entry="west",requires="king",label="THE CRADLE / CONTINUE"},court.transform);
            EditorSceneManager.SaveScene(court.gameObject.scene);
            foreach(string id in new[]{"wake","archive"})
            {
                EditorSceneManager.OpenScene(Root+"/Scenes/Wake_"+id+".unity");
                var room=UnityEngine.Object.FindAnyObjectByType<SliceRoom>();
                foreach(var link in WorldCatalog.Find(id).links.Where(l=>l.target=="observatory" || l.target=="lungs"))
                {
                    var item=room.GetComponentsInChildren<SliceInteraction>(true).First(i=>i.id==link.id);
                    item.kind="gate";item.requires=link.requires;item.memory=link.memory;item.blockedBy=link.blockedBy;
                    item.label=link.target=="lungs"?"THE BREATHING PASSAGE":"THE SIGNAL WELL / OBSERVATORY";
                }
                EditorSceneManager.SaveScene(room.gameObject.scene);
            }
            // The secret return lands on the same upper Garden platform as its concealed entrance.
            var scenes=EditorBuildSettings.scenes.ToList();
            foreach(var data in source.rooms)
            {
                string path=Root+"/Scenes/Wake_"+data.id+".unity";
                if(!scenes.Any(s=>s.path==path))scenes.Add(new EditorBuildSettingsScene(path,true));
            }
            EditorBuildSettings.scenes=scenes.ToArray();AssetDatabase.SaveAssets();
            EditorSceneManager.OpenScene(Root+"/Scenes/WakeSlice.unity");
        }
        static void RegionArt(SliceRoom room)
        {
            foreach(string name in new[]{"Distant cathedral / parallax","Gothic ribs / parallax","Weathering and offerings","Foreground broken piers"})
            {var old=room.transform.Cast<Transform>().FirstOrDefault(t=>t.name==name);if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);}
            foreach(var light in room.GetComponentsInChildren<Light2D>())
                if(light.name=="Window spill")UnityEngine.Object.DestroyImmediate(light.gameObject);
            var root=Group("Regional composition",room.transform);
            var far=Group("Distant regional silhouette",root);far.gameObject.AddComponent<SliceParallax>().factor=.16f;
            var mid=Group("Regional landmark",root);mid.gameObject.AddComponent<SliceParallax>().factor=.04f;
            bool garden=room.id=="garden" || room.id=="rootvault", choir=room.id=="choir", sky=room.id=="observatory";
            Color accent=garden?new Color(.58f,.78f,.39f):choir?new Color(.68f,.62f,.94f):sky?new Color(.51f,.77f,.91f):new Color(.88f,.53f,.38f);
            Color dark=garden?new Color(.028f,.067f,.045f):choir?new Color(.04f,.027f,.08f):new Color(.035f,.044f,.066f);
            float width=room.bounds.width,top=room.bounds.yMax;
            Box("Regional void",new Vector2(width/2,top/2),new Vector2(width+20,top+20),dark,-100,root,false);
            var ambient=room.GetComponentsInChildren<Light2D>().First(l=>l.lightType==Light2D.LightType.Global);
            ambient.color=Color.Lerp(Color.white,accent,.25f);ambient.intensity=.88f;
            if(garden)
            {
                for(int n=0;n<6;n++)
                {
                    float x=n*2.9f-.7f,h=3.2f+(n%3)*.55f;
                    Stroke("Old branching trunk",new[]{new Vector2(x,-.2f),new Vector2(x+.3f,h*.5f),new Vector2(x-.2f,h),new Vector2(x+.45f,h+.5f)},new Color(.12f,.23f,.15f),.20f,-55,far);
                    for(int b=0;b<4;b++)
                    {
                        float y=1.5f+b*.6f;
                        Stroke("Leafing branch",new[]{new Vector2(x+.15f,y),new Vector2(x+(b%2==0?1:-1),y+.5f),new Vector2(x+(b%2==0?1.4f:-1.4f),y+.45f)},new Color(.20f,.34f,.20f),.08f,-54,far);
                    }
                }
                for(int n=0;n<18;n++)
                {float x=n*width/18;Stroke("Living root",new[]{new Vector2(x,-.18f),new Vector2(x+.3f,.04f),new Vector2(x+.8f,.1f)},new Color(.25f,.37f,.21f),.045f,-3,root);}
                Ring("Open sky through canopy",new Vector2(width*.57f,4.05f),1.3f,accent*.65f,.045f,mid);
                if(room.id=="garden")
                {
                    var secret=room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="garden-vault");
                    foreach(var renderer in secret.GetComponentsInChildren<Renderer>())renderer.enabled=false;
                    Stroke("A root bent around an opening",new[]{new Vector2(7.6f,3.15f),new Vector2(7.6f,3.55f),new Vector2(7.85f,3.7f),new Vector2(8.08f,3.5f)},accent,.025f,5,root);
                    var child=room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="child");
                    Box("Child silhouette",(Vector2)child.transform.position+new Vector2(0,-.15f),new Vector2(.15f,.26f),accent,7,child.transform,false);
                }
            }
            else if(choir)
            {
                for(int n=0;n<15;n++)
                {
                    float x=n*1.05f,h=2.3f+Mathf.Abs(7-n)*.28f;
                    Box("Suspended organ pipe",new Vector2(x,h*.5f+.8f),new Vector2(.14f,h),new Color(.22f,.20f,.34f),-50,far,false);
                    Ring("Pipe aperture",new Vector2(x,.8f),.10f,accent*.5f,.02f,far);
                }
                for(int n=0;n<4;n++)Ring("Concentric memory halo",new Vector2(10.6f,3),.4f+n*.4f,Color.Lerp(accent,dark,n*.16f),.025f,mid);
                Stroke("Suspended choir horizon",new[]{new Vector2(-2,2.1f),new Vector2(width+2,2.1f)},accent*.4f,.018f,-20,mid);
            }
            else if(sky)
            {
                for(int n=0;n<3;n++)Ring("Observation lens",new Vector2(5.2f,3.1f),1+n*.43f,accent*(.65f-n*.12f),.035f,mid);
                Stroke("Lens axis",new[]{new Vector2(2.7f,3.1f),new Vector2(7.7f,3.1f)},accent*.5f,.018f,-20,mid);
                for(int n=0;n<20;n++)Box("Distant star",new Vector2(Mathf.Repeat(n*1.73f,width),2+Mathf.Repeat(n*.67f,4)),Vector2.one*.018f,accent*.55f,-65,far,false);
                Box("Observatory plinth",new Vector2(5.2f,.8f),new Vector2(1.2f,1.6f),new Color(.14f,.21f,.28f),-25,mid,false);
            }
            else
            {
                for(int n=0;n<7;n++)
                {
                    float x=n*2.4f-.2f;
                    Stroke("Hanging engine suspension",new[]{new Vector2(x,top+1),new Vector2(x,1.5f)},new Color(.24f,.20f,.22f),.07f,-55,far);
                    Ring("Suspended lung vessel",new Vector2(x,2.4f),.65f,new Color(.31f,.24f,.25f),.11f,far);
                    Box("Copper manifold",new Vector2(x,1.2f),new Vector2(.35f,1.4f),new Color(.26f,.19f,.20f),-52,far,false);
                }
                if(room.id=="mother")
                {
                    Ring("Dormant engine core",new Vector2(7,2.5f),1.6f,accent*.55f,.12f,mid);
                    for(int n=0;n<6;n++)Stroke("Radial engine rib",new[]{new Vector2(7,2.5f),new Vector2(7+Mathf.Cos(n*Mathf.PI/3)*1.6f,2.5f+Mathf.Sin(n*Mathf.PI/3)*1.6f)},accent*.32f,.06f,-20,mid);
                }
                if(room.id=="cradle")
                    for(int n=0;n<5;n++)Stroke("Cradle threads",new[]{new Vector2(2+n*.55f,4.8f),new Vector2(2.3f+n*.5f,2.2f),new Vector2(4.5f,1.1f)},accent*.45f,.016f,-20,mid);
            }
            for(int n=0;n<24;n++)
            {
                var mote=Sprite(garden?"Drifting pollen":"Signal dust",block,new Vector2(Mathf.Repeat(n*1.371f,width),.5f+Mathf.Repeat(n*.719f,top)),new Vector2(.015f,.024f),new Color(accent.r,accent.g,accent.b,.28f),-8,root,false);
                var motion=mote.gameObject.AddComponent<WakeAtmosphere>();motion.sway=.13f;motion.drift=.12f;motion.speed=.25f;motion.phase=n;motion.breathe=true;
            }
            for(int n=0;n<3;n++)Light("Regional light",new Vector2(width*(.15f+n*.35f),2.8f),accent,.55f,3.4f,root);
            foreach(var renderer in room.GetComponentsInChildren<SpriteRenderer>())
            {if(renderer.name=="Masonry")renderer.color=Color.Lerp(new Color(.17f,.22f,.25f),accent,.12f);if(renderer.name=="Walkable rim" || renderer.name=="Worn cornice")renderer.color=Color.Lerp(Color.gray,accent,.35f);}
        }
        static void Ring(string name,Vector2 center,float radius,Color color,float thickness,Transform parent)
        {
            var points=Enumerable.Range(0,49).Select(i=>center+new Vector2(Mathf.Cos(i*Mathf.PI/24),Mathf.Sin(i*Mathf.PI/24))*radius).ToArray();
            Stroke(name,points,color,thickness,-30,parent);
        }
    }
}
