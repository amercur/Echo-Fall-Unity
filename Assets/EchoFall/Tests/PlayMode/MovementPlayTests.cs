using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoFall.Movement.Tests
{
    public sealed class MovementPlayTests
    {
        Keyboard keyboard;
        InputSettings.BackgroundBehavior previousBackground;
#if UNITY_EDITOR
        InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
#endif

        [UnitySetUp]
        public IEnumerator Setup()
        {
            previousBackground = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            previousEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            yield return SceneManager.LoadSceneAsync("MovementLab");
            keyboard = InputSystem.AddDevice<Keyboard>();
            yield return new WaitForSeconds(.2f);
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            if (keyboard != null && keyboard.added) InputSystem.RemoveDevice(keyboard);
            InputSystem.settings.backgroundBehavior = previousBackground;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = previousEditorInput;
#endif
            yield return null;
        }

        void Keys(params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));

        [UnityTest]
        public IEnumerator SceneRunsWithNativeInputPhysicsSpritesAndCamera()
        {
            var motor = Object.FindAnyObjectByType<PlayerMotor>();
            Assert.That(motor, Is.Not.Null);
            Assert.That(motor.Grounded, Is.True);
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(MovementTuning.Step).Within(.000001f));
            float start = motor.Position.x;
            Keys(Key.D);
            yield return new WaitForSeconds(.4f);
            Assert.That(motor.Position.x, Is.GreaterThan(start + .7f));
            Keys(Key.Space);
            yield return new WaitForSeconds(.12f);
            Assert.That(motor.Position.y, Is.GreaterThan(.4f));
            Keys();
            yield return new WaitForSeconds(.03f);
            Assert.That(motor.Velocity.y, Is.LessThanOrEqualTo(2.2f));
            Keys(Key.K);
            yield return new WaitForSeconds(.04f);
            Assert.That(motor.AirDashUsed, Is.True);
            Assert.That(motor.Dashing, Is.True);
            Assert.That(motor.GetComponentInChildren<SpriteRenderer>().sprite, Is.Not.Null);
            Assert.That(Camera.main.GetComponent<RoomCamera>().target, Is.SameAs(motor));
            Keys(Key.R);
            yield return new WaitForSeconds(.08f);
            Assert.That(motor.Position.x, Is.EqualTo(1.21f).Within(.01));
            Assert.That(motor.ResetCount, Is.EqualTo(1));
            Assert.That(Object.FindAnyObjectByType<MovementLabHUD>().status, Is.Not.Null);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator InputEdgesAreConsumedOnceAndUpDoesNotJump()
        {
            var input = Object.FindAnyObjectByType<MovementInput>();
            var motor = input.GetComponent<PlayerMotor>();
            motor.automaticSimulation = false;
            Keys(Key.W);
            yield return null;
            Assert.That(input.Consume().jumpPressed, Is.False);
            Keys(Key.Space);
            yield return null;
            Assert.That(input.Consume().jumpPressed, Is.True);
            Assert.That(input.Consume().jumpPressed, Is.False);
            Keys();
            yield return null;
            Assert.That(input.Consume().jumpReleased, Is.True);
            Assert.That(input.Consume().jumpReleased, Is.False);
        }

        [UnityTest]
        public IEnumerator FixedTrajectoryRunsAt30_60_144RenderTargets()
        {
            int oldRate = Application.targetFrameRate, oldVsync = QualitySettings.vSyncCount;
            var motor = Object.FindAnyObjectByType<PlayerMotor>();
            motor.automaticSimulation = false;
            var driver = motor.gameObject.AddComponent<MovementAcceptanceDriver>();
            driver.motor = motor;
            Vector2? baseline = null;
            try
            {
                QualitySettings.vSyncCount = 0;
                foreach (int hz in new[] { 30, 60, 144 })
                {
                    Application.targetFrameRate = hz;
                    motor.Teleport(new Vector2(1.21f, .02f));
                    for (int i = 0; i < 24; i++) motor.Simulate(MovementTuning.Step, default);
                    driver.steps = 0; driver.limit = 120; driver.renderFrames = 0;
                    float started = Time.realtimeSinceStartup;
                    yield return new WaitUntil(() => driver.steps >= 120 || Time.realtimeSinceStartup - started > 10);
                    Assert.That(driver.steps, Is.EqualTo(120));
                    if (baseline.HasValue) Assert.That(Vector2.Distance(motor.Position, baseline.Value), Is.LessThan(.001f));
                    baseline = motor.Position;
                    TestContext.Out.WriteLine($"Movement render target {hz} Hz: {driver.renderFrames} rendered frames, 120 fixed ticks, final {motor.Position}");
                }
            }
            finally { Application.targetFrameRate = oldRate; QualitySettings.vSyncCount = oldVsync; Object.Destroy(driver); }
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator BelfryClimbRunsInPlayModeAndCameraTracksSummit()
        {
            var motor = Object.FindAnyObjectByType<PlayerMotor>();
            motor.automaticSimulation = false;
            motor.Teleport(new Vector2(21.21f, 1.97f));
            for (int i=0;i<12;i++) motor.Simulate(MovementTuning.Step,default);
            var camera = Camera.main.GetComponent<RoomCamera>(); camera.SnapToTarget();
            var driver = motor.gameObject.AddComponent<MovementAcceptanceDriver>();
            driver.motor=motor; driver.climb=true; driver.limit=3600;
            yield return new WaitUntil(()=>driver.reached || driver.steps>=driver.limit);
            Assert.That(driver.reached,Is.True,$"Climb ended at {motor.Position}");
            Assert.That(motor.ResetCount,Is.Zero);
            Assert.That(camera.transform.position.y,Is.GreaterThan(5));
            Object.Destroy(driver);
            LogAssert.NoUnexpectedReceived();
        }
    }

    [DefaultExecutionOrder(-100)]
    public sealed class MovementAcceptanceDriver : MonoBehaviour
    {
        public PlayerMotor motor;
        public int steps, limit, renderFrames;
        public bool climb, reached;
        int aim=1;
        void Update() { if(steps<limit && !reached) renderFrames++; }
        void FixedUpdate()
        {
            if(steps>=limit || reached) return;
            MovementCommand command;
            if(climb)
            {
                if(motor.Wall!=0) aim=-motor.Wall;
                if(motor.Position.y>8.07f && motor.Position.x<21.31f || motor.Position.y>9.42f) aim=1;
                command=new MovementCommand {move=aim,jumpPressed=motor.Grounded || motor.Wall!=0,
                    dashPressed=motor.Position.y>9.52f && motor.Velocity.x>0 && !motor.AirDashUsed};
            }
            else command=new MovementCommand {move=1,jumpPressed=steps==12,jumpReleased=steps==36,dashPressed=steps==48};
            motor.Simulate(Time.fixedDeltaTime,command); steps++;
            if(climb) reached=motor.Position.x>22.71f && motor.Position.y>7.82f;
        }
    }
}
