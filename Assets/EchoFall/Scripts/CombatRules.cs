using UnityEngine;

namespace EchoFall.Movement
{
    // Source values in world units. No animation event participates in hit detection.
    public static class CombatRules
    {
        public static float Damage(int combo,bool charged) => charged?5.5f:combo==3?3.5f:2;
        public static float Recovery(int combo,bool charged) => charged?.46f:combo==3?.34f:.22f;
        public static Rect Blade(Vector2 feet,int facing,int combo,bool charged,float axis,bool grounded)
        {
            float reach=charged?.96f:combo==3?.8f:.65f;
            if(axis<-.4f && !grounded)return new Rect(feet+new Vector2(-.31f,.16f-reach),new Vector2(.62f,reach));
            if(axis>.4f)return new Rect(feet+new Vector2(-.32f,.25f),new Vector2(.64f,reach+.15f));
            return new Rect(feet+new Vector2(facing>0?-.05f:-.11f-reach,-.05f),new Vector2(reach,.5f));
        }
        public static bool Segment(Rect rect,Vector2 from,Vector2 to,float radius=0)
        {
            rect.xMin-=radius;rect.xMax+=radius;rect.yMin-=radius;rect.yMax+=radius;
            float lo=0,hi=1;Vector2 d=to-from;
            for(int axis=0;axis<2;axis++)
            {
                float min=axis==0?rect.xMin:rect.yMin,max=axis==0?rect.xMax:rect.yMax;
                if(Mathf.Abs(d[axis])<.000001f){if(from[axis]<min || from[axis]>max)return false;continue;}
                float a=(min-from[axis])/d[axis],b=(max-from[axis])/d[axis];
                lo=Mathf.Max(lo,Mathf.Min(a,b));hi=Mathf.Min(hi,Mathf.Max(a,b));if(lo>hi)return false;
            }
            return true;
        }
    }
}
