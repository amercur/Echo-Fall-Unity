using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace EchoFall.Movement.Tests
{
    public sealed class SliceRuleTests
    {
        static SliceCheckpoint Checkpoint(SliceArchive archive) => new SliceCheckpoint
        {
            loop = archive.loop, archive = SliceCheckpoint.Signature(archive), room = "wake", bench = "rest-wake",
            resonance = 1, visited = new System.Collections.Generic.List<string> { "wake" }
        };
        [Test] public void LoadoutChangesAbilityWithoutRemovingMemoryConsequencesOrCheckpoint()
        {
            var archive = new SliceArchive(); archive.Transfer("mercy"); archive.Transfer("return");
            archive.checkpoint = Checkpoint(archive);
            archive.Equip("mercy"); Assert.That(archive.RootsOpen, Is.True);
            Assert.That(archive.checkpoint.Valid(archive), Is.True);
            archive.Equip("return"); Assert.That(archive.RootsOpen, Is.True);
            Assert.Throws<ArgumentException>(() => archive.Equip("fire"));
            archive.Transfer("fire"); Assert.That(archive.checkpoint, Is.Null);
            archive.Equip("mercy"); Assert.That(archive.RootsOpen, Is.False, "Retained EMBER still closes roots.");
        }
        [Test] public void CheckpointRejectsStaleArchiveUnknownIdsAndInvalidEnemyHealth()
        {
            var archive = new SliceArchive(); var cp = Checkpoint(archive);
            Assert.That(cp.Valid(archive), Is.True);
            cp.loop++; Assert.That(cp.Valid(archive), Is.False); cp.loop--;
            cp.bench = "missing"; Assert.That(cp.Valid(archive), Is.False); cp.bench = "rest-wake";
            cp.visited.Add("king"); Assert.That(cp.Valid(archive), Is.False); cp.visited.Remove("king");
            var enemy = new SliceEnemySnapshot { id = "procession/enemy-1", hp = 8 }; cp.enemies.Add(enemy);
            Assert.That(cp.Valid(archive), Is.True);
            foreach (float hp in new[] { float.NaN, float.PositiveInfinity, -1, 0, 11 })
            { enemy.hp = hp; Assert.That(cp.Valid(archive), Is.False); }
            enemy.hp = 8; cp.defeated.Add(enemy.id); Assert.That(cp.Valid(archive), Is.False); cp.defeated.Clear();
            cp.enemies.Add(enemy); Assert.That(cp.Valid(archive), Is.False); cp.enemies.RemoveAt(1);
            archive.Transfer("mercy"); Assert.That(cp.Valid(archive), Is.False);
        }
        [Test] public void InvalidCheckpointDoesNotDiscardArchiveAndOldSaveStillLoads()
        {
            string directory=Path.Combine(Application.temporaryCachePath,"slice-test-"+Guid.NewGuid());
            string path=Path.Combine(directory,"save.json");
            try
            {
                var archive = new SliceArchive(); archive.Transfer("mercy"); SliceMemoryStore.Save(path,archive);
                Assert.That(SliceMemoryStore.Load(path,out _).checkpoint, Is.Null);
                archive.checkpoint = Checkpoint(archive); SliceMemoryStore.Save(path,archive);
                Assert.That(SliceMemoryStore.Load(path,out _).checkpoint.bench, Is.EqualTo("rest-wake"));
                archive.checkpoint.room = "removed-room";
                Assert.Throws<InvalidDataException>(() => SliceMemoryStore.Save(path,archive));
                string corrupt = JsonUtility.ToJson(archive); File.WriteAllText(path,corrupt);
                var copy = SliceMemoryStore.Load(path,out var warning);
                Assert.That(copy.RootsOpen,Is.True); Assert.That(copy.checkpoint,Is.Null); Assert.That(warning,Is.Not.Null);
                Assert.That(File.ReadAllText(path),Is.EqualTo(corrupt));
                archive.checkpoint = Checkpoint(archive); SliceMemoryStore.Save(path,archive);
                archive.checkpoint = null; SliceMemoryStore.Save(path,archive);
                File.WriteAllText(path,"not json");
                copy = SliceMemoryStore.Load(path,out warning);
                Assert.That(copy.RootsOpen,Is.True); Assert.That(copy.checkpoint,Is.Null,
                    "Backup recovery must not resurrect a checkpoint cleared by death or transfer.");
            }
            finally { if(Directory.Exists(directory))Directory.Delete(directory,true); }
        }
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
