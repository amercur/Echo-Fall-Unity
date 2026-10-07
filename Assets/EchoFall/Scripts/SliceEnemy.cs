using UnityEngine;

namespace EchoFall.Movement
{
    // Shared damage, reactions, presentation and bounded movement for every combatant.
    // King specializes its pattern/prediction policy while using the same hit/imprint pipeline.
    public class SliceEnemy : MonoBehaviour
    {
        public string id, kind;
        public bool training;
        public float hp=7,left,right;
        public int strain;
        public Sprite[] frames;
        public SpriteRenderer visual;
        public bool Alive => hp>0;
        public virtual Rect HitRect => new Rect((Vector2)transform.position+new Vector2(-.13f,0),new Vector2(.26f,.38f));
        public string State {get;protected set;}="patrol";
        public virtual bool Red => kind=="lancer";
        public bool Guarding {get;protected set;}
        public bool Missed {get;protected set;}
        public float RecoveryRemaining => State=="recover"?timer:0;
        public float MaxHealth {get;protected set;}
        protected float timer=.8f,clock,hitFlash,stagger,impactHold,homeY;
        protected int facing=-1,chain;
        protected bool connected;
        LineRenderer intent;
        protected virtual float TellReach => Red?1.12f:.41f;
        protected SliceSession Session => SliceSession.Instance;
        protected virtual void Start()
        {
            MaxHealth=SliceCheckpoint.EnemyMax(id);if(MaxHealth<=0)MaxHealth=hp;homeY=transform.position.y;
            var go=new GameObject("Combat intent");go.transform.SetParent(transform,false);
            intent=SliceEffects.Line(go,Color.white,.018f);intent.positionCount=2;
        }
        protected virtual void FixedUpdate(){if(Session!=null && Session.Playing)Tick(Time.fixedDeltaTime);}
        public virtual void Tick(float dt)
        {
            if(!Alive || Session==null || !Session.Playing)return;
            clock+=dt;hitFlash=Mathf.Max(0,hitFlash-dt);Guarding=false;
            if(impactHold>0){impactHold-=dt;return;}
            if(stagger>0){stagger=Mathf.Max(0,stagger-dt);return;}
            timer-=dt;Vector2 delta=Session.motor.Position-(Vector2)transform.position;
            bool close=Mathf.Abs(delta.x)<4 && Mathf.Abs(delta.y)<1.4f;
            if(kind=="drone")
            {
                var p=transform.position;p.y=homeY+Mathf.Sin(clock*2+left)*.18f;transform.position=p;
                Move(facing*.3f*dt);if(transform.position.x<=left || transform.position.x>=right)facing=-facing;
                if(State=="tell" && timer<=0)
                {SliceProjectile.Spawn((Vector2)transform.position+Vector2.up*.18f,(delta+Vector2.up*.2f).normalized*2.35f,this,1);Enter("recover",2.5f);}
                else if(State=="recover" && timer<=0)Enter("patrol",0);
                else if(State=="patrol" && timer<=0 && close){facing=delta.x<0?-1:1;Enter("tell",.7f);}
                return;
            }
            if(State=="patrol")
            {
                if(close)facing=delta.x<0?-1:1;
                if(kind=="sentinel" && !training && close)
                {
                    foreach(var ally in FindObjectsByType<SliceEnemy>())
                        if(ally!=this && ally.Alive && !ally.training && !(ally is SliceKing) && Mathf.Abs(ally.transform.position.x-transform.position.x)<1.3f && Mathf.Abs(ally.transform.position.y-transform.position.y)<.45f)
                        {Guarding=true;Move(Mathf.MoveTowards(transform.position.x,Mathf.Clamp(ally.transform.position.x+facing*.38f,left,right),dt*.85f)-transform.position.x);break;}
                    if(Guarding && Mathf.Abs(delta.x)>.64f)return;
                }
                if(Mathf.Abs(delta.x)<(Red?1.5f:.75f) && Mathf.Abs(delta.y)<.75f && timer<=0)
                {chain=0;Enter("tell",training?.9f:Red?.78f:.46f);}
                else {Move(facing*.55f*dt);if(transform.position.x<=left || transform.position.x>=right)facing=-facing;}
            }
            else if(State=="tell" && timer<=0)
            {connected=false;Enter("strike",Red?.26f:.17f);SliceEffects.Slash((Vector2)transform.position+Vector2.up*.2f,Vector2.right*facing,Red?new Color(1,.3f,.35f):Color.white);}
            else if(State=="strike")
            {
                Move(facing*(Red?4.3f:2.4f)*dt);
                Rect hit=HitRect;hit.xMin-=.12f;hit.xMax+=.12f;
                if(hit.Overlaps(Session.combat.HitRect))
                {var result=Session.combat.ReceiveHit(1,transform.position,Red,this);if(result!=CombatHitResult.Immune)connected=true;}
                // A deflect may already have replaced this state with recovery.
                if(State=="strike" && timer<=0)
                {if(kind=="sentinel" && !training && chain==0){chain++;facing=delta.x<0?-1:1;Enter("tell",.32f);}
                 else {Missed=Red&&!connected;Enter("recover",Red?(connected?.8f:1.5f):.7f);}}
            }
            else if(State=="recover" && timer<=0)Enter("patrol",.2f);
        }
        protected void Enter(string state,float duration){State=state;timer=duration;if(state=="tell")SliceSound.Cue(Red?"red-tell":"white-tell");}
        protected void Move(float dx)
        {
            if(dx==0)return;
            // Sweep the body horizontally; actors cannot lunge through room walls.
            if(Session!=null)
            {
                var hit=Physics2D.BoxCast(HitRect.center,HitRect.size*.9f,0,Vector2.right*Mathf.Sign(dx),Mathf.Abs(dx),Session.motor.solids);
                if(hit.collider!=null)dx=Mathf.Sign(dx)*Mathf.Max(0,hit.distance-.005f);
            }
            var p=transform.position;p.x=Mathf.Clamp(p.x+dx,left,right);transform.position=p;
        }
        protected virtual void LateUpdate()
        {
            if(intent!=null)
            {
                intent.enabled=Alive && (State=="tell" || Guarding);
                var origin=(Vector2)transform.position+Vector2.up*.035f;
                if(Guarding)
                {origin+=Vector2.right*facing*.28f;intent.SetPosition(0,origin);intent.SetPosition(1,origin+Vector2.up*.45f);}
                else if(this is SliceKing && Red)
                {intent.SetPosition(0,origin+Vector2.left*TellReach);intent.SetPosition(1,origin+Vector2.right*TellReach);}
                else
                {intent.SetPosition(0,origin);intent.SetPosition(1,origin+Vector2.right*facing*TellReach);}
                intent.startColor=intent.endColor=Guarding?new Color(.6f,.9f,1,.65f):Red?new Color(1,.35f,.4f,.65f):new Color(.85f,1,1,.65f);
            }
            if(visual==null)return;
            int frame=State=="tell"?4+(int)(clock*8)%2:State=="strike"||State=="sweep"||State=="charge"?6:State=="recover"||State=="transition"?7:(int)(clock*6)%4;
            if(frames!=null && frames.Length>=8)visual.sprite=frames[frame];visual.flipX=facing<0;
            visual.color=hitFlash>0?new Color(1,.65f,.65f):State=="tell"?(Red?new Color(1,.38f,.4f):new Color(1,1,.85f)):Guarding?new Color(.8f,.9f,1):Color.white;
        }
        public virtual void Deflected(bool charged)
        {
            strain=Mathf.Min(15,strain+(charged?5:3));hitFlash=.2f;impactHold=.05f;
            if(training){hp=0;Die();return;}
            stagger=0;Guarding=false;Enter("recover",charged?.65f:.25f);
        }
        public void Damage(float amount,bool heavy) => Hit(amount,heavy?.55f:.12f,Session!=null?Session.motor.Facing:0,heavy);
        public virtual bool Hit(float amount,float staggerTime=.12f,int knock=0,bool bypass=false)
        {
            if(!Alive || amount<=0)return false;
            if(staggerTime<.5f)
                foreach(var ally in FindObjectsByType<SliceEnemy>())
                    if(ally!=this && ally.Alive && ally.Guarding && Mathf.Abs(ally.transform.position.x-transform.position.x)<1 && Mathf.Abs(ally.transform.position.y-transform.position.y)<.45f)
                    {amount*=.3f;SliceEffects.Ring(ally.HitRect.center,new Color(1,.8f,.4f),.32f);SliceSound.Cue("armor");break;}
            hp=training?Mathf.Max(1,hp-amount):Mathf.Max(0,hp-amount);
            Move(knock*.1f);stagger=Mathf.Max(stagger,staggerTime);hitFlash=.16f;impactHold=Mathf.Max(impactHold,.025f);Guarding=false;
            SliceEffects.Dust(HitRect.center,Vector2.right*knock*.5f,5);SliceEffects.Ring(HitRect.center,new Color(.65f,1,1),.22f);SliceSound.Cue(staggerTime>=.5f?"heavy-hit":"hit");
            if(hp<=0)Die();return true;
        }
        protected virtual void Die()
        {
            if(Session!=null){Session.Defeated.Add(id);Session.EnemyHealth.Remove(id);if(training)Session.Notify("The warden bows. You have learned the white deflect.");}
            SliceEffects.Dust(HitRect.center,Vector2.up*.3f,12);SliceEffects.Ring(HitRect.center,Color.cyan,.45f);SliceSound.Cue("defeat");
            if(visual!=null)SliceDeathEcho.Spawn(visual);
            gameObject.SetActive(false);
        }
    }
}
