using NUnit.Framework;
using UnityEngine;
namespace EchoFall.Movement.Tests
{
    public sealed class Phase3CombatRuleTests
    {
        [Test] public void BladeVolumesPreserveDirectionalReachAndGroundedDownFallsBackToSide()
        {
            var side=CombatRules.Blade(Vector2.zero,1,1,false,0,true);
            Assert.That(side.Contains(new Vector2(.59f,.2f)),Is.True);Assert.That(side.Contains(new Vector2(.7f,.2f)),Is.False);
            var up=CombatRules.Blade(Vector2.zero,1,1,false,1,false);Assert.That(up.Contains(new Vector2(0,1)),Is.True);Assert.That(up.Contains(new Vector2(.5f,.2f)),Is.False);
            var down=CombatRules.Blade(Vector2.zero,1,1,false,-1,false);Assert.That(down.Contains(new Vector2(0,-.48f)),Is.True);Assert.That(down.Contains(new Vector2(0,-.6f)),Is.False);
            Assert.That(CombatRules.Blade(Vector2.zero,1,1,false,-1,true),Is.EqualTo(side));
            Assert.That(CombatRules.Blade(Vector2.zero,-1,3,true,0,true).Contains(new Vector2(-1,.2f)),Is.True);
        }
        [Test] public void SweptProjectileCrossesSmallActorsButDoesNotHitParallelMisses()
        {
            var box=new Rect(-.11f,0,.22f,.4f);
            Assert.That(CombatRules.Segment(box,new Vector2(-3,.2f),new Vector2(3,.2f),.07f),Is.True);
            Assert.That(CombatRules.Segment(box,new Vector2(-3,.6f),new Vector2(3,.6f),.07f),Is.False);
            Assert.That(CombatRules.Segment(box,new Vector2(0,2),new Vector2(0,-2)),Is.True);
        }
        [Test] public void Phase2CheckpointsRemainCompatibleAndCourtDataIsValidated()
        {
            var archive=new SliceArchive();
            var cp=new SliceCheckpoint{loop=1,archive="",room="wake",bench="rest-wake",resonance=1};cp.visited.Add("wake");
            Assert.That(cp.Valid(archive),Is.True);
            cp.resonance=1.75f;cp.visited.Add("king");cp.kingStage=2;cp.kingBreaks=3;cp.kingStyle="air";cp.enemies.Add(new SliceEnemySnapshot{id="king/boss",hp=19});
            archive.checkpoint=cp;var copy=JsonUtility.FromJson<SliceArchive>(JsonUtility.ToJson(archive));
            Assert.That(copy.checkpoint.Valid(copy),Is.True);Assert.That(copy.checkpoint.resonance,Is.EqualTo(1.75f));
            cp.enemies[0].hp=41;Assert.That(cp.Valid(archive),Is.False);cp.enemies[0].hp=19;
            cp.kingStage=3;Assert.That(cp.Valid(archive),Is.False);cp.kingStage=2;cp.resonance=float.NaN;Assert.That(cp.Valid(archive),Is.False);
        }
    }
}
