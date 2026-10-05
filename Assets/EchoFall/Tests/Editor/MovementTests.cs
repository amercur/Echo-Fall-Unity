using System;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using EchoFall.Movement.Editor;

namespace EchoFall.Movement.Tests
{
    public sealed class MovementTests
    {
        const float Dt = MovementTuning.Step;
        const int Solid = 8, Platform = 9, Hazard = 10;
        PlayerMotor motor;
        MovementTuning tuning;
        Scene scene;

        [SetUp]
        public void Setup()
        {
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            tuning = ScriptableObject.CreateInstance<MovementTuning>();
            var player = new GameObject("Test player");
            motor = player.AddComponent<PlayerMotor>();
            motor.tuning = tuning;
            motor.solids = 1 << Solid;
            motor.oneWayPlatforms = 1 << Platform;
            motor.hazards = 1 << Hazard;
            motor.roomBounds = new Rect(-50, -50, 100, 100);
            motor.automaticSimulation = false;
            Floor();
            Place(new Vector2(0, .02f));
            Tick(.15f);
        }

        [TearDown]
        public void Teardown()
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            UnityEngine.Object.DestroyImmediate(tuning);
        }

        GameObject Box(string name, Rect rect, int layer = Solid)
        {
            var obj = new GameObject(name, typeof(BoxCollider2D));
            SceneManager.MoveGameObjectToScene(obj, scene);
            obj.layer = layer;
            obj.transform.position = rect.center;
            obj.GetComponent<BoxCollider2D>().size = rect.size;
            obj.GetComponent<BoxCollider2D>().isTrigger = layer != Solid;
            Physics2D.SyncTransforms();
            return obj;
        }
        void Floor() => Box("Floor", new Rect(-40, -1, 80, 1));
        void Place(Vector2 position) { motor.Teleport(position); Physics2D.SyncTransforms(); }
        void Tick(float seconds, float move = 0)
        {
            for (int i = 0; i < Mathf.RoundToInt(seconds / Dt); i++) Step(new MovementCommand { move = move });
        }
        void Step(MovementCommand command) { motor.Simulate(Dt, command); Physics2D.SyncTransforms(); }

        [Test]
        public void GroundAccelerationAndBrakingMatchReference()
        {
            Assert.That(motor.Grounded, Is.True);
            Tick(.4f, 1);
            Assert.That(motor.Velocity.x, Is.EqualTo(2.65f).Within(.001f));
            Assert.That(motor.Position.x, Is.InRange(.89f, .93f));
            Tick(.2f);
            Assert.That(motor.Velocity.x, Is.EqualTo(0).Within(.001f));
            Assert.That(motor.Position.y, Is.EqualTo(0).Within(.003f));
        }

        [Test]
        public void HeldJumpRisesHigherAndUpDoesNotJump()
        {
            Step(new MovementCommand { jumpPressed = true });
            Tick(.1f);
            Step(new MovementCommand { jumpReleased = true });
            Tick(.125f);
            float shortHeight = motor.Position.y;
            Place(new Vector2(0, .02f)); Tick(.15f);
            Step(new MovementCommand { jumpPressed = true }); Tick(.233333f);
            Assert.That(motor.Position.y, Is.GreaterThan(shortHeight + .2f));
            Tick(1);
            Assert.That(motor.Grounded, Is.True);
        }

        [Test]
        public void TapBetweenPhysicsTicksStaysShort()
        {
            Step(new MovementCommand { jumpPressed = true, jumpReleased = true });
            Assert.That(motor.Velocity.y, Is.InRange(1.9f, 2.2f));
        }

        [Test]
        public void SecondJumpInAirDoesNotGrantDoubleJump()
        {
            Step(new MovementCommand { jumpPressed = true }); Tick(.15f);
            float velocity = motor.Velocity.y;
            Step(new MovementCommand { jumpPressed = true });
            Assert.That(motor.Velocity.y, Is.LessThan(velocity));
        }

        [Test]
        public void OneWayAllowsRiseLandingAndDropButFloorRemainsSolid()
        {
            Box("Platform", new Rect(-1, .7f, 2, .2f), Platform);
            Step(new MovementCommand { jumpPressed = true }); Tick(.85f);
            Assert.That(motor.Position.y, Is.EqualTo(.9f).Within(.004f));
            Assert.That(motor.OnPlatform, Is.True);
            Step(new MovementCommand { jumpPressed = true, down = true }); Tick(.75f);
            Assert.That(motor.Position.y, Is.EqualTo(0).Within(.004f));
            Step(new MovementCommand { jumpPressed = true, down = true }); Tick(.1f);
            Assert.That(motor.Position.y, Is.GreaterThan(0));
        }

        [Test]
        public void JumpBufferFiresOnLandingAndExpiredBufferDoesNot()
        {
            Place(new Vector2(0, .055f));
            Step(new MovementCommand { jumpPressed = true }); Tick(.11f);
            Assert.That(motor.Velocity.y, Is.GreaterThan(4));
            Place(new Vector2(0, 2));
            Step(new MovementCommand { jumpPressed = true }); Tick(.75f);
            Assert.That(motor.Grounded, Is.True);
        }

        [Test]
        public void CoyoteJumpWorksOnlyInsideGraceWindow()
        {
            UnityEngine.Object.DestroyImmediate(GameObject.Find("Floor"));
            Box("Ledge", new Rect(-2, -1, 2, 1));
            Place(new Vector2(-.4f, .01f)); Tick(.1f);
            for (int i = 0; i < 120 && motor.Grounded; i++) Step(new MovementCommand { move = 1 });
            Tick(.04f, 1);
            Step(new MovementCommand { move = 1, jumpPressed = true });
            Assert.That(motor.Velocity.y, Is.GreaterThan(5));
            Place(new Vector2(-.4f, .01f)); Tick(.1f);
            for (int i = 0; i < 120 && motor.Grounded; i++) Step(new MovementCommand { move = 1 });
            Tick(.15f, 1);
            Step(new MovementCommand { move = 1, jumpPressed = true });
            Assert.That(motor.Velocity.y, Is.LessThan(0));
        }

        [Test]
        public void DashCannotTunnelThroughThinSolidAndJumpCannotEnterCeiling()
        {
            Box("Thin wall", new Rect(.5f, 0, .04f, 3));
            Step(new MovementCommand { move = 1, dashPressed = true }); Tick(.15f, 1);
            Assert.That(motor.Position.x, Is.LessThanOrEqualTo(.391f));
            Assert.That(motor.Dashing, Is.False);
            Place(new Vector2(-2, .01f)); Tick(.1f);
            Box("Ceiling", new Rect(-3, .75f, 2, .2f));
            Step(new MovementCommand { jumpPressed = true }); Tick(.1f);
            Assert.That(motor.Position.y + tuning.size.y, Is.LessThanOrEqualTo(.751f));
            Assert.That(motor.Velocity.y, Is.LessThanOrEqualTo(0));
        }

        [Test]
        public void ExactlyOneAirDashUntilLandingAndDashBufferSurvivesCooldown()
        {
            Place(new Vector2(0, 10));
            Step(new MovementCommand { dashPressed = true }); Tick(.65f);
            Assert.That(motor.AirDashUsed, Is.True);
            Step(new MovementCommand { dashPressed = true });
            Assert.That(motor.Dashing, Is.False);
            Tick(2);
            Assert.That(motor.Grounded, Is.True);
            Assert.That(motor.AirDashUsed, Is.False);
            Step(new MovementCommand { dashPressed = true }); Tick(.48f);
            Step(new MovementCommand { dashPressed = true }); Tick(.11f);
            Assert.That(motor.Dashing, Is.True, "Dash shortly before cooldown must execute when ready");
        }

        [TestCase(-1)]
        [TestCase(1)]
        public void SameWallTapJumpsGainHeightAndRestoreAirDash(int side)
        {
            float wallX = side < 0 ? -1.28f : 1;
            Box("Tall wall", new Rect(wallX, 0, .28f, 12));
            Place(new Vector2(side * .889f, 2));
            Step(new MovementCommand { move = side, dashPressed = true });
            Tick(.15f, side);
            Assert.That(motor.Wall, Is.EqualTo(side));
            Assert.That(motor.Velocity.y, Is.GreaterThanOrEqualTo(-1.1f));
            for (int jump = 0; jump < 3; jump++)
            {
                float height = motor.Position.y, startX = motor.Position.x;
                Step(new MovementCommand { move = side, jumpPressed = true });
                Assert.That(motor.AirDashUsed, Is.False);
                Tick(.041667f, side);
                Step(new MovementCommand { move = side, jumpReleased = true });
                float maxDistance = 0;
                for (int i = 0; i < 48 && motor.Wall != side; i++)
                {
                    Step(new MovementCommand { move = side });
                    maxDistance = Mathf.Max(maxDistance, Mathf.Abs(motor.Position.x - startX));
                }
                Assert.That(motor.Wall, Is.EqualTo(side));
                Assert.That(maxDistance, Is.LessThan(.35f));
                Assert.That(motor.Position.y, Is.GreaterThan(height + .15f));
            }
        }

        [Test]
        public void HazardReturnsToSafeFootingWithoutCombatOrDeathState()
        {
            Box("Hazard", new Rect(1, -.01f, 1, .5f), Hazard);
            Tick(.65f, 1);
            Assert.That(motor.ResetCount, Is.GreaterThan(0));
            Assert.That(motor.Position.x, Is.LessThan(1));
            Assert.That(motor.Position.y, Is.GreaterThanOrEqualTo(-.001f));
        }

        [Test]
        public void FixedMovementMatchesAcrossRenderRates()
        {
            Vector2? baseline = null;
            foreach (int hz in new[] { 30, 60, 144 })
            {
                Place(Vector2.zero); Tick(.1f);
                double accumulator = 0;
                int ticks = 0;
                for (int frame = 0; frame < hz; frame++)
                {
                    accumulator += 1.0 / hz;
                    while (accumulator + 1e-9 >= 1.0 / 120)
                    {
                        Step(new MovementCommand { move = 1, jumpPressed = ticks == 12, jumpReleased = ticks == 36, dashPressed = ticks == 48 });
                        accumulator -= 1.0 / 120; ticks++;
                    }
                }
                Assert.That(ticks, Is.EqualTo(120));
                if (baseline.HasValue) Assert.That(Vector2.Distance(motor.Position, baseline.Value), Is.LessThan(.0001f));
                baseline = motor.Position;
            }
        }

        [Test]
        public void RoomLimitsConvertOriginalPlayerTopToFeet()
        {
            UnityEngine.Object.DestroyImmediate(GameObject.Find("Floor"));
            motor.roomBounds = new Rect(0, 0, 32, 12);
            Place(new Vector2(3, 12.5f)); Step(default);
            Assert.That(motor.Position.y, Is.EqualTo(12.2f).Within(.001f));
            Place(new Vector2(3, -.8f)); Step(default);
            Assert.That(motor.ResetCount, Is.Zero, "Top-left Y has not passed bottom + 60px yet");
            Place(new Vector2(3, -1.02f)); Step(default);
            Assert.That(motor.ResetCount, Is.EqualTo(1));
        }

        [Test]
        public void CameraStaysInsideBoundsAtDifferentAspectRatiosAndTracksHeight()
        {
            var camera = new GameObject("Camera", typeof(Camera), typeof(RoomCamera)).GetComponent<RoomCamera>();
            camera.bounds = new Rect(0, -.88f, 32, 12);
            foreach (float aspect in new[] { 4f / 3, 16f / 9, 21f / 9 })
            {
                var left = camera.DesiredPosition(Vector2.zero, Vector2.left * 7.2f, 2.7f, aspect);
                var right = camera.DesiredPosition(new Vector2(32, 12), Vector2.right * 7.2f, 2.7f, aspect);
                Assert.That(left.x - 2.7f * aspect, Is.GreaterThanOrEqualTo(-.001f));
                Assert.That(right.x + 2.7f * aspect, Is.LessThanOrEqualTo(32.001f));
                Assert.That(left.y - 2.7f, Is.GreaterThanOrEqualTo(-.881f));
                Assert.That(right.y + 2.7f, Is.LessThanOrEqualTo(11.121f));
                Assert.That(right.y, Is.GreaterThan(left.y + 4));
            }
        }

        [Test]
        public void AuthoredLabBelfryShaftCanBeClimbedWithoutUpgrades()
        {
            EditorSceneManager.OpenScene(MovementLabBuilder.ScenePath);
            motor = UnityEngine.Object.FindAnyObjectByType<PlayerMotor>();
            motor.automaticSimulation = false;
            motor.Teleport(new Vector2(21.21f, 1.97f));
            Physics2D.SyncTransforms();
            Tick(.1f);
            int aim = 1;
            bool reached = false;
            for (int i = 0; i < 3600; i++)
            {
                bool jump = motor.Grounded || motor.Wall != 0;
                if (motor.Wall != 0) aim = -motor.Wall;
                if (motor.Position.y > 8.07f && motor.Position.x < 21.31f) aim = 1;
                if (motor.Position.y > 9.42f) aim = 1;
                bool dash = motor.Position.y > 9.52f && motor.Velocity.x > 0 && !motor.AirDashUsed;
                Step(new MovementCommand { move = aim, jumpPressed = jump, dashPressed = dash });
                if (motor.Position.x > 22.71f && motor.Position.y > 7.82f) { reached = true; break; }
            }
            Assert.That(reached, Is.True, $"Shaft failed at {motor.Position}, wall {motor.Wall}");
            Assert.That(motor.ResetCount, Is.Zero);
        }

        [Test]
        public void ImportedSpritesPreserveScalePivotAndAllFortyFrames()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(MovementLabBuilder.PrefabPath);
            var visual = prefab.GetComponentInChildren<PlayerVisual>();
            Assert.That(visual.frames.Length, Is.EqualTo(40));
            foreach (var sprite in visual.frames)
            {
                Assert.That(sprite.rect.width, Is.EqualTo(192));
                Assert.That(sprite.rect.height, Is.EqualTo(192));
                Assert.That(sprite.pivot.x, Is.EqualTo(96).Within(.01));
                Assert.That(sprite.pivot.y, Is.EqualTo(30.471).Within(.01));
                Assert.That(sprite.pixelsPerUnit, Is.EqualTo(100));
            }
            Assert.That(visual.transform.localScale.x, Is.EqualTo(.34f).Within(.0001));
        }

        [Test]
        public void AuthoredLabHasConnectedPlayerValidMaterialsAndCameraBounds()
        {
            var lab = EditorSceneManager.OpenScene(MovementLabBuilder.ScenePath);
            var players = UnityEngine.Object.FindObjectsByType<PlayerMotor>();
            Assert.That(players.Length, Is.EqualTo(1));
            var player = players[0];
            Assert.That(PrefabUtility.GetPrefabInstanceStatus(player.gameObject), Is.EqualTo(PrefabInstanceStatus.Connected));
            Assert.That(player.tuning, Is.Not.Null);
            Assert.That(player.GetComponent<MovementInput>().actions, Is.Not.Null);
            Assert.That(player.GetComponent<Rigidbody2D>().bodyType, Is.EqualTo(RigidbodyType2D.Kinematic));
            Assert.That(player.GetComponent<BoxCollider2D>().size, Is.EqualTo(player.tuning.size));
            var camera = UnityEngine.Object.FindAnyObjectByType<RoomCamera>();
            Assert.That(camera.target, Is.SameAs(player));
            Assert.That(camera.bounds, Is.EqualTo(player.roomBounds));
            Assert.That(camera.bounds.width, Is.GreaterThan(0));
            Assert.That(UnityEngine.Object.FindAnyObjectByType<MovementLabHUD>().player, Is.SameAs(player));
            foreach (var root in lab.GetRootGameObjects())
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                Assert.That(PrefabUtility.GetPrefabInstanceStatus(t.gameObject), Is.Not.EqualTo(PrefabInstanceStatus.MissingAsset));
                Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject), Is.Zero);
                var sprite = t.GetComponent<SpriteRenderer>();
                if (sprite != null) { Assert.That(sprite.sprite, Is.Not.Null, t.name); Assert.That(sprite.sharedMaterial, Is.Not.Null, t.name); }
            }
        }
    }
}
