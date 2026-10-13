using EchoFall.Movement.Editor;
using System.Threading.Tasks;
public static class Phase7Build
{
    public static async Task<string> QA(){await Task.Delay(100);ReleaseBuild.QA();return "QA build completed";}
    public static async Task<string> Release(){await Task.Delay(100);ReleaseBuild.Windows();return "Release build completed";}
}
