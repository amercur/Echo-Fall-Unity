using System.Linq;
using UnityEngine;

namespace EchoFall.Movement
{
    public sealed class SliceMother : SliceEnemy
    {
        public static SliceMother Active => FindAnyObjectByType<SliceMother>();
        public Vector2 arena=new Vector2(3.5f,12);
        public bool Started {get;private set;}
        public override bool IsBoss=>true;
        public override bool EncounterStarted=>Started;
        public int Stage {get;private set;}=1;
        public int RecallIndex {get;private set;}
        public int Attack {get;private set;}
        public int AidStrikes {get;private set;}
        public float TargetX {get;private set;}
        public override bool Red=>Attack%3==2;
        public override Rect HitRect=>new Rect((Vector2)transform.position+new Vector2(-.32f,0),new Vector2(.64f,1.05f));
        float aid=2;
        bool heavy;
        LineRenderer falling,sealLeft,sealRight;
        protected override void Start()
        {
            base.Start();MaxHealth=Session.MotherMax>0?Session.MotherMax:Session.Did("sacrifice")?38:48;
            if(!Session.EnemyHealth.ContainsKey(id))hp=MaxHealth;else hp=Mathf.Min(hp,MaxHealth);
            Stage=Mathf.Max(1,Session.MotherStage);RecallIndex=Session.MotherRecall;Attack=Session.MotherAttack;
            State="idle";timer=1.2f;
            falling=MakeLine("A falling signal",.022f);
            sealLeft=MakeLine("Chamber seal west",.035f);sealRight=MakeLine("Chamber seal east",.035f);
        }
        LineRenderer MakeLine(string name,float width){var go=new GameObject(name);go.transform.SetParent(transform,false);var l=SliceEffects.Line(go,Color.white,width);l.positionCount=2;return l;}
        public override void Tick(float dt)
        {
            if(!Alive || Session==null || !Session.Playing)return;
            if(!Started)
            {
                var p=Session.motor.Position;
                if(p.x<arena.x+.3f || p.x>arena.y-.1f || p.y>1.6f)return;
                Started=true;Session.motor.roomBounds=new Rect(arena.x,Session.Room.bounds.yMin,arena.y-arena.x,Session.Room.bounds.height);
                Session.Notify(Session.Did("sacrifice")?"THE MOTHER / One cage stands open.":"THE MOTHER / Why do you insist on an ending?");SliceSound.Cue("boss-intro");
            }
            clock+=dt;hitFlash=Mathf.Max(0,hitFlash-dt);transform.position=new Vector3(transform.position.x,.02f+Mathf.Sin(clock*1.8f)*.09f,0);
            if(Session.Archive.MatureMercy){aid-=dt;if(aid<=0){aid=4;AidStrikes++;Hit(2.5f,.12f,0,true);if(!Alive)return;}}
            if(impactHold>0){impactHold-=dt;return;}
            if(Stage==1 && hp<=MaxHealth*.5f){Stage=2;Enter("transition",1.4f);SliceProjectile.ClearAll();Session.Notify("THE MOTHER / RECALL. Your archived selves answer.");SliceSound.Cue("boss-phase");}
            timer-=dt;
            if(State=="transition"){if(timer<=0)Enter("idle",.6f);return;}
            if(State=="idle" && timer<=0){TargetX=Session.motor.Position.x;heavy=Red;Enter("tell",.9f);}
            else if(State=="tell" && timer<=0)
            {
                if(heavy)foreach(int d in new[]{-1,1})SliceProjectile.Spawn(new Vector2(transform.position.x,.17f),Vector2.right*d*2.45f,this,1,true);
                else
                {
                    Vector2 direction=(Session.combat.HitRect.center-HitRect.center).normalized;
                    foreach(float angle in new[]{-.20f,0,.20f})
                    {var v=new Vector2(direction.x*Mathf.Cos(angle)-direction.y*Mathf.Sin(angle),direction.y*Mathf.Cos(angle)+direction.x*Mathf.Sin(angle));SliceProjectile.Spawn(HitRect.center,v*2.6f,this,1);}
                }
                SliceProjectile.Spawn(new Vector2(TargetX,2.92f),Vector2.down*3.9f,this,1,heavy);
                Attack++;if((Attack%2==0 || Stage==2) && FindObjectsByType<MemoryEcho>().Count(e=>e.recalled && e.Alive)<3)Recall();
                Enter("idle",Stage==2?1.8f:2.25f);
            }
        }
        public MemoryEcho Recall()
        {
            string identity=Session.Archive.memories.Count==0?"return":Session.Archive.memories[RecallIndex%Session.Archive.memories.Count];RecallIndex++;
            var go=new GameObject("Recalled / "+MemoryRules.Name(identity));go.transform.SetParent(Session.Room.transform,false);
            go.transform.position=new Vector2(Session.motor.Position.x<(arena.x+arena.y)*.5f?arena.y-1.1f:arena.x+.4f,.02f);
            var e=go.AddComponent<MemoryEcho>();e.id="recall/"+RecallIndex;e.kind="echo";e.recalled=true;e.identity=identity;e.hp=Session.Archive.Remembers("betrayal")?9:6;e.left=arena.x+.2f;e.right=arena.y-.4f;
            var model=new GameObject("Remembered Wanderer");model.transform.SetParent(go.transform,false);
            var player=Session.motor.GetComponentInChildren<PlayerVisual>();e.visual=model.AddComponent<SpriteRenderer>();e.frames=player.frames;model.transform.localScale=player.transform.localScale;e.visual.sprite=e.frames[0];e.visual.sortingOrder=20;
            SliceEffects.Ring(e.HitRect.center,new Color(.7f,.6f,1),.5f);Session.Notify("RECALLED / "+MemoryRules.Name(identity));return e;
        }
        public void RestoreIntegrity(float value){if(Alive)hp=Mathf.Min(MaxHealth,hp+value);}
        public override bool Hit(float amount,float staggerTime=.12f,int knock=0,bool bypass=false)
        {
            if(!Started || !Alive || amount<=0)return false;
            hp=Mathf.Max(0,hp-amount);hitFlash=.16f;impactHold=.03f;
            SliceEffects.Ring(HitRect.center,Color.cyan,.45f);SliceSound.Cue(amount>=5?"heavy-hit":"hit");Session.motor.GetComponentInChildren<PlayerVisual>()?.Impact(.03f,.03f);
            if(hp<=0)Die();return true;
        }
        public override void Deflected(bool charged){strain=Mathf.Min(15,strain+(charged?5:3));hitFlash=.2f;impactHold=.05f;}
        protected override void Die()
        {
            if(Session.Flags.Contains("mother"))return;
            Session.Flags.Add("mother");Session.Flags.Add("garden-path");Session.motor.roomBounds=Session.Room.bounds;
            Session.combat.Heal(2);Session.combat.ClearMark();SliceProjectile.ClearAll();
            foreach(var e in FindObjectsByType<MemoryEcho>())if(e.recalled)e.gameObject.SetActive(false);
            Session.Notify("A FUTURE UNMADE / The Mother opens her arms. For once, she lets the bodies go.");base.Die();
        }
        protected override void LateUpdate()
        {
            base.LateUpdate();if(falling==null)return;
            falling.enabled=Alive && Started && State=="tell";falling.startColor=falling.endColor=heavy?new Color(1,.3f,.45f,.6f):new Color(.7f,1,1,.6f);
            falling.SetPosition(0,new Vector3(TargetX,0));falling.SetPosition(1,new Vector3(TargetX,2.92f));
            foreach(var seal in new[]{sealLeft,sealRight}){seal.enabled=Alive && Started;float x=seal==sealLeft?arena.x:arena.y;seal.SetPosition(0,new Vector3(x,0));seal.SetPosition(1,new Vector3(x,1.8f));}
        }
    }
}
