using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Phase6Review
{
    static string Output=>Path.Combine(Directory.GetCurrentDirectory(),"Docs/Validation/phase6");
    static async Task Frames(int count=8){int end=Time.frameCount+count;float limit=Time.realtimeSinceStartup+15;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Player loop stopped");await Task.Delay(10);}}
    static async Task Ready(){float limit=Time.realtimeSinceStartup+15;while(SliceSession.Instance==null||!SliceSession.Instance.Playing){if(Time.realtimeSinceStartup>limit)throw new Exception("Room load stalled");await Task.Delay(20);}await Frames();foreach(var e in UnityEngine.Object.FindObjectsByType<SliceEnemy>())e.enabled=false;}
    static async Task Load(SliceSession s,string room){s.StartCoroutine(s.LoadRoom(room,"default"));await Ready();if(s.Room.id!=room)throw new Exception("Wrong room");}
    static async Task Capture(string name){await Frames();ScreenCapture.CaptureScreenshot(Path.Combine(Output,name+".png"));await Frames(12);}
    public static async Task<object> Run()
    {
        bool old=SliceSession.EphemeralSave,bg=Application.runInBackground;SliceSession.EphemeralSave=true;Application.runInBackground=true;Directory.CreateDirectory(Output);int first=Time.frameCount;
        try
        {
            await SceneManager.LoadSceneAsync("WakeSlice");await Ready();var s=SliceSession.Instance;
            foreach(var id in new[]{"respect","sacrifice","fire"})s.Archive.Transfer(id);
            await Load(s,"king");s.motor.automaticSimulation=false;s.motor.Teleport(new Vector2(4,.02f));var king=SliceKing.Active;king.Tick(.01f);king.Hit(100,.55f,0,true);
            if(!s.Flags.Contains("king"))throw new Exception("King not defeated");
            await Load(s,"cradle");await Load(s,"mother");s.motor.automaticSimulation=false;s.motor.Teleport(new Vector2(6,.02f));s.follow.SnapToTarget();var mother=SliceMother.Active;mother.Tick(.01f);mother.Tick(1.3f);await Capture("mother-white-tell");
            mother.Tick(.91f);SliceProjectile.ClearAll();mother.Tick(2.3f);mother.Tick(.91f);SliceProjectile.ClearAll();mother.Tick(2.3f);await Capture("mother-red-tell");
            mother.Hit(20,.55f,0,true);mother.Tick(.05f);mother.Tick(.01f);mother.Recall();await Capture("mother-recall");
            mother.Hit(100,.55f,0,true);await Capture("mother-defeat");if(!s.Flags.Contains("mother"))throw new Exception("Mother not defeated");
            await Load(s,"garden");s.ChildDialogue();await Capture("child-history");s.ChooseOption(1);s.ChooseOption(2);s.ChooseOption(0);
            await Load(s,"choir");s.motor.automaticSimulation=false;s.motor.Teleport(new Vector2(10,0));s.follow.SnapToTarget();s.ChoirDialogue();await Capture("choir-choices");
            if(!s.OptionLabels.Contains("SHARE THE CHOICE"))throw new Exception("Shared outcome unavailable");
            s.SetScreen(SliceScreen.Playing);if(!s.CompleteEnding("SHARE"))throw new Exception("Ending refused");await Capture("ending-share");s.ChooseOption(1);await Capture("credits");s.ChooseOption(1);s.ChooseOption(0);await Ready();
            foreach(string ending in new[]{"RESET","BREAK","REMEMBER"})
            {s.Flags.UnionWith(new[]{"king","mother","child"});await Load(s,"choir");if(!s.CompleteEnding(ending))throw new Exception(ending+" failed");await Capture("ending-"+ending.ToLowerInvariant());s.ChooseOption(0);s.ChooseOption(0);await Ready();}
            return new{firstFrame=first,lastFrame=Time.frameCount,endings=s.Archive.endings.ToArray(),archiveValid=s.Archive.Valid(),ephemeral=true};
        }
        finally{SliceSession.EphemeralSave=old;Application.runInBackground=bg;}
    }
}
