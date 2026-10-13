using System;
using System.IO;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public static class Phase7Review
{
    static async Task Frames(int n=6){int end=Time.frameCount+n;float limit=Time.realtimeSinceStartup+20;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Stalled");await Task.Delay(10);}}
    public static async Task<object> Run()
    {
        bool old=SliceSession.EphemeralSave,bg=Application.runInBackground;bool large=PlayerPreferences.Current.largeText;SliceSession.EphemeralSave=true;Application.runInBackground=true;
        var bindings=PlayerPreferences.Current.bindings;var background=InputSystem.settings.backgroundBehavior;var editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;Keyboard keyboard=null;
        try
        {
            await SceneManager.LoadSceneAsync("WakeSlice");while(SliceSession.Instance==null||!SliceSession.Instance.Playing)await Task.Delay(20);
            var s=SliceSession.Instance;PlayerPreferences.Current.largeText=true;s.ShowSettings();await Frames();ScreenCapture.CaptureScreenshot("Docs/Validation/phase7/settings-large.png");await Frames();
            s.ShowBindings();await Frames();ScreenCapture.CaptureScreenshot("Docs/Validation/phase7/bindings.png");await Frames();
            s.SetScreen(SliceScreen.Playing);await Frames(120);ScreenCapture.CaptureScreenshot("Docs/Validation/phase7/wake.png");await Frames();
            PlayerPreferences.Current.bindings=new System.Collections.Generic.List<PlayerPreferences.Binding>(bindings);PlayerPreferences.Current.bindings.RemoveAll(b=>b.action=="Jump");PlayerPreferences.Current.bindings.Add(new PlayerPreferences.Binding{action="Jump",key="p"});
            var input=s.motor.GetComponent<MovementInput>();input.enabled=false;input.enabled=true;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;keyboard=InputSystem.AddDevice<Keyboard>();
            float y=s.motor.Position.y;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.P));InputSystem.Update();await Frames(12);bool jumped=s.motor.Position.y>y+.1f;if(!jumped)throw new Exception("Rebound jump did not respond");InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            return new{settingsOptions=9,frame=Time.frameCount,reboundJump=jumped,music=UnityEngine.Object.FindAnyObjectByType<SliceSound>().MusicContext};
        }
        finally{if(keyboard!=null)InputSystem.RemoveDevice(keyboard);InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;PlayerPreferences.Current.bindings=bindings;SliceSession.EphemeralSave=old;Application.runInBackground=bg;PlayerPreferences.Current.largeText=large;}
    }
}
