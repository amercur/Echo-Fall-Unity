using System.Collections;
using System.Linq;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoFall.Movement.Tests
{
    public sealed class SlicePlayTests
    {
        Keyboard keyboard;
        string saveDirectory;
        InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        InputSettings.EditorInputBehaviorInPlayMode editorInput;
#endif
        SliceSession S=>SliceSession.Instance;
        [UnitySetUp] public IEnumerator Setup()
        {
            SliceSession.EphemeralSave=true;
            saveDirectory=Path.Combine(Application.temporaryCachePath,"slice-play-"+System.Guid.NewGuid());
            SliceSession.SavePathOverride=Path.Combine(saveDirectory,"save.json");
            background=InputSystem.settings.backgroundBehavior; InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorInput=InputSystem.settings.editorInputBehaviorInPlayMode; InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard=InputSystem.AddDevice<Keyboard>();
            yield return SceneManager.LoadSceneAsync("WakeSlice");
            yield return Ready();
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            InputSystem.RemoveDevice(keyboard); InputSystem.settings.backgroundBehavior=background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
#endif
            SliceSession.EphemeralSave=false;
            yield return SceneManager.LoadSceneAsync("MovementLab");
            SliceSession.SavePathOverride=null;
            if(Directory.Exists(saveDirectory))Directory.Delete(saveDirectory,true);
        }
        IEnumerator Ready()
        {
            float deadline=Time.realtimeSinceStartup+10;
            while(S==null || !S.Playing) { Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline),"Room transition timed out"); yield return null; }
            yield return new WaitForSeconds(.08f);
        }
        void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); InputSystem.Update(); }
        [UnityTest] public IEnumerator PlayerFeedbackWorksInEveryRoomAndNeverBlocksActions()
        {
            foreach(string room in new[]{"wake","belfry","cistern","archive","procession"})
            {
                yield return S.LoadRoom(room,"default");yield return Ready();
                var visual=S.motor.GetComponentInChildren<PlayerVisual>(); var renderer=visual.GetComponent<SpriteRenderer>();
                S.combat.Strike(true,0);yield return null;yield return null;
                Assert.That(visual.Pose,Is.EqualTo(WandererPose.Attack));Assert.That(visual.FrameIndex,Is.InRange(22,29));
                visual.Impact(.05f,.05f);Assert.That(visual.PoseHoldRemaining,Is.GreaterThan(0));
                S.combat.Strike(true,1);
                Assert.That(S.combat.Combo,Is.EqualTo(3));Assert.That(visual.PoseHoldRemaining,Is.Zero,"New attack interrupts visual hold immediately.");
                Assert.That(Time.timeScale,Is.EqualTo(1));
                S.combat.Rest();S.combat.BeginGuard();yield return null;yield return null;
                Assert.That(visual.Pose,Is.EqualTo(WandererPose.Guard));
                S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null);yield return null;yield return null;
                Assert.That(visual.Pose,Is.EqualTo(WandererPose.Deflect));
                S.combat.Rest();S.combat.Hurt(2,S.motor.Position+Vector2.right,true,null,true);yield return null;yield return null;
                Assert.That(renderer.color.g,Is.LessThan(1));Assert.That(visual.Pose,Is.EqualTo(WandererPose.Hurt));
                S.combat.Rest();S.motor.Simulate(MovementTuning.Step,new MovementCommand{move=1,dashPressed=true});
                yield return null;yield return null;
                Assert.That(S.motor.Dashing,Is.True);Assert.That(visual.Pose,Is.EqualTo(WandererPose.Dash));
                S.SetScreen(SliceScreen.Pause);yield return new WaitForSeconds(.2f);Assert.That(visual.LiveGhosts,Is.Zero);
                S.SetScreen(SliceScreen.Playing);
            }
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator TransferAndDeathAnimateWithoutDelayingExtractionOrNextLife()
        {
            var visual=S.motor.GetComponentInChildren<PlayerVisual>();var renderer=visual.GetComponent<SpriteRenderer>();
            S.EndRun(false);yield return null;yield return null;
            Assert.That(visual.Pose,Is.EqualTo(WandererPose.Transfer));
            yield return new WaitForSeconds(.2f);Assert.That(renderer.color.a,Is.LessThan(1));
            S.ChooseOption(0);yield return Ready();Assert.That(renderer.color.a,Is.EqualTo(1));
            S.combat.PayIntegrity(6);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));
            yield return null;yield return null;Assert.That(visual.Pose,Is.EqualTo(WandererPose.Death));
            S.ChooseOption(0);yield return Ready();Assert.That(renderer.color.a,Is.EqualTo(1));
            Assert.That(visual.PoseHoldRemaining,Is.Zero);Assert.That(visual.LiveGhosts,Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator StrongHitFeedbackLeavesSimulationAndCameraFollowUntouched()
        {
            var visual=S.motor.GetComponentInChildren<PlayerVisual>();
            var enemy=Object.FindAnyObjectByType<SliceEnemy>();enemy.enabled=false;
            S.motor.automaticSimulation=false;
            S.motor.EnterRoom((Vector2)enemy.transform.position-Vector2.right*.5f);
            yield return null;yield return null;
            var camera=Camera.main;var impulse=camera.GetComponent<PlayerCameraImpulse>();
            S.follow.SnapToTarget();S.follow.enabled=false;
            Vector3 cameraPosition=camera.transform.position;
            int impacts=visual.StrongImpactCount;float hp=enemy.hp;
            S.combat.Strike(true,0);
            Assert.That(enemy.hp,Is.LessThan(hp));Assert.That(visual.StrongImpactCount,Is.EqualTo(impacts+1));
            Assert.That(visual.PoseHoldRemaining,Is.GreaterThan(0));
            yield return null;yield return null;
            Assert.That(impulse.Offset.magnitude,Is.GreaterThan(0).And.LessThan(.066f));
            Assert.That(camera.transform.position,Is.EqualTo(cameraPosition));
            Vector2 before=S.motor.Position;
            S.motor.Simulate(MovementTuning.Step,new MovementCommand{move=1,dashPressed=true});
            Assert.That(S.motor.Position.x,Is.GreaterThan(before.x));
            yield return null;yield return null;
            Assert.That(visual.Pose,Is.EqualTo(WandererPose.Dash));Assert.That(visual.PoseHoldRemaining,Is.Zero);
            yield return new WaitForSeconds(.2f);
            Assert.That(impulse.Offset,Is.EqualTo(Vector2.zero));Assert.That(camera.transform.position,Is.EqualTo(cameraPosition));
            Assert.That(Time.timeScale,Is.EqualTo(1));S.follow.enabled=true;
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator NativeInputMovementAttackAndPauseAreOperational()
        {
            float x=S.motor.Position.x;
            Keys(Key.D); yield return new WaitForSeconds(.35f); Keys();
            Assert.That(S.motor.Position.x,Is.GreaterThan(x+.5f));
            Keys(Key.Space); yield return new WaitForSeconds(.12f); Keys();
            Assert.That(S.motor.Position.y,Is.GreaterThan(.3f));
            Keys(Key.J); yield return null; Keys(); yield return null;
            Assert.That(S.combat.Combo,Is.EqualTo(1));
            Keys(Key.Escape); yield return null; Keys(); yield return null;
            Assert.That(S.Screen,Is.EqualTo(SliceScreen.Pause)); var position=S.motor.Position;
            yield return new WaitForSeconds(.15f); Assert.That(S.motor.Position,Is.EqualTo(position));
            S.ChooseOption(0); Assert.That(S.Playing,Is.True); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator FiveRoomsAndEveryOpenGateHaveValidSpawnsAndCollision()
        {
            foreach(string id in new[]{"wake","procession","belfry","archive","cistern"})
            {
                yield return S.LoadRoom(id,"default"); yield return Ready();
                Assert.That(S.Room.id,Is.EqualTo(id)); Assert.That(S.motor.roomBounds,Is.EqualTo(S.Room.bounds));
                Assert.That(S.Room.GetComponentsInChildren<BoxCollider2D>().Length,Is.GreaterThan(0));
                foreach(var gate in S.Room.GetComponentsInChildren<SliceInteraction>().Where(i=>i.kind=="gate"))
                {
                    Assert.That(new[]{"wake","procession","belfry","archive","cistern","king"},Does.Contain(gate.target));
                    Assert.That(gate.entry,Is.Not.Null.And.Not.Empty);
                }
            }
            Assert.That(S.Visited.Count,Is.EqualTo(5)); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator WhiteDeflectCompletesTrainingAndRedBypassesOrdinaryDash()
        {
            var warden=Object.FindAnyObjectByType<SliceEnemy>();
            S.motor.Teleport((Vector2)warden.transform.position-Vector2.right*.5f);
            S.combat.BeginGuard(); bool reflected=S.combat.Hurt(1,warden.transform.position,false,warden);
            Assert.That(reflected,Is.True); Assert.That(warden.Alive,Is.False); Assert.That(S.combat.Resonance,Is.EqualTo(1));
            yield return new WaitForSeconds(.12f); // A successful deflect grants brief invulnerability, even against red.
            S.motor.EnterRoom(new Vector2(2,.02f)); S.motor.Simulate(.01f,new MovementCommand { move=1,dashPressed=true });
            Assert.That(S.motor.Dashing,Is.True);
            S.combat.Hurt(2,S.motor.Position+Vector2.right,true,null);
            Assert.That(S.combat.Integrity,Is.EqualTo(4)); yield return null; LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator EnemyDamageBacktracksButTransferResetsRunState()
        {
            yield return S.LoadRoom("procession","west");
            var enemy=Object.FindObjectsByType<SliceEnemy>().First(e=>e.id=="procession/enemy-0");
            enemy.Damage(2,false); float health=enemy.hp;
            yield return S.LoadRoom("wake","default"); yield return S.LoadRoom("procession","west");
            enemy=Object.FindObjectsByType<SliceEnemy>().First(e=>e.id=="procession/enemy-0");
            Assert.That(enemy.hp,Is.EqualTo(health));
            enemy.Damage(100,true); Assert.That(S.Defeated,Does.Contain(enemy.id));
            S.EndRun(false); S.ChooseOption(0); yield return Ready();
            Assert.That(S.Archive.loop,Is.EqualTo(2)); Assert.That(S.Archive.active,Is.EqualTo("return"));
            yield return S.LoadRoom("procession","west");
            enemy=Object.FindObjectsByType<SliceEnemy>().First(e=>e.id=="procession/enemy-0");
            Assert.That(enemy.hp,Is.EqualTo(7)); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator MercyChangesCreatureAndCreatesActualRootCollisionNextLife()
        {
            S.MakeDecision("mercy"); Assert.That(S.combat.Integrity,Is.EqualTo(4)); Assert.That(S.Archive.RootsOpen,Is.False);
            S.EndRun(false); S.ChooseOption(0); yield return Ready();
            Assert.That(S.Archive.active,Is.EqualTo("mercy")); Assert.That(S.Room.returnedCreature.gameObject.activeSelf,Is.True);
            yield return S.LoadRoom("cistern","west");
            Assert.That(S.Room.rootBridge.gameObject.activeSelf,Is.True);
            Assert.That(S.Room.rootBridge.GetComponentsInChildren<BoxCollider2D>().Length,Is.EqualTo(1));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator FatalMercyStillTransfersExactlyOneDecisionAndEmberClosesRoots()
        {
            S.combat.PayIntegrity(5); S.MakeDecision("mercy"); Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));
            S.ChooseOption(0); yield return Ready(); Assert.That(S.Archive.memories.Count,Is.EqualTo(1));
            S.MakeDecision("fire"); S.EndRun(false); S.ChooseOption(0); yield return Ready();
            Assert.That(S.Room.emberScar.gameObject.activeSelf,Is.True); Assert.That(S.Room.returnedCreature.gameObject.activeSelf,Is.False);
            yield return S.LoadRoom("cistern","root"); Assert.That(S.Room.rootBridge.gameObject.activeSelf,Is.False);
            Assert.That(S.Archive.RootsOpen,Is.False); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ChargedGuardCountersRedAndMatureImprintConsumesResonance()
        {
            Keys(Key.F); yield return new WaitForSeconds(.55f); Keys(); yield return null;
            bool countered=S.combat.Hurt(2,S.motor.Position+Vector2.right,true,null);
            Assert.That(countered,Is.True); Assert.That(S.combat.Integrity,Is.EqualTo(6));
            yield return S.LoadRoom("procession","west");
            var e=Object.FindObjectsByType<SliceEnemy>().First(x=>x.id=="procession/enemy-1");
            e.enabled=false; S.motor.EnterRoom((Vector2)e.transform.position-Vector2.right*.8f); S.combat.Rest();
            S.combat.Imprint(); Assert.That(S.combat.Marked,Is.SameAs(e)); Assert.That(S.combat.Resonance,Is.EqualTo(0));
            yield return new WaitForSeconds(2.1f); Assert.That(e.Alive,Is.False); Assert.That(S.combat.Marked,Is.Null); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator CompletedFiveRoomRunUsesGlassThenChangesTheNextWake()
        {
            S.MakeDecision("mercy");
            yield return S.LoadRoom("procession","west");
            foreach(var enemy in Object.FindObjectsByType<SliceEnemy>())enemy.Damage(100,true);
            yield return S.LoadRoom("belfry","low");
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="bell-secret")); S.ChooseOption(0);
            yield return S.LoadRoom("cistern","west");
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="pogo-secret")); S.ChooseOption(0);
            yield return S.LoadRoom("archive","west");
            Assert.That(S.SliceComplete,Is.True);
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="slice-glass"));
            Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer)); Assert.That(S.ModalTitle,Is.EqualTo("THE GLASS ACCEPTS YOU"));
            S.ChooseOption(0); yield return Ready(); Assert.That(S.Room.id,Is.EqualTo("wake"));
            Assert.That(S.Room.returnedCreature.gameObject.activeSelf,Is.True); Assert.That(S.Visited.Count,Is.EqualTo(1)); LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator RememberedCreatureDoesNotBlockCompletionOfLaterLives()
        {
            S.Archive.Transfer("mercy");
            Assert.That(S.Decision,Is.Null); Assert.That(S.CreatureChoiceKnown,Is.True);
            foreach(string room in new[]{"wake","procession","belfry","cistern","archive"}) S.Visited.Add(room);
            for(int i=0;i<4;i++)S.Defeated.Add("procession/enemy-"+i);
            S.Consumed.Add("bell-secret"); S.Consumed.Add("pogo-secret");
            Assert.That(S.SliceComplete,Is.True); S.EndRun(true); S.ChooseOption(0); yield return Ready();
            Assert.That(S.Archive.active,Is.EqualTo("return")); Assert.That(S.Archive.RootsOpen,Is.True);
            Assert.That(S.Room.returnedCreature.gameObject.activeSelf,Is.True); LogAssert.NoUnexpectedReceived();
        }
        IEnumerator RestAtAnchor()
        {
            var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");
            S.motor.EnterRoom((Vector2)bench.transform.position-Vector2.up*.25f);
            yield return new WaitForSeconds(.12f);
            Assert.That(S.motor.Grounded,Is.True);
            S.Interact(bench); Assert.That(S.Resting,Is.True);
            yield return new WaitForSeconds(.55f);
            Assert.That(S.Archive.checkpoint,Is.Not.Null,S.CurrentMessage);
        }
        [UnityTest] public IEnumerator BenchReloadRestoresFiveRoomProgressAndLoadoutButNotLaterActions()
        {
            SliceSession.EphemeralSave=false;
            S.Archive.Transfer("mercy"); S.Archive.Transfer("return");
            S.MakeDecision("fire");
            yield return S.LoadRoom("procession","west");
            var enemies=S.Room.GetComponentsInChildren<SliceEnemy>();
            enemies.First(e=>e.id=="procession/enemy-0").Damage(100,true);
            enemies.First(e=>e.id=="procession/enemy-1").Damage(2,false);
            yield return S.LoadRoom("belfry","low");
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="bell-secret")); S.ChooseOption(0);
            yield return S.LoadRoom("cistern","west");
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="pogo-secret")); S.ChooseOption(0);
            S.Flags.Add("sluice");
            yield return S.LoadRoom("archive","west");
            S.Flags.Add("archive-lift");
            yield return RestAtAnchor();
            Assert.That(S.EquipMemory("mercy"),Is.True);
            S.Consumed.Add("belfry-cache"); S.combat.PayIntegrity(3);
            yield return SceneManager.LoadSceneAsync("WakeSlice"); yield return Ready();
            Assert.That(S.Room.id,Is.EqualTo("archive")); Assert.That(S.Visited.Count,Is.EqualTo(5));
            Assert.That(S.Decision,Is.EqualTo("fire")); Assert.That(S.Archive.active,Is.EqualTo("mercy"));
            Assert.That(S.Archive.RootsOpen,Is.True); Assert.That(S.combat.Integrity,Is.EqualTo(6));
            Assert.That(S.Consumed,Does.Contain("bell-secret").And.Contain("pogo-secret"));
            Assert.That(S.Consumed,Does.Not.Contain("belfry-cache")); Assert.That(S.Flags.Count,Is.EqualTo(2));
            Assert.That(S.motor.SafePosition.x,Is.EqualTo(6).Within(.02f));
            yield return S.LoadRoom("procession","west");
            Assert.That(S.Room.GetComponentsInChildren<SliceEnemy>().Any(e=>e.id=="procession/enemy-0"),Is.False);
            Assert.That(S.Room.GetComponentsInChildren<SliceEnemy>().First(e=>e.id=="procession/enemy-1").hp,Is.EqualTo(8));
            foreach(var e in S.Room.GetComponentsInChildren<SliceEnemy>())e.Damage(100,true);
            Assert.That(S.SliceComplete,Is.True); S.EndRun(true); S.ChooseOption(0); yield return Ready();
            Assert.That(S.Archive.checkpoint,Is.Null); Assert.That(S.Room.id,Is.EqualTo("wake"));
            Assert.That(S.Archive.RootsOpen,Is.False); Assert.That(S.Visited.Count,Is.EqualTo(1));
            yield return SceneManager.LoadSceneAsync("WakeSlice"); yield return Ready();
            Assert.That(S.Room.id,Is.EqualTo("wake")); Assert.That(S.Archive.active,Is.EqualTo("fire"));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator InterruptedRestNeverSavesAndFatalDamageClearsCompletedCheckpoint()
        {
            SliceSession.EphemeralSave=false;
            var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");
            S.motor.EnterRoom((Vector2)bench.transform.position-Vector2.up*.25f); yield return new WaitForSeconds(.12f);
            S.Interact(bench); yield return new WaitForSeconds(.15f);
            Keys(Key.J); yield return null; Keys(); yield return new WaitForSeconds(.5f);
            Assert.That(S.Archive.checkpoint,Is.Null); Assert.That(File.Exists(SliceSession.SavePathOverride),Is.False);
            S.Interact(bench); yield return new WaitForSeconds(.1f); S.combat.Hurt(1,S.motor.Position+Vector2.right,true,null,true);
            yield return new WaitForSeconds(.5f); Assert.That(S.Archive.checkpoint,Is.Null);
            yield return RestAtAnchor(); S.combat.PayIntegrity(6);
            Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));
            Assert.That(SliceMemoryStore.Load(SliceSession.SavePathOverride,out _).checkpoint,Is.Null);
            yield return SceneManager.LoadSceneAsync("WakeSlice"); yield return Ready();
            Assert.That(S.Room.id,Is.EqualTo("wake")); Assert.That(S.Archive.checkpoint,Is.Null);
        }
        [UnityTest] public IEnumerator FailedCheckpointAndLoadoutSavesKeepPreviousDataAndCanRetry()
        {
            SliceSession.EphemeralSave=false; yield return RestAtAnchor();
            S.Archive.Transfer("mercy"); S.Archive.Transfer("return");
            string path=SliceSession.SavePathOverride;
            // A file as the parent directory produces a deterministic failure without permissions tricks.
            File.WriteAllText(Path.Combine(saveDirectory,"blocked"),"blocked");
            SliceSession.SavePathOverride=Path.Combine(saveDirectory,"blocked","save.json");
            Assert.That(S.EquipMemory("mercy"),Is.False); Assert.That(S.Archive.active,Is.EqualTo("return"));
            S.SetScreen(SliceScreen.Playing);
            var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");
            yield return new WaitForSeconds(.1f); S.Interact(bench); yield return new WaitForSeconds(.55f);
            Assert.That(S.Archive.checkpoint,Is.Null); Assert.That(S.CurrentMessage,Does.Contain("could not be saved"));
            Assert.That(SliceMemoryStore.Load(path,out _).loop,Is.EqualTo(1));
            SliceSession.SavePathOverride=path; yield return RestAtAnchor();
            Assert.That(S.EquipMemory("mercy"),Is.True);
            Assert.That(SliceMemoryStore.Load(path,out _).active,Is.EqualTo("mercy"));
        }
        [UnityTest] public IEnumerator DeflectAndLateBlockConsumeOneWindowAndRestClearsCombatTimers()
        {
            S.combat.BeginGuard();
            Assert.That(S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null),Is.True);
            Assert.That(S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null),Is.False);
            Assert.That(S.combat.Resonance,Is.EqualTo(1)); Assert.That(S.combat.Guarding,Is.False);
            yield return new WaitForSeconds(.12f);
            Assert.That(S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null),Is.False);
            Assert.That(S.combat.Integrity,Is.EqualTo(5));
            S.combat.Rest(); S.combat.BeginGuard();
            yield return new WaitForSeconds(.24f);
            S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null);
            S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null);
            Assert.That(S.combat.Fracture,Is.EqualTo(.5f)); Assert.That(S.combat.Integrity,Is.EqualTo(6));
            yield return new WaitForSeconds(.22f); S.combat.Hurt(1,S.motor.Position+Vector2.right,true,null);
            Assert.That(S.combat.Integrity,Is.EqualTo(4)); Assert.That(S.combat.Fracture,Is.Zero);
            S.combat.Rest(); S.combat.Hurt(1,S.motor.Position+Vector2.right,true,null);
            Assert.That(S.combat.Integrity,Is.EqualTo(5),"Rest must not retain old invulnerability.");
        }
        [UnityTest] public IEnumerator ChargedReleaseDeflectsWhiteButShortHoldDoesNotCounterRed()
        {
            Keys(Key.F); yield return new WaitForSeconds(.35f); Keys(); yield return null;
            Assert.That(S.combat.Hurt(2,S.motor.Position+Vector2.right,true,null),Is.False);
            Assert.That(S.combat.Integrity,Is.EqualTo(4)); S.combat.Rest();
            Keys(Key.F); yield return new WaitForSeconds(.55f); Keys(); yield return null;
            Assert.That(S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null),Is.True);
            Assert.That(S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null),Is.False);
            Assert.That(S.combat.Integrity,Is.EqualTo(6));
        }
        [UnityTest] public IEnumerator NativePauseMenuEquipsRetainedMemoryOnlyInSafeAreas()
        {
            S.Archive.Transfer("return"); S.Archive.Transfer("mercy"); S.Archive.Transfer("fire");
            Keys(Key.Escape); yield return null; Keys(); yield return null;
            Assert.That(S.Screen,Is.EqualTo(SliceScreen.Pause));
            Keys(Key.Digit2); yield return null; Keys(); yield return null;
            Assert.That(S.ModalTitle,Is.EqualTo("RETAINED MEMORIES"));
            Keys(Key.Digit2); yield return null; Keys(); yield return null;
            Assert.That(S.Playing,Is.True); Assert.That(S.Archive.active,Is.EqualTo("mercy"));
            Assert.That(S.Archive.RootsOpen,Is.False);
            var digits=new[]{Key.Digit1,Key.Digit2,Key.Digit3};
            for(int i=0;i<digits.Length;i++)
            {
                int selected=-1;
                S.Show("CHOICES","",SliceScreen.Dialogue,("ONE",()=>selected=0),("TWO",()=>selected=1),("THREE",()=>selected=2));
                Keys(digits[i]); yield return null; Keys(); yield return null;
                Assert.That(selected,Is.EqualTo(i),"Numbered modal choice must consume its native key.");
            }
            S.SetScreen(SliceScreen.Playing);
            yield return S.LoadRoom("procession","west");
            Assert.That(S.EquipMemory("return"),Is.False); Assert.That(S.Archive.active,Is.EqualTo("mercy"));
            LogAssert.NoUnexpectedReceived();
        }
    }
}
