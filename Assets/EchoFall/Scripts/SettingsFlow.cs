using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace EchoFall.Movement
{
    public sealed partial class SliceSession
    {
        public void ShowSettings()
        {
            var p=PlayerPreferences.Current;
            Show("SETTINGS", "Select a level to cycle it. Changes apply immediately.\nAdjust sound and visual comfort.",SliceScreen.Pause,
                ("MASTER / "+Percent(p.master),()=>ChangeSetting(()=>p.master=PlayerPreferences.Next(p.master))),
                ("MUSIC / "+Percent(p.music),()=>ChangeSetting(()=>p.music=PlayerPreferences.Next(p.music))),
                ("SOUND EFFECTS / "+Percent(p.sfx),()=>ChangeSetting(()=>p.sfx=PlayerPreferences.Next(p.sfx))),
                ("CAMERA SHAKE / "+Percent(p.shake),()=>ChangeSetting(()=>p.shake=PlayerPreferences.Next(p.shake))),
                ("HIT FLASH / "+Percent(p.flash),()=>ChangeSetting(()=>p.flash=PlayerPreferences.Next(p.flash))),
                ("LARGER TEXT / "+(p.largeText?"ON":"OFF"),()=>ChangeSetting(()=>p.largeText=!p.largeText)),
                ("KEYBOARD CONTROLS",ShowBindings),
                (Opening.WaitingAtTitle?"TITLE":"RESUME",ReturnFromOverlay),
                ("QUIT GAME",()=>Show("LEAVE THE WORLD?","Progress resumes from your last completed signal anchor.\nUnsaved exploration after that anchor will be lost.",SliceScreen.Pause,("QUIT",()=>Application.Quit()),("STAY",ShowSettings))));
        }
        static string Percent(float v)=>Mathf.RoundToInt(v*100)+"%";
        string capturing;
        string ControlsText
        {
            get{var p=PlayerPreferences.Current;return "A / D move | "+p.Key("Jump","SPACE")+" jump | "+p.Key("Dash","K")+" / SHIFT dash\n"+p.Key("Attack","J")+" / X strike (hold for charged cut) | W / S aim\n"+p.Key("Guard","F")+" tap: white deflect | hold, release: red counter\n"+p.Key("Imprint","Q")+" imprint | "+p.Key("Mend","H")+" hold: mend | "+p.Key("Memory","C")+" memory\n"+p.Key("Interact","E")+" interact | M map | R transfer\nController: A jump, X strike, B dash, Y interact.\nLB guard, RB imprint, LT memory, RT mend.";}
        }
        public void ShowBindings()
        {
            var choices=new List<(string,System.Action)>();
            foreach(string name in PlayerPreferences.Rebindable){string action=name;choices.Add((name.ToUpperInvariant()+" / "+PlayerPreferences.Current.EffectiveKey(name).ToUpperInvariant(),()=>{capturing=action;Show("BIND "+action.ToUpperInvariant(),"Press a keyboard key. Escape cancels.\nMovement, aim and menu keys remain fixed. Reserved or conflicting keys are ignored.",SliceScreen.Pause);}));}
            choices.Add(("BACK",ShowSettings));Show("KEYBOARD CONTROLS","Choose an action to replace its primary key.\nController bindings and alternate keys remain available.",SliceScreen.Pause,choices.ToArray());
        }
        bool CaptureBinding()
        {
            if(capturing==null)return false;var keyboard=Keyboard.current;if(keyboard==null)return true;
            foreach(var key in keyboard.allKeys)if(key.wasPressedThisFrame)
            {
                if(key==keyboard.escapeKey){capturing=null;ShowBindings();return true;}
                string name=key.name;
                if(name.Length==1&&"123456789wasdrmxl".Contains(name)||name=="tab"||name=="enter"||name=="leftShift"||name=="rightShift")return true;
                var p=PlayerPreferences.Current;
                foreach(string action in PlayerPreferences.Rebindable)if(action!=capturing&&p.EffectiveKey(action)==name)return true;
                p.bindings.RemoveAll(b=>b.action==capturing);p.bindings.Add(new PlayerPreferences.Binding{action=capturing,key=name});
                p.Apply(interact);combat.ApplyBindings();capturing=null;if(!p.Save())Notify("Bindings apply now, but could not be saved.");ShowBindings();return true;
            }
            return true;
        }
        void ChangeSetting(System.Action change){change();if(!PlayerPreferences.Current.Save())Notify("Settings could not be saved. They still apply until you close the game.");ShowSettings();}
        void OnApplicationFocus(bool focused)
        {
            if(!focused&&Playing)Show("THE WORLD WAITS","Return when you are ready.",SliceScreen.Pause,("RESUME",()=>SetScreen(SliceScreen.Playing)),("SETTINGS",ShowSettings));
        }
    }
}
