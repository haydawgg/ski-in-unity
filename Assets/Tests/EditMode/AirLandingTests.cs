using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class AirLandingTests
    {
        [Test] public void AngularMomentumConservedAndTuckSpeedsSpin()
        {
            var c=ScriptableObject.CreateInstance<TrickConfig>();c.angularDrag=0;
            var o=new GameObject();var body=o.AddComponent<Rigidbody>();var input=o.AddComponent<SkierInput>();input.injected=true;var air=o.AddComponent<AirControlSystem>();
            body.angularVelocity=new Vector3(0,4,0);air.Begin(body,input,c,Vector3.zero);
            for(int i=0;i<200;i++)air.Step(body,input,c,.01f);
            Assert.That(body.angularVelocity.y,Is.EqualTo(4).Within(.001));
            var momentum=air.AngularMomentum;input.tuck=true;air.Step(body,input,c,.01f);
            Assert.That(air.AngularMomentum,Is.EqualTo(momentum));Assert.That(body.angularVelocity.y,Is.GreaterThan(6));
            Object.DestroyImmediate(o);Object.DestroyImmediate(c);
        }
        [Test] public void SlopeTransitionPreservesMoreSpeedAndFailedTrickBails()
        {
            var c=ScriptableObject.CreateInstance<TrickConfig>();
            var flat=LandingSystem.Evaluate(Quaternion.identity,new Vector3(0,-12,20),Vector3.zero,Vector3.up,true,c);
            var r=Quaternion.Euler(25,0,0);var slope=LandingSystem.Evaluate(r,r*new Vector3(0,-6,20),Vector3.zero,r*Vector3.up,true,c);
            Assert.That(slope.retention,Is.GreaterThan(flat.retention));
            Assert.That(LandingSystem.Evaluate(Quaternion.Euler(130,0,0),new Vector3(0,-15,20),Vector3.zero,Vector3.up,true,c).quality,Is.EqualTo(LandingQuality.Bail));
            Assert.That(LandingSystem.Evaluate(Quaternion.Euler(0,180,0),new Vector3(0,-4,20),Vector3.zero,Vector3.up,true,c).switched,Is.True);
            Object.DestroyImmediate(c);
        }
        [Test] public void FifteenMeterPerSecondRampLaunchFollowsBallisticArc()
        {
            var prior=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            var o=new GameObject();var body=o.AddComponent<Rigidbody>();body.linearDamping=0;
            float angle=25*Mathf.Deg2Rad,speed=15;body.linearVelocity=new Vector3(0,speed*Mathf.Sin(angle),speed*Mathf.Cos(angle));
            float time=2*speed*Mathf.Sin(angle)/Physics.gravity.magnitude;
            for(int i=0;i<Mathf.RoundToInt(time/.01f);i++)Physics.Simulate(.01f);
            Assert.That(body.position.z,Is.EqualTo(speed*Mathf.Cos(angle)*time).Within(.25));Assert.That(body.position.y,Is.EqualTo(0).Within(.2));
            Object.DestroyImmediate(o);Physics.simulationMode=prior;
        }
    }
}
