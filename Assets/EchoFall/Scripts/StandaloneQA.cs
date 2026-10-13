#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EchoFall.Movement
{
    // Opt-in development-player validation. No driver is compiled into the release player.
    public sealed class StandaloneQA : MonoBehaviour
    {
        [Serializable] public sealed class Sample{public string room;public float medianMs,p95Ms;public long managedBytes,totalBytes;public int objects;}
        [Serializable] public sealed class Report{public bool passed;public string error,version,device,savePath;public string[] endings;public List<Sample> samples=new List<Sample>();public List<string> checks=new List<string>();}
        static string output;
        static bool resume;
        Report report=new Report();
        SliceSession S=>SliceSession.Instance;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Boot()
        {
            var args=Environment.GetCommandLineArgs();int i=Array.IndexOf(args,"-echo-qa");if(i<0||i+1>=args.Length)return;
            output=Path.GetFullPath(args[i+1]);Directory.CreateDirectory(output);resume=args.Contains("-echo-qa-resume");
            SliceSession.EphemeralSave=false;SliceSession.SavePathOverride=Path.Combine(output,"archive.json");
            Application.runInBackground=true;var go=new GameObject("Development validation");DontDestroyOnLoad(go);go.AddComponent<StandaloneQA>();
        }
        async Task Frames(int count=3)
        {int end=Time.frameCount+count;float limit=Time.realtimeSinceStartup+20;while(Time.frameCount<end){if(Time.realtimeSinceStartup>limit)throw new Exception("Frame loop stalled");await Task.Delay(10);}}
        async Task Ready()
        {float limit=Time.realtimeSinceStartup+30;while(S==null||S.Room==null||S.Screen==SliceScreen.Loading){if(Time.realtimeSinceStartup>limit)throw new Exception("Room stalled");await Task.Delay(20);}await Frames();}
        async Task Load(string room)
        {S.StartCoroutine(S.LoadRoom(room,"default"));await Ready();if(S.Room.id!=room)throw new Exception("Wrong room: "+room);}
        void Check(bool value,string label){if(!value)throw new Exception(label);report.checks.Add(label);}
        async Task Rest()
        {
            var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");S.motor.EnterRoom((Vector2)bench.transform.position-Vector2.up*.25f);
            S.motor.automaticSimulation=true;float limit=Time.realtimeSinceStartup+5;
            while(!S.motor.Grounded){if(Time.realtimeSinceStartup>limit)throw new Exception("Anchor grounding: "+S.Screen);await Frames(1);}
            S.Interact(bench);limit=Time.realtimeSinceStartup+5;
            while(S.Resting){if(Time.realtimeSinceStartup>limit)throw new Exception("Anchor rest stalled");await Frames(1);}
            Check(S.Archive.checkpoint!=null,"Anchor saved in "+S.Room.id+" / "+S.CurrentMessage);
        }
        async Task Profile(string room)
        {
            await Load(room);S.combat.Rest();
            if(room=="mother"){S.motor.Teleport(new Vector2(5,.02f));SliceMother.Active.Tick(.01f);for(int i=0;i<3;i++)SliceMother.Active.Recall();}
            if(room=="procession")S.motor.Teleport(new Vector2(5,.02f));
            var samples=new List<float>();long start=GC.GetTotalMemory(false);
            for(int n=0;n<120;n++){await Frames(1);samples.Add(Time.unscaledDeltaTime*1000);}
            samples.Sort();report.samples.Add(new Sample{room=room,medianMs=samples[60],p95Ms=samples[114],managedBytes=GC.GetTotalMemory(false)-start,totalBytes=UnityEngine.Profiling.Profiler.GetTotalAllocatedMemoryLong(),objects=FindObjectsByType<Transform>().Length});
        }
        async Task Finish(string ending)
        {
            S.SetScreen(SliceScreen.Playing);await Load("king");S.motor.automaticSimulation=false;S.motor.Teleport(new Vector2(4,.02f));var king=SliceKing.Active;king.Tick(.01f);king.Hit(100,.55f,0,true);Check(S.Flags.Contains("king"),"King victory "+ending);
            await Load("cradle");await Rest();await Load("mother");S.motor.automaticSimulation=false;S.motor.Teleport(new Vector2(5,.02f));var mother=SliceMother.Active;mother.Tick(.01f);mother.Tick(1.3f);mother.Tick(.91f);mother.Hit(25,.55f,0,true);mother.Tick(.06f);mother.Tick(.01f);mother.Recall();await Frames();mother.Hit(100,.55f,0,true);Check(S.Flags.Contains("mother"),"Mother victory "+ending);
            await Load("garden");S.ChildDialogue();S.ChooseOption(1);S.ChooseOption(2);S.ChooseOption(0);Check(S.Flags.Contains("child"),"Child permission "+ending);await Rest();
            await Load("choir");Check(S.CompleteEnding(ending),"Saved "+ending);await Frames(10);ScreenCapture.CaptureScreenshot(Path.Combine(output,ending+".png"));await Frames(5);
        }
        async void Start()
        {
            try
            {
                report.version=Application.version;report.device=SystemInfo.graphicsDeviceName;report.savePath=SliceSession.SavePathOverride;
                await Ready();
                if(resume){Check(S.Screen==SliceScreen.Ending,"Ending restored after process restart");Check(S.Archive.endings.Contains("BREAK")&&S.Archive.endings.Contains("SHARE"),"Both discoveries survived restart");S.ChooseOption(0);S.ChooseOption(0);await Ready();Check(S.Playing&&S.Room.id=="wake","Ending replay returns to Wake");}
                else
                {
                    Check(S.Playing,"Standalone launched into Wake");
                    await Frames(10);ScreenCapture.CaptureScreenshot(Path.Combine(output,"wake.png"));await Frames(5);
                    S.motor.automaticSimulation=false;var before=S.motor.Position;for(int i=0;i<40;i++)S.motor.Simulate(1f/120,new MovementCommand{move=1});Check(S.motor.Position.x>before.x+.3f,"Standalone movement simulation");S.motor.automaticSimulation=true;
                    await Rest();await SceneManager.LoadSceneAsync("WakeSlice");await Ready();Check(S.Archive.checkpoint!=null,"Standalone anchor reload");
                    foreach(string room in new[]{"procession","lungs","mother","choir"})await Profile(room);
                    // Deliberately staged encounter completion; not a difficulty or human-input claim.
                    await Finish("BREAK");S.ChooseOption(0);S.ChooseOption(0);await Ready();
                    S.Archive.Transfer("respect");S.Archive.Transfer("sacrifice");await Finish("SHARE");
                }
                report.endings=S.Archive.endings.ToArray();Check(S.Archive.Valid(),"Final archive valid");report.passed=true;
            }
            catch(Exception e){report.error=e.ToString();Debug.LogException(e);}
            finally{File.WriteAllText(Path.Combine(output,resume?"restart.json":"runtime.json"),JsonUtility.ToJson(report,true));Application.Quit(report.passed?0:1);}
        }
    }
}
#endif
