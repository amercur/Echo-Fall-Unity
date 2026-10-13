using UnityEngine;

namespace EchoFall.Movement
{
    // Additive room variants are recreated from the archive on every load, never saved as scene objects.
    public static class MemoryWorld
    {
        public static bool BreathingPassage(SliceArchive archive) => archive.Remembers("defiance") && !archive.Remembers("obedience");
        public static bool GuardBlocks(string requires, SliceArchive archive, System.Collections.Generic.ISet<string> flags) =>
            requires == "archive-lift" && archive.Remembers("betrayal") && !flags.Contains("betrayal-guard");
        public static void Apply(SliceRoom room, SliceSession session)
        {
            if (room.transform.Find("Remembered world") != null) return;
            var root = new GameObject("Remembered world"); root.transform.SetParent(room.transform, false);
            if(room.id=="cradle")
            {
                bool open=session.Archive.Remembers("sacrifice") && !session.Archive.Remembers("betrayal");
                for(int i=0;i<4;i++)
                {
                    var cage=new GameObject(open?"An empty cage":"A borrowed heartbeat");cage.transform.SetParent(root.transform,false);
                    cage.transform.position=new Vector3(2+i*2.5f,3.1f,0);
                    var outline=SliceEffects.Line(cage,open?new Color(.3f,.8f,.65f,.55f):new Color(.65f,.3f,.4f,.55f),.025f);
                    outline.useWorldSpace=false;outline.positionCount=open?4:5;
                    var points=new[]{new Vector3(-.2f,-.4f),new Vector3(-.2f,.4f),new Vector3(.2f,.4f),new Vector3(.2f,-.4f),new Vector3(-.2f,-.4f)};
                    for(int n=0;n<outline.positionCount;n++)outline.SetPosition(n,points[n]);
                }
            }
            if (room.id == "cistern" && session.Archive.Remembers("obedience")) Platform(root.transform, "The ordered bridge", new Vector2(7.25f,.22f),new Vector2(8.3f,.2f));
            if (room.id == "archive" && session.Archive.Remembers("defiance")) Platform(root.transform,"A step that was not there",new Vector2(6.65f,2.27f),new Vector2(.9f,.15f));
            if (room.id == "archive" && session.Archive.Remembers("betrayal") && !session.Defeated.Contains("archive/betrayal-guard"))
            {
                var go = new GameObject("A hand remembered");go.transform.SetParent(root.transform,false);go.transform.position=new Vector3(9.6f,.12f,0);
                var echo=go.AddComponent<MemoryEcho>();echo.id="archive/betrayal-guard";echo.kind="sentinel";echo.hp=12;echo.left=8.8f;echo.right=10.6f;echo.identity=session.Archive.lastEcho;
                var model=new GameObject("Previous self");model.transform.SetParent(go.transform,false);model.transform.localPosition=Vector3.up*.2f;
                var player=session.motor.GetComponentInChildren<PlayerVisual>();echo.visual=model.AddComponent<SpriteRenderer>();
                if(player!=null){echo.visual.sprite=player.GetComponent<SpriteRenderer>().sprite;model.transform.localScale=player.transform.localScale;echo.frames=player.frames;}
                echo.visual.sortingOrder=20;echo.visual.color=new Color(.55f,.86f,.92f,.65f);
            }
            foreach(var npc in room.GetComponentsInChildren<SliceInteraction>())
            {
                if(npc.id=="keeper" && session.Archive.Remembers("sacrifice") && !session.Archive.Remembers("betrayal")) npc.label="THE KEEPER / EMPTY CAGES";
                if(npc.id=="keeper" && session.Archive.Remembers("betrayal")) npc.label="THE KEEPER / A CLOSED HAND";
                if(npc.id=="echo" && session.Archive.Age("abandon")>=2 && !session.Archive.Remembers("respect"))
                {
                    npc.label="AN EMPTY PLACE";
                    foreach(var sprite in npc.GetComponentsInChildren<SpriteRenderer>())sprite.color=new Color(.5f,.7f,.8f,.12f);
                    foreach(var line in npc.GetComponentsInChildren<LineRenderer>())line.startColor=line.endColor=new Color(.5f,.7f,.8f,.12f);
                }
            }
        }
        static void Platform(Transform parent,string name,Vector2 top,Vector2 size)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.layer=LayerMask.NameToLayer("EchoPlatform");go.transform.position=top-Vector2.up*size.y*.5f;
            go.AddComponent<BoxCollider2D>().size=size;
            var line=SliceEffects.Line(go,new Color(.65f,.8f,.68f),.035f);line.positionCount=2;
            line.SetPosition(0,new Vector3(-size.x*.5f,size.y*.5f,0));line.SetPosition(1,new Vector3(size.x*.5f,size.y*.5f,0));line.useWorldSpace=false;
        }
    }
    // A remembered identity, not a recording or a following companion.
    public sealed class MemoryEcho : SliceEnemy
    {
        public bool recalled;
        public override bool Persistent=>!recalled;
        public string identity;
        public bool Recognized { get; private set; }
        public bool Refused { get; private set; }
        float recognition=1.3f,skill=2;
        public override void Tick(float dt)
        {
            if(!Alive || Session==null || !Session.Playing)return;
            if(!Recognized)
            {
                if(Vector2.Distance(Session.motor.Position,transform.position)>4)return;
                recognition-=dt;if(recognition>0)return;Recognized=true;
                if(recalled?identity=="respect":Session.Archive.Remembers("respect"))
                {Refused=true;if(!recalled){Session.Defeated.Add(id);Session.Flags.Add("betrayal-guard");Session.EnemyHealth.Remove(id);}Session.combat.GainResonance(1);Session.Notify("No. I remember being you.");gameObject.SetActive(false);if(recalled)Destroy(gameObject);return;}
                Session.Notify("I remember the hand. I do not remember why.");
            }
            skill-=dt;
            if(skill<=0)
            {
                if(identity=="fire"){SliceProjectile.Spawn(HitRect.center,(Session.combat.HitRect.center-HitRect.center).normalized*2.2f,this,1);skill=1.8f;}
                if(identity=="mercy" || identity=="sacrifice"){if(recalled)SliceMother.Active?.RestoreIntegrity(1.5f);else hp=Mathf.Min(MaxHealth,hp+1.5f);skill=4;}
                if(identity=="defiance" && Mathf.Abs(Session.motor.Position.x-transform.position.x)>.9f){Move(Mathf.Sign(Session.motor.Position.x-transform.position.x)*.85f);skill=2.5f;}
            }
            base.Tick(dt);
        }
        public override bool Hit(float amount,float staggerTime=.12f,int knock=0,bool bypass=false)
        {
            if(recalled && skill<=0)
            {
                if(identity=="return" && State=="patrol" && staggerTime<.5f){skill=2;SliceEffects.Ring(HitRect.center,Color.cyan,.35f);return false;}
                if(identity=="obedience"){amount*=.35f;skill=2.5f;SliceEffects.Ring(HitRect.center,Color.cyan,.35f);}
            }
            return base.Hit(amount,staggerTime,knock,bypass);
        }
        protected override void Die(){if(!recalled)Session?.Flags.Add("betrayal-guard");base.Die();if(recalled)Destroy(gameObject);}
        protected override void LateUpdate()
        {
            base.LateUpdate();
            if(visual==null)return;
            if(frames!=null && frames.Length>=32)
            {
                int pose=hitFlash>0?31:!Recognized?((int)(clock*4)%4):State=="tell"?30:State=="strike"?22+Mathf.Clamp((int)((.17f-timer)/.17f*8),0,7):State=="recover"?21:4+(int)(clock*24)%16;
                visual.sprite=frames[pose];
            }
            if(hitFlash<=0)visual.color*=new Color(.6f,.85f,1,.65f);
        }
    }
}
