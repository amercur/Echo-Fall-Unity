using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace EchoFall.Movement
{
    public sealed class SliceHUD : MonoBehaviour
    {
        public SliceSession session;
        Text title, subtitle, health, status, prompt, message, modalTitle, modalBody, loop, map;
        RectTransform root;
        RectTransform safeFrame;
        Image transition;
        GameObject modal, shade;
        readonly Button[] buttons = new Button[3];
        Font font;
        readonly Color ink = new Color(.028f,.046f,.068f,.94f), cyan = new Color(.48f,.88f,.86f), gold = new Color(.85f,.74f,.51f);
        void Awake()
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600,900); scaler.matchWidthOrHeight = .5f;
            gameObject.AddComponent<GraphicRaycaster>(); root = GetComponent<RectTransform>();
            var safe=new GameObject("Camera safe frame",typeof(RectTransform)); safe.transform.SetParent(root,false);
            safeFrame=safe.GetComponent<RectTransform>(); safeFrame.anchorMin=Vector2.zero; safeFrame.anchorMax=Vector2.one; safeFrame.offsetMin=safeFrame.offsetMax=Vector2.zero; root=safeFrame;
            if (FindAnyObjectByType<EventSystem>() == null) new GameObject("Slice UI input", typeof(EventSystem), typeof(InputSystemUIInputModule));
            Panel("Title backing", root, new Vector2(0,1), new Vector2(0,1), new Vector2(24,-24), new Vector2(680,96), new Color(.03f,.05f,.075f,.72f));
            title = Label(root,"THE WAKE",28,Color.white, new Vector2(0,1),new Vector2(48,-40),new Vector2(670,38));
            subtitle = Label(root,"E C H O   /   F A L L",13,gold,new Vector2(0,1),new Vector2(49,-82),new Vector2(660,25));
            loop = Label(root,"",16,cyan,new Vector2(1,1),new Vector2(-360,-40),new Vector2(310,30)); loop.alignment = TextAnchor.MiddleRight;
            map = Label(root,"",13,new Color(.58f,.67f,.73f),new Vector2(1,1),new Vector2(-390,-78),new Vector2(340,100)); map.alignment = TextAnchor.UpperRight;
            Panel("Vitals backing",root,Vector2.zero,Vector2.zero,new Vector2(24,24),new Vector2(690,92),ink);
            health = Label(root,"",23,cyan,Vector2.zero,new Vector2(46,95),new Vector2(650,36));
            status = Label(root,"",13,new Color(.65f,.73f,.8f),Vector2.zero,new Vector2(46,56),new Vector2(650,25));
            prompt = Label(root,"",18,gold,new Vector2(.5f,0),new Vector2(-480,175),new Vector2(960,38)); prompt.alignment = TextAnchor.MiddleCenter;
            message = Label(root,"",15,new Color(.68f,.78f,.8f),new Vector2(0,1),new Vector2(49,-121),new Vector2(760,58)); message.alignment = TextAnchor.UpperLeft;
            var hint = Label(root,"E  INTERACT    ESC  CONTROLS\nR  TRANSFER",13,new Color(.56f,.67f,.72f),new Vector2(1,0),new Vector2(-340,84),new Vector2(290,55)); hint.alignment = TextAnchor.MiddleRight;
            shade = Panel("Modal shade",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color(.015f,.025f,.04f,.83f)).gameObject;
            modal = Panel("Memory panel",root,new Vector2(.5f,.5f),new Vector2(.5f,.5f),new Vector2(-450,-265),new Vector2(900,530),ink).gameObject;
            var panel = modal.GetComponent<RectTransform>();
            Panel("Gold rule",panel,new Vector2(0,1),new Vector2(0,1),new Vector2(40,-36),new Vector2(820,2),gold);
            modalTitle = Label(panel,"",30,Color.white,new Vector2(0,1),new Vector2(44,-62),new Vector2(810,52));
            modalBody = Label(panel,"",19,new Color(.7f,.79f,.82f),new Vector2(0,1),new Vector2(44,-124),new Vector2(810,190));
            for(int i=0;i<3;i++)
            {
                var rect = Panel("Choice " + i,panel,new Vector2(0,1),new Vector2(0,1),new Vector2(44,-350-i*53),new Vector2(812,44),new Color(.09f,.15f,.18f));
                buttons[i] = rect.gameObject.AddComponent<Button>(); int index=i; buttons[i].onClick.AddListener(()=>session.ChooseOption(index));
                var text = Label(rect,"",17,cyan,new Vector2(0,1),new Vector2(16,-6),new Vector2(780,32)); text.alignment=TextAnchor.MiddleLeft;
            }
            transition=Panel("Room transition",root,Vector2.zero,Vector2.one,Vector2.zero,Vector2.zero,new Color(.012f,.022f,.032f,1)).GetComponent<Image>();
        }
        public System.Collections.IEnumerator FadeRoom(float opacity,float duration)
        {
            if(transition==null)yield break;
            float start=transition.color.a,elapsed=0;
            while(elapsed<duration)
            {
                elapsed+=Time.unscaledDeltaTime;
                var color=transition.color; color.a=Mathf.Lerp(start,opacity,Mathf.SmoothStep(0,1,elapsed/duration)); transition.color=color;
                yield return null;
            }
            var final=transition.color; final.a=opacity; transition.color=final;
        }
        RectTransform Panel(string name, Transform parent, Vector2 min, Vector2 max, Vector2 position, Vector2 size, Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=min; rect.anchorMax=max; rect.pivot=Vector2.zero; rect.anchoredPosition=position; rect.sizeDelta=size;
            go.GetComponent<Image>().color=color; go.GetComponent<Image>().raycastTarget = name == "Modal shade" || name.StartsWith("Choice"); return rect;
        }
        Text Label(Transform parent,string value,int size,Color color,Vector2 anchor,Vector2 position,Vector2 dimensions)
        {
            var go=new GameObject("Text",typeof(RectTransform),typeof(Text)); go.transform.SetParent(parent,false);
            var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=anchor; rect.pivot=new Vector2(0,1); rect.anchoredPosition=position; rect.sizeDelta=dimensions;
            var t=go.GetComponent<Text>(); t.font=font; t.fontSize=size; t.color=color; t.text=value; t.raycastTarget=false; t.supportRichText=false; return t;
        }
        void Update()
        {
            if(session==null)return;
            if(Camera.main!=null && safeFrame!=null) { var viewport=Camera.main.rect; safeFrame.anchorMin=viewport.min; safeFrame.anchorMax=viewport.max; }
            if(session.Room!=null) { title.text=session.Room.title; subtitle.text=session.Room.subtitle; }
            loop.text="LIFE " + session.Archive.loop.ToString("00") + "   /   " + (session.Archive.active=="fire"?"EMBER":session.Archive.active.ToUpperInvariant());
            health.text="INTEGRITY   " + new string('◆',session.combat.Integrity) + new string('◇',6-session.combat.Integrity) + "    ·    RESONANCE  " + session.combat.Resonance;
            status.text="J  STRIKE   F  DEFLECT   Q  IMPRINT   H  MEND   C  MEMORY" + (session.combat.Fracture>0?"   FRACTURE "+session.combat.Fracture:"");
            map.text="THE FIRST RETURN\n"+session.Visited.Count+" / 5 PLACES WITNESSED";
            prompt.text=session.Playing && session.Nearest!=null ? "[ E / Y ]   "+session.Nearest.label : "";
            message.text=string.IsNullOrEmpty(session.CurrentMessage)?session.Objective:session.CurrentMessage;
            bool show=!session.Playing && session.Screen!=SliceScreen.Loading;
            shade.SetActive(show); modal.SetActive(show);
            if(show)
            {
                modalTitle.text=session.ModalTitle; modalBody.text=session.ModalBody;
                for(int i=0;i<3;i++) { buttons[i].gameObject.SetActive(i<session.OptionLabels.Count); if(i<session.OptionLabels.Count) buttons[i].GetComponentInChildren<Text>().text=(i+1)+"   "+session.OptionLabels[i]; }
            }
        }
    }
}
