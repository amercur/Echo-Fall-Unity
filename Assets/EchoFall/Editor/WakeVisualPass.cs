using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace EchoFall.Movement.Editor
{
    public static class WakeVisualPass
    {
        const string Root="Assets/EchoFall";
        const string GroupName="Visual quality — authored composition";
        static Sprite block,arch,spire,ledge;
        static Material unlit,lit,veil;
        static string roomId;
        static int meshId;
        static readonly Color Stone=new Color(.22f,.29f,.32f), Pale=new Color(.52f,.61f,.61f), Cyan=new Color(.43f,.8f,.79f), Gold=new Color(.72f,.53f,.31f);
        [MenuItem("Echo Fall/Apply Visual Quality Pass")]
        public static void Apply()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            Prepare();
            var audit=new System.Collections.Generic.List<string>();
            foreach(var id in new[]{"wake","belfry","cistern","archive","procession"})
            {
                var scene=EditorSceneManager.OpenScene(Root+"/Scenes/Wake_"+id+".unity");
                var room=UnityEngine.Object.FindAnyObjectByType<SliceRoom>();
                string before=CollisionSignature(room);
                ApplyRoom(room);
                if(before!=CollisionSignature(room))throw new InvalidOperationException("Visual pass altered gameplay collision in "+id);
                audit.Add(id+": all gameplay colliders preserved exactly");
                EditorSceneManager.SaveScene(scene);
            }
            var bootstrap=EditorSceneManager.OpenScene(Root+"/Scenes/WakeSlice.unity");
            var camera=UnityEngine.Object.FindAnyObjectByType<RoomCamera>();
            if(camera.GetComponent<WakeCameraPresentation>()==null)camera.gameObject.AddComponent<WakeCameraPresentation>();
            var player=UnityEngine.Object.FindAnyObjectByType<SlicePlayerPresentation>();
            var sprite=player.visual.GetComponent<SpriteRenderer>(); sprite.sharedMaterial=unlit;
            if(player.transform.Find("Foot contact shadow")==null)
            {
                var shadow=Sprite("Foot contact shadow",block,player.transform.position,new Vector2(.5f,.10f),new Color(.005f,.013f,.02f,.6f),5,player.transform,false);
                shadow.sharedMaterial=veil; shadow.transform.localPosition=new Vector3(0,.005f,0);
            }
            EditorSceneManager.SaveScene(bootstrap);
            AssetDatabase.SaveAssets();
            File.WriteAllLines("Docs/Validation/visual-collision-audit.txt",audit);
            Debug.Log("Visual pass applied to five rooms and WakeSlice; gameplay geometry preserved.");
        }
        static string CollisionSignature(SliceRoom room) => string.Join("\n",room.GetComponentsInChildren<BoxCollider2D>(true).Select(c=>c.gameObject.layer+"|"+c.transform.position.ToString("R")+"|"+c.transform.lossyScale.ToString("R")+"|"+c.size.ToString("R")+"|"+c.offset.ToString("R")+"|"+c.isTrigger).OrderBy(s=>s));
        public static void ApplyCurrentRoom(SliceRoom room)
        {
            Prepare();
            if(!AssetDatabase.IsValidFolder(Root+"/Art/VisualMeshes/"+room.id))AssetDatabase.CreateFolder(Root+"/Art/VisualMeshes",room.id);
            ApplyRoom(room);
        }
        static void Prepare()
        {
            block=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Block.png");
            arch=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/ruin-arch.png");
            spire=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/choir-spire.png");
            ledge=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/ruin-ledge.png");
            unlit=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WakeUnlit.mat");
            lit=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WakeLit.mat");
            veil=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/AtmosphericVeil.mat");
            if(veil==null) { veil=new Material(Shader.Find("EchoFall/Atmospheric Veil")); veil.SetFloat("_Motion",1); AssetDatabase.CreateAsset(veil,Root+"/Materials/AtmosphericVeil.mat"); }
            foreach(var id in new[]{"wake","belfry","cistern","archive","procession"})Directory.CreateDirectory(Root+"/Art/VisualMeshes/"+id);
            AssetDatabase.Refresh();
        }
        static Transform Group(string name,Transform parent)
        { var go=new GameObject(name); go.transform.SetParent(parent,false); return go.transform; }
        static SpriteRenderer Sprite(string name,Sprite sprite,Vector2 p,Vector2 scale,Color color,int order,Transform parent,bool lighting=false)
        {
            var go=Group(name,parent); go.position=p; go.localScale=new Vector3(scale.x,scale.y,1);
            var renderer=go.gameObject.AddComponent<SpriteRenderer>(); renderer.sprite=sprite; renderer.sharedMaterial=lighting?lit:unlit; renderer.color=color; renderer.sortingOrder=order; return renderer;
        }
        static void Box(string name,Vector2 p,Vector2 size,Color color,int order,Transform parent,bool lighting=false) => Sprite(name,block,p,size,color,order,parent,lighting);
        static void Line(string name,Vector2[] points,Color color,float width,int order,Transform parent,bool loop=false)
        {
            var go=Group(name,parent); var line=go.gameObject.AddComponent<LineRenderer>(); line.sharedMaterial=unlit; line.useWorldSpace=false;
            line.positionCount=points.Length; line.SetPositions(points.Select(p=>go.InverseTransformPoint(p)).ToArray());
            line.startColor=line.endColor=color; line.startWidth=line.endWidth=width; line.sortingOrder=order; line.loop=loop; line.numCornerVertices=2;
        }
        static void Polygon(string name,Vector2[] points,Color color,int order,Transform parent)
        {
            var go=Group(name,parent); string path=Root+"/Art/VisualMeshes/"+roomId+"/Shape_"+(meshId++).ToString("000")+".asset";
            var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path); bool create=mesh==null; if(create)mesh=new Mesh(); else mesh.Clear();
            mesh.name=name; mesh.vertices=points.Select(p=>go.InverseTransformPoint(p)).ToArray();
            mesh.colors=Enumerable.Repeat(color,points.Length).ToArray(); mesh.uv=points.Select(p=>Vector2.zero).ToArray();
            var triangles=new int[(points.Length-2)*3]; for(int i=0;i<points.Length-2;i++){triangles[i*3]=0;triangles[i*3+1]=i+1;triangles[i*3+2]=i+2;}
            mesh.triangles=triangles; mesh.RecalculateBounds();
            if(create)AssetDatabase.CreateAsset(mesh,path); else EditorUtility.SetDirty(mesh);
            go.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=go.gameObject.AddComponent<MeshRenderer>(); renderer.sharedMaterial=unlit; renderer.sortingOrder=order;
        }
        static void Glow(string name,Vector2 p,Vector2 size,Color color,int order,Transform parent)
        { var sprite=Sprite(name,block,p,size,color,order,parent); sprite.sharedMaterial=veil; }
        static Vector2[] Circle(Vector2 center,float radius,int count=64,float start=0,float end=6.2831853f)
        { return Enumerable.Range(0,count).Select(i=>center+new Vector2(Mathf.Cos(Mathf.Lerp(start,end,i/(float)(count-1))),Mathf.Sin(Mathf.Lerp(start,end,i/(float)(count-1))))*radius).ToArray(); }
        static void Light(string name,Vector2 p,Color color,float power,float radius,Transform parent)
        {
            var go=Group(name,parent); go.position=p; var light=go.gameObject.AddComponent<Light2D>(); light.lightType=Light2D.LightType.Point;
            light.color=color; light.intensity=power; light.pointLightInnerRadius=.2f; light.pointLightOuterRadius=radius; light.shadowIntensity=.35f;
        }
        static void ApplyRoom(SliceRoom room)
        {
            roomId=room.id; meshId=0;
            bool hadPass=room.transform.Find(GroupName)!=null;
            // Earlier scene authoring supplied world vertices to local-space renderers.
            // Correct the original portal/creature strokes once, before decorating them.
            if(!hadPass)foreach(var line in room.GetComponentsInChildren<LineRenderer>(true))
                if(!line.useWorldSpace && line.transform.position!=Vector3.zero)
                    for(int i=0;i<line.positionCount;i++)line.SetPosition(i,line.transform.InverseTransformPoint(line.GetPosition(i)));
            foreach(var name in new[]{GroupName,"Distant cathedral / parallax","Gothic ribs / parallax","Weathering and offerings","Foreground broken piers"})
            { var old=room.transform.Find(name); if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject); }
            foreach(var light in room.GetComponentsInChildren<Light2D>(true))
            {
                if(light.name=="Window spill")UnityEngine.Object.DestroyImmediate(light.gameObject);
                else if(light.lightType==Light2D.LightType.Global) {light.intensity=.82f; light.color=new Color(.73f,.80f,.86f);}
                else {light.intensity=Mathf.Min(light.intensity,.65f); light.shadowIntensity=.3f;}
            }
            var root=Group(GroupName,room.transform);
            var far=Group("01 / distant civilization",root); far.gameObject.AddComponent<SliceParallax>().factor=.14f;
            var middle=Group("02 / cathedral shell",root); middle.gameObject.AddComponent<SliceParallax>().factor=.055f;
            var landmarks=Group("03 / room landmark",root);
            var surface=Group("04 / surface language",root);
            var foreground=Group("05 / framing and decay",root); foreground.gameObject.AddComponent<SliceParallax>().factor=-.012f;
            float width=room.bounds.width,top=room.bounds.yMax;
            Color haze=room.id=="cistern"?new Color(.10f,.25f,.25f,.45f):room.id=="archive"?new Color(.22f,.18f,.25f,.45f):new Color(.13f,.22f,.3f,.5f);
            Box("Deep void",new Vector2(width*.5f,top*.5f),new Vector2(width+14,top+12),new Color(.018f,.034f,.055f),-100,root);
            Glow("Distant suspended light",new Vector2(width*.55f,top*.60f),new Vector2(17,9),haze,-90,far);
            // Three sparse architectural masses; their scale hierarchy, not repetition,
            // carries the sense of a much larger civilization beyond the room.
            for(int i=0;i<3;i++)
            {
                float x=width*(.12f+i*.36f), scale=i==1?.82f:.62f;
                Sprite("Lost choir tower",spire,new Vector2(x,-.25f),Vector2.one*scale,new Color(.22f,.32f,.41f),-80,far);
                Sprite("Far broken arcade",arch,new Vector2(x+1.1f,-.15f),Vector2.one*.7f,new Color(.23f,.32f,.39f),-78,far);
            }
            Glow("Floor depth",new Vector2(width*.5f,.4f),new Vector2(width+4,2.6f),new Color(.13f,.25f,.29f,.22f),-65,middle);
            foreach(float x in new[]{-.35f,width*.5f,width+.35f})
            {
                Sprite("Cathedral pier",spire,new Vector2(x,-.4f),new Vector2(.85f,1.12f),new Color(.23f,.31f,.37f),-55,middle);
                Sprite("Ruined high arch",arch,new Vector2(x+1.9f,-.05f),Vector2.one*1.17f,new Color(.27f,.35f,.40f),-54,middle);
            }
            // Each room has one authored landmark and a distinct light temperature.
            switch(room.id)
            {
                case "wake": Rose(new Vector2(5.65f,3.15f),1.25f,landmarks); break;
                case "belfry": Belfry(landmarks); break;
                case "cistern": Cistern(landmarks); break;
                case "archive": Archive(landmarks); break;
                case "procession": Procession(landmarks); break;
            }
            Surfaces(room,surface);
            foreach(var item in room.GetComponentsInChildren<SliceInteraction>(true))DecorateInteraction(item,root);
            // Only a few drifting motes in the shaft of light, rather than a starfield.
            for(int i=0;i<18;i++)
            {
                float x=width*.18f+Mathf.Repeat(i*1.317f,width*.65f), y=.65f+Mathf.Repeat(i*.783f,Mathf.Min(5,top-.5f));
                var mote=Sprite("Airborne ash",block,new Vector2(x,y),new Vector2(.012f,.018f),new Color(.6f,.74f,.76f,.22f),-10,root);
                var motion=mote.gameObject.AddComponent<WakeAtmosphere>(); motion.sway=.09f; motion.drift=.13f; motion.speed=.23f; motion.phase=i; motion.breathe=true;
            }
            foreach(float x in new[]{-.75f,width+.75f})
                Sprite("Near stone silhouette",spire,new Vector2(x,-.9f),Vector2.one*.52f,new Color(.025f,.047f,.06f),18,foreground);
            foreach(var sprite in room.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if(sprite.name=="Masonry")sprite.color=new Color(.16f,.22f,.26f);
                if(sprite.name=="Walkable rim" || sprite.name=="Worn cornice")sprite.color=new Color(.62f,.68f,.64f);
                if(sprite.name=="Carved floating ledge")sprite.color=new Color(.82f,.88f,.88f);
                if(sprite.name=="Bell core" || sprite.name=="Relic")sprite.color=new Color(.7f,.58f,.38f);
            }
            if(room.rootBridge!=null)
                foreach(var line in room.rootBridge.GetComponentsInChildren<LineRenderer>())
                {line.startColor=line.endColor=new Color(.26f,.46f,.32f);line.startWidth=line.endWidth=.042f;}
        }
        static void Rose(Vector2 p,float r,Transform root)
        {
            Glow("Light through broken rose",p,Vector2.one*r*4,new Color(.25f,.53f,.58f,.24f),-48,root);
            for(int i=0;i<16;i++)
            {
                if(i==3 || i==4)continue;
                float a=i*Mathf.PI/8,b=a+Mathf.PI/8-.025f;
                Vector2 A=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),B=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
                Polygon("Rose / cut stone",new[]{p+A*r,p+B*r,p+B*(r-.12f),p+A*(r-.12f)},i%3==0?new Color(.23f,.34f,.38f):new Color(.16f,.25f,.29f),-43,root);
                Line("Rose tracery",new[]{p+A*.23f,p+A*(r-.16f)},new Color(.19f,.34f,.36f),.026f,-42,root);
                if(i%2==0)Polygon("Remaining glass",new[]{p+A*.3f,p+A*(r-.18f),p+B*(r-.18f)},new Color(.14f,.31f,.34f,.27f),-44,root);
            }
            Line("Empty heart",Circle(p,.24f,36),new Color(.27f,.45f,.44f),.028f,-42,root);
            Light("Rose window spill",p,Cyan,.85f,3.4f,root);
            Glow("Rose floor reflection",new Vector2(p.x,.09f),new Vector2(3,.4f),new Color(.34f,.65f,.63f,.14f),-3,root);
        }
        static void Chain(string name,Vector2 top,Vector2 bottom,Transform root,int order=-8)
        {
            int count=Mathf.CeilToInt(Vector2.Distance(top,bottom)/.11f);
            for(int i=0;i<count;i++)
            {
                Vector2 p=Vector2.Lerp(top,bottom,i/(float)count);
                var points=Circle(Vector2.zero,1,12).Select(v=>p+new Vector2(v.x*(i%2==0?.026f:.012f),v.y*.058f)).ToArray();
                Line(name,points,new Color(.25f,.3f,.32f),.013f,order,root);
            }
        }
        static void Belfry(Transform root)
        {
            Chain("West suspension",new Vector2(6.3f,10),new Vector2(6.5f,6.5f),root,-12);
            Chain("Broken east suspension",new Vector2(8.2f,10),new Vector2(8f,6.1f),root,-12);
            Polygon("The silent bell",new[]{new Vector2(6.2f,5.7f),new Vector2(6.5f,6.15f),new Vector2(6.7f,7.1f),new Vector2(7.1f,7.45f),new Vector2(7.5f,7.1f),new Vector2(7.7f,6.15f),new Vector2(8f,5.7f)},new Color(.28f,.25f,.20f),-11,root);
            Line("Bell lip",new[]{new Vector2(6.2f,5.7f),new Vector2(8,5.7f)},new Color(.48f,.41f,.28f),.085f,-10,root);
            Line("Bell fracture",new[]{new Vector2(7.2f,7.35f),new Vector2(7.1f,6.9f),new Vector2(7.35f,6.6f),new Vector2(7.18f,5.73f)},new Color(.08f,.13f,.17f),.055f,-9,root);
            Chain("Hanging clapper",new Vector2(7.1f,5.85f),new Vector2(7.1f,5.2f),root,-9);
            Sprite("Clapper shard",spire,new Vector2(7.1f,4.8f),Vector2.one*.09f,new Color(.5f,.5f,.41f),-8,root);
            Glow("Cold skylight",new Vector2(4.4f,8.3f),new Vector2(3.1f,7),new Color(.26f,.45f,.67f,.26f),-25,root);
            Light("Belfry sky opening",new Vector2(4.5f,8),new Color(.55f,.72f,1),1,4.8f,root);
            Light("Shaft readable fill",new Vector2(4.4f,3.6f),Cyan,.55f,3.4f,root);
        }
        static void Cistern(Transform root)
        {
            for(int i=0;i<4;i++)Sprite("Drowned aqueduct arch",arch,new Vector2(3.4f+i*2.2f,.15f),Vector2.one*.58f,new Color(.24f,.41f,.4f),-38,root);
            Line("Fractured aqueduct crown",new[]{new Vector2(1.9f,2.73f),new Vector2(5.2f,2.73f),new Vector2(5.6f,2.56f),new Vector2(6.1f,2.7f),new Vector2(11.4f,2.7f)},new Color(.2f,.34f,.34f),.12f,-37,root);
            Glow("Water mist",new Vector2(7,-.05f),new Vector2(10,2.4f),new Color(.16f,.48f,.43f,.28f),-4,root);
            for(int i=0;i<3;i++)
            {
                var waterfall=Sprite("Falling seep",block,new Vector2(4.8f+i*2.1f,.95f),new Vector2(.09f,2.7f),new Color(.19f,.5f,.46f,.13f),-32,root);
                waterfall.sharedMaterial=veil; var motion=waterfall.gameObject.AddComponent<WakeAtmosphere>();motion.sway=.012f;motion.drift=0;motion.breathe=true;motion.speed=.5f;
            }
            Vine(new Vector2(2.1f,3.3f),1.8f,root); Vine(new Vector2(11.2f,3.6f),2,root);
            Light("Water reflected light",new Vector2(6.8f,.65f),new Color(.35f,.8f,.62f),.7f,4.1f,root);
        }
        static void Archive(Transform root)
        {
            for(int shelf=0;shelf<3;shelf++)
            {
                float y=.7f+shelf*.78f;
                Sprite("Archive shelf masonry",ledge,new Vector2(5.3f,y-.15f),new Vector2(1.3f,.18f),new Color(.27f,.26f,.32f),-29,root);
                for(int book=0;book<11;book++)
                {
                    float x=3+book*.44f;
                    Sprite("Slotted memory reliquary",spire,new Vector2(x,y),Vector2.one*.085f,new Color(.39f,.41f,.45f),-28,root);
                    if((book+shelf)%4==0)Glow("Stored pulse",new Vector2(x,y+.27f),new Vector2(.14f,.36f),new Color(.65f,.5f,.32f,.30f),-27,root);
                }
            }
            Vector2 center=new Vector2(8.45f,3.45f);
            Line("Fractured archive lens",Circle(center,.94f,54,.25f,5.7f),new Color(.43f,.35f,.26f),.055f,-23,root);
            Line("Lens inner band",Circle(center,.82f,52,.7f,5.8f),new Color(.28f,.31f,.39f),.025f,-23,root);
            Polygon("Suspended glass",new[]{center+new Vector2(-.27f,.57f),center+new Vector2(.38f,.17f),center+new Vector2(.09f,-.65f),center+new Vector2(-.34f,-.22f)},new Color(.2f,.33f,.4f,.7f),-22,root);
            Line("Glass fault",new[]{center+new Vector2(-.27f,.57f),center+new Vector2(.09f,-.65f)},new Color(.4f,.64f,.68f),.017f,-21,root);
            Glow("Archive amber dust",center,new Vector2(4,4),new Color(.58f,.38f,.20f,.17f),-24,root);
            Light("The last filed life",center,new Color(1,.7f,.4f),.85f,3.5f,root);
        }
        static void Procession(Transform root)
        {
            Sprite("The divided gate",arch,new Vector2(10.8f,-.06f),Vector2.one*1.3f,new Color(.46f,.42f,.46f),-26,root);
            Sprite("Fallen pier",spire,new Vector2(12.3f,.18f),new Vector2(.34f,.5f),new Color(.22f,.24f,.29f),-24,root).transform.rotation=Quaternion.Euler(0,0,65);
            foreach(float x in new[]{4.7f,9.1f,13.7f})
            {
                Chain("Pilgrim hanging",new Vector2(x,4.65f),new Vector2(x,3.8f),root,-19);
                var cloth=Group("Torn pilgrim standard",root);
                Polygon("Rose cloth",new[]{new Vector2(x-.24f,3.85f),new Vector2(x+.3f,3.85f),new Vector2(x+.26f,2.3f),new Vector2(x+.04f,2.52f),new Vector2(x-.2f,2.2f)},new Color(.29f,.15f,.22f),-18,cloth);
                Line("Faded emblem",new[]{new Vector2(x,3.57f),new Vector2(x+.12f,3.15f),new Vector2(x,2.95f),new Vector2(x-.12f,3.15f),new Vector2(x,3.57f)},new Color(.47f,.33f,.32f),.015f,-17,cloth);
                var motion=cloth.gameObject.AddComponent<WakeAtmosphere>();motion.sway=.035f;motion.drift=0;motion.speed=.37f;motion.phase=x;
            }
            Glow("Dust beyond the court",new Vector2(12,2.8f),new Vector2(8,5),new Color(.42f,.27f,.28f,.2f),-40,root);
            Light("Fallen court dusk",new Vector2(11,3.3f),new Color(1,.65f,.49f),.7f,4.2f,root);
            Light("Pilgrim moonlight",new Vector2(3,3),new Color(.55f,.73f,1),.6f,3.4f,root);
        }
        static void Vine(Vector2 top,float length,Transform root)
        {
            var points=Enumerable.Range(0,18).Select(i=>top+new Vector2(Mathf.Sin(i*.7f)*.055f,-length*i/17)).ToArray();
            Line("Trailing root",points,new Color(.17f,.28f,.23f),.025f,-3,root);
            for(int i=2;i<16;i+=3)
            {
                var p=points[i]; float sign=i%2==0?1:-1;
                Polygon("Wilted leaf",new[]{p,p+new Vector2(sign*.14f,.06f),p+new Vector2(sign*.08f,-.09f)},new Color(.22f,.34f,.26f),-2,root);
            }
        }
        static void Surfaces(SliceRoom room,Transform root)
        {
            var colliders=room.GetComponentsInChildren<BoxCollider2D>(true);
            foreach(var collider in colliders)
            {
                if(collider.isTrigger || (room.rootBridge!=null && collider.transform.IsChildOf(room.rootBridge)))continue;
                Vector2 center=collider.transform.TransformPoint(collider.offset), size=Vector2.Scale(collider.size,collider.transform.lossyScale);
                float top=center.y+size.y*.5f,left=center.x-size.x*.5f,right=center.x+size.x*.5f;
                if(size.x<size.y)
                {
                    // Vertical shaft collision gets a carved stone casing, never extra collision.
                    for(float y=center.y-size.y*.5f+.2f;y<top;y+=.55f)
                    {Box("Pier course",new Vector2(center.x,y),new Vector2(size.x+.055f,.075f),new Color(.3f,.38f,.39f),3,root,true);}
                    continue;
                }
                Line("Walkable edge highlight",new[]{new Vector2(left+.015f,top),new Vector2(right-.015f,top)},new Color(.65f,.72f,.68f),.015f,5,root);
                if(size.x>.65f)
                {
                    for(int i=0;i<3;i++)
                    {
                        float x=Mathf.Lerp(left+.13f,right-.13f,(i+.5f)/3);
                        Polygon("Chipped cornice",new[]{new Vector2(x,top-.05f),new Vector2(x+.08f,top-.055f),new Vector2(x+.055f,top-.12f)},new Color(.07f,.12f,.16f),4,root);
                    }
                }
                if(size.x>3)
                {
                    foreach(float x in new[]{left+.27f,right-.35f})
                    {
                        Polygon("Fallen masonry",new[]{new Vector2(x-.13f,top),new Vector2(x-.09f,top+.08f),new Vector2(x+.06f,top+.1f),new Vector2(x+.15f,top)},new Color(.27f,.32f,.33f),3,root);
                        Box("Votive wax",new Vector2(x+.2f,top+.07f),new Vector2(.025f,.14f),new Color(.46f,.43f,.3f),4,root);
                        Glow("Votive ember",new Vector2(x+.2f,top+.15f),new Vector2(.16f,.22f),new Color(.93f,.57f,.22f,.48f),5,root);
                    }
                }
            }
        }
        static void DecorateInteraction(SliceInteraction item,Transform root)
        {
            var old= item.transform.Find("Signal thread"); if(old!=null)UnityEngine.Object.DestroyImmediate(old.gameObject);
            var art=Group("Threshold / "+item.id,root); Vector2 p=item.transform.position;
            if(item.kind=="gate" || item.kind=="mirror")
            {
                Color c=item.kind=="mirror"?Gold:Cyan;
                Glow("Threshold veil",p+Vector2.up*.08f,new Vector2(.46f,.9f),new Color(c.r,c.g,c.b,.18f),-2,art);
                Line("Inlaid threshold",new[]{p+new Vector2(-.24f,-.33f),p+new Vector2(.24f,-.33f)},new Color(c.r,c.g,c.b,.55f),.018f,4,art);
                if(!string.IsNullOrEmpty(item.requires)||!string.IsNullOrEmpty(item.memory))
                    Line("Sealed knot",Circle(p+.08f*Vector2.up,.085f,20),new Color(.54f,.42f,.32f),.013f,3,art);
                if(item.kind=="mirror")
                {
                    Line("Broken glass seam",new[]{p+new Vector2(-.07f,.38f),p+new Vector2(.05f,.15f),p+new Vector2(-.03f,-.2f)},new Color(.64f,.75f,.73f),.012f,3,art);
                }
            }
            else if(item.kind=="lore" && item.label.StartsWith("SEALED"))
            {
                Sprite("Walled threshold",arch,p-Vector2.up*.35f,Vector2.one*.23f,new Color(.38f,.4f,.42f),-2,art,true);
                Chain("Bound door",p+new Vector2(-.17f,.35f),p+new Vector2(.17f,-.22f),art,3);
            }
            else if(item.kind=="relic" || item.kind=="cache")
            {
                // Source signal glyph remains, now enclosed in a physical reliquary.
                Line("Relic frame",new[]{p+new Vector2(-.12f,-.13f),p+new Vector2(-.12f,.12f),p+new Vector2(.12f,.12f),p+new Vector2(.12f,-.13f)},new Color(.4f,.38f,.31f),.025f,3,art);
            }
        }
    }
}
