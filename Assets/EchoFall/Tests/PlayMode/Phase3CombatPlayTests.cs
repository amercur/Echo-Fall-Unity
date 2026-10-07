using System.Collections;
using System.Collections.Generic;
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
    public sealed class Phase3CombatPlayTests
    {
        SliceSession S=>SliceSession.Instance;
        Keyboard keyboard;string directory;
        InputSettings.BackgroundBehavior background;
        InputSettings.EditorInputBehaviorInPlayMode editorInput;
        [UnitySetUp] public IEnumerator Setup()
        {
            SliceSession.EphemeralSave=true;directory=Path.Combine(Application.temporaryCachePath,"phase3-"+System.Guid.NewGuid());SliceSession.SavePathOverride=Path.Combine(directory,"save.json");
            background=InputSystem.settings.backgroundBehavior;editorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard=InputSystem.AddDevice<Keyboard>();yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();
        }
        [UnityTearDown] public IEnumerator Teardown()
        {
            InputSystem.RemoveDevice(keyboard);InputSystem.settings.backgroundBehavior=background;InputSystem.settings.editorInputBehaviorInPlayMode=editorInput;
            yield return SceneManager.LoadSceneAsync("MovementLab");SliceSession.EphemeralSave=false;SliceSession.SavePathOverride=null;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Ready(){float end=Time.realtimeSinceStartup+15;while(S==null||!S.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}yield return new WaitForSeconds(.1f);}
        void Keys(params Key[] keys){InputSystem.QueueStateEvent(keyboard,new KeyboardState(keys));InputSystem.Update();}
        void Place(Vector2 p){S.motor.EnterRoom(p);for(int i=0;i<12;i++)S.motor.Simulate(MovementTuning.Step,default);}
        SliceEnemy Target(float hp=100)
        {
            var target=Object.FindAnyObjectByType<SliceEnemy>();foreach(var e in Object.FindObjectsByType<SliceEnemy>())e.enabled=false;
            target.training=false;target.hp=hp;target.transform.position=new Vector2(4.45f,0);target.left=3;target.right=7;
            S.motor.automaticSimulation=false;Place(new Vector2(4,.01f));return target;
        }
        [UnityTest] public IEnumerator NativeComboBufferedThirdChargeAndDashRemainResponsive()
        {
            var target=Target();
            Keys(Key.J);yield return null;Keys();yield return new WaitForSeconds(.07f);
            Assert.That(S.combat.Combo,Is.EqualTo(1));Assert.That(target.hp,Is.EqualTo(98));
            Keys(Key.J);yield return null;Keys();yield return new WaitForSeconds(.2f);
            Assert.That(S.combat.Combo,Is.EqualTo(2));Assert.That(target.hp,Is.EqualTo(96));
            target.transform.position=new Vector2(4.45f,0);
            Keys(Key.J);yield return null;Keys();yield return new WaitForSeconds(.25f);
            Assert.That(S.combat.Combo,Is.EqualTo(3));Assert.That(target.hp,Is.EqualTo(92.5f));Assert.That(S.combat.Resonance,Is.EqualTo(.75f));
            yield return new WaitForSeconds(.4f);target.transform.position=new Vector2(4.45f,0);
            Keys(Key.J);yield return new WaitForSeconds(.6f);float before=target.hp;Keys();yield return null;yield return null;
            Assert.That(target.hp,Is.EqualTo(before-5.5f));Assert.That(S.combat.Combo,Is.EqualTo(3));Assert.That(target.strain,Is.EqualTo(2));
            S.motor.Simulate(MovementTuning.Step,new MovementCommand{move=1,dashPressed=true});yield return null;yield return null;
            Assert.That(S.combat.DashFollowup,Is.GreaterThan(.3f));Assert.That(S.combat.RecoveryRemaining,Is.LessThanOrEqualTo(.06f));
            Assert.That(Time.timeScale,Is.EqualTo(1));LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator RisingPogoImprintFractionAndMendMatchSource()
        {
            var target=Target();target.transform.position=new Vector2(4,.8f);
            S.combat.Strike(false,1);Assert.That(target.hp,Is.EqualTo(98));
            S.combat.ClearTransient();S.motor.EnterRoom(new Vector2(4,.7f));target.transform.position=new Vector2(4,.1f);
            S.combat.Strike(false,-1);Assert.That(S.motor.Velocity.y,Is.EqualTo(5.35f));Assert.That(S.motor.DashCooldown,Is.Zero);Assert.That(target.hp,Is.EqualTo(96));
            Place(new Vector2(4,.01f));target.transform.position=new Vector2(4.45f,0);S.combat.ClearTransient();S.combat.GainResonance(.75f);
            Assert.That(S.combat.Resonance,Is.EqualTo(1.25f));target.strain=3;S.combat.Imprint();Assert.That(S.combat.Resonance,Is.EqualTo(.25f));
            S.combat.Imprint();Assert.That(target.hp,Is.EqualTo(88));Assert.That(target.strain,Is.Zero);
            S.combat.GainResonance(1);S.combat.Imprint();yield return new WaitForSeconds(2.1f);Assert.That(target.hp,Is.EqualTo(81));
            S.combat.Rest();S.combat.PayIntegrity(2);Keys(Key.H);yield return new WaitForSeconds(1.1f);Keys();
            Assert.That(S.combat.Integrity,Is.EqualTo(5));Assert.That(S.combat.Resonance,Is.EqualTo(2));
            S.combat.Strike(true,0);Keys(Key.H);yield return new WaitForSeconds(.3f);Keys();Assert.That(S.combat.MendProgress,Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator DeflectWindowsCounterStrainReturnCooldownAndKnockback()
        {
            var target=Target();S.Archive.active="fire";
            S.combat.BeginGuard();yield return new WaitForSeconds(.10f);
            Assert.That(S.combat.ReceiveHit(1,S.motor.Position+Vector2.right,false,target),Is.EqualTo(CombatHitResult.Deflected));Assert.That(target.strain,Is.EqualTo(3));
            S.combat.ClearTransient();S.combat.BeginGuard();yield return new WaitForSeconds(.18f);
            Assert.That(S.combat.ReceiveHit(1,S.motor.Position+Vector2.right,false,target),Is.EqualTo(CombatHitResult.Blocked));Assert.That(S.combat.Fracture,Is.EqualTo(.5f));
            yield return new WaitForSeconds(.22f);S.combat.ReceiveHit(1,S.motor.Position+Vector2.right,true,null);
            Assert.That(S.combat.Integrity,Is.EqualTo(4));Assert.That(S.motor.Velocity.x,Is.EqualTo(-2.7f));Assert.That(S.motor.Velocity.y,Is.EqualTo(1.7f));
            S.combat.ClearTransient();Assert.That(S.combat.ReleaseCounter(.479f),Is.False);Assert.That(S.combat.ReleaseCounter(.48f),Is.True);
            Assert.That(S.combat.ReceiveHit(1,S.motor.Position+Vector2.right,true,target),Is.EqualTo(CombatHitResult.Deflected));Assert.That(target.strain,Is.EqualTo(8));Assert.That(S.combat.Resonance,Is.EqualTo(2.5f));
            yield return new WaitForSeconds(.12f);S.Archive.active="return";S.combat.BeginGuard();S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null);Assert.That(S.combat.Integrity,Is.EqualTo(5));
            yield return new WaitForSeconds(.12f);S.combat.BeginGuard();S.combat.Hurt(1,S.motor.Position+Vector2.right,false,null);Assert.That(S.combat.Integrity,Is.EqualTo(5));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ReflectedProjectileSweepsBackIntoItsSourceAndEmberGeneratesResonance()
        {
            var target=Target();target.transform.position=new Vector2(6,0);S.combat.BeginGuard();
            var shot=SliceProjectile.Spawn(new Vector2(5,.2f),Vector2.left*20,target,1);shot.enabled=false;shot.Tick(.1f);
            Assert.That(shot.Reflected,Is.True);Assert.That(shot.Damage,Is.EqualTo(4));Assert.That(shot.Velocity.x,Is.GreaterThan(0));
            shot.Tick(.5f);Assert.That(target.hp,Is.EqualTo(96));Assert.That(S.combat.Resonance,Is.EqualTo(1.2f).Within(.001));
            var ember=SliceProjectile.Spawn(new Vector2(4,.2f),Vector2.right*20,null,2.7f);ember.enabled=false;ember.Tick(.2f);
            Assert.That(target.hp,Is.EqualTo(93.3f).Within(.001));Assert.That(S.combat.Resonance,Is.EqualTo(1.4f).Within(.001));
            yield return null;LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator SentinelProtectsLancerHeavyBypassesAndEveryArchetypeAttacks()
        {
            yield return S.LoadRoom("procession","west");yield return Ready();S.motor.automaticSimulation=false;
            var enemies=Object.FindObjectsByType<SliceEnemy>();foreach(var enemy in enemies)enemy.enabled=false;
            var sentinel=enemies.First(e=>e.id=="procession/enemy-3");var lancer=enemies.First(e=>e.kind=="lancer");var drone=enemies.First(e=>e.kind=="drone");
            Place(new Vector2(lancer.transform.position.x-2,.01f));sentinel.Tick(.01f);Assert.That(sentinel.Guarding,Is.True);
            float hp=lancer.hp;lancer.Hit(2,.12f);Assert.That(lancer.hp,Is.EqualTo(hp-.6f).Within(.001));lancer.Hit(2,.55f);Assert.That(lancer.hp,Is.EqualTo(hp-2.6f).Within(.001));
            // Isolate each pattern while retaining its real shared reaction/damage path.
            sentinel.gameObject.SetActive(false);lancer.transform.position=new Vector2(8,0);lancer.left=4;lancer.right=12;Place(new Vector2(6.6f,.01f));
            for(int i=0;i<250&&lancer.State!="tell";i++)lancer.Tick(MovementTuning.Step);
            Assert.That(lancer.State,Is.EqualTo("tell"));float x=lancer.transform.position.x;Place(new Vector2(3,.01f));
            for(int i=0;i<180&&lancer.State!="recover";i++)lancer.Tick(MovementTuning.Step);
            Assert.That(lancer.transform.position.x,Is.LessThan(x-.5f));Assert.That(lancer.Missed,Is.True);Assert.That(lancer.RecoveryRemaining,Is.GreaterThan(1.4f));
            lancer.Deflected(true);Assert.That(lancer.strain,Is.EqualTo(5));
            drone.left=4;drone.right=9;drone.transform.position=new Vector2(7,drone.transform.position.y);S.motor.EnterRoom(new Vector2(5,drone.transform.position.y));float y=drone.transform.position.y;
            for(int i=0;i<500&&!Object.FindObjectsByType<SliceProjectile>().Any();i++)drone.Tick(MovementTuning.Step);
            Assert.That(Object.FindObjectsByType<SliceProjectile>().Any(),Is.True);Assert.That(drone.transform.position.y,Is.Not.EqualTo(y));
            sentinel.gameObject.SetActive(true);sentinel.transform.position=new Vector2(5.5f,0);sentinel.left=3;sentinel.right=9;lancer.gameObject.SetActive(false);drone.gameObject.SetActive(false);
            Place(new Vector2(5,.01f));int tells=0;string previous="";
            for(int i=0;i<400;i++){sentinel.Tick(MovementTuning.Step);if(sentinel.State=="tell"&&previous!="tell")tells++;previous=sentinel.State;}
            Assert.That(tells,Is.GreaterThanOrEqualTo(2));yield return null;LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator KingPredictionPhasesPatternsVictoryAndBacktracking()
        {
            yield return S.LoadRoom("king","west");yield return Ready();S.motor.automaticSimulation=false;
            var king=SliceKing.Active;king.enabled=false;Assert.That(king.Started,Is.False);
            Place(new Vector2(3,.01f));king.Tick(.01f);Assert.That(king.Started,Is.True);
            var exit=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="gate");Assert.That(exit.LockReason(S),Is.Not.Null);
            Assert.That(king.Hit(2),Is.False);Assert.That(king.hp,Is.EqualTo(40));
            Assert.That(king.Hit(5.5f,.55f,0,true),Is.True);Assert.That(king.Breaks,Is.EqualTo(1));
            S.motor.EnterRoom(new Vector2(3,1));king.Hit(2);Assert.That(king.Breaks,Is.EqualTo(2));
            Place(new Vector2(3,.01f));S.motor.Simulate(MovementTuning.Step,new MovementCommand{move=1,dashPressed=true});yield return null;yield return null;
            king.Hit(2);Assert.That(king.Breaks,Is.EqualTo(3));for(int i=0;i<6;i++)king.Tick(.01f);Assert.That(king.Stage,Is.EqualTo(2));Assert.That(king.State,Is.EqualTo("transition"));
            var states=new HashSet<string>();var patterns=new HashSet<int>();
            for(int i=0;i<2400;i++){king.Tick(MovementTuning.Step);states.Add(king.State);patterns.Add(king.Pattern);S.combat.Heal(6);}
            Assert.That(states,Does.Contain("sweep"));Assert.That(states,Does.Contain("charge"));Assert.That(patterns.Count,Is.EqualTo(3));
            king.Deflected(true);Assert.That(king.Exposed,Is.EqualTo(1.8f));Assert.That(king.strain,Is.EqualTo(5));
            king.Hit(100,.8f,0,true);Assert.That(S.Flags,Does.Contain("king"));Assert.That(S.Defeated,Does.Contain("king/boss"));Assert.That(exit.LockReason(S),Is.Null);Assert.That(S.motor.roomBounds,Is.EqualTo(S.Room.bounds));
            yield return S.LoadRoom("procession","east");yield return S.LoadRoom("king","west");yield return Ready();Assert.That(SliceKing.Active,Is.Null);
            S.Visited.UnionWith(new[]{"wake","belfry","cistern","archive","procession"});S.Consumed.UnionWith(new[]{"bell-secret","pogo-secret"});
            S.Defeated.UnionWith(new[]{"procession/enemy-0","procession/enemy-1","procession/enemy-2","procession/enemy-3"});S.MakeDecision("fire");Assert.That(S.SliceComplete,Is.True,"Court visits must not invalidate the original five-room completion route.");
            var glass=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="victory");S.Interact(glass);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));
            S.ChooseOption(0);yield return Ready();Assert.That(S.Room.id,Is.EqualTo("wake"));Assert.That(S.Flags,Is.Empty);Assert.That(S.KingBreaks,Is.Zero);LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator BossVictoryCheckpointReloadAndCombatDeathKeepArchiveSemantics()
        {
            S.Archive.Transfer("mercy");S.Archive.Transfer("return");
            yield return S.LoadRoom("king","west");yield return Ready();S.motor.automaticSimulation=false;Place(new Vector2(3,.01f));var king=SliceKing.Active;king.Tick(.01f);king.Hit(100,.8f,0,true);
            yield return S.LoadRoom("wake","default");yield return Ready();S.motor.automaticSimulation=true;
            var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");S.motor.EnterRoom((Vector2)bench.transform.position-Vector2.up*.25f);yield return new WaitForSeconds(.1f);
            SliceSession.EphemeralSave=false;S.Interact(bench);yield return new WaitForSeconds(.55f);
            Assert.That(S.Archive.checkpoint,Is.Not.Null);Assert.That(S.Archive.checkpoint.defeated,Does.Contain("king/boss"));
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();Assert.That(S.Flags,Does.Contain("king"));Assert.That(S.Archive.active,Is.EqualTo("return"));Assert.That(S.Archive.RootsOpen,Is.True);
            yield return S.LoadRoom("king","west");yield return Ready();Assert.That(SliceKing.Active,Is.Null);
            S.combat.Hurt(6,S.motor.Position+Vector2.right,true,null,true);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));Assert.That(SliceMemoryStore.Load(SliceSession.SavePathOverride,out _).checkpoint,Is.Null);
            S.ChooseOption(0);yield return Ready();Assert.That(S.Flags,Is.Empty);Assert.That(S.Archive.RootsOpen,Is.True);Assert.That(S.Archive.active,Is.EqualTo("return"));
            yield return S.LoadRoom("king","west");yield return Ready();Assert.That(SliceKing.Active.hp,Is.EqualTo(40));
            Place(new Vector2(3,.01f));SliceKing.Active.Tick(.01f);Assert.That(SliceKing.Active.Started,Is.True);
            S.combat.Hurt(6,SliceKing.Active.transform.position,true,SliceKing.Active,true);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));S.ChooseOption(0);yield return Ready();
            yield return S.LoadRoom("king","west");yield return Ready();Place(new Vector2(3,.01f));SliceKing.Active.Tick(.01f);
            S.EndRun(false);S.ChooseOption(0);yield return Ready();Assert.That(S.Room.id,Is.EqualTo("wake"));Assert.That(S.Archive.RootsOpen,Is.True);LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator KingTakesActualBladePogoAndImprintDamage()
        {
            yield return S.LoadRoom("king","west");yield return Ready();S.motor.automaticSimulation=false;
            var king=SliceKing.Active;king.enabled=false;Place(new Vector2(3,.01f));king.Tick(.01f);
            Place((Vector2)king.transform.position-Vector2.right*.5f);S.combat.Strike(false,0);Assert.That(king.hp,Is.EqualTo(40));
            S.combat.Strike(true,0);Assert.That(king.hp,Is.EqualTo(34.5f));Assert.That(king.strain,Is.EqualTo(2));
            S.combat.ClearTransient();S.motor.EnterRoom((Vector2)king.transform.position+Vector2.up*1.15f);S.combat.Strike(false,-1);
            Assert.That(king.hp,Is.EqualTo(32.5f));Assert.That(S.motor.Velocity.y,Is.EqualTo(5.35f));
            Place((Vector2)king.transform.position-Vector2.right*.6f);S.combat.Rest();S.combat.Imprint();Assert.That(S.combat.Marked,Is.SameAs(king));S.combat.Imprint();Assert.That(king.hp,Is.EqualTo(21.5f));
            for(int i=0;i<4;i++){S.combat.Strike(true,0);yield return new WaitForSeconds(.48f);}
            Assert.That(king.Alive,Is.False);Assert.That(S.Flags,Does.Contain("king"));Assert.That(S.combat.Marked,Is.Null);LogAssert.NoUnexpectedReceived();
        }
    }
}
