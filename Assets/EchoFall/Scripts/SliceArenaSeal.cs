using UnityEngine;
namespace EchoFall.Movement
{
    public sealed class SliceArenaSeal : MonoBehaviour
    {
        public SliceKing king;
        LineRenderer line;
        void Start(){line=SliceEffects.Line(gameObject,new Color(.9f,.65f,.3f,.6f),.025f);line.positionCount=2;line.SetPosition(0,transform.position);line.SetPosition(1,transform.position+Vector3.up*2.5f);}
        void LateUpdate(){if(line!=null)line.enabled=king!=null&&king.Started&&king.Alive;}
    }
}
