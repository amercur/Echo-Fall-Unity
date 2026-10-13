using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoFall.Movement.Tests
{
    public sealed class LateGamePlayTests
    {
        SliceSession S=>SliceSession.Instance;string directory;
        [UnitySetUp] public IEnumerator Setup()
        {
            directory=Path.Combine(Application.temporaryCachePath,"phase6-"+System.Guid.NewGuid());SliceSession.SavePathOverride=Path.Combine(directory,"save.json");SliceSession.EphemeralSave=true;
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();
        }
        [UnityTearDown] public IEnumerator Cleanup(){yield return SceneManager.LoadSceneAsync("MovementLab");SliceSession.SavePathOverride=null;SliceSession.EphemeralSave=false;if(Directory.Exists(directory))Directory.Delete(directory,true);}
        IEnumerator Ready(){float end=Time.realtimeSinceStartup+15;while(S==null||!S.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}foreach(var e in Object.FindObjectsByType<SliceEnemy>())e.enabled=false;}
        IEnumerator Rest()
        {
            var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");S.motor.EnterRoom((Vector2)bench.transform.position-Vector2.up*.25f);
            yield return new WaitForSeconds(.15f);S.Interact(bench);yield return new WaitForSeconds(.6f);Assert.That(S.Archive.checkpoint,Is.Not.Null);
        }
        void BeginMother(){S.motor.automaticSimulation=false;S.motor.Teleport(new Vector2(5,.02f));SliceMother.Active.Tick(.01f);Assert.That(SliceMother.Active.Started,Is.True);}
        [UnityTest] public IEnumerator MotherPatternsPhaseRecallAndVictoryUseSharedCombat()
        {
            S.Archive.Transfer("fire");yield return S.LoadRoom("mother","default");yield return Ready();var m=SliceMother.Active;Assert.That(m.frames.Length,Is.EqualTo(8));BeginMother();
            Assert.That(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="gate").LockReason(S),Is.Not.Null);
            m.Tick(1.3f);Assert.That(m.State,Is.EqualTo("tell"));m.Tick(.91f);Assert.That(Object.FindObjectsByType<SliceProjectile>().Length,Is.EqualTo(4));SliceProjectile.ClearAll();
            m.Tick(2.3f);m.Tick(.91f);Assert.That(m.RecallIndex,Is.EqualTo(1));var e=Object.FindAnyObjectByType<MemoryEcho>();Assert.That(e.identity,Is.EqualTo("fire"));Assert.That(e.Persistent,Is.False);SliceProjectile.ClearAll();
            m.Tick(2.3f);Assert.That(m.Red,Is.True);m.Tick(.91f);Assert.That(Object.FindObjectsByType<SliceProjectile>().All(p=>p.Heavy),Is.True);
            m.Hit(25,.55f,0,true);m.Tick(.05f);m.Tick(.01f);Assert.That(m.Stage,Is.EqualTo(2));
            S.combat.PayIntegrity(3);m.Hit(100,.55f,0,true);int hp=S.combat.Integrity;Assert.That(S.Flags,Does.Contain("mother"));Assert.That(S.Flags,Does.Contain("garden-path"));Assert.That(S.Defeated,Does.Contain("mother/boss"));
            Assert.That(Object.FindObjectsByType<MemoryEcho>().Any(x=>x.recalled),Is.False);Assert.That(Object.FindObjectsByType<SliceProjectile>(),Is.Empty);m.Hit(100);Assert.That(S.combat.Integrity,Is.EqualTo(hp));
            yield return S.LoadRoom("garden","default");yield return Ready();yield return S.LoadRoom("mother","east");yield return Ready();Assert.That(SliceMother.Active,Is.Null);
        }
        [UnityTest] public IEnumerator SacrificeRecallKinshipAndMatureMercyChangeMother()
        {
            foreach(var id in new[]{"mercy","respect","sacrifice"})S.Archive.Transfer(id);
            yield return S.LoadRoom("mother","default");yield return Ready();var m=SliceMother.Active;Assert.That(m.MaxHealth,Is.EqualTo(38));BeginMother();m.Tick(2.1f);Assert.That(m.AidStrikes,Is.EqualTo(1));
            var heal=m.Recall();yield return null;heal.enabled=false;S.motor.Teleport((Vector2)heal.transform.position+Vector2.left*.7f);float before=m.hp;heal.Tick(2.1f);Assert.That(m.hp,Is.GreaterThan(before));
            var kin=m.Recall();yield return null;kin.enabled=false;S.motor.Teleport((Vector2)kin.transform.position+Vector2.left*.7f);kin.Tick(1.4f);Assert.That(kin.Refused,Is.True);Assert.That(S.Flags.Contains("betrayal-guard"),Is.False);
        }
        [UnityTest] public IEnumerator RedProjectilesRequireChargedCounterAndReflectionsDamageMother()
        {
            yield return S.LoadRoom("mother","default");yield return Ready();BeginMother();var m=SliceMother.Active;
            S.combat.ClearTransient();S.combat.BeginGuard();var p=SliceProjectile.Spawn(S.combat.HitRect.center+Vector2.right*.1f,Vector2.left,m,1,true);p.Tick(.1f);Assert.That(p.Friendly,Is.False);Assert.That(S.combat.Integrity,Is.EqualTo(5));
            S.combat.ClearTransient();S.combat.ReleaseCounter(.5f);p=SliceProjectile.Spawn(S.combat.HitRect.center+Vector2.right*.1f,Vector2.left,m,1,true);p.Tick(.1f);Assert.That(p.Reflected,Is.True);
            float hp=m.hp;for(int i=0;i<120 && p!=null && p.gameObject.activeSelf;i++)p.Tick(.01f);Assert.That(m.hp,Is.LessThan(hp));
        }
        [UnityTest] public IEnumerator MotherCheckpointRestoresPhaseAndDeathTransferResetsEncounter()
        {
            SliceSession.EphemeralSave=false;S.Archive.Transfer("sacrifice");yield return S.LoadRoom("mother","default");yield return Ready();BeginMother();var m=SliceMother.Active;m.Hit(22,.55f,0,true);m.Tick(.05f);m.Tick(.01f);m.Recall();
            yield return S.LoadRoom("cradle","default");yield return Ready();yield return Rest();Assert.That(S.Archive.checkpoint.enemies.All(e=>!e.id.StartsWith("recall/")),Is.True);
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();yield return S.LoadRoom("mother","default");yield return Ready();m=SliceMother.Active;Assert.That(m.Stage,Is.EqualTo(2));Assert.That(m.hp,Is.EqualTo(16));Assert.That(m.RecallIndex,Is.EqualTo(1));
            BeginMother();S.combat.PayIntegrity(6);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));S.ChooseOption(0);yield return Ready();Assert.That(S.MotherStage,Is.Zero);Assert.That(S.Flags.Contains("mother"),Is.False);
            yield return S.LoadRoom("mother","default");yield return Ready();Assert.That(SliceMother.Active.hp,Is.EqualTo(38));
        }
        [UnityTest] public IEnumerator ChildRemainsNonCombatAndChoirRequiresActualProgression()
        {
            yield return S.LoadRoom("garden","default");yield return Ready();Assert.That(S.Room.GetComponentsInChildren<SliceEnemy>(),Is.Empty);S.ChildDialogue();S.ChooseOption(0);Assert.That(S.ChildStage,Is.Zero);S.ChildDialogue();S.ChooseOption(1);S.ChooseOption(2);S.ChooseOption(0);Assert.That(S.Flags,Does.Contain("child"));
            yield return S.LoadRoom("choir","default");yield return Ready();S.ChoirDialogue();Assert.That(S.ModalTitle,Is.EqualTo("AN UNFINISHED CHOIR"));Assert.That(S.CompleteEnding("RESET"),Is.False);
            S.SetScreen(SliceScreen.Playing);S.Flags.Add("king");S.Flags.Add("mother");S.ChoirDialogue();Assert.That(S.OptionLabels,Does.Not.Contain("SHARE THE CHOICE"));S.SetScreen(SliceScreen.Playing);
            S.Archive.Transfer("respect");S.Archive.Transfer("mercy");S.ChoirDialogue();Assert.That(S.OptionLabels,Does.Contain("SHARE THE CHOICE"));
        }
        [UnityTest] public IEnumerator EndingReloadCarryAndSecondOutcomeKeepDiscoveryHistory()
        {
            SliceSession.EphemeralSave=false;S.Archive.Transfer("respect");S.Archive.Transfer("sacrifice");S.RecordDecision("defiance");S.Flags.UnionWith(new[]{"king","mother","child"});
            yield return S.LoadRoom("choir","default");yield return Ready();Assert.That(S.CompleteEnding("SHARE"),Is.True);Assert.That(S.Archive.checkpoint,Is.Null);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Ending));
            yield return SceneManager.LoadSceneAsync("WakeSlice");float end=Time.realtimeSinceStartup+15;while(S==null||S.Screen!=SliceScreen.Ending){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
            Assert.That(S.Archive.endings,Does.Contain("SHARE"));S.ChooseOption(0);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));S.ChooseOption(0);yield return Ready();Assert.That(S.Archive.Remembers("defiance"),Is.True);Assert.That(S.Flags,Is.Empty);
            S.Flags.UnionWith(new[]{"king","mother","child"});yield return S.LoadRoom("choir","default");yield return Ready();Assert.That(S.CompleteEnding("BREAK"),Is.True);Assert.That(S.Archive.endings,Is.EquivalentTo(new[]{"SHARE","BREAK"}));Assert.That(SliceMemoryStore.Load(SliceSession.SavePathOverride,out _).outcomes.Count,Is.EqualTo(2));
        }
    }
}
