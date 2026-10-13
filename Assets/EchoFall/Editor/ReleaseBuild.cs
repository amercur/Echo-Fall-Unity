using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace EchoFall.Movement.Editor
{
    public static class ReleaseBuild
    {
        public static string[] Scenes()=>new[]{"Assets/EchoFall/Scenes/WakeSlice.unity"}.Concat(WorldCatalog.Data.rooms.Select(r=>"Assets/EchoFall/Scenes/Wake_"+r.id+".unity")).ToArray();
        [MenuItem("Echo-Fall/Build Windows Playtest")]
        public static void Windows()=>Build(false);
        [MenuItem("Echo-Fall/Build Windows QA")]
        public static void QA()=>Build(true);
        public static void Build(bool development)
        {
            PlayerSettings.productName="Echo-Fall";PlayerSettings.bundleVersion="0.7.0";
            // Keep companyName unchanged: it is part of the established persistentDataPath.
            PlayerSettings.defaultScreenWidth=1600;PlayerSettings.defaultScreenHeight=900;
            PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.resizableWindow=true;
            PlayerSettings.runInBackground=false;PlayerSettings.usePlayerLog=true;
            AssetDatabase.SaveAssets();
            string folder=development?"Builds/WindowsQA":"Builds/Windows";Directory.CreateDirectory(folder);
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=Scenes(),locationPathName=folder+"/Echo-Fall.exe",target=BuildTarget.StandaloneWindows64,options=development?BuildOptions.Development:BuildOptions.None});
            Directory.CreateDirectory("Docs/Validation/phase7");
            File.WriteAllText("Docs/Validation/phase7/build-"+(development?"qa":"release")+".json",JsonUtility.ToJson(new Result{result=report.summary.result.ToString(),errors=report.summary.totalErrors,warnings=report.summary.totalWarnings,bytes=report.summary.totalSize,seconds=report.summary.totalTime.TotalSeconds,scenes=Scenes(),messages=report.steps.SelectMany(s=>s.messages).Where(m=>m.type==LogType.Error||m.type==LogType.Warning).Select(m=>m.content).ToArray()},true));
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Windows build failed: "+report.summary.result);
        }
        [Serializable] sealed class Result{public string result;public int errors,warnings;public ulong bytes;public double seconds;public string[] scenes,messages;}
    }
}
