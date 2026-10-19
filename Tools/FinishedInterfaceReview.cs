using System;
using System.IO;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class FinishedInterfaceReview
{
    static async Task Frames(int count){int end=Time.frameCount+count;float limit=Time.realtimeSinceStartup+15;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Frame timeout");await Task.Delay(10);}}
    static async Task Capture(string name){await Frames(12);Canvas.ForceUpdateCanvases();ScreenCapture.CaptureScreenshot("Docs/Validation/finished/"+name+".png");await Frames(4);}
    public static async Task<string> Run()
    {
        bool old=SliceSession.EphemeralSave,large=PlayerPreferences.Current.largeText,bg=Application.runInBackground;
        SliceSession.EphemeralSave=true;Application.runInBackground=true;
        try{
            float bootstrapLimit=Time.realtimeSinceStartup+20;
            while(SliceSession.Instance!=null&&SliceSession.Instance.Screen==SliceScreen.Loading){if(Time.realtimeSinceStartup>bootstrapLimit)throw new Exception("Bootstrap timeout");await Task.Delay(20);}
            await SceneManager.LoadSceneAsync("WakeSlice");float limit=Time.realtimeSinceStartup+20;
            while(SliceSession.Instance==null||!SliceSession.Instance.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Load timeout");await Task.Delay(20);}
            var s=SliceSession.Instance;PlayerPreferences.Current.largeText=true;
            await Capture("hud-large");s.GetComponent<WorldMap>().Open();await Capture("map-large");s.GetComponent<WorldMap>().Close();
            s.Archive.Transfer("sacrifice");s.StartCoroutine(s.LoadRoom("cradle","default"));await Task.Delay(100);
            while(!s.Playing)await Task.Delay(20);await Capture("remembered-cages");
            return "Large HUD, map, and remembered cage presentation captured";
        }finally{PlayerPreferences.Current.largeText=large;SliceSession.EphemeralSave=old;Application.runInBackground=bg;}
    }
}
