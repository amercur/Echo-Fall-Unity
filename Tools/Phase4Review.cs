using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Phase4Review
{
    static async Task Frames(int count=6){int end=Time.frameCount+count;float limit=Time.realtimeSinceStartup+15;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Stopped player loop");await Task.Delay(10);}}
    static async Task Ready(){float limit=Time.realtimeSinceStartup+15;while(SliceSession.Instance==null||!SliceSession.Instance.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Room load failed");await Task.Delay(25);}await Frames();}
    static string DirectoryPath=>Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/phase4");
    static void Render(string name)
    {
        var s=SliceSession.Instance;var go=new GameObject("World review camera");var camera=go.AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;camera.rect=new Rect(0,0,1,1);
        camera.transform.position=new Vector3(s.Room.bounds.center.x,(s.Room.bounds.yMax-.5f)/2,-10);camera.orthographicSize=Mathf.Max(3.1f,s.Room.bounds.width/3.2f);camera.aspect=1.6f;
        const int width=1600,height=1000;var rt=new RenderTexture(width,height,24);var old=RenderTexture.active;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(DirectoryPath,name+".png"),texture.EncodeToPNG());}
        finally{RenderTexture.active=old;camera.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(go);}
    }
    public static async Task<object> Capture()
    {
        bool background=Application.runInBackground,ephemeral=SliceSession.EphemeralSave;Application.runInBackground=true;SliceSession.EphemeralSave=true;Directory.CreateDirectory(DirectoryPath);
        int first=Time.frameCount;var evidence=new List<object>();
        try
        {
            await SceneManager.LoadSceneAsync("WakeSlice");await Ready();var s=SliceSession.Instance;
            foreach(string id in new[]{"cradle","lungs","mother","garden","observatory","choir","rootvault"})
            {
                s.StartCoroutine(s.LoadRoom(id,"default"));await Ready();foreach(var enemy in UnityEngine.Object.FindObjectsByType<SliceEnemy>())enemy.enabled=false;
                s.motor.automaticSimulation=false;s.follow.SnapToTarget();await Frames();Render(id);
                evidence.Add(new{room=id,actors=s.Room.GetComponentsInChildren<SliceEnemy>().Length,visited=s.Visited.Contains(id)});
                if(id=="garden"){s.GetComponent<WorldMap>().Open();await Frames();ScreenCapture.CaptureScreenshot(Path.Combine(DirectoryPath,"map-fog.png"));await Frames(8);s.GetComponent<WorldMap>().Close();}
            }
            s.Archive.Transfer("mercy");s.Room.Apply(s);await Frames();Render("rootvault-mercy");
            s.Flags.UnionWith(WorldCatalog.Data.flags);s.Visited.UnionWith(WorldCatalog.Data.rooms.Select(r=>r.id));
            s.GetComponent<WorldMap>().Open();await Frames();ScreenCapture.CaptureScreenshot(Path.Combine(DirectoryPath,"map-full.png"));await Frames(8);s.GetComponent<WorldMap>().Close();
            s.StartCoroutine(s.LoadRoom("garden","default"));await Ready();s.GetComponent<WorldMap>().Close();
            return new{firstFrame=first,lastFrame=Time.frameCount,evidence,ephemeral=true};
        }
        finally{Application.runInBackground=background;SliceSession.EphemeralSave=ephemeral;}
    }
}
