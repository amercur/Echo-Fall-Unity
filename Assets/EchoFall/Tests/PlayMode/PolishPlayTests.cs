using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoFall.Movement.Tests
{
    public sealed class PolishPlayTests
    {
        SliceSession S=>SliceSession.Instance;
        [UnitySetUp] public IEnumerator Setup(){SliceSession.EphemeralSave=true;yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();}
        [UnityTearDown] public IEnumerator Cleanup(){yield return SceneManager.LoadSceneAsync("MovementLab");SliceSession.EphemeralSave=false;}
        IEnumerator Ready(){float limit=Time.realtimeSinceStartup+15;while(S==null||!S.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(limit));yield return null;}}
        [UnityTest] public IEnumerator MusicTransportSurvivesPauseAndRoomChange()
        {
            var sound=Object.FindAnyObjectByType<SliceSound>();yield return null;int count=sound.MusicChanges;S.ShowSettings();yield return new WaitForSeconds(.1f);Assert.That(sound.MusicChanges,Is.EqualTo(count));
            S.SetScreen(SliceScreen.Playing);yield return S.LoadRoom("archive","default");yield return Ready();Assert.That(sound.MusicChanges,Is.EqualTo(count));
            S.EndRun(false);yield return null;Assert.That(sound.MusicContext,Is.EqualTo(3));
        }
        [UnityTest] public IEnumerator RecalledEchoesDoNotAccumulateAfterRefusalOrDeath()
        {
            S.Archive.Transfer("respect");yield return S.LoadRoom("mother","default");yield return Ready();var m=SliceMother.Active;m.enabled=false;S.motor.automaticSimulation=false;
            for(int i=0;i<12;i++){var echo=m.Recall();yield return null;echo.enabled=false;S.motor.Teleport((Vector2)echo.transform.position+Vector2.left*.4f);echo.Tick(1.4f);yield return null;Assert.That(echo==null,Is.True);}
            S.Archive.Forget("respect");var hostile=m.Recall();yield return null;hostile.Hit(100,.55f);yield return null;Assert.That(hostile==null,Is.True);
        }
        [UnityTest] public IEnumerator ZeroShakeDoesNotAffectMovementAndSettingsKeepLife()
        {
            var prefs=PlayerPreferences.Current;float old=prefs.shake;try{prefs.shake=0;var impulse=Camera.main.GetComponent<PlayerCameraImpulse>();impulse.Kick(.06f);yield return null;Assert.That(impulse.Offset,Is.EqualTo(Vector2.zero));int life=S.Archive.loop;S.ShowSettings();Assert.That(S.OptionLabels.Count,Is.EqualTo(9));Assert.That(S.Archive.loop,Is.EqualTo(life));S.ChooseOption(7);Assert.That(S.Playing,Is.True);}finally{prefs.shake=old;}
        }
    }
}
