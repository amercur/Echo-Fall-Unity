using System.IO;
using System.Threading.Tasks;
using EchoFall.Movement.Editor;
public static class FinishedBuild
{
    public static Task<string> QA()=>Build(true);
    public static Task<string> Release()=>Build(false);
    static async Task<string> Build(bool qa)
    {
        string name="build-"+(qa?"qa":"release")+".json";
        string original="Docs/Validation/phase7/"+name;
        string previous=File.Exists(original)?File.ReadAllText(original):null;
        Directory.CreateDirectory("Docs/Validation/finished");await Task.Delay(100);
        try{ReleaseBuild.Build(qa);File.Copy(original,"Docs/Validation/finished/"+name,true);return "Finished presentation build completed";}
        finally{if(previous!=null)File.WriteAllText(original,previous);}
    }
}
