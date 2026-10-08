using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace EchoFall.Movement.Tests
{
    public sealed class WorldExpansionPlayTests
    {
        SliceSession S=>SliceSession.Instance;
        string directory;
        [UnitySetUp] public IEnumerator Setup()
        {
            directory=Path.Combine(Application.temporaryCachePath,"phase4-"+System.Guid.NewGuid());
            SliceSession.SavePathOverride=Path.Combine(directory,"save.json");SliceSession.EphemeralSave=true;
            yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            yield return SceneManager.LoadSceneAsync("MovementLab");SliceSession.SavePathOverride=null;SliceSession.EphemeralSave=false;
            if(Directory.Exists(directory))Directory.Delete(directory,true);
        }
        IEnumerator Ready()
        {
            float end=Time.realtimeSinceStartup+15;
            while(S==null || !S.Playing){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}
            foreach(var e in Object.FindObjectsByType<SliceEnemy>())e.enabled=false;
        }
        SliceInteraction Item(string id)=>S.Room.GetComponentsInChildren<SliceInteraction>(true).Single(i=>i.id==id);
        [UnityTest] public IEnumerator EveryDirectedGateTransitionsToNamedEntryWithoutLosingRunState()
        {
            S.Archive.Transfer("mercy");S.Flags.UnionWith(WorldCatalog.Data.flags);S.Defeated.Add("king/boss");S.Consumed.Add("bell-secret");
            foreach(var place in WorldCatalog.Data.rooms)
            foreach(var link in place.links)
            {
                yield return S.LoadRoom(place.id,"default");yield return Ready();
                var gate=Item(link.id);Assert.That(gate.LockReason(S),Is.Null,place.id+"/"+link.id);
                S.Interact(gate);yield return Ready();
                Assert.That(S.Room.id,Is.EqualTo(link.target));Assert.That(S.Visited,Does.Contain(link.target));
                Assert.That(Vector2.Distance(S.motor.Position,S.Room.Spawn(link.entry)),Is.LessThan(.2f),link.id);
                Assert.That(S.Consumed,Does.Contain("bell-secret"));Assert.That(S.Defeated,Does.Contain("king/boss"));
                Assert.That(Object.FindObjectsByType<SliceProjectile>(),Is.Empty);
            }
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ShortcutsUnlockFromFarSideAndRemainOpenOnBacktracking()
        {
            foreach(var route in new[]{new[]{"wake","wake-return","observatory","well-lever","observatory-west","well-link"},new[]{"archive","archive-memory","lungs","breath-lever","lungs-archive","breath-link"},new[]{"mother","mother-high","lungs","lungs-lever","lungs-east","lung-lift"}})
            {
                yield return S.LoadRoom(route[0],"default");yield return Ready();
                // Locate the matching target instead of depending on a presentation name.
                var near=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="gate" && i.requires==route[5]);
                Assert.That(near.LockReason(S),Is.Not.Null);S.Interact(near);Assert.That(S.Room.id,Is.EqualTo(route[0]));
                yield return S.LoadRoom(route[2],"default");yield return Ready();S.Interact(Item(route[3]));
                Assert.That(S.Flags,Does.Contain(route[5]));S.Interact(Item(route[4]));yield return Ready();Assert.That(S.Room.id,Is.EqualTo(route[0]));
                near=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="gate" && i.requires==route[5]);Assert.That(near.LockReason(S),Is.Null);
                S.Interact(near);yield return Ready();Assert.That(S.Room.id,Is.EqualTo(route[2]));
            }
        }
        [UnityTest] public IEnumerator ChildPermissionAndSecretConsumptionPersistAcrossRooms()
        {
            yield return S.LoadRoom("garden","default");yield return Ready();Assert.That(Item("garden-east").LockReason(S),Is.Not.Null);
            S.Interact(Item("child"));S.ChooseOption(0);Assert.That(S.ChildStage,Is.Zero);Assert.That(S.Playing,Is.True);
            S.Interact(Item("child"));S.ChooseOption(1);S.ChooseOption(2);S.ChooseOption(0);
            Assert.That(S.ChildStage,Is.EqualTo(3));Assert.That(Item("garden-east").LockReason(S),Is.Null);
            Assert.That(WorldCatalog.Visible(S.Visited),Does.Not.Contain("rootvault"));
            S.Interact(Item("garden-vault"));yield return Ready();S.Interact(Item("vault-cache"));S.ChooseOption(0);
            Assert.That(S.Consumed,Does.Contain("vault-cache"));S.Interact(Item("vault-return"));yield return Ready();
            Assert.That(Item("garden-east").LockReason(S),Is.Null);S.Interact(Item("garden-vault"));yield return Ready();
            Assert.That(Item("vault-cache").gameObject.activeSelf,Is.False);Assert.That(WorldCatalog.Visible(S.Visited),Does.Contain("rootvault"));
        }
        [UnityTest] public IEnumerator EveryNewAnchorSavesWorldAndLoadoutAndDeathsRebuildEachRegion()
        {
            S.Archive.Transfer("mercy");S.Archive.Transfer("return");S.Archive.Equip("mercy");
            foreach(string id in new[]{"cradle","garden","choir"})
            {
                yield return S.LoadRoom(id,"default");yield return Ready();
                S.Flags.UnionWith(new[]{"king","well-link","garden-path"});S.Defeated.Add("king/boss");S.Consumed.Add("vault-cache");S.ChildStage=2;
                S.EnemyHealth["lungs/enemy-0"]=2;
                var bench=S.Room.GetComponentsInChildren<SliceInteraction>().First(i=>i.kind=="bench");
                S.motor.EnterRoom((Vector2)bench.transform.position-Vector2.up*.25f);yield return new WaitForSeconds(.16f);
                SliceSession.EphemeralSave=false;S.Interact(bench);yield return new WaitForSeconds(.65f);
                Assert.That(S.Archive.checkpoint,Is.Not.Null,id);Assert.That(S.combat.Integrity,Is.EqualTo(6));Assert.That(S.CanChangeMemory,Is.True);
                yield return SceneManager.LoadSceneAsync("WakeSlice");yield return Ready();
                Assert.That(S.Room.id,Is.EqualTo(id));Assert.That(S.Archive.active,Is.EqualTo("mercy"));Assert.That(S.ChildStage,Is.EqualTo(2));
                Assert.That(S.Flags,Does.Contain("well-link"));Assert.That(S.Defeated,Does.Contain("king/boss"));Assert.That(S.EnemyHealth["lungs/enemy-0"],Is.EqualTo(2));Assert.That(S.Consumed,Does.Contain("vault-cache"));
                S.combat.Hurt(99,S.motor.Position-Vector2.right,true,null);Assert.That(S.Screen,Is.EqualTo(SliceScreen.Transfer));
                Assert.That(SliceMemoryStore.Load(SliceSession.SavePathOverride,out _).checkpoint,Is.Null);
                S.ChooseOption(0);yield return Ready();Assert.That(S.Room.id,Is.EqualTo("wake"));Assert.That(S.Flags,Is.Empty);Assert.That(S.Consumed,Is.Empty);Assert.That(S.ChildStage,Is.Zero);
                Assert.That(S.Visited,Is.EquivalentTo(new[]{"wake"}));Assert.That(S.Archive.Remembers("mercy"),Is.True);S.Archive.Equip("mercy");
            }
        }
        [UnityTest] public IEnumerator MapPausesCombatAndHidesDistantRooms()
        {
            var map=S.GetComponent<WorldMap>();map.Open();Assert.That(S.Screen,Is.EqualTo(SliceScreen.Map));Assert.That(S.motor.automaticSimulation,Is.False);
            var old=S.motor.Position;yield return new WaitForSeconds(.2f);Assert.That(S.motor.Position,Is.EqualTo(old));
            Assert.That(GameObject.Find("rootvault"),Is.Null);Assert.That(GameObject.Find("choir"),Is.Null);
            map.Close();yield return null;Assert.That(S.Playing,Is.True);Assert.That(S.motor.automaticSimulation,Is.True);LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator VaultPogoAndRememberedBridgeProvideDifferentReturnRoutes()
        {
            yield return S.LoadRoom("rootvault","entry");yield return Ready();
            Assert.That(S.Room.rootBridge.gameObject.activeSelf,Is.False);
            var bell=Object.FindAnyObjectByType<SlicePogo>();S.motor.automaticSimulation=false;
            S.motor.EnterRoom((Vector2)bell.transform.position+Vector2.up*.3f);S.combat.Strike(false,-1);
            Assert.That(S.motor.Velocity.y,Is.EqualTo(5.35f));Assert.That(S.motor.AirDashUsed,Is.False);
            S.Archive.Transfer("mercy");S.Room.Apply(S);Assert.That(S.Room.rootBridge.gameObject.activeSelf,Is.True);
            S.Archive.Transfer("fire");S.Room.Apply(S);Assert.That(S.Room.rootBridge.gameObject.activeSelf,Is.False);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
