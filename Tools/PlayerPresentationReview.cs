using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;

// Pipeline Play Mode review. Only ephemeral sessions; no authored assets or saves are changed.
public static class PlayerPresentationReview
{
    static readonly List<object> captures=new List<object>();
    static string directory;
    static async Task Frames(int count=2)
    {
        int end=Time.frameCount+count;float deadline=Time.realtimeSinceStartup+10;
        while(Time.frameCount<end) {if(Time.realtimeSinceStartup>deadline)throw new Exception("Player loop stopped");await Task.Delay(10);}
    }
    static async Task Ready()
    {
        float deadline=Time.realtimeSinceStartup+15;
        while(SliceSession.Instance==null || !SliceSession.Instance.Playing)
        {if(Time.realtimeSinceStartup>deadline)throw new Exception("Room did not load");await Task.Delay(25);}
        await Frames(12);
        foreach(var enemy in UnityEngine.Object.FindObjectsByType<SliceEnemy>())enemy.enabled=false;
    }
    static void Capture(string name,PlayerVisual visual,bool wide=false)
    {
        var go=new GameObject("Temporary review camera");var camera=go.AddComponent<Camera>();camera.CopyFrom(Camera.main);
        camera.enabled=false;camera.orthographic=true;
        camera.transform.position=wide?Camera.main.transform.position:(Vector3)visual.motor.RenderPosition+new Vector3(0,.3f,-10);
        camera.orthographicSize=wide?Camera.main.orthographicSize:.7f;
        int w=wide?1280:480,h=wide?Mathf.RoundToInt(1280/Camera.main.aspect):480;
        camera.aspect=wide?Camera.main.aspect:1;
        var rt=new RenderTexture(w,h,24);var old=RenderTexture.active;
        var texture=new Texture2D(w,h,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            texture.ReadPixels(new Rect(0,0,w,h),0,0);texture.Apply();
            File.WriteAllBytes(Path.Combine(directory,name+".png"),texture.EncodeToPNG());
            captures.Add(new {name,pose=visual.Pose.ToString(),frame=visual.FrameIndex,gameFrame=Time.frameCount,ghosts=visual.LiveGhosts});
        }
        finally {RenderTexture.active=old;camera.targetTexture=null;rt.Release();UnityEngine.Object.Destroy(rt);UnityEngine.Object.Destroy(texture);UnityEngine.Object.Destroy(go);}
    }
    public static async Task<object> CaptureAll()
    {
        if(!Application.isPlaying)throw new Exception("Enter Play Mode first");
        bool background=Application.runInBackground,ephemeral=SliceSession.EphemeralSave;
        Application.runInBackground=true;SliceSession.EphemeralSave=true;captures.Clear();
        directory=Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/player-presentation");Directory.CreateDirectory(directory);
        int first=Time.frameCount;
        try
        {
            await SceneManager.LoadSceneAsync("MovementLab");await Frames(20);
            var motor=UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();motor.automaticSimulation=false;
            var visual=motor.GetComponentInChildren<PlayerVisual>();
            Capture("lab-idle",visual);Capture("movement-lab",visual,true);
            for(int i=0;i<25;i++)motor.Simulate(MovementTuning.Step,new MovementCommand{move=1});
            await Frames(5);Capture("lab-run",visual);
            motor.Simulate(MovementTuning.Step,new MovementCommand{jumpPressed=true});await Frames();Capture("lab-rise",visual);
            for(int i=0;i<150 && motor.Velocity.y>=0;i++)motor.Simulate(MovementTuning.Step,default);
            for(int i=0;i<20;i++)motor.Simulate(MovementTuning.Step,default);
            await Frames();Capture("lab-fall",visual);
            for(int i=0;i<180 && !motor.Grounded;i++)motor.Simulate(MovementTuning.Step,default);
            await Frames();Capture("lab-land",visual);
            motor.Simulate(MovementTuning.Step,new MovementCommand{move=1,dashPressed=true});await Frames();
            for(int i=0;i<4;i++){motor.Simulate(MovementTuning.Step,new MovementCommand{move=1});await Frames();}
            Capture("lab-dash",visual);
            var wall=new GameObject("Review wall");wall.layer=LayerMask.NameToLayer("EchoSolid");wall.transform.position=new Vector3(2.1f,2,0);wall.AddComponent<BoxCollider2D>().size=new Vector2(.1f,4);Physics2D.SyncTransforms();
            motor.EnterRoom(new Vector2(1.8f,2));await Frames();
            for(int i=0;i<35;i++)motor.Simulate(MovementTuning.Step,new MovementCommand{move=1});
            await Frames();Capture("lab-wall-slide",visual);
            motor.Simulate(MovementTuning.Step,new MovementCommand{move=-1,jumpPressed=true});await Frames();Capture("lab-wall-jump",visual);
            UnityEngine.Object.Destroy(wall);
            await SceneManager.LoadSceneAsync("WakeSlice");await Ready();
            foreach(string room in new[]{"wake","belfry","cistern","archive","procession"})
            {
                var s=SliceSession.Instance;s.StartCoroutine(s.LoadRoom(room,"default"));await Ready();
                motor=s.motor;visual=motor.GetComponentInChildren<PlayerVisual>();s.follow.SnapToTarget();
                Capture(room,visual,true);
                s.combat.Strike(true,0);await Frames(3);Capture(room+"-attack",visual);
            }
            var session=SliceSession.Instance;visual=session.motor.GetComponentInChildren<PlayerVisual>();
            session.combat.Rest();session.combat.BeginGuard();await Frames();Capture("guard",visual);
            session.combat.Hurt(1,session.motor.Position+Vector2.right,false,null);await Frames();Capture("deflect",visual);
            session.combat.Rest();session.combat.Hurt(2,session.motor.Position+Vector2.right,true,null,true);await Frames();Capture("hurt",visual);
            session.EndRun(false);await Frames(10);Capture("transfer",visual);session.ChooseOption(0);await Ready();
            session.combat.PayIntegrity(6);await Frames(10);Capture("death",visual);
            session.ChooseOption(0);await Ready();
            return new {firstFrame=first,lastFrame=Time.frameCount,captures};
        }
        finally {Application.runInBackground=background;SliceSession.EphemeralSave=ephemeral;}
    }
}
