using UnityEngine;
namespace EchoFall.Movement
{
    public sealed class SliceProjectile : MonoBehaviour
    {
        public Vector2 Velocity {get;private set;}
        public bool Friendly {get;private set;}
        public bool Reflected {get;private set;}
        public float Damage {get;private set;}
        SliceEnemy source;float life=4;LineRenderer line;
        public static SliceProjectile Spawn(Vector2 position,Vector2 velocity,SliceEnemy source,float damage)
        {
            var go=new GameObject(source==null?"Ember bolt":"White bolt");go.transform.position=position;
            var p=go.AddComponent<SliceProjectile>();p.Velocity=velocity;p.source=source;p.Damage=damage;p.Friendly=source==null;p.life=source==null?1.1f:4;
            p.line=SliceEffects.Line(go,source==null?new Color(1,.5f,.2f):Color.white,.04f);p.line.useWorldSpace=false;p.line.positionCount=2;
            p.line.SetPosition(0,Vector3.zero);p.line.SetPosition(1,-(Vector3)velocity.normalized*.18f);return p;
        }
        public static void ClearAll(){foreach(var p in FindObjectsByType<SliceProjectile>())p.Expire();}
        void Expire(){gameObject.SetActive(false);Destroy(gameObject);}
        void FixedUpdate(){Tick(Time.fixedDeltaTime);}
        public void Tick(float dt)
        {
            var s=SliceSession.Instance;if(s==null||!s.Playing||!gameObject.activeSelf)return;
            life-=dt;if(life<=0){Expire();return;}
            Vector2 old=transform.position,next=old+Velocity*dt;
            var wall=Physics2D.Linecast(old,next,s.motor.solids);if(wall.collider!=null)next=wall.point;
            transform.position=next;
            if(!Friendly&&CombatRules.Segment(s.combat.HitRect,old,next,.07f))
            {
                if(s.combat.ReceiveHit(1,old,false,source)==CombatHitResult.Deflected)
                {
                    Friendly=Reflected=true;Damage=4;life=2;
                    Vector2 target=source!=null&&source.Alive?source.HitRect.center:s.motor.Position+Vector2.right*s.motor.Facing*3+Vector2.up*.2f;
                    // Reflect from contact, not from the far end of a long simulation tick.
                    next=s.combat.HitRect.center;transform.position=next;
                    Velocity=(target-next).normalized*5.4f;line.startColor=line.endColor=new Color(1,.8f,.4f);line.SetPosition(1,-(Vector3)Velocity.normalized*.18f);return;
                }
                Expire();return;
            }
            if(Friendly)
            {
                SliceEnemy nearest=null;float nearestDistance=float.MaxValue;
                foreach(var e in FindObjectsByType<SliceEnemy>())
                    if(e.Alive&&CombatRules.Segment(e.HitRect,old,next,.07f))
                    {float distance=Vector2.Dot(e.HitRect.center-old,Velocity.normalized);if(distance<nearestDistance){nearest=e;nearestDistance=distance;}}
                if(nearest!=null){if(nearest.Hit(Damage,.12f,(int)Mathf.Sign(Velocity.x),Reflected))s.combat.GainResonance(.2f);Expire();return;}
            }
            if(wall.collider!=null)Expire();
        }
    }
}
