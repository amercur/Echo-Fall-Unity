using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Phase3Review
{
    static async Task Frames(int count=3){int end=Time.frameCount+count;float limit=Time.realtimeSinceStartup+12;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Stopped player loop");await Task.Delay(10);}}
    static async Task Ready(){float limit=Time.realtimeSinceStartup+15;while(SliceSession.Instance==null||!SliceSession.Instance.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Room load failed");await Task.Delay(25);}await Frames(15);}
    static void Render(string name,Vector2? focus=null)
    {
        string directory=Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/phase3");Directory.CreateDirectory(directory);
        foreach(var enemy in UnityEngine.Object.FindObjectsByType<SliceEnemy>())enemy.SendMessage("LateUpdate");
        var go=new GameObject("Review camera");var camera=go.AddComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;camera.rect=new Rect(0,0,1,1);
        camera.transform.position=focus.HasValue?(Vector3)focus.Value+new Vector3(0,.7f,-10):Camera.main.transform.position;
        camera.orthographicSize=focus.HasValue?1.3f:Camera.main.orthographicSize;camera.aspect=focus.HasValue?1.6f:Camera.main.aspect;
        int width=1280,height=Mathf.RoundToInt(width/camera.aspect);var rt=new RenderTexture(width,height,24);var old=RenderTexture.active;var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG());}
        finally{RenderTexture.active=old;camera.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(go);}
    }
    public static async Task<object> Capture()
    {
        bool background=Application.runInBackground,ephemeral=SliceSession.EphemeralSave;Application.runInBackground=true;SliceSession.EphemeralSave=true;
        var evidence=new List<object>();int first=Time.frameCount;
        try
        {
            await SceneManager.LoadSceneAsync("WakeSlice");await Ready();var s=SliceSession.Instance;
            s.StartCoroutine(s.LoadRoom("procession","west"));await Ready();
            foreach(var e in UnityEngine.Object.FindObjectsByType<SliceEnemy>())e.enabled=false;
            var pair=UnityEngine.Object.FindObjectsByType<SliceEnemy>().First(e=>e.kind=="lancer");
            s.motor.automaticSimulation=false;s.motor.EnterRoom((Vector2)pair.transform.position-Vector2.right*2);
            var sentinel=UnityEngine.Object.FindObjectsByType<SliceEnemy>().First(e=>e.id=="procession/enemy-3");sentinel.Tick(.01f);await Frames();
            Render("sentinel-lancer",pair.transform.position);evidence.Add(new{pair=sentinel.Guarding});
            s.StartCoroutine(s.LoadRoom("king","west"));await Ready();var king=SliceKing.Active;king.enabled=false;
            s.motor.EnterRoom(new Vector2(3.05f,.02f));for(int i=0;i<12;i++)s.motor.Simulate(MovementTuning.Step,default);king.Tick(.01f);s.follow.SnapToTarget();await Frames();
            Render("court-intro");Render("king-idle",king.transform.position);
            ScreenCapture.CaptureScreenshot(Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/phase3/court-hud.png"));await Frames(8);
            for(int i=0;i<200&&king.State!="tell";i++)king.Tick(MovementTuning.Step);await Frames();Render("king-white-tell",king.transform.position);
            for(int i=0;i<1500 && !(king.State=="tell"&&king.Red);i++)king.Tick(MovementTuning.Step);await Frames();Render("king-red-tell",king.transform.position);
            evidence.Add(new{phase=king.Stage,pattern=king.Pattern,state=king.State,hp=king.hp});
            king.Hit(21,.55f,0,true);king.Tick(.05f);king.Tick(.01f);await Frames();Render("king-phase-two",king.transform.position);
            evidence.Add(new{phase=king.Stage,state=king.State,hp=king.hp});
            king.Hit(100,.8f,0,true);await Frames();Render("king-defeat",king.transform.position);
            var glass=s.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="victory");evidence.Add(new{victory=s.Flags.Contains("king"),unlocked=glass.LockReason(s)==null});
            s.Interact(glass);await Frames();
            string screen=Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/phase3/court-transfer.png");ScreenCapture.CaptureScreenshot(screen);await Frames(8);
            s.ChooseOption(0);await Ready();return new{firstFrame=first,lastFrame=Time.frameCount,evidence,room=s.Room.id};
        }
        finally{Application.runInBackground=background;SliceSession.EphemeralSave=ephemeral;}
    }
}

