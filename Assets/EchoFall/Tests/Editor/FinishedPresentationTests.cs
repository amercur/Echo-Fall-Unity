using NUnit.Framework;
using UnityEngine;
namespace EchoFall.Movement.Tests
{
    public sealed class FinishedPresentationTests
    {
        [Test] public void DedicatedPosesAndPropsAreAvailable()
        {
            foreach(string state in new[]{"Rise","Fall","Land","WallSlide","WallJump","Dash","Hurt","Death","Transfer","Up","Down","Charged","Combo2","Combo3"})
            {var frames=Resources.LoadAll<Sprite>("Presentation/Player/"+state);Assert.That(frames.Length,Is.GreaterThanOrEqualTo(4),state);foreach(var f in frames){Assert.That(f.pixelsPerUnit,Is.EqualTo(100));Assert.That(f.pivot.y,Is.EqualTo(30.47082f).Within(.01f));}}
            foreach(string prop in new[]{"keeper","child","creature","tree","vessel","cage","cage-open","plinth"})Assert.That(Resources.Load<Sprite>("Presentation/Props/"+prop),Is.Not.Null,prop);
        }
        [Test] public void AuthoredAudioPaletteAndDissolveAreComplete()
        {
            var p=Resources.Load<AudioPalette>("EchoAudio");Assert.That(p,Is.Not.Null);
            foreach(var f in typeof(AudioPalette).GetFields())Assert.That(f.GetValue(p),Is.Not.Null,f.Name);
            Assert.That(p.stepStone,Is.Not.SameAs(p.stepMetal));Assert.That(p.gardenAmbience,Is.Not.SameAs(p.choirAmbience));
            Assert.That(Resources.Load<Shader>("Presentation/EchoDissolve"),Is.Not.Null);
        }
    }
}
