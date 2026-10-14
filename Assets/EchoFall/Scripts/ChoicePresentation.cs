using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace EchoFall.Movement
{
    // Pointer and controller share feedback without changing selection ownership.
    public sealed class ChoicePresentation : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        Image image;
        RectTransform rect;
        bool hover,focused;
        float amount;
        public bool Focused { get; set; }
        public Color Accent { get; set; }=new Color(.48f,.88f,.86f);
        void Awake(){image=GetComponent<Image>();rect=GetComponent<RectTransform>();}
        public void OnPointerEnter(PointerEventData data){hover=true;}
        public void OnPointerExit(PointerEventData data){hover=false;}
        void OnDisable(){hover=focused=false;amount=0;transform.localScale=Vector3.one;}
        void Update()
        {
            bool next=hover||Focused;
            if(next&&!focused)SliceSound.Cue("focus");focused=next;
            amount=Mathf.Lerp(amount,next?1:0,1-Mathf.Exp(-24*Time.unscaledDeltaTime));
            image.color=Color.Lerp(new Color(.09f,.15f,.18f),new Color(Accent.r*.3f,Accent.g*.3f,Accent.b*.3f),amount);
            rect.localScale=new Vector3(1+amount*.008f,1,1);
        }
    }
}
