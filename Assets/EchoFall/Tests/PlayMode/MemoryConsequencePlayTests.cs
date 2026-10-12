using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoFall.Movement.Tests
{
    public sealed class MemoryConsequencePlayTests
    {
        SliceSession S=>SliceSession.Instance;
        string directory;
        [UnitySetUp] public IEnumerator Setup()
        {
            directory=Path.Combine(Application.temporaryCachePath,"phase5-"+System.Guid.NewGuid());SliceSession.SavePathOverride=Path.Combine(directory,"save.json");SliceSession.EphemeralSave=true;
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("MovementLab");SliceSession.SavePathOverride=null;SliceSession.EphemeralSave=false;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Ready()
        {
            float end=Time.realtimeSinceStartup+15;
            while(S==null||!S.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
            foreach(var e in Object.FindObjectsByType<SliceEnemy>())e.enabled=false;
        }
        SliceInteraction Item(string id)=>S.Room.GetComponentsInChildren<SliceInteraction>(true).Single(i=>i.id==id);
        [UnityTest] public IEnumerator CheckpointRestoresNewDecisionsConsumptionAndRoomMendUse()
        {
            SliceSession.EphemeralSave=false;S.Archive.Transfer("sacrifice");S.RecordDecision("respect");S.RecordDecision("betrayal");S.Consumed.Add("keeper");
            S.combat.PayIntegrity(3);S.combat.MemoryAbility();Assert.That(S.SacrificeUsed,Does.Contain("wake"));
            var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");
            S.motor.EnterRoom((Vector2)bench.transform.position-Vector2.up*.25f);yield return new WaitForSeconds(.15f);S.Interact(bench);yield return new WaitForSeconds(.6f);
            Assert.That(S.Archive.checkpoint,Is.Not.Null);Assert.That(S.Archive.checkpoint.Valid(S.Archive),Is.True);
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();
            Assert.That(S.Decisions,Is.EquivalentTo(new[]{"respect","betrayal"}));Assert.That(S.Consumed,Does.Contain("keeper"));Assert.That(S.SacrificeUsed,Does.Contain("wake"));
            S.combat.PayIntegrity(2);S.combat.MemoryAbility();Assert.That(S.combat.Integrity,Is.EqualTo(4));
        }
        [UnityTest] public IEnumerator FailedReplacementDoesNotForgetOrAdvanceLoop()
        {
            foreach(var id in new[]{"return","mercy","fire","defiance","obedience"})S.Archive.Transfer(id);
            S.RecordDecision("respect");S.EndRun(false);S.CommitTransfer("respect");S.ChooseOption(2);
            Directory.CreateDirectory(directory);File.WriteAllText(Path.Combine(directory,"blocked"),"blocked");SliceSession.SavePathOverride=Path.Combine(directory,"blocked","save.json");SliceSession.EphemeralSave=false;
            int loop=S.Archive.loop;S.ChooseOption(0);Assert.That(S.Archive.loop,Is.EqualTo(loop));Assert.That(S.Archive.Remembers("fire"),Is.True);Assert.That(S.Archive.Remembers("respect"),Is.False);
            SliceSession.SavePathOverride=Path.Combine(directory,"save.json");S.ChooseOption(0);yield return Ready();Assert.That(S.Archive.Remembers("respect"),Is.True);Assert.That(S.Archive.Remembers("fire"),Is.False);
        }
        [UnityTest] public IEnumerator DecisionsSurviveRestartAtExtractionAndOnlySelectionCrosses()
        {
            SliceSession.EphemeralSave=false;
            S.MakeDecision("mercy");yield return S.LoadRoom("cradle","default");yield return Ready();
            S.Interact(Item("keeper"));S.ChooseOption(0);Assert.That(S.Decisions,Does.Contain("sacrifice"));
            S.EndRun(false);Assert.That(S.OptionLabels.Count,Is.EqualTo(2));
            yield return SceneManager.LoadSceneAsync("WakeSlice");
            float end=Time.realtimeSinceStartup+15;while(S==null||S.Screen!=SliceScreen.Transfer){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
            Assert.That(S.OptionLabels.Count,Is.EqualTo(2));S.CommitTransfer("sacrifice");yield return Ready();
            Assert.That(S.Archive.Remembers("sacrifice"),Is.True);Assert.That(S.Archive.Remembers("mercy"),Is.False);Assert.That(S.Archive.pending,Is.Empty);
        }
        [UnityTest] public IEnumerator FullArchiveReplacementWaitsForConfirmation()
        {
            foreach(var id in new[]{"return","mercy","fire","defiance","obedience"})S.Archive.Transfer(id);
            S.RecordDecision("respect");S.EndRun(false);S.CommitTransfer("respect");Assert.That(S.OptionLabels.Count,Is.EqualTo(6));
            S.ChooseOption(2);Assert.That(S.Archive.Remembers("fire"),Is.True);S.ChooseOption(0);yield return Ready();
            Assert.That(S.Archive.memories.Count,Is.EqualTo(5));Assert.That(S.Archive.Remembers("fire"),Is.False);Assert.That(S.Archive.RootsOpen,Is.True);
        }
        [UnityTest] public IEnumerator ForgettingRebuildsWorldAndRemovesGeneratedBridge()
        {
            S.Archive.Transfer("obedience");yield return S.LoadRoom("cistern","default");yield return Ready();
            Assert.That(S.Room.transform.Find("Remembered world/The ordered bridge"),Is.Not.Null);
            yield return S.LoadRoom("wake","default");yield return Ready();S.motor.Teleport(new Vector2(1,.12f));
            Assert.That(S.ForgetMemory("obedience"),Is.True);yield return Ready();yield return S.LoadRoom("cistern","default");yield return Ready();
            Assert.That(S.Room.transform.Find("Remembered world/The ordered bridge"),Is.Null);Assert.That(S.Archive.active,Is.EqualTo("return"));
        }
        [UnityTest] public IEnumerator KeeperAndEchoReactToRememberedHistory()
        {
            S.Archive.Transfer("betrayal");yield return S.LoadRoom("cradle","default");yield return Ready();S.Interact(Item("keeper"));
            Assert.That(S.ModalBody,Does.Contain("nothing here"));Assert.That(S.OptionLabels.Count,Is.EqualTo(1));S.ChooseOption(0);
            S.Archive.Transfer("respect");yield return S.LoadRoom("archive","default");yield return Ready();
            var echo=Object.FindAnyObjectByType<MemoryEcho>();Assert.That(echo,Is.Not.Null);S.motor.Teleport(echo.transform.position+Vector3.left);echo.Tick(1.4f);
            Assert.That(echo.Refused,Is.True);Assert.That(S.Defeated,Does.Contain("archive/betrayal-guard"));
            yield return S.LoadRoom("wake","default");yield return Ready();yield return S.LoadRoom("archive","default");yield return Ready();Assert.That(Object.FindAnyObjectByType<MemoryEcho>(),Is.Null);
        }
        [UnityTest] public IEnumerator MercyAidWaitsForThirdTransferAndOrderStrengthensKing()
        {
            S.Archive.Transfer("mercy");yield return S.LoadRoom("king","default");yield return Ready();var king=SliceKing.Active;S.motor.Teleport(new Vector2(3,.12f));king.Tick(2.1f);Assert.That(king.AidStrikes,Is.Zero);
            S.Archive.Transfer("return");S.Archive.Transfer("obedience");S.EnemyHealth.Clear();yield return S.LoadRoom("wake","default",true);yield return Ready();yield return S.LoadRoom("king","default");yield return Ready();
            king=SliceKing.Active;Assert.That(king.MaxHealth,Is.EqualTo(48));S.motor.Teleport(new Vector2(3,.12f));king.Tick(2.1f);Assert.That(king.AidStrikes,Is.EqualTo(1));Assert.That(king.hp,Is.EqualTo(45.5f));
        }
        [UnityTest] public IEnumerator SacrificeMendCannotBeResetByReentryOrLoadout()
        {
            S.Archive.Transfer("sacrifice");S.combat.PayIntegrity(3);S.combat.MemoryAbility();Assert.That(S.combat.Integrity,Is.EqualTo(5));
            S.combat.ClearTransient();S.combat.MemoryAbility();Assert.That(S.combat.Integrity,Is.EqualTo(5));
            yield return S.LoadRoom("belfry","default");yield return Ready();S.combat.MemoryAbility();Assert.That(S.combat.Integrity,Is.EqualTo(6));
            yield return S.LoadRoom("wake","default");yield return Ready();S.combat.PayIntegrity(2);S.combat.MemoryAbility();Assert.That(S.combat.Integrity,Is.EqualTo(4));
        }
        [UnityTest] public IEnumerator DefianceAirJumpAndBlinkRemainCollisionBound()
        {
            S.Archive.Transfer("defiance");S.motor.automaticSimulation=false;S.motor.Teleport(new Vector2(2,2));
            S.motor.Simulate(.01f,new MovementCommand{jumpPressed=true});Assert.That(S.motor.DoubleJumpUsed,Is.True);Assert.That(S.motor.Velocity.y,Is.GreaterThan(5));
            for(int i=0;i<20;i++)S.motor.Simulate(.01f,default);float velocity=S.motor.Velocity.y;
            S.motor.Simulate(.01f,new MovementCommand{jumpPressed=true});Assert.That(S.motor.Velocity.y,Is.LessThan(velocity));
            var wall=new GameObject("Blink test wall");wall.layer=LayerMask.NameToLayer("EchoSolid");wall.transform.position=new Vector2(2.8f,2);wall.AddComponent<BoxCollider2D>().size=new Vector2(.2f,6);Physics2D.SyncTransforms();
            S.combat.MemoryAbility();Assert.That(S.motor.Position.x,Is.LessThan(2.7f));Object.Destroy(wall);yield return null;
        }
    }
}
