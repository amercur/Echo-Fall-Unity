using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace EchoFall.Movement.Tests
{
    public sealed class OpeningPlayTests
    {
        SliceSession S=>SliceSession.Instance;
        string save;
        InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        InputSettings.EditorInputBehaviorInPlayMode editorInput;
#endif
        [UnitySetUp] public IEnumerator Setup()
        {
            background=InputSystem.settings.backgroundBehavior;InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            save=Path.Combine(Application.temporaryCachePath,"phase8-"+Guid.NewGuid()+".json");
            SliceSession.EphemeralSave=true;SliceSession.SavePathOverride=save;OpeningExperience.ShowTitleInTests=true;
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return WaitFor(SliceScreen.Title);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("MovementLab");OpeningExperience.ShowTitleInTests=false;SliceSession.EphemeralSave=false;SliceSession.SavePathOverride=null;
            InputSystem.settings.backgroundBehavior=background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
#endif
            foreach(string suffix in new[]{"",".bak",".tmp"})if(File.Exists(save+suffix))File.Delete(save+suffix);
        }
        IEnumerator WaitFor(SliceScreen screen){float limit=Time.realtimeSinceStartup+20;while(S==null||S.Screen!=screen){Assert.That(Time.realtimeSinceStartup,Is.LessThan(limit));yield return null;}}
        [UnityTest] public IEnumerator TitleAndFieldGuideKeepControlsResponsiveAndDoNotAdvanceLife()
        {
            var pad=InputSystem.AddDevice<Gamepad>();
            try{
                int life=S.Archive.loop;Vector2 position=S.motor.Position;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.DpadDown));yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;
                InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));yield return null;yield return null;
                Assert.That(S.ModalTitle,Is.EqualTo("THE FIELD GUIDE"));Assert.That(S.Archive.loop,Is.EqualTo(life));Assert.That(S.motor.Position,Is.EqualTo(position));
                InputSystem.QueueStateEvent(pad,new GamepadState());yield return null;S.ReturnFromOverlay();Assert.That(S.Screen,Is.EqualTo(SliceScreen.Title));
                S.ShowSettings();S.ChooseOption(7);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Title));
                S.ChooseOption(0);Assert.That(S.Playing,Is.True);Assert.That(S.motor.automaticSimulation,Is.True);
            }finally{InputSystem.RemoveDevice(pad);}
            var keyboard=InputSystem.AddDevice<Keyboard>();
            try{
                float x=S.motor.Position.x;InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.D));yield return new WaitForSeconds(.5f);
                Assert.That(S.motor.Position.x,Is.GreaterThan(x+.6f));Assert.That(S.Opening.Knows("move"),Is.True);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));yield return new WaitForSeconds(.08f);
                Assert.That(S.Opening.Knows("jump"),Is.True);Assert.That(S.motor.Velocity.y,Is.GreaterThan(0));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.K));yield return new WaitForSeconds(.05f);Assert.That(S.Opening.Knows("dash"),Is.True);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
            }finally{InputSystem.RemoveDevice(keyboard);}
        }
        [UnityTest] public IEnumerator WardenAcceptsActualDeflectAndLearnsWithoutAProgressionLock()
        {
            S.ChooseOption(0);yield return null;var w=S.Room.GetComponentsInChildren<SliceEnemy>().Single(e=>e.training);w.enabled=false;
            S.motor.Teleport((Vector2)w.transform.position+Vector2.left*.3f);S.motor.Simulate(MovementTuning.Step,new MovementCommand{move=1});
            S.combat.Strike(false,0);Assert.That(S.Opening.Knows("strike"),Is.True);
            S.combat.Rest();S.combat.BeginGuard();var hit=S.combat.ReceiveHit(1,w.transform.position,false,w);
            Assert.That(hit,Is.EqualTo(CombatHitResult.Deflected));Assert.That(w.Alive,Is.False);Assert.That(S.Opening.Knows("deflect"),Is.True);
            Assert.That(S.Room.GetComponentsInChildren<Collider2D>().Any(c=>c.transform.IsChildOf(S.Room.transform.Find("Opening identity"))),Is.False);
        }
        [UnityTest] public IEnumerator FirstChoiceSurvivesRestartExtractionAndChangesTheNextLoop()
        {
            S.ChooseOption(0);yield return null;SliceSession.EphemeralSave=false;
            S.Interact(S.Room.GetComponentsInChildren<SliceInteraction>().Single(i=>i.id=="creature"));S.ChooseOption(0);
            Assert.That(S.Decision,Is.EqualTo("mercy"));Assert.That(S.combat.Integrity,Is.EqualTo(4));
            S.EndRun(false);Assert.That(File.Exists(save),Is.True);
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return WaitFor(SliceScreen.Title);
            S.ChooseOption(0);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));Assert.That(S.OptionLabels[0],Does.Contain("LIFE I SPARED"));
            S.ChooseOption(0);yield return WaitFor(SliceScreen.Playing);yield return null;
            Assert.That(S.Archive.Remembers("mercy"),Is.True);Assert.That(S.Archive.loop,Is.EqualTo(2));Assert.That(S.Room.returnedCreature.gameObject.activeSelf,Is.True);
            Assert.That(S.Room.transform.Find("Opening identity/Remembered hand trail").gameObject.activeSelf,Is.True);
            yield return S.LoadRoom("cistern","west");Assert.That(S.Room.rootBridge.gameObject.activeSelf,Is.True);
            var persisted=SliceMemoryStore.Load(save,out _);Assert.That(persisted.RootsOpen,Is.True);
        }
        [UnityTest] public IEnumerator EmberPresentsAnAbsenceAndRetainsItsConflictWithMercy()
        {
            S.ChooseOption(0);yield return null;S.MakeDecision("fire");Assert.That(S.Room.emberScar.gameObject.activeSelf,Is.True);
            var creature=S.Room.GetComponentsInChildren<SliceInteraction>().Single(i=>i.id=="creature");Assert.That(creature.GetComponentsInChildren<Renderer>().All(r=>!r.enabled),Is.True);
            S.Archive.Transfer("mercy");S.EndRun(false);S.CommitTransfer("fire");yield return WaitFor(SliceScreen.Playing);yield return null;
            Assert.That(S.Room.returnedCreature.gameObject.activeSelf,Is.False);Assert.That(S.Room.emberScar.gameObject.activeSelf,Is.True);
            yield return S.LoadRoom("cistern","west");Assert.That(S.Room.rootBridge.gameObject.activeSelf,Is.False);
        }
    }
}
