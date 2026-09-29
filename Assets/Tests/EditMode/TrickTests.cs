using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class TrickTests
    {
        [TestCase(180)][TestCase(360)][TestCase(540)][TestCase(720)][TestCase(1080)]
        public void SpinsQuantizeWithinTolerance(int spin){Assert.That(TrickTracker.Quantize(spin-20,30),Is.EqualTo(spin));}
        [Test]public void CompositionNamesAndScoresActualMotion()
        {
            var c=ScriptableObject.CreateInstance<TrickConfig>();var r=new TrickRecord{yaw=710,pitch=110,roll=150,grab="Safety",grabTime=.8f,landing=LandingQuality.Clean};
            Assert.That(TrickTracker.Name(r,c),Is.EqualTo("Cork 720 Safety"));int score=ComboSystem.Calculate(r,c);r.landing=LandingQuality.Perfect;Assert.That(ComboSystem.Calculate(r,c),Is.GreaterThan(score));r.landing=LandingQuality.Bail;Assert.That(ComboSystem.Calculate(r,c),Is.Zero);
            r=new TrickRecord{pitch=-355,grab="Mute",grabTime=1};Assert.That(TrickTracker.Name(r,c),Is.EqualTo("Backflip Mute"));Object.DestroyImmediate(c);
        }
        [Test]public void TrackerAccumulatesBeyondAFullRevolution()
        {
            var o=new GameObject();var tracker=o.AddComponent<TrickTracker>();for(int i=0;i<=144;i++)tracker.TrackRotation(Quaternion.Euler(0,i*5,0));Assert.That(tracker.current.yaw,Is.EqualTo(720).Within(.2));Object.DestroyImmediate(o);
        }
    }
}
