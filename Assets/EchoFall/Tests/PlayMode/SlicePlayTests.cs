using System.Collections;
using System.Linq;
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
        InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        InputSettings.EditorInputBehaviorInPlayMode editorInput;
#endif
        SliceSession S=>SliceSession.Instance;
        [UnitySetUp] public IEnumerator Setup()
        {
            SliceSession.EphemeralSave=true;
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
        }
        IEnumerator Ready()
        {
            float deadline=Time.realtimeSinceStartup+10;
            while(S==null || !S.Playing) { Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline),"Room transition timed out"); yield return null; }
            yield return new WaitForSeconds(.08f);
        }
        void Keys(params Key[] keys) { InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys)); InputSystem.Update(); }
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
                    Assert.That(new[]{"wake","procession","belfry","archive","cistern"},Does.Contain(gate.target));
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
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="cistern-record")); S.ChooseOption(0);
            yield return S.LoadRoom("archive","west");
            Assert.That(S.SliceComplete,Is.True);
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.id=="slice-glass"));
            Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer)); Assert.That(S.ModalTitle,Is.EqualTo("THE GLASS ACCEPTS YOU"));
            S.ChooseOption(0); yield return Ready(); Assert.That(S.Room.id,Is.EqualTo("wake"));
            Assert.That(S.Room.returnedCreature.gameObject.activeSelf,Is.True); Assert.That(S.Visited.Count,Is.EqualTo(1)); LogAssert.NoUnexpectedReceived();
        }
    }
}
