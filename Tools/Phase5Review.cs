using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;

public static class Phase5Review
{
    public static object Inspect()=>new { current=SliceSession.Instance.Room.id, rooms=WorldCatalog.Data.rooms.Select(r=>r.id).ToArray(), sessions=UnityEngine.Object.FindObjectsByType<SliceSession>().Length };
    static async Task Load(SliceSession s,string room)
    {
        s.StartCoroutine(s.LoadRoom(room,"default"));await Ready();
        if(s.Room.id!=room)throw new Exception("Expected "+room+", found "+s.Room.id+"; instance changed="+(s!=SliceSession.Instance));
    }
    static string Output=>Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/phase5");
    static async Task Frames(int count=8){int end=Time.frameCount+count;float limit=Time.realtimeSinceStartup+15;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Player loop stopped");await Task.Delay(10);}}
    static async Task Ready(){float limit=Time.realtimeSinceStartup+15;while(SliceSession.Instance==null||!SliceSession.Instance.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Room load stalled");await Task.Delay(25);}await Frames();foreach(var enemy in UnityEngine.Object.FindObjectsByType<SliceEnemy>())enemy.enabled=false;}
    static async Task Capture(string name){await Frames();ScreenCapture.CaptureScreenshot(Path.Combine(Output,name+".png"));await Frames(12);}
    public static async Task<object> Run()
    {
        bool old=SliceSession.EphemeralSave,background=Application.runInBackground;SliceSession.EphemeralSave=true;Application.runInBackground=true;Directory.CreateDirectory(Output);int first=Time.frameCount;
        try
        {
            await UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("WakeSlice");await Ready();var s=SliceSession.Instance;
            foreach(string id in new[]{"mercy","defiance","obedience","sacrifice","betrayal"})s.Archive.Transfer(id);
            s.ShowMemories();await Capture("five-memory-menu");
            var choices=s.hud.GetComponentsInChildren<UnityEngine.UI.Button>().Where(b=>b.gameObject.activeInHierarchy).ToArray();
            if(choices.Length!=6)throw new Exception("Not all five memories plus forgetting are visible");
            s.SetScreen(SliceScreen.Playing);await Load(s,"cistern");s.motor.Teleport(new Vector2(6,.3f));s.motor.automaticSimulation=false;s.follow.SnapToTarget();await Capture("order-cistern");
            await Load(s,"archive");s.motor.Teleport(new Vector2(8.6f,.12f));s.motor.automaticSimulation=false;s.follow.SnapToTarget();await Capture("betrayal-echo");
            s.Archive.Forget("betrayal");await Load(s,"cradle");s.motor.Teleport(new Vector2(3,.12f));s.motor.automaticSimulation=false;s.follow.SnapToTarget();await Capture("sacrifice-cradle");
            s.Interact(s.Room.GetComponentsInChildren<SliceInteraction>().Single(i=>i.id=="keeper"));await Capture("keeper-remembers");s.SetScreen(SliceScreen.Playing);
            foreach(var id in new[]{"mercy","fire","defiance","obedience","sacrifice","betrayal","respect","abandon"})s.RecordDecision(id);
            s.EndRun(false);await Capture("extraction-choices");
            s.CommitTransfer("respect");await Ready();s.StartCoroutine(s.LoadRoom("archive","default"));await Ready();
            return new{firstFrame=first,lastFrame=Time.frameCount,menuChoices=choices.Length,archiveValid=s.Archive.Valid(),loop=s.Archive.loop,ephemeral=true};
        }
        finally{SliceSession.EphemeralSave=old;Application.runInBackground=background;}
    }
}
