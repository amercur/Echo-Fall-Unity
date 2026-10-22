using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace EchoFall.Movement.Tests
{
    public sealed class OpeningKnowledgeTests
    {
        [Test] public void KnowledgeRoundTripsWithoutChangingBindingsOrOldSettingsCompatibility()
        {
            string path=Path.Combine(Application.temporaryCachePath,"opening-knowledge-"+System.Guid.NewGuid()+".json");
            try{
                var p=new PlayerPreferences();p.learned.Add("jump");p.bindings.Add(new PlayerPreferences.Binding{action="Jump",key="z"});Assert.That(p.Save(path),Is.True);
                var copy=PlayerPreferences.Load(path);Assert.That(copy.learned,Does.Contain("jump"));Assert.That(copy.Key("Jump","SPACE"),Is.EqualTo("Z"));
                File.WriteAllText(path,"{\"master\":0.7,\"music\":0.45,\"sfx\":0.8,\"shake\":1,\"flash\":1,\"bindings\":[]}");Assert.That(PlayerPreferences.Load(path).Valid(),Is.True);
            }finally{foreach(string suffix in new[]{"",".tmp",".bak"})if(File.Exists(path+suffix))File.Delete(path+suffix);}
        }
    }
}
