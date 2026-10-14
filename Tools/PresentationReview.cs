using System;
using System.IO;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class PresentationReview
{
    public static Task<object> After()=>Run("after");
    static async Task Frames(int count){int end=Time.frameCount+count;float limit=Time.realtimeSinceStartup+20;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Render stalled");await Task.Delay(10);}}
    public static async Task<object> Run(string label="before")
    {
        bool old=SliceSession.EphemeralSave,bg=Application.runInBackground;
        SliceSession.EphemeralSave=true;Application.runInBackground=true;
        try{
            string dir="Docs/Validation/presentation/"+label;Directory.CreateDirectory(dir);
            // Let any bootstrap load finish before replacing its owning scene.
            float bootstrapLimit=Time.realtimeSinceStartup+20;
            while(SliceSession.Instance!=null && SliceSession.Instance.Screen==SliceScreen.Loading)
            {if(Time.realtimeSinceStartup>bootstrapLimit)throw new Exception("Prior bootstrap stalled");await Task.Delay(20);}
            await SceneManager.LoadSceneAsync("WakeSlice");
            float limit=Time.realtimeSinceStartup+20;
            while(SliceSession.Instance==null||!SliceSession.Instance.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Wake failed to load");await Task.Delay(20);}
            var s=SliceSession.Instance;await Frames(30);
            ScreenCapture.CaptureScreenshot(dir+"/wake.png");await Frames(3);
            s.ShowSettings();await Frames(30);ScreenCapture.CaptureScreenshot(dir+"/settings.png");await Frames(3);
            s.SetScreen(SliceScreen.Playing);
            if(label!="before")foreach(var place in WorldCatalog.Data.rooms)
            {
                s.StartCoroutine(s.LoadRoom(place.id,"default"));await Task.Delay(100);
                limit=Time.realtimeSinceStartup+15;
                while(!s.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Room failed: "+place.id);await Task.Delay(20);}
                if(s.Room.id!=place.id)throw new Exception("Wrong room: "+s.Room.id+" expected "+place.id);
                if(UnityEngine.Object.FindObjectsByType<SliceRoom>().Length!=1)throw new Exception("Overlapping room capture");
                await Frames(30);Canvas.ForceUpdateCanvases();ScreenCapture.CaptureScreenshot(dir+"/"+place.id+".png");await Frames(8);
            }
            s.EndRun(false);await Frames(20);ScreenCapture.CaptureScreenshot(dir+"/transfer.png");await Frames(3);
            return new{frame=Time.frameCount,room=s.Room.id,player=s.motor.RenderPosition,camera=Camera.main.transform.position,objects=UnityEngine.Object.FindObjectsByType<GameObject>().Length};
        }finally{SliceSession.EphemeralSave=old;Application.runInBackground=bg;}
    }
}
