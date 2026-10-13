using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace EchoFall.Movement.Tests
{
    public sealed class LateGameRuleTests
    {
        static HashSet<string> Complete()=>new HashSet<string>{"king","mother","child"};
        [TestCase("RESET")][TestCase("BREAK")][TestCase("REMEMBER")]
        public void MainEndingsRequireEveryEncounter(string id)
        {
            var a=new SliceArchive();var flags=Complete();Assert.That(EndingRules.Eligible(id,a,flags),Is.True);
            foreach(string flag in new[]{"king","mother","child"}){flags.Remove(flag);Assert.That(EndingRules.Eligible(id,a,flags),Is.False);flags.Add(flag);}
        }
        [Test] public void ShareRequiresRetainedCareAndExtractedKinshipNotOneFinalChoice()
        {
            var a=new SliceArchive();a.Transfer("respect");Assert.That(EndingRules.Eligible("SHARE",a,Complete()),Is.False);
            a.Transfer("mercy");Assert.That(EndingRules.Eligible("SHARE",a,Complete()),Is.True);
            a.kinship=0;Assert.That(EndingRules.Eligible("SHARE",a,Complete()),Is.False);a.kinship=1;
            a.Forget("respect");Assert.That(EndingRules.Eligible("SHARE",a,Complete()),Is.False);
            a.Transfer("respect");a.Forget("mercy");a.Transfer("sacrifice");Assert.That(EndingRules.Eligible("SHARE",a,Complete()),Is.True);
        }
        [Test] public void ContradictoryHistoryKeepsItsConsequencesInSharedEnding()
        {
            var a=new SliceArchive();foreach(var id in new[]{"respect","mercy","fire","betrayal"})a.Transfer(id);
            Assert.That(EndingRules.Eligible("SHARE",a,Complete()),Is.True,"Original eligibility does not erase or veto contradictory history.");
            string callback=EndingRules.Callback(a,new HashSet<string>(),new HashSet<string>());
            Assert.That(callback,Does.Contain("does not answer"));Assert.That(callback,Does.Contain("absence"));
        }
        [Test] public void EndingDiscoveryAndSnapshotSurviveSaveTransferAndForgetting()
        {
            var dir=Path.Combine(Application.temporaryCachePath,"phase6-"+Guid.NewGuid());var path=Path.Combine(dir,"save.json");
            try
            {
                var a=new SliceArchive();a.Transfer("respect");a.endings.Add("RESET");a.currentEnding="RESET";a.pending.Add("return");
                a.outcomes.Add(new EndingRecord{id="RESET",loop=a.loop,epilogue="The Keeper remembers.",memories=new List<string>{"respect"},flags=new List<string>(Complete())});
                SliceMemoryStore.Save(path,a);var b=SliceMemoryStore.Load(path,out var warning);Assert.That(warning,Is.Null);Assert.That(b.currentEnding,Is.EqualTo("RESET"));
                b.Transfer("return");b.Forget("respect");SliceMemoryStore.Save(path,b);b=SliceMemoryStore.Load(path,out _);
                Assert.That(b.endings,Is.EquivalentTo(new[]{"RESET"}));Assert.That(b.outcomes[0].memories,Does.Contain("respect"));Assert.That(b.currentEnding,Is.Null.Or.Empty);
                b.currentEnding="SHARE";Assert.That(b.Valid(),Is.False);
            }
            finally{if(Directory.Exists(dir))Directory.Delete(dir,true);}
        }
        [Test] public void MotherCheckpointRejectsInvalidRecallMetadataAndTransientActors()
        {
            var a=new SliceArchive();var cp=new SliceCheckpoint{loop=1,archive="",room="cradle",bench="cradle-rest",resonance=2,motherStage=2,motherRecall=4,motherAttack=6,motherMax=38};cp.visited.Add("cradle");cp.enemies.Add(new SliceEnemySnapshot{id="mother/boss",hp=18});
            Assert.That(cp.Valid(a),Is.True);cp.motherMax=float.NaN;Assert.That(cp.Valid(a),Is.False);cp.motherMax=38;
            cp.enemies.Add(new SliceEnemySnapshot{id="recall/1",hp=6});Assert.That(cp.Valid(a),Is.False);
        }
    }
}
