using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace EchoFall.Movement.Tests
{
    public sealed class FinishedPresentationPlayTests
    {
        SliceSession S=>SliceSession.Instance;
        [UnitySetUp] public IEnumerator Setup(){SliceSession.EphemeralSave=true;yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();}
        [UnityTearDown] public IEnumerator Cleanup(){yield return SceneManager.LoadSceneAsync("MovementLab");SliceSession.EphemeralSave=false;}
        IEnumerator Ready(){float limit=Time.realtimeSinceStartup+15;while(S==null||!S.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(limit));yield return null;}}
        [UnityTest] public IEnumerator MaterialsAndDecorationFollowRoomsWithoutAddingColliders()
        {
            foreach(string id in new[]{"wake","cradle","garden"})
            {
                yield return S.LoadRoom(id,"default");yield return Ready();yield return new WaitForSeconds(.15f);
                var decor=S.Room.transform.Find("Finished presentation");Assert.That(decor,Is.Not.Null);Assert.That(decor.GetComponentsInChildren<Collider2D>().Length,Is.Zero);
                SliceSound.Cue("step");var audio=Object.FindAnyObjectByType<SliceSound>();Assert.That(audio.LastFootMaterial,Is.EqualTo(id=="cradle"?FootMaterial.Metal:id=="garden"?FootMaterial.Moss:FootMaterial.Stone));
            }
        }
        [UnityTest] public IEnumerator RememberedCagesAndEchoRemnantsRemainVisualOnly()
        {
            S.Archive.Transfer("sacrifice");yield return S.LoadRoom("cradle","default");yield return Ready();
            int cages=0;foreach(var sprite in S.Room.GetComponentsInChildren<SpriteRenderer>())if(sprite.name=="Authored cage-open")cages++;
            Assert.That(cages,Is.EqualTo(4));int life=S.Archive.loop;
            SliceDeathEcho.Spawn(S.motor.GetComponentInChildren<SpriteRenderer>());yield return new WaitForSeconds(.8f);
            Assert.That(Object.FindObjectsByType<SliceDeathEcho>().Length,Is.Zero);Assert.That(S.Archive.loop,Is.EqualTo(life));
        }
    }
}
