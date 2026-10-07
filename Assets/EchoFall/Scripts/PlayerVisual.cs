using UnityEngine;

namespace EchoFall.Movement
{
    public enum WandererPose { Idle, Run, Rise, Fall, Land, WallSlide, WallJump, Dash, Attack, Guard, Deflect, Hurt, Rest, Death, Transfer }

    // Poses observe gameplay. They never gate an action or move the collider.
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(SpriteRenderer))]
    public sealed class PlayerVisual : MonoBehaviour
    {
        public PlayerMotor motor;
        public Sprite[] frames;
        public WandererPose Pose { get; private set; }
        public int FrameIndex { get; private set; }
        public float PoseHoldRemaining => hold;
        public int StrongImpactCount { get; private set; }
        public int LiveGhosts { get { int count=0; foreach(var ghost in ghosts) if(ghost!=null && ghost.enabled)count++; return count; } }
        SpriteRenderer spriteRenderer, rim;
        readonly SpriteRenderer[] ghosts = new SpriteRenderer[5];
        readonly float[] ghostLife = new float[5];
        SliceCombat combat;
        PlayerCameraImpulse cameraImpulse;
        Vector3 baseScale;
        Quaternion baseRotation;
        Color baseColor;
        float clock, landing, wallJump, hurt, deflect, attack, attackDuration, hold, transferTime, ghostClock, dustClock;
        float fallSpeed, attackAxis;
        int teleport, wallSequence, nextGhost, combo;
        bool grounded, dashing, heavy, transferring;

        void Awake()
        {
            spriteRenderer=GetComponent<SpriteRenderer>(); baseScale=transform.localScale;
            baseRotation=transform.localRotation; baseColor=spriteRenderer.color;
            combat=GetComponentInParent<SliceCombat>();
        }
        void Start()
        {
            if(motor==null)motor=GetComponentInParent<PlayerMotor>();
            var go=new GameObject("Wanderer silhouette rim"); go.transform.SetParent(transform,false);
            go.transform.localScale=Vector3.one*1.025f; rim=go.AddComponent<SpriteRenderer>();
            rim.sharedMaterial=spriteRenderer.sharedMaterial; rim.sortingOrder=spriteRenderer.sortingOrder-1;
            rim.color=new Color(.6f,.85f,.84f,.18f);
            for(int i=0;i<ghosts.Length;i++)
            {
                var echo=new GameObject("Wanderer dash echo "+i); ghosts[i]=echo.AddComponent<SpriteRenderer>();
                ghosts[i].sharedMaterial=spriteRenderer.sharedMaterial; ghosts[i].sortingOrder=spriteRenderer.sortingOrder-2;
                ghosts[i].enabled=false;
            }
            if(Camera.main!=null)
                cameraImpulse=Camera.main.GetComponent<PlayerCameraImpulse>() ?? Camera.main.gameObject.AddComponent<PlayerCameraImpulse>();
            if(motor!=null) { teleport=motor.TeleportSequence; wallSequence=motor.WallJumpSequence; grounded=motor.Grounded; }
        }
        public void Bind(SliceCombat value) => combat=value;
        public void PlayAttack(bool charged, float axis, int chain, float duration)
        { heavy=charged; attackAxis=axis; combo=chain; attackDuration=duration; attack=duration; hold=0; hurt=0; deflect=0; }
        public void PlayHurt(int damage)
        { hurt=.48f; attack=deflect=hold=0; if(cameraImpulse!=null)cameraImpulse.Kick(damage>=2?.055f:.035f); }
        public void PlayDeflect(bool red)
        { deflect=red?.23f:.16f; attack=hurt=0; Impact(red?.045f:.025f,red?.045f:.025f); }
        public void Impact(float duration=.04f,float impulse=.04f)
        { hold=Mathf.Max(hold,Mathf.Min(.055f,duration)); StrongImpactCount++; if(cameraImpulse!=null)cameraImpulse.Kick(impulse); }
        public void ResetTransient()
        {
            landing=wallJump=hurt=deflect=attack=hold=transferTime=0; transferring=false;
            for(int i=0;i<ghostLife.Length;i++) {ghostLife[i]=0;if(ghosts[i]!=null)ghosts[i].enabled=false;}
            if(cameraImpulse!=null)cameraImpulse.Clear();
        }
        void LateUpdate()
        {
            if(motor==null || frames==null || frames.Length<40)return;
            var session=SliceSession.Instance;
            bool live=session==null || session.Playing;
            bool transfer=session!=null && session.Screen==SliceScreen.Transfer;
            float dt=live?Mathf.Min(Time.deltaTime,.05f):0;
            float real=Mathf.Min(Time.unscaledDeltaTime,.05f);
            if(teleport!=motor.TeleportSequence)
            { ResetTransient();teleport=motor.TeleportSequence;grounded=motor.Grounded;fallSpeed=0; }
            if(live)
            {
                if(motor.WallJumpSequence!=wallSequence)
                { wallJump=.16f;landing=hold=0;SliceEffects.Dust(motor.Position+Vector2.up*.2f,new Vector2(-motor.Facing*.4f,.2f),4); }
                wallSequence=motor.WallJumpSequence;
                if(motor.Grounded && !grounded && fallSpeed < -1.6f)
                { landing=.16f;SliceEffects.Landing(motor.Position); }
                if(motor.Dashing && !dashing) {hold=0;landing=0;SliceEffects.Dust(motor.Position,Vector2.left*motor.Facing*.7f,5);}
                // Movement transitions interrupt pose-only hit-stop immediately.
                if(motor.Dashing || (!motor.Grounded && grounded) || Mathf.Sign(motor.Velocity.y)!=Mathf.Sign(fallSpeed))hold=0;
                float poseDt=hold>0?0:dt; hold=Mathf.Max(0,hold-dt);
                clock+=poseDt; attack=Mathf.Max(0,attack-poseDt);
                landing=Mathf.Max(0,landing-dt);wallJump=Mathf.Max(0,wallJump-dt);
                hurt=Mathf.Max(0,hurt-dt);deflect=Mathf.Max(0,deflect-poseDt);
                dustClock-=dt;
                if(dustClock<=0 && (motor.Grounded && Mathf.Abs(motor.Velocity.x)>1 || motor.Wall!=0 && motor.Velocity.y<-.5f))
                { SliceEffects.Dust(motor.Position,Vector2.left*motor.Facing*.3f,2);dustClock=.18f; }
                grounded=motor.Grounded;fallSpeed=motor.Velocity.y;dashing=motor.Dashing;
            }
            if(transfer)
            {
                if(!transferring) { transferTime=0;ResetGhosts();SliceEffects.Dust(motor.Position+Vector2.up*.25f,Vector2.up*.5f,9); }
                transferTime+=real;
            }
            transferring=transfer;
            WandererPose next=transfer?(combat!=null && combat.Integrity<=0?WandererPose.Death:WandererPose.Transfer):
                motor.Dashing?WandererPose.Dash:
                deflect>0?WandererPose.Deflect:
                combat!=null && (combat.Guarding || combat.ChargingGuard)?WandererPose.Guard:
                attack>0?WandererPose.Attack:
                hurt>.32f?WandererPose.Hurt:
                wallJump>0?WandererPose.WallJump:
                !motor.Grounded?(motor.Wall!=0 && motor.Velocity.y<0?WandererPose.WallSlide:motor.Velocity.y>0?WandererPose.Rise:WandererPose.Fall):
                session!=null && session.Resting?WandererPose.Rest:
                landing>0 && Mathf.Abs(motor.Velocity.x)<.25f?WandererPose.Land:
                Mathf.Abs(motor.Velocity.x)>.25f?WandererPose.Run:WandererPose.Idle;
            if(next!=Pose) {Pose=next;clock=0;}
            float progress=attackDuration>0?1-attack/attackDuration:0;
            switch(Pose)
            {
                case WandererPose.Run:FrameIndex=4+(int)(clock*36*Mathf.Clamp(Mathf.Abs(motor.Velocity.x)/2.65f,.25f,1.2f))%16;break;
                case WandererPose.Rise:case WandererPose.WallJump:FrameIndex=20;break;
                case WandererPose.Fall:case WandererPose.Dash:FrameIndex=21;break;
                case WandererPose.WallSlide:case WandererPose.Hurt:FrameIndex=31;break;
                case WandererPose.Guard:FrameIndex=30;break;
                case WandererPose.Deflect:FrameIndex=31;break;
                case WandererPose.Attack:FrameIndex=22+Mathf.Clamp((int)(progress*8),0,7);break;
                case WandererPose.Rest:FrameIndex=32+Mathf.Clamp((int)(clock/.45f*8),0,7);break;
                case WandererPose.Land:FrameIndex=21;break;
                case WandererPose.Death:FrameIndex=34+Mathf.Clamp((int)(transferTime*10),0,5);break;
                case WandererPose.Transfer:FrameIndex=30+(int)(transferTime*6)%2;break;
                default:FrameIndex=(int)(clock*4)%4;break;
            }
            spriteRenderer.sprite=frames[FrameIndex];spriteRenderer.flipX=motor.Facing<0;
            float squash=landing/.16f, lean=0, sx=1+squash*.10f, sy=1-squash*.10f, lift=0;
            if(Pose==WandererPose.Dash) {sx=1.08f;sy=.92f;lean=-9*motor.Facing;}
            else if(Pose==WandererPose.WallSlide)lean=5*motor.Wall;
            else if(Pose==WandererPose.WallJump) {sx=.95f;sy=1.05f;lean=-7*motor.Facing*wallJump/.16f;}
            else if(Pose==WandererPose.Attack)lean=-motor.Facing*Mathf.Sin(progress*Mathf.PI)*(heavy?9:combo==3?7:4)+(attackAxis>.4f?-4:attackAxis<-.4f?4:0);
            else if(Pose==WandererPose.Hurt)lean=8*motor.Facing;
            else if(Pose==WandererPose.Run)lean=-1.5f*motor.Facing;
            float alpha=1;
            if(transfer)
            {
                float t=Mathf.Clamp01(transferTime/.65f);
                if(Pose==WandererPose.Death) {sy=1-.28f*t;sx=1+.08f*t;lean=12*motor.Facing*t;alpha=1-.75f*t;}
                else {lift=t*.16f;sy=1+.08f*t;alpha=1-.65f*t;}
            }
            var tint=combat!=null && combat.MendProgress>0?new Color(.65f,1,.9f):baseColor;
            if(hurt>0) {tint=Color.Lerp(baseColor,new Color(1,.4f,.4f),Mathf.Clamp01(hurt/.48f));alpha*=.6f+.4f*Mathf.Abs(Mathf.Cos(hurt*45));}
            if(Pose==WandererPose.Deflect || Pose==WandererPose.Transfer)tint=new Color(.65f,1,1);
            tint.a=baseColor.a*alpha;spriteRenderer.color=tint;
            transform.position=motor.RenderPosition+Vector2.up*lift;
            transform.localScale=Vector3.Scale(baseScale,new Vector3(sx,sy,1));
            transform.localRotation=baseRotation*Quaternion.Euler(0,0,lean);
            if(rim!=null) {rim.sprite=spriteRenderer.sprite;rim.flipX=spriteRenderer.flipX;var color=rim.color;color.a=.18f*alpha;rim.color=color;}
            UpdateGhosts(live?dt:real,live && motor.Dashing);
        }
        void ResetGhosts() {for(int i=0;i<ghostLife.Length;i++)ghostLife[i]=0;}
        void UpdateGhosts(float dt,bool dash)
        {
            ghostClock-=dt;
            if(dash && ghostClock<=0 && ghosts[0]!=null)
            {
                int slot=nextGhost++%ghosts.Length;var ghost=ghosts[slot];
                ghost.transform.SetPositionAndRotation(transform.position,transform.rotation);ghost.transform.localScale=transform.lossyScale;
                ghost.sprite=spriteRenderer.sprite;ghost.flipX=spriteRenderer.flipX;ghostLife[slot]=.16f;ghostClock=.035f;
            }
            for(int i=0;i<ghosts.Length;i++)if(ghosts[i]!=null)
            {ghostLife[i]-=dt;ghosts[i].enabled=ghostLife[i]>0;ghosts[i].color=new Color(.46f,.85f,.85f,Mathf.Clamp01(ghostLife[i]/.16f)*.22f);}
        }
        void OnDisable()
        {
            if(spriteRenderer==null)return;
            transform.localScale=baseScale;transform.localRotation=baseRotation;spriteRenderer.color=baseColor;ResetTransient();
        }
        void OnDestroy() {foreach(var ghost in ghosts)if(ghost!=null)Destroy(ghost.gameObject);if(rim!=null)Destroy(rim.gameObject);}
    }
}
