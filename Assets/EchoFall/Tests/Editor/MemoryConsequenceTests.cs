using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace EchoFall.Movement.Tests
{
    public sealed class MemoryConsequenceTests
    {
        [Test] public void CapacityReplacementAndDuplicateExtractionAreAtomic()
        {
            var a=new SliceArchive();foreach(var id in new[]{"return","mercy","fire","defiance","obedience"})a.Transfer(id);
            int loop=a.loop;Assert.Throws<InvalidOperationException>(()=>a.Transfer("respect"));Assert.That(a.loop,Is.EqualTo(loop));
            a.Transfer("mercy");Assert.That(a.memories.Count,Is.EqualTo(5));
            var copy=a.Copy();copy.Forget("fire");copy.Transfer("respect");
            Assert.That(copy.Valid(),Is.True);Assert.That(copy.RootsOpen,Is.True);Assert.That(a.Remembers("fire"),Is.True);
        }
        [Test] public void DelayedMercyUsesAcquisitionAgeAndForgettingRestartsIt()
        {
            var a=new SliceArchive();for(int i=0;i<7;i++)a.Transfer("return");a.Transfer("mercy");
            Assert.That(a.MatureMercy,Is.False);a.Transfer("mercy");Assert.That(a.MatureMercy,Is.False);
            a.Transfer("return");Assert.That(a.MatureMercy,Is.True);
            a.Transfer("fire");Assert.That(a.MatureMercy,Is.False);a.Forget("fire");Assert.That(a.MatureMercy,Is.True);
            a.Forget("mercy");a.Transfer("mercy");Assert.That(a.Age("mercy"),Is.EqualTo(1));Assert.That(a.MatureMercy,Is.False);
        }
        [Test] public void ForgettingEquippedMemoryClearsCheckpointButPreservesKinshipHistory()
        {
            var a=new SliceArchive();a.Transfer("respect");a.checkpoint=new SliceCheckpoint();
            Assert.That(a.Forget("respect"),Is.True);Assert.That(a.active,Is.EqualTo("return"));Assert.That(a.checkpoint,Is.Null);Assert.That(a.kinship,Is.EqualTo(1));Assert.That(a.Valid(),Is.True);
        }
        [Test] public void LegacyAndPendingExtractionSurviveDiskRoundTrip()
        {
            var dir=Path.Combine(Application.temporaryCachePath,"phase5-"+Guid.NewGuid());var path=Path.Combine(dir,"save.json");Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(path,"{\"version\":1,\"loop\":8,\"active\":\"mercy\",\"memories\":[\"mercy\"]}");
                var a=SliceMemoryStore.Load(path,out var warning);Assert.That(warning,Is.Null);Assert.That(a.Age("mercy"),Is.Zero);
                a.pending.AddRange(new[]{"respect","betrayal"});SliceMemoryStore.Save(path,a);
                var b=SliceMemoryStore.Load(path,out warning);Assert.That(b.pending,Is.EquivalentTo(a.pending));Assert.That(b.Age("mercy"),Is.Zero);
                b.Transfer("respect");SliceMemoryStore.Save(path,b);Assert.That(SliceMemoryStore.Load(path,out _).pending,Is.Empty);
            }
            finally{Directory.Delete(dir,true);}
        }
        [Test] public void MemoryPassageConflictsDoNotRemoveMechanicalShortcut()
        {
            var a=new SliceArchive();a.Transfer("defiance");var gate=new WorldLink{requires="breath-link"};var flags=new System.Collections.Generic.HashSet<string>();
            Assert.That(gate.Open(a,flags),Is.True);a.Transfer("obedience");Assert.That(gate.Open(a,flags),Is.False);
            flags.Add("breath-link");Assert.That(gate.Open(a,flags),Is.True);a.Forget("obedience");Assert.That(MemoryWorld.BreathingPassage(a),Is.True);
        }
        [Test] public void CheckpointAcceptsDecisionsAndRoomMendUseButRejectsUnknowns()
        {
            var a=new SliceArchive();a.Transfer("obedience");var cp=new SliceCheckpoint{loop=a.loop,archive=SliceCheckpoint.Signature(a),room="wake",bench="rest-wake",resonance=2};cp.visited.Add("wake");
            cp.decisions.Add("sacrifice");cp.sacrificeUsed.Add("cradle");cp.enemies.Add(new SliceEnemySnapshot{id="king/boss",hp=48});
            Assert.That(cp.Valid(a),Is.True);cp.decisions.Add("ending");Assert.That(cp.Valid(a),Is.False);
        }
    }
}
