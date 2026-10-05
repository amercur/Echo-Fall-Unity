using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace EchoFall.Movement.Tests
{
    public sealed class SliceRuleTests
    {
        [Test] public void ArchiveTransferPreservesIdentityAndEmberOverridesRoots()
        {
            var a=new SliceArchive(); Assert.That(a.RootsOpen,Is.False);
            a.Transfer("mercy"); Assert.That(a.RootsOpen,Is.True); Assert.That(a.loop,Is.EqualTo(2));
            a.Transfer("return"); Assert.That(a.RootsOpen,Is.True); Assert.That(a.active,Is.EqualTo("return"));
            a.Transfer("fire"); Assert.That(a.RootsOpen,Is.False); Assert.That(a.Remembers("mercy"),Is.True);
            a.Transfer("fire"); Assert.That(a.memories.Count,Is.EqualTo(3)); Assert.That(a.Valid(),Is.True);
        }
        [TestCase("unknown")][TestCase("")][TestCase(null)]
        public void UnknownMemoryIsRejected(string id) { Assert.Throws<ArgumentException>(()=>new SliceArchive().Transfer(id)); }
        [Test] public void SaveRoundTripAndCorruptPrimaryRecoversBackup()
        {
            string directory=Path.Combine(Application.temporaryCachePath,"slice-test-"+Guid.NewGuid());
            string path=Path.Combine(directory,"save.json");
            try
            {
                var a=new SliceArchive(); a.Transfer("mercy"); SliceMemoryStore.Save(path,a);
                var copy=SliceMemoryStore.Load(path,out var warning); Assert.That(copy.RootsOpen,Is.True); Assert.That(warning,Is.Null);
                a.Transfer("fire"); SliceMemoryStore.Save(path,a); File.WriteAllText(path,"not json");
                copy=SliceMemoryStore.Load(path,out warning); Assert.That(copy.active,Is.EqualTo("mercy")); Assert.That(warning,Is.Not.Null);
                Assert.That(File.ReadAllText(path),Is.EqualTo("not json"),"Read recovery must not overwrite evidence.");
            }
            finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
        }
        [Test] public void InvalidArchiveNeverPassesValidation()
        {
            var a=new SliceArchive { version=2 }; Assert.That(a.Valid(),Is.False);
            a.version=1; a.memories.Add("mercy"); Assert.That(a.Valid(),Is.False);
            a.active="mercy"; Assert.That(a.Valid(),Is.True); a.memories.Add("mercy"); Assert.That(a.Valid(),Is.False);
        }
    }
}
