using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

namespace EchoFall.Movement
{
    public sealed class WorldMap : MonoBehaviour
    {
        SliceSession session;
        InputAction toggle;
        GameObject panel;
        Transform content;
        Font font;
        void Start()
        {
            session=GetComponent<SliceSession>();font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            toggle=new InputAction("World map",InputActionType.Button,"<Keyboard>/m");toggle.AddBinding("<Gamepad>/rightStickPress");toggle.Enable();
        }
        void OnDestroy(){toggle?.Dispose();if(panel!=null)Destroy(panel);}
        void Update()
        {
            if(toggle!=null && toggle.WasPressedThisFrame())
            {
                if(session.Screen==SliceScreen.Map)Close();
                else if(session.Playing)Open();
            }
            if(panel!=null && session.Screen!=SliceScreen.Map){Destroy(panel);panel=null;}
        }
        public void Close(){if(session.Screen==SliceScreen.Map)session.SetScreen(SliceScreen.Playing);}
        static Vector2 Position(WorldPlace room)=>new Vector2(110+room.x,690-room.y);
        RectTransform Rect(string name,Vector2 pos,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(content,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=Vector2.zero;rect.anchoredPosition=pos;rect.sizeDelta=size;
            go.GetComponent<Image>().color=color;go.GetComponent<Image>().raycastTarget=false;return rect;
        }
        void Label(string value,Vector2 pos,Vector2 size,int points,Color color)
        {
            var go=new GameObject(value,typeof(RectTransform),typeof(Text));go.transform.SetParent(content,false);
            var rect=go.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=Vector2.zero;rect.anchoredPosition=pos;rect.sizeDelta=size;
            var text=go.GetComponent<Text>();text.font=font;text.fontSize=points;text.color=color;text.text=value;text.alignment=TextAnchor.MiddleCenter;text.raycastTarget=false;
        }
        public void Open()
        {
            if(session==null)session=GetComponent<SliceSession>();
            if(!session.Playing)return;
            session.SetScreen(SliceScreen.Map);
            panel=new GameObject("Discovered world",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
            var canvas=panel.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=100;
            var scaler=panel.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1400,900);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
            var layout=new GameObject("Map safe frame",typeof(RectTransform));layout.transform.SetParent(panel.transform,false);
            var layoutRect=layout.GetComponent<RectTransform>();layoutRect.anchorMin=layoutRect.anchorMax=new Vector2(.5f,.5f);layoutRect.sizeDelta=new Vector2(1400,900);content=layout.transform;
            var backing=Rect("Map backdrop",new Vector2(700,450),new Vector2(4000,3000),new Color(.018f,.033f,.05f,.98f));
            var visible=WorldCatalog.Visible(session.Visited);var drawn=new HashSet<string>();
            foreach(var room in WorldCatalog.Data.rooms)
            {
                if(!visible.Contains(room.id))continue;
                foreach(var link in room.links)
                {
                    if(!visible.Contains(link.target) || (!session.Visited.Contains(room.id) && !session.Visited.Contains(link.target)))continue;
                    string key=string.CompareOrdinal(room.id,link.target)<0?room.id+"/"+link.target:link.target+"/"+room.id;
                    if(!drawn.Add(key))continue;
                    var a=Position(room);var b=Position(WorldCatalog.Find(link.target));var delta=b-a;
                    bool open=link.Open(session.Archive,session.Flags);var color=open?new Color(.35f,.65f,.64f):new Color(.78f,.47f,.29f);
                    int count=open?1:12;
                    for(int n=0;n<count;n++)
                    {
                        var p=Vector2.Lerp(a,b,(n+.5f)/count);
                        var stroke=Rect(open?"Open connection":"Locked connection",p,new Vector2(delta.magnitude/count*(open?1:.55f),2),color);
                        stroke.localRotation=Quaternion.Euler(0,0,Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg);
                    }
                }
            }
            foreach(var room in WorldCatalog.Data.rooms)
            {
                if(!visible.Contains(room.id))continue;
                bool seen=session.Visited.Contains(room.id),current=session.Room.id==room.id;
                var pos=Position(room);Rect(room.id,pos,new Vector2(155,58),current?new Color(.10f,.31f,.31f):new Color(.08f,.14f,.19f));
                Label(seen?room.name:"UNEXPLORED",pos+Vector2.up*9,new Vector2(153,30),12,seen?Color.white:Color.gray);
                Label(current?"YOU ARE HERE":!seen?"?":room.benches.Length>0?"SIGNAL ANCHOR":room.region.ToUpperInvariant(),pos-Vector2.up*15,new Vector2(150,21),10,new Color(.51f,.78f,.77f));
            }
            Label("PATHS YOU REMEMBER",new Vector2(700,820),new Vector2(1100,50),30,Color.white);
            Label(session.Visited.Count+" PLACES MAPPED THIS LIFE",new Vector2(700,775),new Vector2(1100,30),15,new Color(.73f,.65f,.44f));
            Label("SOLID / OPEN     DASHED / LOCKED     UNKNOWN ROOMS ARE UNNAMED\nAnchors save discovery, shortcuts and victories. Transfer rebuilds this world's routes.",new Vector2(700,115),new Vector2(1200,65),16,new Color(.61f,.73f,.77f));
            var button=Rect("Return to world",new Vector2(700,55),new Vector2(390,40),new Color(.12f,.23f,.27f));button.GetComponent<Image>().raycastTarget=true;
            button.gameObject.AddComponent<Button>().onClick.AddListener(Close);
            Label("RETURN / M / RIGHT STICK / ESC",new Vector2(700,55),new Vector2(380,35),16,Color.white);
        }
    }
}
