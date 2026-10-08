using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace EchoFall.Movement.Tests
{
    public sealed class WorldExpansionTests
    {
        [TearDown] public void Cleanup()=>EditorSceneManager.OpenScene("Assets/EchoFall/Scenes/WakeSlice.unity");
        [Test] public void AllAuthoredTransitionsMatchCatalogAndNamedSafeEntries()
        {
            var entries=new Dictionary<string,HashSet<string>>();
            foreach(var place in WorldCatalog.Data.rooms)
            {
                string path="Assets/EchoFall/Scenes/Wake_"+place.id+".unity";
                Assert.That(EditorBuildSettings.scenes.Any(s=>s.enabled && s.path==path),Is.True,path);
                EditorSceneManager.OpenScene(path);var room=Object.FindAnyObjectByType<SliceRoom>();
                entries[place.id]=new HashSet<string>(room.spawns.Select(s=>s.id));
                foreach(var transform in room.GetComponentsInChildren<Transform>(true))Assert.That(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject),Is.Zero,place.id);
                foreach(var sprite in room.GetComponentsInChildren<SpriteRenderer>(true))Assert.That(sprite.sprite,Is.Not.Null,sprite.name);
                var gates=room.GetComponentsInChildren<SliceInteraction>(true).Where(i=>i.kind=="gate").ToArray();
                Assert.That(gates.Length,Is.EqualTo(place.links.Length),place.id);
                foreach(var link in place.links)
                {
                    var gate=gates.Single(g=>g.id==link.id);Assert.That(gate.target,Is.EqualTo(link.target));Assert.That(gate.entry,Is.EqualTo(link.entry));
                    Assert.That(gate.requires??"",Is.EqualTo(link.requires));Assert.That(gate.memory??"",Is.EqualTo(link.memory));Assert.That(gate.blockedBy??"",Is.EqualTo(link.blockedBy));
                }
                foreach(var e in room.GetComponentsInChildren<SliceEnemy>(true))Assert.That(WorldCatalog.EnemyMax(e.id),Is.EqualTo(e.hp),e.id);
                foreach(var spawn in room.spawns)
                {
                    Assert.That(room.bounds.Contains(spawn.feet),Is.True,place.id+"/"+spawn.id);
                    Assert.That(Physics2D.OverlapBox(spawn.feet+Vector2.up*.22f,new Vector2(.20f,.36f),0,LayerMask.GetMask("EchoSolid")),Is.Null,place.id+"/"+spawn.id+" inside solid");
                }
            }
            foreach(var place in WorldCatalog.Data.rooms)foreach(var link in place.links)Assert.That(entries[link.target],Does.Contain(link.entry),place.id+" -> "+link.target);
        }
        [Test] public void WorldIsConnectedAndSecretDoesNotLeakIntoMapFog()
        {
            var visited=new HashSet<string>{"wake"};var queue=new Queue<string>(visited);
            while(queue.Count>0)foreach(var link in WorldCatalog.Find(queue.Dequeue()).links)if(visited.Add(link.target))queue.Enqueue(link.target);
            Assert.That(visited.Count,Is.EqualTo(13));
            var fog=WorldCatalog.Visible(new HashSet<string>{"garden"});Assert.That(fog,Does.Contain("observatory"));Assert.That(fog,Does.Not.Contain("rootvault"));Assert.That(fog,Does.Not.Contain("wake"));
            Assert.That(WorldCatalog.Visible(new HashSet<string>{"garden","rootvault"}),Does.Contain("rootvault"));
        }
        [Test] public void ExpandedCheckpointRemainsStrictAndOldArchiveCompatible()
        {
            var archive=new SliceArchive();var checkpoint=new SliceCheckpoint{loop=1,archive="",room="garden",bench="garden-rest",resonance=1.25f,childStage=2};
            checkpoint.visited.AddRange(WorldCatalog.Data.rooms.Select(r=>r.id));checkpoint.flags.AddRange(new[]{"king","lung-lift","well-link","breath-link","garden-path"});
            checkpoint.consumed.Add("vault-cache");checkpoint.enemies.Add(new SliceEnemySnapshot{id="mother/enemy-0",hp=3});
            Assert.That(checkpoint.Valid(archive),Is.True);archive.checkpoint=checkpoint;
            Assert.That(JsonUtility.FromJson<SliceArchive>(JsonUtility.ToJson(archive)).checkpoint.Valid(archive),Is.True);
            checkpoint.bench="rest-wake";Assert.That(checkpoint.Valid(archive),Is.False);checkpoint.bench="garden-rest";
            checkpoint.childStage=4;Assert.That(checkpoint.Valid(archive),Is.False);checkpoint.childStage=2;
            checkpoint.flags.Add("mother");Assert.That(checkpoint.Valid(archive),Is.False,"No invented Mother victory.");
            Assert.That(JsonUtility.FromJson<SliceArchive>("{\"version\":1,\"loop\":1,\"active\":\"return\",\"memories\":[]}").Valid(),Is.True);
        }
        [TestCase("cradle",1.85f,0f,3f,1f,4.9f,2f,7f,3f)]
        [TestCase("garden",2.15f,0f,3.5f,1.02f,6.2f,2.07f,7.95f,2.92f)]
        [TestCase("mother",3.3f,0f,4f,1.1f,3.3f,1.67f,2.5f,2.47f)]
        public void TraversalStairsReachHighRouteWithoutChangingMovement(string id,float startX,float startY,float x1,float y1,float x2,float y2,float x3,float y3)
        {
            EditorSceneManager.OpenScene("Assets/EchoFall/Scenes/Wake_"+id+".unity");
            var room=Object.FindAnyObjectByType<SliceRoom>();var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EchoFall/Prefabs/Player.prefab"));
            var motor=go.GetComponent<PlayerMotor>();motor.automaticSimulation=false;motor.roomBounds=room.bounds;motor.EnterRoom(new Vector2(startX,startY+.02f));
            for(int n=0;n<30;n++){motor.Simulate(MovementTuning.Step,default);Physics2D.SyncTransforms();}
            foreach(var target in new[]{new Vector2(x1,y1),new Vector2(x2,y2),new Vector2(x3,y3)})
            {
                bool arrived=false;
                float direction=Mathf.Sign(target.x-motor.Position.x);
                if(motor.Position.y>.1f)
                {
                    var support=room.GetComponentsInChildren<BoxCollider2D>().First(c=>Mathf.Abs(c.bounds.max.y-motor.Position.y)<.04f && c.bounds.min.x<=motor.Position.x && c.bounds.max.x>=motor.Position.x);
                    float edge=direction>0?support.bounds.max.x-.18f:support.bounds.min.x+.18f;
                    for(int n=0;n<150 && direction*(edge-motor.Position.x)>.06f;n++)
                    {motor.Simulate(MovementTuning.Step,new MovementCommand{move=direction});Physics2D.SyncTransforms();}
                }
                for(int n=0;n<220;n++)
                {
                    float dx=target.x-motor.Position.x;float move=Mathf.Abs(dx)<.04f?0:Mathf.Sign(dx);
                    motor.Simulate(MovementTuning.Step,new MovementCommand{move=move,jumpPressed=n==0});Physics2D.SyncTransforms();
                    if(n>10 && motor.Grounded && Mathf.Abs(motor.Position.y-target.y)<.04f){arrived=true;break;}
                }
                Assert.That(arrived,Is.True,id+" target "+target+" reached "+motor.Position);
            }
            motor.Simulate(MovementTuning.Step,new MovementCommand{down=true,jumpPressed=true});
            for(int n=0;n<25;n++)motor.Simulate(MovementTuning.Step,default);
            Assert.That(motor.Position.y,Is.LessThan(y3-.15f),"Drop-through remains available.");
        }
        [Test] public void SuspendedLungsWallJumpRouteReachesUpperMachinery()
        {
            EditorSceneManager.OpenScene("Assets/EchoFall/Scenes/Wake_lungs.unity");
            var room=Object.FindAnyObjectByType<SliceRoom>();var go=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EchoFall/Prefabs/Player.prefab"));
            var motor=go.GetComponent<PlayerMotor>();motor.automaticSimulation=false;motor.roomBounds=room.bounds;motor.EnterRoom(new Vector2(4.8f,2.24f));
            for(int n=0;n<30;n++){motor.Simulate(MovementTuning.Step,default);Physics2D.SyncTransforms();}
            bool arrived=false;
            for(int n=0;n<1800;n++)
            {
                float dx=6.95f-motor.Position.x;
                motor.Simulate(MovementTuning.Step,new MovementCommand{move=Mathf.Abs(dx)<.05f?0:Mathf.Sign(dx),jumpPressed=n==0 || motor.Wall!=0 && n%12==0});Physics2D.SyncTransforms();
                if(motor.Grounded && motor.Position.x>6.5f && motor.Position.y>3.4f){arrived=true;break;}
            }
            Assert.That(motor.WallJumpSequence,Is.GreaterThan(0));Assert.That(arrived,Is.True,motor.Position.ToString());
        }
    }
}
