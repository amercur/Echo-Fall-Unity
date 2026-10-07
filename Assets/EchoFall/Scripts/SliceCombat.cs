using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoFall.Movement
{
    public enum CombatHitResult { Immune, Deflected, Blocked, Damaged }
    public sealed class SliceCombat : MonoBehaviour
    {
        public PlayerMotor motor;
        public int Integrity {get;private set;}=6;
        public float Resonance {get;private set;}
        public float Fracture {get;private set;}
        public float AttackVisual {get;private set;}
        public bool Guarding=>guardTime>0;
        public bool ChargingGuard=>holdingGuard;
        public float MendProgress=>mendTime;
        public int Combo {get;private set;}
        public float ImprintTime=>markTime;
        public SliceEnemy Marked=>marked;
        public float DashFollowup {get;private set;}
        public float RecoveryRemaining=>Mathf.Max(0,recovery);
        public Rect HitRect=>new Rect(motor.Position+new Vector2(-.11f,0),new Vector2(.22f,.4f));
        public bool Busy=>recovery>0 || Guarding || holdingAttack || holdingGuard || mend.IsPressed() || ability.IsPressed() || imprint.IsPressed();
        InputAction attack,guard,imprint,mend,ability,aim;
        float recovery,comboWindow,attackHold,guardHold,guardTime,perfectTime,counterTime,guardCooldown,invulnerable,buffer,markTime,mendTime,abilityCooldown,deflectBuffer,returnMendCooldown;
        bool holdingAttack,holdingGuard,attackPressed,attackReleased,guardPressed,guardReleased,imprintPressed,abilityPressed,previousDash;
        SliceEnemy marked;
        int charge,lastReset;
        PlayerVisual presentation;
        SliceSession Session=>SliceSession.Instance;
        void Awake()
        {
            motor=GetComponent<PlayerMotor>();presentation=GetComponentInChildren<PlayerVisual>();
            attack=Button("Attack","<Keyboard>/j","<Gamepad>/buttonWest");attack.AddBinding("<Keyboard>/x");
            guard=Button("Guard","<Keyboard>/f","<Gamepad>/leftShoulder");imprint=Button("Imprint","<Keyboard>/q","<Gamepad>/rightShoulder");
            mend=Button("Mend","<Keyboard>/h","<Gamepad>/rightTrigger");ability=Button("Memory","<Keyboard>/c","<Gamepad>/leftTrigger");ability.AddBinding("<Keyboard>/l");
            aim=new InputAction("Aim",InputActionType.Value);aim.AddCompositeBinding("1DAxis").With("Negative","<Keyboard>/s").With("Positive","<Keyboard>/w");aim.AddBinding("<Gamepad>/leftStick/y");aim.Enable();
            attack.performed+=_=>attackPressed=true;attack.canceled+=_=>attackReleased=true;
            guard.performed+=_=>guardPressed=true;guard.canceled+=_=>guardReleased=true;
            imprint.performed+=_=>imprintPressed=true;ability.performed+=_=>abilityPressed=true;
        }
        static InputAction Button(string name,string key,string pad){var a=new InputAction(name,InputActionType.Button);a.AddBinding(key);a.AddBinding(pad);a.Enable();return a;}
        void OnDestroy(){attack?.Dispose();guard?.Dispose();imprint?.Dispose();mend?.Dispose();ability?.Dispose();aim?.Dispose();}
        void Update()
        {
            bool a=attackPressed,ae=attackReleased,g=guardPressed,ge=guardReleased,q=imprintPressed,c=abilityPressed;
            attackPressed=attackReleased=guardPressed=guardReleased=imprintPressed=abilityPressed=false;
            if(Session==null || !Session.Playing){holdingAttack=holdingGuard=false;return;}
            if(a||g||q||c||mend.IsPressed())Session.CancelRest();
            float dt=Time.deltaTime;
            recovery-=dt;comboWindow-=dt;guardTime-=dt;perfectTime-=dt;counterTime-=dt;guardCooldown-=dt;invulnerable-=dt;abilityCooldown-=dt;
            DashFollowup=Mathf.Max(0,DashFollowup-dt);returnMendCooldown=Mathf.Max(0,returnMendCooldown-dt);deflectBuffer=Mathf.Max(0,deflectBuffer-dt);
            AttackVisual=Mathf.Max(0,AttackVisual-dt);
            if(motor.ResetCount!=lastReset){lastReset=motor.ResetCount;Hurt(1,motor.Position+Vector2.right,false,null,true);}
            if(motor.Dashing&&!previousDash){recovery=Mathf.Min(recovery,.06f);DashFollowup=.5f;attackHold=guardHold=mendTime=0;}
            previousDash=motor.Dashing;
            if(a){holdingAttack=true;attackHold=0;Strike(false,aim.ReadValue<float>());}
            if(holdingAttack)attackHold+=dt;
            if(ae){if(holdingAttack&&attackHold>=.55f)Strike(true,aim.ReadValue<float>());holdingAttack=false;}
            if(buffer>0&&recovery<=0){buffer=0;Strike(false,aim.ReadValue<float>());}buffer-=dt;
            if(g){holdingGuard=true;guardHold=0;deflectBuffer=.12f;}
            if(deflectBuffer>0&&guardCooldown<=0){BeginGuard();deflectBuffer=0;}
            if(holdingGuard)guardHold+=dt;
            if(ge){if(holdingGuard)ReleaseCounter(guardHold);holdingGuard=false;}
            if(q)Imprint();
            if(marked!=null&&!marked.Alive)ClearMark();
            if(marked!=null){markTime+=dt;if(markTime>=2)Detonate(true);}
            if(mend.IsPressed()&&motor.Grounded&&Mathf.Abs(motor.Velocity.x)<.2f&&Resonance>=1&&Integrity<6&&recovery<=0&&!holdingGuard)
            {mendTime+=dt;if(mendTime>=1){Resonance--;Heal(1);Fracture=0;mendTime=0;SliceSound.Cue("mend");}}
            else mendTime=0;
            if(c)MemoryAbility();
        }
        public void BeginGuard()
        {
            if(guardCooldown>0)return;
            guardTime=.30f;perfectTime=Session!=null&&Session.Archive.active=="return"?.21f:.145f;guardCooldown=.32f;mendTime=0;
            SliceEffects.Ring(motor.Position+Vector2.up*.22f,Color.cyan,.3f);
        }
        public bool ReleaseCounter(float held)
        {
            if(held<.48f)return false;
            counterTime=perfectTime=guardTime=.21f;guardCooldown=.36f;guardHold=0;
            SliceEffects.Ring(motor.Position+Vector2.up*.25f,new Color(1,.5f,.35f),.5f);SliceSound.Cue("counter");return true;
        }
        public void Strike(bool charged,float direction)
        {
            if(Session==null||!Session.Playing)return;
            Session.CancelRest();
            if(!charged&&recovery>0){buffer=Mathf.Min(.42f,recovery+.085f);return;}
            Combo=charged?3:comboWindow>0?Combo%3+1:1;comboWindow=.72f;recovery=CombatRules.Recovery(Combo,charged);AttackVisual=recovery;buffer=mendTime=0;
            presentation?.PlayAttack(charged,direction,Combo,recovery);SliceSound.Cue(charged?"charged":"slash");
            float damage=CombatRules.Damage(Combo,charged)+(Session.Decision=="fire"?1:0);
            bool down=direction<-.4f&&!motor.Grounded;
            Vector2 dir=down?Vector2.down:direction>.4f?Vector2.up:Vector2.right*motor.Facing;
            if(!charged&&!down&&Session.Archive.active=="fire")
            {SliceProjectile.Spawn(motor.Position+Vector2.up*.25f,dir*5.9f,null,2.7f+(Session.Decision=="fire"?1:0));return;}
            Rect area=CombatRules.Blade(motor.Position,motor.Facing,Combo,charged,direction,motor.Grounded);
            SliceEffects.Slash(motor.Position+Vector2.up*.25f,dir,charged?new Color(1,.75f,.42f):new Color(.6f,1,1));
            bool hit=false;
            foreach(var enemy in FindObjectsByType<SliceEnemy>())if(enemy.Alive&&area.Overlaps(enemy.HitRect))
            {bool connected=enemy.Hit(damage,charged||Combo==3?.55f:.12f,dir.y==0?motor.Facing:0,charged);if(connected&&charged)enemy.strain=Mathf.Min(15,enemy.strain+2);hit|=connected;}
            if(hit&&(charged||Combo==3))presentation?.Impact();
            if(down)
            {
                foreach(var point in FindObjectsByType<SlicePogo>())if(area.Contains(point.transform.position))hit=true;
                foreach(var hazard in Physics2D.OverlapAreaAll(area.min,area.max,motor.hazards))if(hazard!=null)hit=true;
                if(hit){motor.Pogo();invulnerable=Mathf.Max(invulnerable,.12f);SliceSound.Cue("pogo");}
            }
            if(hit)GainResonance(.25f);
        }
        public bool Hurt(int amount,Vector2 source,bool red,SliceEnemy attacker,bool bypass=false)=>ReceiveHit(amount,source,red,attacker,bypass)==CombatHitResult.Deflected;
        public CombatHitResult ReceiveHit(int amount,Vector2 source,bool red,SliceEnemy attacker,bool bypass=false)
        {
            if(Session==null||!Session.Playing||Integrity<=0)return CombatHitResult.Immune;
            if(!bypass&&(invulnerable>0||(!red&&motor.Dashing)))return CombatHitResult.Immune;
            Session.CancelRest();
            bool faces=!motor.Grounded||(source.x-motor.Position.x)*motor.Facing>=0;
            if(!bypass&&faces&&((red&&counterTime>0)||(!red&&(perfectTime>0||counterTime>0))))
            {
                bool charged=counterTime>0;Fracture=0;GainResonance(charged?1.5f:1);motor.DeflectLift();
                if(Session.Archive.active=="return"&&returnMendCooldown<=0){Heal(1);returnMendCooldown=4;}
                guardTime=perfectTime=counterTime=0;invulnerable=.10f;guardCooldown=.055f;
                presentation?.PlayDeflect(charged);SliceEffects.Dust(motor.Position+Vector2.up*.3f,Vector2.up*.5f,6);
                if(attacker!=null)attacker.Deflected(charged);
                SliceEffects.Ring(motor.Position+Vector2.up*.2f,Color.white,.85f);SliceSound.Cue(charged?"counter":"deflect");
                Session.Notify(charged?"RESONANT COUNTER / strain fractures the attacker":"PERFECT DEFLECT / +1 Resonance");return CombatHitResult.Deflected;
            }
            if(!bypass&&!red&&faces&&guardTime>0)
            {Fracture=Mathf.Min(Integrity-1,Fracture+.5f);guardTime=perfectTime=0;invulnerable=.2f;mendTime=0;Session.Notify("LATE BLOCK / fracture debt");return CombatHitResult.Blocked;}
            int total=amount+Mathf.CeilToInt(Fracture);Fracture=0;
            Integrity=attacker!=null&&attacker.training?Mathf.Max(1,Integrity-total):Mathf.Max(0,Integrity-total);
            mendTime=attackHold=guardHold=0;invulnerable=1;motor.CombatKnockback(source.x);
            presentation?.PlayHurt(total);SliceEffects.Ring(motor.Position+Vector2.up*.2f,new Color(1,.35f,.4f),.6f);
            if(Integrity<=0)Session.EndRun(false);return CombatHitResult.Damaged;
        }
        public void PayIntegrity(int cost){Integrity=Mathf.Max(0,Integrity-cost);presentation?.PlayHurt(cost);if(Integrity==0)Session.EndRun(false);}
        public void Heal(int amount)=>Integrity=Mathf.Min(6,Integrity+amount);
        public void GainResonance(float amount)=>Resonance=Mathf.Clamp(Resonance+amount,0,3);
        public void Rest(){Integrity=6;Resonance=3;Fracture=0;ClearTransient();}
        public void RestoreBench(float resonance){Integrity=6;Resonance=Mathf.Clamp(resonance,1,3);Fracture=0;ClearTransient();}
        public void DiscardInput(){attackPressed=attackReleased=guardPressed=guardReleased=imprintPressed=abilityPressed=holdingAttack=holdingGuard=false;}
        public void ClearMark(){marked=null;markTime=0;charge=0;}
        public void ClearTransient()
        {
            presentation?.ResetTransient();recovery=buffer=guardTime=perfectTime=counterTime=mendTime=AttackVisual=attackHold=guardHold=markTime=0;
            DashFollowup=deflectBuffer=returnMendCooldown=guardCooldown=invulnerable=abilityCooldown=comboWindow=0;
            Combo=0;previousDash=false;lastReset=motor.ResetCount;ClearMark();DiscardInput();SliceProjectile.ClearAll();
        }
        SliceEnemy Closest(float radius)
        {
            SliceEnemy nearest=null;
            foreach(var e in FindObjectsByType<SliceEnemy>())
            {float d=Vector2.Distance(HitRect.center,e.HitRect.center);if(e.Alive&&(!(e is SliceKing king)||king.Started)&&d<radius){radius=d;nearest=e;}}
            return nearest;
        }
        public void Imprint()
        {
            if(marked!=null){Detonate(false);return;}
            var target=Closest(1.32f);if(target==null||Resonance<1){Session.Notify("Imprint needs Resonance and a nearby enemy.");return;}
            marked=target;charge=Mathf.FloorToInt(Resonance);Resonance-=charge;markTime=mendTime=0;invulnerable=Mathf.Max(invulnerable,.15f);
            SliceEffects.Ring(target.HitRect.center,Color.cyan,.6f);SliceSound.Cue("imprint");
        }
        void Detonate(bool mature)
        {
            var target=marked;int spent=charge;ClearMark();
            if(target!=null&&target.Alive){float damage=3+2*spent+target.strain+(mature?2:0);target.strain=0;target.Hit(damage,.8f,0,true);presentation?.Impact(.05f,.055f);SliceSound.Cue("detonate");}
        }
        public void MemoryAbility()
        {
            if(Session==null||!Session.Playing||abilityCooldown>0)return;
            if(Session.Archive.active=="return"){guardTime=.34f;perfectTime=.24f;abilityCooldown=.9f;return;}
            if(Session.Archive.active=="fire")
            {
                var area=new Rect(motor.Position+new Vector2(-1.66f,-.05f),new Vector2(3.1f,.5f));bool hit=false;
                foreach(var enemy in FindObjectsByType<SliceEnemy>())if(enemy.Alive&&area.Overlaps(enemy.HitRect))hit|=enemy.Hit(CombatRules.Damage(Combo,false)+2+(Session.Decision=="fire"?1:0),.12f,0,true);
                if(hit)GainResonance(.25f);SliceEffects.Ring(motor.Position+Vector2.up*.2f,new Color(1,.5f,.2f),1.55f);
            }
            else {var target=Closest(4.5f);if(target!=null){target.Hit(target is SliceKing?4:5,.12f,0,true);SliceEffects.Ring(target.HitRect.center,Color.cyan,.7f);}}
            abilityCooldown=4;SliceSound.Cue("memory");
        }
    }
}
