using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.U2D.Sprites;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace EchoFall.Movement.Editor
{
    // Import effective source geometry, then author only the explicit first-slice adaptations.
    // This builder owns WakeSlice/Wake_*; it never rebuilds MovementLab or the Player prefab.
    public static class WakeSliceBuilder
    {
        const string Root = "Assets/EchoFall";
        [Serializable] public class Source { public Room[] rooms; }
        [Serializable] public class RectData { public float x,y,w,h; }
        [Serializable] public class SpawnData { public string id; public float x,y; }
        [Serializable] public class Item { public string id,kind,label,story,target,entry,requires,memory,blockedBy,flag; public float x,y; }
        [Serializable] public class Enemy { public string type; public float x,floor,left,right; public bool training; }
        [Serializable] public class Decor { public string kind; public float x,y,scale; }
        [Serializable] public class Room
        {
            public string id,name; public float width,top,bottom;
            public RectData[] ground,platforms,solids,hazards,pogo;
            public SpawnData[] spawns; public Item[] interactions; public Enemy[] enemies; public Decor[] decor;
        }
        static Sprite block, arch, ledge, spire;
        static Material lit, unlit;
        static readonly Color Stone = new Color(.18f,.25f,.3f), Edge = new Color(.36f,.46f,.49f), Cyan = new Color(.35f,.86f,.81f), Gold = new Color(.88f,.66f,.37f);
        static Vector2 Point(float x,float y) => new Vector2(x/100,(452-y)/100);

        // Incremental Phase 3 authoring: never rebuild the existing five-room composition.
        [MenuItem("Echo Fall/Build Phase 3 Court")]
        public static void BuildCourt()
        {
            if(EditorApplication.isPlaying)throw new InvalidOperationException("Stop Play Mode first.");
            AssetDatabase.Refresh();
            block=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Block.png");
            lit=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WakeLit.mat");
            unlit=AssetDatabase.LoadAssetAtPath<Material>(Root+"/Materials/WakeUnlit.mat");
            arch=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/ruin-arch.png");
            spire=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/choir-spire.png");
            ledge=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Wake/ruin-ledge.png");
            SliceEnemyArt("king");
            var data=JsonUtility.FromJson<Source>(File.ReadAllText(Root+"/Data/KingSource.json")).rooms[0];
            BuildRoom(data);
            var room=UnityEngine.Object.FindAnyObjectByType<SliceRoom>();
            var end=room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="king-east");
            end.kind="victory";end.requires="king";end.label="A FUTURE UNMADE / TRANSFER";
            end.story="The Court releases you. The deeper chapter remains beyond this milestone.";
            var actor=Group("The King",room.transform);actor.position=new Vector2(9.27f,0);
            var king=actor.gameObject.AddComponent<SliceKing>();king.id="king/boss";king.kind="king";king.hp=40;king.left=3.22f;king.right=10.67f;
            king.frames=AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Wake/king.png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
            king.visual=Sprite("Original King atlas",king.frames[0],actor.position,Vector2.one*.65f,Color.white,9,actor,false);
            WakeVisualPass.ApplyCurrentRoom(room);
            // Arena seals are visible at the same boundaries used by the motor clamp.
            foreach(float x in new[]{2.65f,11.2f})
            {
                var seal=Group("Court seal",room.transform);seal.position=new Vector2(x,0);
                var gate=seal.gameObject.AddComponent<SliceArenaSeal>();gate.king=king;
            }
            EditorSceneManager.SaveScene(room.gameObject.scene);
            var procession=EditorSceneManager.OpenScene(Root+"/Scenes/Wake_procession.unity");
            var gateToKing=UnityEngine.Object.FindObjectsByType<SliceInteraction>().First(i=>i.target=="king");
            gateToKing.kind="gate";gateToKing.label="THE COURT / FACE THE KING";gateToKing.requires=gateToKing.memory=gateToKing.blockedBy=null;
            // Existing source roster: one deliberate Sentinel/Lancer pair; no new swarm.
            var lancer=UnityEngine.Object.FindObjectsByType<SliceEnemy>().First(e=>e.id=="procession/enemy-1");
            var sentinel=UnityEngine.Object.FindObjectsByType<SliceEnemy>().First(e=>e.id=="procession/enemy-3");
            sentinel.transform.position=new Vector2(lancer.transform.position.x+.65f,lancer.transform.position.y);
            sentinel.left=lancer.left;sentinel.right=lancer.right;
            EditorSceneManager.SaveScene(procession);
            string path=Root+"/Scenes/Wake_king.unity";
            if(!EditorBuildSettings.scenes.Any(s=>s.path==path))EditorBuildSettings.scenes=EditorBuildSettings.scenes.Concat(new[]{new EditorBuildSettingsScene(path,true)}).ToArray();
            AssetDatabase.SaveAssets();EditorSceneManager.OpenScene(Root+"/Scenes/WakeSlice.unity");
        }

        [MenuItem("Echo Fall/Build Wake Slice")]
        public static void Build()
        {
            if(EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play Mode before authoring.");
            AssetDatabase.Refresh();
            PrepareArt();
            var source=JsonUtility.FromJson<Source>(File.ReadAllText(Root+"/Data/WakeSource.json"));
            foreach(var room in source.rooms) BuildRoom(room);
            BuildBootstrap();
            var paths = new[] { Root+"/Scenes/WakeSlice.unity", Root+"/Scenes/MovementLab.unity" }
                .Concat(source.rooms.Select(r=>Root+"/Scenes/Wake_"+r.id+".unity"));
            var others=EditorBuildSettings.scenes.Where(s=>!paths.Contains(s.path));
            EditorBuildSettings.scenes=paths.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(others).ToArray();
            AssetDatabase.SaveAssets();
            WakeVisualPass.Apply();
            Debug.Log("Wake slice authored: five connected source rooms, original movement prefab preserved.");
        }
        static void PrepareArt()
        {
            block=AssetDatabase.LoadAssetAtPath<Sprite>(Root+"/Art/Block.png");
            lit=Material("WakeLit","Universal Render Pipeline/2D/Sprite-Lit-Default");
            unlit=Material("WakeUnlit","Universal Render Pipeline/2D/Sprite-Unlit-Default");
            arch=ImportSingle("ruin-arch",new Vector2(.5f,1-441.696f/512));
            spire=ImportSingle("choir-spire",new Vector2(.5f,1-567.831f/640));
            ledge=ImportSingle("ruin-ledge",new Vector2(.5f,1-49.404f/160));
            foreach(string kind in new[]{"sentinel","lancer","drone"}) SliceEnemyArt(kind);
        }
        static Material Material(string name,string shader)
        {
            string path=Root+"/Materials/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){ material=new Material(Shader.Find(shader)); AssetDatabase.CreateAsset(material,path); }
            return material;
        }
        static Sprite ImportSingle(string name,Vector2 pivot)
        {
            string path=Root+"/Art/Wake/"+name+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Single;
            importer.spritePixelsPerUnit=100; importer.spritePivot=pivot;
            var settings=new TextureImporterSettings(); importer.ReadTextureSettings(settings); settings.spriteAlignment=(int)SpriteAlignment.Custom; settings.spritePivot=pivot; importer.SetTextureSettings(settings);
            importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport(); return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
        static void SliceEnemyArt(string kind)
        {
            string path=Root+"/Art/Wake/"+kind+".png";
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=TextureImporterType.Sprite; importer.spriteImportMode=SpriteImportMode.Multiple;
            importer.spritePixelsPerUnit=100; importer.alphaIsTransparency=true; importer.mipmapEnabled=false; importer.textureCompression=TextureImporterCompression.Uncompressed;
            importer.SaveAndReimport();
            var factory=new SpriteDataProviderFactories(); factory.Init();
            var provider=factory.GetSpriteEditorDataProviderFromObject(importer); provider.InitSpriteEditorDataProvider();
            var existing=provider.GetSpriteRects();
            var rects=Enumerable.Range(0,8).Select(i=>new SpriteRect { name=kind+"_"+i, spriteID=i<existing.Length?existing[i].spriteID:GUID.Generate(), rect=new Rect(i%4*256,(1-i/4)*256,256,256), alignment=SpriteAlignment.Custom,pivot=new Vector2(118.597f/256,1-202.719f/256) }).ToArray();
            provider.SetSpriteRects(rects); provider.GetDataProvider<ISpriteNameFileIdDataProvider>().SetNameFileIdPairs(rects.Select(r=>new SpriteNameFileIdPair(r.name,r.spriteID))); provider.Apply(); importer.SaveAndReimport();
        }
        static Transform Group(string name,Transform parent=null)
        { var go=new GameObject(name); if(parent!=null)go.transform.SetParent(parent,false); return go.transform; }
        static SpriteRenderer Sprite(string name,Sprite sprite,Vector2 position,Vector2 scale,Color color,int order,Transform parent,bool lighting=true)
        {
            var go=Group(name,parent); go.position=position; go.localScale=new Vector3(scale.x,scale.y,1);
            var r=go.gameObject.AddComponent<SpriteRenderer>(); r.sprite=sprite; r.color=color; r.sortingOrder=order; r.sharedMaterial=lighting?lit:unlit; return r;
        }
        static void Box(string name,Vector2 position,Vector2 size,Color color,int order,Transform parent,bool lighting=true)
            => Sprite(name,block,position,size,color,order,parent,lighting);
        static void Stroke(string name,Vector2[] points,Color color,float width,int order,Transform parent)
        {
            var go=Group(name,parent); var line=go.gameObject.AddComponent<LineRenderer>(); line.sharedMaterial=unlit; line.startColor=line.endColor=color;
            line.startWidth=line.endWidth=width; line.sortingOrder=order; line.positionCount=points.Length; line.useWorldSpace=false;
            line.SetPositions(points.Select(p=>go.InverseTransformPoint(p)).ToArray());
        }
        static void BuildRoom(Room data)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var root=Group(data.name); var room=root.gameObject.AddComponent<SliceRoom>(); room.id=data.id;
            room.title=data.name.Split('/')[0].Trim(); room.subtitle=data.name.Contains("/")?data.name.Split('/')[1].Trim():data.name;
            room.bounds=new Rect(0,(452-data.bottom)/100,data.width/100,(data.bottom-data.top)/100);
            room.spawns=data.spawns.Select(p=>new SliceSpawn{id=p.id,feet=Point(p.x,p.y)+Vector2.up*.02f}).ToArray();
            var collision=Group("Collision — gameplay only",root); var architecture=Group("Architecture — no collision",root);
            foreach(var r in data.ground) Geometry(r,"EchoSolid",collision,architecture,false);
            foreach(var r in data.solids) Geometry(r,"EchoSolid",collision,architecture,false);
            foreach(var r in data.platforms) Geometry(r,"EchoPlatform",collision,architecture,true);
            foreach(var r in data.hazards)
            {
                Collider(r,"EchoHazard",collision);
                Box("Black water",Point(r.x+r.w/2,r.y+r.h/2),new Vector2(r.w/100,r.h/100),new Color(.025f,.13f,.17f),2,architecture,false);
                Stroke("Water surface",new[]{Point(r.x,r.y),Point(r.x+r.w,r.y)},Cyan*.5f,.015f,3,architecture);
            }
            Scenery(data,root);
            foreach(var d in data.decor) Sprite("Original "+d.kind,d.kind=="spire"?spire:arch,Point(d.x,d.y),Vector2.one*d.scale,Color.white,-2,architecture);
            var actors=Group("Encounters",root); int counter=0;
            foreach(var e in data.enemies)
            {
                var go=Group(e.type+" "+counter,actors); go.position=Point(e.x,e.floor);
                var enemy=go.gameObject.AddComponent<SliceEnemy>(); enemy.id=data.id+"/enemy-"+counter++; enemy.kind=e.type; enemy.training=e.training;
                enemy.hp=e.type=="lancer"?10:e.type=="drone"?5:7; enemy.left=e.left/100; enemy.right=e.right/100;
                enemy.frames=AssetDatabase.LoadAllAssetsAtPath(Root+"/Art/Wake/"+e.type+".png").OfType<Sprite>().OrderBy(s=>s.name).ToArray();
                enemy.visual=Sprite("Animated original art",enemy.frames[0],go.position,Vector2.one*.34f,Color.white,9,go);
            }
            var interactions=Group("Interactions",root);
            foreach(var item in data.interactions)
            {
                if(item.kind=="style" || item.kind=="body" || item.kind=="door")
                { item.kind="lore"; item.story="A life recorded, not erased. The glass carries one decision; the world keeps its consequences. The deeper forms and identities await a later chapter."; }
                if(item.target!=null && !new[]{"wake","belfry","cistern","archive","procession"}.Contains(item.target))
                { item.kind="lore"; item.requires=item.memory=item.blockedBy=null; item.label="SEALED / BEYOND THIS SLICE"; item.story="Beyond this threshold lies another chapter. Explore the five open rooms, then return to the transfer glass."; }
                // Keep one original decision in Wake; move the crowded glass to a distinct upper dais in Archive.
                if(item.kind=="mirror")continue;
                CreateInteraction(item,interactions);
            }
            if(data.id=="archive")
                CreateInteraction(new Item{id="slice-glass",kind="mirror",x=860,y=135,label="THE TRANSFER GLASS"},interactions);
            if(data.id=="wake")
            {
                // Retain immediate access to the first return without forcing a difficult climb.
                CreateInteraction(new Item{id="first-transfer",kind="mirror",x=340,y=327,label="THE TRANSFER GLASS"},interactions);
                var creature=Group("Remembered creature",architecture); room.returnedCreature=creature;
                Creature(creature,new Vector2(9,.12f),Cyan);
                var scar=Group("Ember scar",architecture); room.emberScar=scar;
                Stroke("Charred outline",new[]{new Vector2(8.7f,.01f),new Vector2(8.85f,.07f),new Vector2(9.03f,.02f),new Vector2(9.3f,.03f)},new Color(.8f,.3f,.15f),.04f,4,scar);
                Light("Scar warmth",new Vector2(9,.15f),Gold,.5f,1.3f,scar);
            }
            if(data.id=="cistern")
            {
                var bridge=Group("MERCY — living root bridge",root); room.rootBridge=bridge;
                // Remembered geometry is deliberately separate from the imported source collision.
                Geometry(new RectData{x=420,y=310,w=570,h=15},"EchoPlatform",bridge,bridge,true);
                for(int i=0;i<5;i++) Stroke("Living root",new[]{new Vector2(3.9f,1.4f-i*.05f),new Vector2(6.5f,1.25f-i*.07f),new Vector2(10,1.5f-i*.04f)},Cyan*.7f,.025f,4,bridge);
            }
            foreach(var r in data.pogo)
            {
                var point=Group("Pogo bell",root); point.position=Point(r.x+r.w/2,r.y);
                point.gameObject.AddComponent<SlicePogo>(); Diamond("Bell core",point.position,.16f,Gold,point);
                Light("Bell glow",point.position,Gold,.5f,.7f,point);
            }
            var global=Group("Ambient 2D",root).gameObject.AddComponent<Light2D>(); global.lightType=Light2D.LightType.Global;
            global.color=new Color(.57f,.68f,.8f); global.intensity=.7f;
            EditorSceneManager.SaveScene(scene,Root+"/Scenes/Wake_"+data.id+".unity");
        }
        static void Collider(RectData r,string layer,Transform parent)
        {
            var go=Group(layer,parent); go.position=Point(r.x+r.w/2,r.y+r.h/2); go.gameObject.layer=LayerMask.NameToLayer(layer);
            var box=go.gameObject.AddComponent<BoxCollider2D>(); box.size=new Vector2(r.w/100,r.h/100); box.isTrigger=layer=="EchoHazard";
        }
        static void Geometry(RectData r,string layer,Transform collision,Transform art,bool platform)
        {
            Collider(r,layer,collision); Vector2 p=Point(r.x+r.w/2,r.y), size=new Vector2(r.w/100,r.h/100);
            if(platform)
            {
                Sprite("Carved floating ledge",ledge,p,new Vector2(size.x/3.84f,.5f),Color.white,2,art);
                Box("Walkable rim",p-Vector2.up*.025f,new Vector2(size.x,.05f),Edge,3,art);
            }
            else
            {
                var wall=Sprite("Masonry",block,p-Vector2.up*size.y*.5f,size,Stone,0,art);
                if(size.x>.8f) wall.gameObject.AddComponent<ShadowCaster2D>();
                for(float y=.18f;y<size.y;y+=.24f)
                {
                    Stroke("Mortar course",new[]{p+new Vector2(-size.x/2,-y),p+new Vector2(size.x/2,-y)},new Color(.10f,.16f,.2f),.012f,1,art);
                    for(float x=.24f+((int)(y/.24f)%2)*.26f;x<size.x;x+=.56f)
                        Box("Stone joint",p+new Vector2(x-size.x/2,-y+.1f),new Vector2(.012f,.20f),new Color(.10f,.16f,.2f),1,art);
                }
                Box("Worn cornice",p-Vector2.up*.04f,new Vector2(size.x,.08f),Edge,2,art);
                Box("Cornice shadow",p-Vector2.up*.11f,new Vector2(size.x,.045f),new Color(.07f,.12f,.16f),2,art);
            }
        }
        static void Scenery(Room room,Transform root)
        {
            float width=room.width/100; float height=(452-room.top)/100;
            var sky=Group("Distant cathedral / parallax",root); sky.gameObject.AddComponent<SliceParallax>().factor=.18f;
            var mid=Group("Gothic ribs / parallax",root); mid.gameObject.AddComponent<SliceParallax>().factor=.07f;
            Color far=room.id=="cistern"?new Color(.055f,.15f,.16f):new Color(.075f,.12f,.18f);
            for(int i=-4;i<width+6;i++)
            {
                float h=1.4f+Mathf.Abs(Mathf.Sin(i*12.3f))*2.4f;
                Box("Distant tower",new Vector2(i*1.4f,h*.5f),new Vector2(.9f,h),far,-30,sky,false);
                Sprite("Distant spire",spire,new Vector2(i*1.4f,0),Vector2.one*(h/5.68f),far,-30,sky,false);
                for(int k=0;k<3;k++) Box("Unlit window",new Vector2(i*1.4f-.17f+k*.17f,h-.55f),new Vector2(.045f,.3f),new Color(.025f,.065f,.105f),-29,sky,false);
            }
            for(float x=-3;x<width+4;x+=2.6f)
            {
                Stroke("Pointed vault",new[]{new Vector2(x-1.2f,-.2f),new Vector2(x-1.2f,height*.45f),new Vector2(x-.75f,height*.78f),new Vector2(x,height),new Vector2(x+.75f,height*.78f),new Vector2(x+1.2f,height*.45f),new Vector2(x+1.2f,-.2f)},new Color(.105f,.18f,.23f),.065f,-20,mid);
                Stroke("Vault mullion",new[]{new Vector2(x,.6f),new Vector2(x,height)},new Color(.08f,.135f,.18f),.025f,-21,mid);
                var beam=Sprite("Dust in window light",block,new Vector2(x+.35f,height*.45f),new Vector2(.38f,height*1.2f),new Color(.35f,.66f,.7f,.025f),-17,mid,false);
                beam.transform.rotation=Quaternion.Euler(0,0,16);
            }
            var details=Group("Weathering and offerings",root);
            if(room.id=="belfry")
            {
                Stroke("Bell suspension",new[]{new Vector2(7.4f,10),new Vector2(7.4f,8.4f)},Edge*.65f,.045f,-5,details);
                Stroke("Broken bronze bell",new[]{new Vector2(6.7f,7.6f),new Vector2(6.95f,7.8f),new Vector2(7.1f,8.3f),new Vector2(7.4f,8.45f),new Vector2(7.7f,8.3f),new Vector2(7.82f,7.8f),new Vector2(8.1f,7.6f),new Vector2(6.7f,7.6f)},Gold*.48f,.10f,-5,details);
                Diamond("Bell clapper",new Vector2(7.4f,7.55f),.14f,Gold*.5f,details);
            }
            if(room.id=="archive")
                for(int shelf=0;shelf<4;shelf++)for(int book=0;book<18;book++)
                {
                    float x=2.2f+book*.28f,y=.6f+shelf*.68f;
                    Box("Filed glass life",new Vector2(x,y),new Vector2(.13f,.39f),new Color(.14f,.22f,.25f),-7,details,false);
                    Box("Archive inscription",new Vector2(x,y+.04f),new Vector2(.012f,.18f),new Color(.42f,.36f,.22f),-6,details,false);
                }
            if(room.id=="procession")
                for(int i=0;i<6;i++)
                {
                    float x=1.8f+i*2.3f;
                    Stroke("Pilgrim standard",new[]{new Vector2(x,.1f),new Vector2(x,3.7f),new Vector2(x+.55f,3.7f)},Stone,.045f,-5,details);
                    Box("Faded banner",new Vector2(x+.23f,3.05f),new Vector2(.42f,1.2f),new Color(.18f,.11f,.17f),-5,details,false);
                    Stroke("Frayed hem",new[]{new Vector2(x+.04f,2.48f),new Vector2(x+.13f,2.38f),new Vector2(x+.23f,2.57f),new Vector2(x+.4f,2.44f)},new Color(.18f,.11f,.17f),.07f,-5,details);
                }
            for(int i=0;i<(int)(width*7);i++)
            {
                float x=i*.147f, h=.03f+Mathf.Abs(Mathf.Sin(i*7.3f))*.12f;
                if(room.id=="cistern" && x>3.1f && x<11.4f)continue;
                Box("Broken stone",new Vector2(x,h*.4f),new Vector2(.06f,h),i%4==0?Edge:Stone,3,details);
                if(i%11==0) { Box("Votive",new Vector2(x,.10f),new Vector2(.023f,.15f),Gold*.6f,4,details); Diamond("Flame",new Vector2(x,.19f),.026f,Gold,details); }
            }
            for(int i=0;i<100;i++)
            {
                float x=Mathf.Repeat(i*2.71828f,width),y=Mathf.Repeat(i*1.618f,Mathf.Max(3,height));
                Box("Dust mote",new Vector2(x,y),Vector2.one*(i%4==0?.012f:.006f),new Color(.4f,.7f,.73f,.28f),-8,details,false);
            }
            var foreground=Group("Foreground broken piers",root); foreground.gameObject.AddComponent<SliceParallax>().factor=-.035f;
            foreach(float x in new[]{-.65f,width+.65f})
                Sprite("Foreground buttress",spire,new Vector2(x,-.5f),Vector2.one*.48f,new Color(.025f,.055f,.075f),18,foreground,false);
            for(float x=1;x<width;x+=3.2f)Light("Window spill",new Vector2(x,2.9f),room.id=="archive"?Gold:Cyan,.65f,3.1f,root);
        }
        static void Light(string name,Vector2 position,Color color,float intensity,float radius,Transform parent)
        {
            var go=Group(name,parent); go.position=position; var light=go.gameObject.AddComponent<Light2D>(); light.lightType=Light2D.LightType.Point;
            light.color=color; light.intensity=intensity; light.pointLightOuterRadius=radius; light.pointLightInnerRadius=.1f; light.shadowIntensity=.65f;
        }
        static void Diamond(string name,Vector2 center,float size,Color color,Transform parent)
        {
            var r=Sprite(name,block,center,Vector2.one*size,color,5,parent,false); r.transform.rotation=Quaternion.Euler(0,0,45);
        }
        static void Creature(Transform parent,Vector2 p,Color color)
        {
            Stroke("Small curled body",new[]{p+new Vector2(-.2f,-.05f),p+new Vector2(-.14f,.05f),p+new Vector2(.06f,.09f),p+new Vector2(.16f,.01f),p+new Vector2(.08f,-.06f),p+new Vector2(-.2f,-.05f)},color,.055f,5,parent);
            Diamond("Living ember",p+new Vector2(.04f,.02f),.06f,Gold,parent);
        }
        static void CreateInteraction(Item item,Transform parent)
        {
            var go=Group(item.label,parent); go.position=Point(item.x,item.y);
            var i=go.gameObject.AddComponent<SliceInteraction>(); i.id=item.id; i.kind=item.kind; i.label=item.label; i.story=item.story;
            i.target=item.target; i.entry=item.entry; i.requires=item.requires; i.memory=item.memory; i.blockedBy=item.blockedBy; i.flag=item.flag;
            Vector2 feet=go.position-Vector3.up*.35f;
            if(item.kind=="gate" || item.kind=="mirror")
            {
                Sprite(item.kind=="mirror"?"Transfer glass frame":"Passage arch",arch,feet,Vector2.one*.24f,Color.white,-1,go);
                Stroke("Signal thread",new[]{feet+new Vector2(-.14f,.15f),feet+new Vector2(0,.7f),feet+new Vector2(.14f,.15f)},item.kind=="mirror"?Gold:Cyan,.012f,1,go);
                Light("Threshold glow",feet+Vector2.up*.4f,item.kind=="mirror"?Gold:Cyan,.65f,1.3f,go);
            }
            else if(item.kind=="bench")
            {
                Box("Anchor seat",feet+Vector2.up*.1f,new Vector2(.65f,.07f),Edge,4,go);
                foreach(float x in new[]{-.23f,.23f})Box("Anchor leg",feet+new Vector2(x,.02f),new Vector2(.04f,.2f),Stone,4,go);
                Diamond("Signal",feet+Vector2.up*.42f,.11f,Cyan,go); Light("Anchor aura",feet+Vector2.up*.4f,Cyan,.8f,1.8f,go);
            }
            else if(item.kind=="creature")Creature(go,feet+Vector2.up*.13f,new Color(.58f,.53f,.42f));
            else { Diamond("Relic",go.position,.1f,item.kind=="lever"?Cyan:Gold,go); Light("Relic aura",go.position,Gold,.4f,.8f,go); }
        }
        static void BuildBootstrap()
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var player=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Prefabs/Player.prefab"));
            player.name="Wanderer"; player.transform.position=new Vector3(1.21f,.02f,0);
            var motor=player.GetComponent<PlayerMotor>(); var combat=player.AddComponent<SliceCombat>(); combat.motor=motor;
            var visual=player.GetComponentInChildren<PlayerVisual>(); visual.GetComponent<SpriteRenderer>().sharedMaterial=lit;
            var presentation=player.AddComponent<SlicePlayerPresentation>(); presentation.combat=combat; presentation.visual=visual;
            Light("Wanderer signal",new Vector2(1.21f,.4f),Cyan,.45f,1.1f,player.transform);
            var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(RoomCamera)); camera.tag="MainCamera";
            var view=camera.GetComponent<Camera>(); view.orthographic=true; view.orthographicSize=2.7f; view.backgroundColor=new Color(.025f,.045f,.075f); view.clearFlags=CameraClearFlags.SolidColor; view.nearClipPlane=.1f; view.farClipPlane=100;
            camera.transform.position=new Vector3(4.8f,1.82f,-10); var follow=camera.GetComponent<RoomCamera>(); follow.target=motor;
            var session=new GameObject("Wake session").AddComponent<SliceSession>(); session.motor=motor; session.combat=combat; session.follow=follow;
            session.gameObject.AddComponent<SliceSound>();
            var hud=new GameObject("Echo Fall HUD",typeof(RectTransform)).AddComponent<SliceHUD>(); hud.session=session; session.hud=hud;
            EditorSceneManager.SaveScene(scene,Root+"/Scenes/WakeSlice.unity");
        }
    }
}
