using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using EchoFall.Movement;
using UnityEngine;
using UnityEngine.SceneManagement;
public static class Phase8Review
{
    static async Task Wait(Func<bool> condition){float end=Time.realtimeSinceStartup+20;while(!condition()){if(Time.realtimeSinceStartup>end)throw new Exception("Review wait timed out");await Task.Delay(20);}}
    static async Task Capture(string name)
    {
        await Task.Delay(350);Canvas.ForceUpdateCanvases();
        foreach(var label in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>())
            if(label.transform.parent.name=="Field guide cue" && label.preferredHeight>label.rectTransform.rect.height+1)
                throw new Exception("Teaching card clips at "+name+": "+label.preferredHeight+" > "+label.rectTransform.rect.height);
        ScreenCapture.CaptureScreenshot("Docs/Validation/phase8/"+name+".png");await Task.Delay(120);
    }
    public static async Task<string> Route()
    {
        bool old=SliceSession.EphemeralSave,bg=Application.runInBackground,force=OpeningExperience.ShowTitleInTests;
        SliceSession.EphemeralSave=true;Application.runInBackground=true;OpeningExperience.ShowTitleInTests=true;
        try{
            await SceneManager.LoadSceneAsync("MovementLab");await Task.Delay(100);await SceneManager.LoadSceneAsync("WakeSlice");
            await Wait(()=>SliceSession.Instance!=null&&SliceSession.Instance.Screen==SliceScreen.Title);var s=SliceSession.Instance;s.ChooseOption(0);
            var motor=s.motor;motor.automaticSimulation=false;
            for(int i=0;i<160&&motor.Position.x<2.2f;i++)motor.Simulate(MovementTuning.Step,new MovementCommand{move=1});
            motor.Simulate(MovementTuning.Step,new MovementCommand{move=1,jumpPressed=true});
            for(int i=0;i<180;i++)motor.Simulate(MovementTuning.Step,new MovementCommand{move=motor.Position.x<3.35f?1:0});
            await Task.Delay(80);
            if(!motor.Grounded||motor.Position.y<.8f||s.Nearest==null||s.Nearest.id!="first-transfer")throw new Exception("First glass not reachable from the spawn: "+motor.Position);
            await Capture("first-ledge-route");s.Interact(s.Nearest);
            if(s.ModalTitle!="THE GLASS IS LISTENING")throw new Exception("Glass interaction missing");
            return "Reached listening glass using the real collision motor from the authored spawn: "+motor.Position+". No geometry or movement values changed.";
        }finally{SliceSession.EphemeralSave=old;Application.runInBackground=bg;OpeningExperience.ShowTitleInTests=force;}
    }
    public static async Task<string> Run()
    {
        bool old=SliceSession.EphemeralSave,bg=Application.runInBackground,force=OpeningExperience.ShowTitleInTests;
        SliceSession.EphemeralSave=true;Application.runInBackground=true;OpeningExperience.ShowTitleInTests=true;
        try{
            Directory.CreateDirectory("Docs/Validation/phase8");
            await SceneManager.LoadSceneAsync("MovementLab");await Task.Delay(100);await SceneManager.LoadSceneAsync("WakeSlice");
            await Wait(()=>SliceSession.Instance!=null&&SliceSession.Instance.Screen==SliceScreen.Title);
            var s=SliceSession.Instance;int first=Time.frameCount;await Capture("title");s.ChooseOption(0);await Capture("arrival");
            s.Opening.ShowLessons(0);await Capture("field-guide");s.SetScreen(SliceScreen.Playing);
            s.motor.Teleport(new Vector2(4.5f,0));s.Opening.Learn("strike");await Task.Delay(2500);await Capture("warden-lesson");
            bool large=PlayerPreferences.Current.largeText;try{PlayerPreferences.Current.largeText=true;await Capture("warden-large-text");}finally{PlayerPreferences.Current.largeText=large;}
            foreach(var enemy in s.Room.GetComponentsInChildren<SliceEnemy>())enemy.enabled=false;
            s.motor.Teleport(new Vector2(8.65f,0));s.Interact(s.Room.GetComponentsInChildren<SliceInteraction>().Single(i=>i.id=="creature"));await Capture("choice");
            s.ChooseOption(0);await Capture("deed");s.EndRun(false);await Capture("first-transfer");s.CommitTransfer("mercy");await Wait(()=>s.Playing);await Capture("second-arrival");
            s.motor.Teleport(new Vector2(8.65f,0));await Task.Delay(700);await Capture("returned-creature");
            s.Interact(s.Room.GetComponentsInChildren<SliceInteraction>().Single(i=>i.id=="creature"));await Capture("recognition");s.ChooseOption(0);
            s.StartCoroutine(s.LoadRoom("cistern","west"));await Wait(()=>s.Playing&&s.Room.id=="cistern");await Capture("remembered-cistern");
            s.StartCoroutine(s.LoadRoom("wake","default"));await Wait(()=>s.Playing&&s.Room.id=="wake");s.Archive.Transfer("fire");
            s.StartCoroutine(s.LoadRoom("wake","default",true));await Wait(()=>s.Playing&&s.Room.id=="wake");s.motor.Teleport(new Vector2(8.65f,0));await Capture("ember-absence");
            return "Opening captures complete, frame "+first+" -> "+Time.frameCount+"; archive valid="+s.Archive.Valid();
        }finally{SliceSession.EphemeralSave=old;Application.runInBackground=bg;OpeningExperience.ShowTitleInTests=force;}
    }
    public static async Task<string> BossHUD()
    {
        bool old=SliceSession.EphemeralSave,bg=Application.runInBackground;
        SliceSession.EphemeralSave=true;Application.runInBackground=true;
        try{
            await SceneManager.LoadSceneAsync("MovementLab");await Task.Delay(100);await SceneManager.LoadSceneAsync("WakeSlice");
            await Wait(()=>SliceSession.Instance!=null&&SliceSession.Instance.Playing);var s=SliceSession.Instance;
            s.StartCoroutine(s.LoadRoom("king","default"));await Wait(()=>s.Playing&&s.Room.id=="king");
            s.motor.Teleport(new Vector2(3.2f,0));await Wait(()=>SliceKing.Active!=null&&SliceKing.Active.Started);
            await Capture("boss-hud");
            var hud=UnityEngine.Object.FindAnyObjectByType<SliceHUD>();
            var transforms=hud.GetComponentsInChildren<Transform>(true);
            if(transforms.Single(t=>t.name=="Field guide cue").gameObject.activeSelf || !transforms.Single(t=>t.name=="Boss backing").gameObject.activeSelf)
                throw new Exception("Teaching card competes with active boss HUD");
            return "King encounter starts with boss health visible and contextual teaching hidden; field guide remains available through pause.";
        }finally{SliceSession.EphemeralSave=old;Application.runInBackground=bg;}
    }
}
