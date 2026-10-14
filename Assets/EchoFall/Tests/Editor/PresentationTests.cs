using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
namespace EchoFall.Movement.Tests
{
    public sealed class PresentationTests
    {
        [Test] public void CameraConvergenceIsIndependentOfRenderRate()
        {
            EditorSceneManager.OpenScene("Assets/EchoFall/Scenes/MovementLab.unity");
            var motor=Object.FindAnyObjectByType<PlayerMotor>();var camera=Object.FindAnyObjectByType<RoomCamera>();
            motor.Teleport(new Vector2(15,5));camera.target=motor;camera.SnapToTarget();
            Vector3 start=camera.transform.position-Vector3.right;
            Vector3 expected=Vector3.zero;
            foreach(int rate in new[]{30,60,144})
            {
                camera.transform.position=start;
                for(int i=0;i<rate;i++)camera.Simulate(1f/rate);
                if(rate==30)expected=camera.transform.position;
                else Assert.That(Vector3.Distance(expected,camera.transform.position),Is.LessThan(.0001f));
            }
        }
    }
}
