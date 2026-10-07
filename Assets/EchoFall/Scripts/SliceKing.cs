using UnityEngine;
namespace EchoFall.Movement
{
    public sealed class SliceKing : SliceEnemy
    {
        public Vector2 arena=new Vector2(2.65f,11.2f);
        public bool Started {get;private set;}
        public int Stage {get;private set;}=1;
        public int Breaks {get;private set;}
        public string LastStyle {get;private set;}="";
        public float Exposed {get;private set;}
        public int Pattern {get;private set;}
        public override bool Red => Pattern==0;
        protected override float TellReach => Red?2.07f:1.96f;
        public override Rect HitRect => new Rect((Vector2)transform.position+new Vector2(-.27f,0),new Vector2(.54f,.92f));
        int attack;
        public static SliceKing Active => FindAnyObjectByType<SliceKing>();
        protected override void Start(){base.Start();MaxHealth=40;left=arena.x+.57f;right=arena.y-.53f;State="idle";timer=1.1f;}
        public void RestorePrediction(int stage,int breaks,string style)
        {Stage=Mathf.Max(1,stage);Breaks=breaks;LastStyle=style??"";}
        public override void Tick(float dt)
        {
            if(!Alive || Session==null || !Session.Playing)return;
            if(!Started)
            {
                if(Session.motor.Position.x<arena.x+.3f)return;
                Started=true;timer=1.1f;Session.motor.roomBounds=new Rect(arena.x,Session.Room.bounds.yMin,arena.y-arena.x,Session.Room.bounds.height);
                Session.Notify("THE KING / Deflect his rhythm. Jump, charge, or dash to break prediction.");SliceSound.Cue("boss-intro");
            }
            clock+=dt;hitFlash=Mathf.Max(0,hitFlash-dt);
            if(impactHold>0){impactHold-=dt;return;}
            if(Stage==1 && (hp<=MaxHealth*.5f || Breaks>=3))
            {Stage=2;Enter("transition",1.4f);Exposed=1.4f;SliceProjectile.ClearAll();Session.Notify("THE KING / PREDICTION BROKEN. A new rhythm begins.");SliceSound.Cue("boss-phase");}
            timer-=dt;Exposed=Mathf.Max(0,Exposed-dt);
            if(State=="transition"){if(timer<=0)Enter("idle",.6f);return;}
            if(State=="idle" && timer<=0)
            {attack++;Pattern=Stage==2?new[]{2,0,1}[attack%3]:attack%3;facing=Session.motor.Position.x<transform.position.x?-1:1;chain=0;Enter("tell",Pattern==0?.85f:Stage==2?.5f:.65f);}
            else if(State=="tell" && timer<=0)
            {Enter(Red?"sweep":"charge",Red?.26f:.35f);SliceEffects.Slash(HitRect.center,Vector2.right*facing,Red?new Color(1,.3f,.35f):Color.white);}
            else if(State=="charge" || State=="sweep")
            {
                if(State=="charge")Move(facing*4.6f*dt);
                var box=HitRect;box.xMin-=Red?1.8f:.35f;box.xMax+=Red?1.8f:.35f;
                if(Red){box.yMin=transform.position.y;box.yMax=transform.position.y+.42f;}else box.yMax-=.18f;
                if(box.Overlaps(Session.combat.HitRect))Session.combat.ReceiveHit(1,transform.position,Red,this);
                if(timer<=0)
                {if(Pattern==2 && chain<(Stage==2?2:1)){chain++;facing=Session.motor.Position.x<transform.position.x?-1:1;Enter("tell",Stage==2?.38f:.30f);}
                 else {Enter("idle",1.1f);Exposed=.9f;}}
            }
        }
        public override void Deflected(bool charged)
        {strain=Mathf.Min(15,strain+(charged?5:3));hitFlash=.2f;Exposed=charged?1.8f:.65f;impactHold=.05f;}
        public override bool Hit(float amount,float staggerTime=.12f,int knock=0,bool bypass=false)
        {
            if(!Alive || !Started || Session==null || amount<=0)return false;
            if(!bypass && Session.motor.Grounded && Session.combat.DashFollowup<=0 && Exposed<=0)
            {SliceEffects.Ring(HitRect.center,new Color(1,.8f,.4f),.5f);SliceSound.Cue("armor");return false;}
            string style=!Session.motor.Grounded?"air":Session.combat.DashFollowup>0?"dash":bypass?"charge":"ground";
            if(style!=LastStyle && style!="ground")Breaks++;LastStyle=style;
            hp=Mathf.Max(0,hp-amount);hitFlash=.12f;impactHold=.03f;
            SliceEffects.Dust(HitRect.center,Vector2.up*.2f,6);SliceEffects.Ring(HitRect.center,Color.cyan,.4f);SliceSound.Cue(amount>=5?"heavy-hit":"hit");
            Session.motor.GetComponentInChildren<PlayerVisual>()?.Impact(.03f,.03f);
            if(hp<=0)Die();return true;
        }
        protected override void Die()
        {
            Session.Flags.Add("king");Session.motor.roomBounds=Session.Room.bounds;
            Session.combat.Heal(2);Session.combat.ClearMark();SliceProjectile.ClearAll();
            Session.Notify("A FUTURE UNMADE / The King kneels. The threshold opens. Return to the Wake, or carry this victory through the Court glass.");
            base.Die();
        }
    }
}
