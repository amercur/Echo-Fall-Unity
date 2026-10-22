using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EchoFall.Movement
{
    // Observes successful actions. Knowledge never authorizes gameplay or changes an archive.
    public sealed class OpeningExperience : MonoBehaviour
    {
        public static bool ShowTitleInTests;
        public static readonly string[] Lessons={"move","jump","dash","strike","deflect","anchor","wall","pogo","imprint","mend","choice","transfer"};
        readonly HashSet<string> learned=new HashSet<string>();
        SliceSession session;
        Vector2 lastPosition;
        string room;
        int life,wallSequence;
        float distance,ackUntil;
        string acknowledgement;
        bool wasGrounded;
        public bool WaitingAtTitle {get;private set;}
        public string Lesson {get;private set;}="move";
        public bool Isolated=>SliceSession.EphemeralSave||SliceSession.SavePathOverride!=null;
        public bool Knows(string id)=>learned.Contains(id);
        void Awake()
        {
            session=GetComponent<SliceSession>();
            if(!Isolated && PlayerPreferences.Current.learned!=null)foreach(var id in PlayerPreferences.Current.learned)if(Array.IndexOf(Lessons,id)>=0)learned.Add(id);
        }
        public void Boot()
        {
            if(!Isolated||ShowTitleInTests)ShowTitle();
        }
        public void ShowTitle()
        {
            WaitingAtTitle=true;
            string action=!string.IsNullOrEmpty(session.Archive.currentEnding)?"RETURN TO THE ENDING":session.Archive.pending.Count>0?"RETURN TO THE GLASS":session.Archive.checkpoint!=null?"CONTINUE FROM THE ANCHOR":session.Archive.loop>1?"RETURN TO THE WAKE":"WAKE";
            session.Show("ECHO//FALL","The world remembers what you did.\nIt does not remember why.\n\nA handprint waits on the other side of the glass.",SliceScreen.Title,
                (action,Begin),("FIELD GUIDE",()=>ShowLessons(0)),("SETTINGS",session.ShowSettings));
        }
        public void Begin()
        {
            WaitingAtTitle=false;session.ResumeOpening();SliceSound.Cue("signal");
            if(session.Playing)session.Notify(session.Archive.loop==1?"You know how to hold the blade. You cannot remember whose hand taught you.":Arrival());
        }
        string Arrival()=>session.Archive.Remembers("fire")?"The warmth crossed with you. Something at the eastern threshold did not.":session.Archive.RootsOpen?"A small breath answers yours. It is waiting where you left it.":"The same stone. Another hand against the glass.";
        public void Learn(string id)
        {
            if(Array.IndexOf(Lessons,id)<0||!learned.Add(id))return;
            acknowledgement=Name(id)+" / REMEMBERED";ackUntil=Time.unscaledTime+2.2f;
            if(!Isolated){var p=PlayerPreferences.Current;if(p.learned==null)p.learned=new List<string>();if(!p.learned.Contains(id))p.learned.Add(id);if(!p.Save())session.Notify("Your field guide applies now, but could not be saved.");}
        }
        void Update()
        {
            if(session.Room==null||!session.Playing)return;
            var motor=session.motor;
            if(room!=session.Room.id||life!=session.Archive.loop)
            {
                bool transferred=life>0&&life!=session.Archive.loop;room=session.Room.id;life=session.Archive.loop;
                lastPosition=motor.Position;wasGrounded=motor.Grounded;wallSequence=motor.WallJumpSequence;
                if(transferred){Learn("transfer");session.Notify(Arrival());}
            }
            float travel=Vector2.Distance(motor.Position,lastPosition);if(travel<.3f)distance+=travel;
            if(distance>.6f)Learn("move");
            if(wasGrounded&&!motor.Grounded&&motor.Velocity.y>1)Learn("jump");
            if(motor.Dashing)Learn("dash");
            if(wallSequence!=motor.WallJumpSequence)Learn("wall");
            wasGrounded=motor.Grounded;wallSequence=motor.WallJumpSequence;lastPosition=motor.Position;
            Lesson=SelectLesson();
        }
        string SelectLesson()
        {
            float x=session.motor.Position.x;
            if(session.Room.id=="wake")
            {
                if(session.Decision!=null)return "transfer";
                if(session.Archive.loop>1&&(session.Archive.RootsOpen||session.Archive.Remembers("fire")))return "consequence";
                if(!Knows("move")&&x<2.5f)return "move";
                if(x<4.1f&&!Knows("jump"))return "jump";
                if(x>=4.1f&&x<6.8f&&!session.Defeated.Contains("wake/enemy-0"))return Knows("strike")?"deflect":"strike";
                if(x>=7.7f&&!session.CreatureChoiceKnown)return "choice";
                if(session.Nearest!=null&&session.Nearest.kind=="bench"&&!Knows("anchor"))return "anchor";
                if(!Knows("dash"))return "dash";
                return "explore";
            }
            if(session.Room.id=="belfry"&&!Knows("wall"))return "wall";
            if(session.Room.id=="cistern"&&!Knows("pogo"))return "pogo";
            if(session.combat.Integrity<6&&session.combat.Resonance>=1&&!Knows("mend"))return "mend";
            if(session.Room.id=="procession"&&session.combat.Resonance>=1&&!Knows("imprint"))return "imprint";
            return "explore";
        }
        public string Guidance
        {
            get
            {
                if(!session.Playing)return "";
                if(Time.unscaledTime<ackUntil)return acknowledgement;
                return Instruction(Lesson);
            }
        }
        static string Key(string action,string pad)
        {
            var gamepad=Gamepad.current;var keyboard=Keyboard.current;
            bool usePad=gamepad!=null&&(keyboard==null||gamepad.lastUpdateTime>keyboard.lastUpdateTime);
            return usePad?pad:PlayerPreferences.Current.EffectiveKey(action).ToUpperInvariant();
        }
        public static string Name(string id)
        {
            switch(id){case "move":return "FIND YOUR FEET";case "jump":return "THE FIRST LEDGE";case "dash":return "CROSS THE SILENCE";case "strike":return "THE OLD PROMISE";case "deflect":return "ANSWER THE WHITE LIGHT";case "anchor":return "A PLACE TO REST";case "wall":return "THE BROKEN ASCENT";case "pogo":return "WAKE THE BELL";case "imprint":return "LEAVE A MARK";case "mend":return "DRAW A BREATH";case "choice":return "A SMALL, WARM THING";case "transfer":return "THE LISTENING GLASS";default:return "A TRACE REMAINS";}
        }
        public static string Instruction(string id)
        {
            switch(id)
            {
                case "move":return "A / D or left stick  /  Find your feet. The anchor is a safe place.";
                case "jump":return Key("Jump","A")+"  /  Jump to the low ledge. Hold to rise; release for a shorter leap.";
                case "dash":return Key("Dash","B")+"  /  Dash across the open floor. You can also dash once in the air.";
                case "strike":return Key("Attack","X")+"  /  Try your blade on the warden. It cannot take your last integrity.";
                case "deflect":return Key("Guard","LB")+"  /  Face the warden. Tap as its white strike reaches you. Blades alone will not release it.";
                case "anchor":return Key("Interact","Y")+"  /  Rest beside the signal anchor. Stay still until your life is saved.";
                case "wall":return Key("Jump","A")+"  /  Press toward the shaft wall and jump again. Each wall jump restores your air dash.";
                case "pogo":return "Down + "+Key("Attack","X")+"  /  Strike the hanging bell from above to rise again. Practice from the near ledge.";
                case "imprint":return Key("Imprint","RB")+"  /  With Resonance, mark a nearby enemy. Press again to release the mark.";
                case "mend":return Key("Mend","RT")+"  /  Hold while standing still. Spend one Resonance to mend one integrity.";
                case "choice":return Key("Interact","Y")+"  /  Listen to the small breath at the eastern threshold. You may leave without choosing.";
                case "transfer":return "Return left to the glass on the first ledge. "+Key("Interact","Y")+" to listen; R / Back also offers transfer.";
                case "consequence":return "Look again where the creature lay. The Cistern below remembers the same hand.";
                default:return "M / right stick: paths seen this life. Escape / Start: field guide and controls.";
            }
        }
        public void ShowLessons(int page)
        {
            var choices=new List<(string,Action)>();
            for(int i=page*6;i<Math.Min(Lessons.Length,page*6+6);i++)
            {
                string id=Lessons[i];choices.Add(((Knows(id)?"+ ":"")+Name(id),()=>session.Show(Name(id),Instruction(id),SliceScreen.Pause,("BACK",()=>ShowLessons(page)))));
            }
            choices.Add((page==0?"MORE ENTRIES":"EARLIER ENTRIES",()=>ShowLessons(1-page)));
            choices.Add((WaitingAtTitle?"TITLE":"RETURN",()=>{if(WaitingAtTitle)ShowTitle();else session.SetScreen(SliceScreen.Playing);}));
            session.Show("THE FIELD GUIDE","Practice leaves a trace. Marked entries record something you have done.\nEvery action remains available from the beginning.",SliceScreen.Pause,choices.ToArray());
        }
    }
}
