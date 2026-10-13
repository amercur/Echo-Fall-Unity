using System.IO;
using NUnit.Framework;

namespace EchoFall.Movement.Tests
{
    public sealed class PolishTests
    {
        [Test] public void SettingsRejectNonFiniteAndOutOfRange()
        {var p=new PlayerPreferences();Assert.That(p.Valid(),Is.True);p.shake=float.NaN;Assert.That(p.Valid(),Is.False);p.shake=1;p.master=2;Assert.That(p.Valid(),Is.False);}
        [Test] public void SettingsRoundTripAndCorruptPrimaryRecoversBackup()
        {
            string dir=Path.Combine(Path.GetTempPath(),"echo-settings-"+System.Guid.NewGuid());string path=Path.Combine(dir,"settings.json");
            try{var p=new PlayerPreferences{master=0,shake=0,flash=0,largeText=true};Assert.That(p.Save(path),Is.True);p.master=.5f;Assert.That(p.Save(path),Is.True);File.WriteAllText(path,"not json");var loaded=PlayerPreferences.Load(path);Assert.That(loaded.master,Is.Zero);Assert.That(loaded.largeText,Is.True);Assert.That(loaded.flash,Is.Zero);}finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void ReleaseSceneListExcludesPrototypesAndIncludesEveryDestination()
        {var scenes=Editor.ReleaseBuild.Scenes();Assert.That(scenes.Length,Is.EqualTo(14));Assert.That(scenes[0],Does.EndWith("WakeSlice.unity"));Assert.That(string.Join(";",scenes),Does.Not.Contain("MovementLab"));Assert.That(string.Join(";",scenes),Does.Not.Contain("SampleScene"));}
        [Test] public void RepeatedTransfersForgetAndEndingsKeepArchiveValid()
        {
            var archive=new SliceArchive();for(int n=0;n<250;n++){string id=MemoryRules.Ids[n%MemoryRules.Ids.Count];if(!archive.Remembers(id)&&archive.memories.Count==5)archive.Forget(archive.memories[0]);archive.Transfer(id);Assert.That(archive.Valid(),Is.True);Assert.That(archive.memories.Count,Is.LessThanOrEqualTo(5));}
        }
    }
}
