using System;
using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoFall.Movement.Tests
{
    public sealed class ReferenceTrajectoryTests
    {
        [Serializable] public class ReferenceData { public string sha256; public Trajectory[] cases; }
        [Serializable] public class Trajectory { public string name; public Box[] solids, platforms; public Sample[] samples; }
        [Serializable] public class Box { public float x,y,width,height; public Rect Rect => new Rect(x,y,width,height); }
        [Serializable] public class Sample
        {
            public Command command;
            public float x, y, vx, vy, cameraX, cameraY;
            public bool grounded, airDashUsed;
        }
        [Serializable] public class Command
        {
            public float move;
            public bool down, jumpPressed, jumpReleased, dashPressed;
            public MovementCommand ToMovement() => new MovementCommand { move=move, down=down, jumpPressed=jumpPressed, jumpReleased=jumpReleased, dashPressed=dashPressed };
        }

        [TestCase("run-brake")]
        [TestCase("held-jump")]
        [TestCase("short-jump")]
        [TestCase("jump-dash")]
        [TestCase("platform-drop")]
        [TestCase("thin-wall")]
        [TestCase("ceiling")]
        [TestCase("camera-travel")]
        public void MatchesOriginalPerTickTrajectory(string name)
        {
            var setup = EditorSceneManager.GetSceneManagerSetup();
            var data = JsonUtility.FromJson<ReferenceData>(File.ReadAllText("Docs/Validation/movement-reference.json"));
            var trajectory = Array.Find(data.cases, c => c.name == name);
            MovementTuning tuning = null;
            try
            {
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                tuning = ScriptableObject.CreateInstance<MovementTuning>();
                AddBox(new Rect(0,-1,32,1),8);
                foreach(var box in trajectory.solids) AddBox(box.Rect,8);
                foreach(var box in trajectory.platforms) AddBox(box.Rect,9);
                var go = new GameObject("Reference player");
                var motor = go.AddComponent<PlayerMotor>(); motor.tuning=tuning;
                motor.solids=1<<8; motor.oneWayPlatforms=1<<9; motor.hazards=1<<10;
                motor.roomBounds=new Rect(0,-.88f,32,12); motor.automaticSimulation=false;
                motor.Teleport(new Vector2(1.21f,0));
                Physics2D.SyncTransforms(); motor.Simulate(MovementTuning.Step,default); Physics2D.SyncTransforms();
                var camera = new GameObject("Reference camera",typeof(Camera),typeof(RoomCamera)).GetComponent<RoomCamera>();
                camera.GetComponent<Camera>().orthographic=true; camera.GetComponent<Camera>().orthographicSize=2.7f; camera.GetComponent<Camera>().aspect=16f/9f;
                camera.target=motor; camera.bounds=motor.roomBounds; camera.SnapToTarget();
                for(int i=0;i<trajectory.samples.Length;i++)
                {
                    var s=trajectory.samples[i]; motor.Simulate(MovementTuning.Step,s.command.ToMovement()); Physics2D.SyncTransforms(); camera.Simulate(MovementTuning.Step);
                    string context=$"{name}, tick {i}";
                    Assert.That(motor.Position.x,Is.EqualTo(s.x).Within(.001f),context+" x");
                    Assert.That(motor.Position.y,Is.EqualTo(s.y).Within(.001f),context+" y");
                    Assert.That(motor.Velocity.x,Is.EqualTo(s.vx).Within(.001f),context+" vx");
                    Assert.That(motor.Velocity.y,Is.EqualTo(s.vy).Within(.001f),context+" vy");
                    Assert.That(motor.Grounded,Is.EqualTo(s.grounded),context+" grounded");
                    Assert.That(motor.AirDashUsed,Is.EqualTo(s.airDashUsed),context+" air dash");
                    Assert.That(camera.transform.position.x,Is.EqualTo(s.cameraX).Within(.001f),context+" camera x");
                    Assert.That(camera.transform.position.y,Is.EqualTo(s.cameraY).Within(.001f),context+" camera y");
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(tuning);
                if (Array.Exists(setup, s => s.isLoaded && s.isActive && !string.IsNullOrEmpty(s.path))) EditorSceneManager.RestoreSceneManagerSetup(setup);
                else EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            }
        }

        static void AddBox(Rect rect,int layer)
        {
            var obj=new GameObject("Reference box",typeof(BoxCollider2D)); obj.layer=layer; obj.transform.position=rect.center;
            var box=obj.GetComponent<BoxCollider2D>(); box.size=rect.size; box.isTrigger=layer==9;
        }
    }
}
