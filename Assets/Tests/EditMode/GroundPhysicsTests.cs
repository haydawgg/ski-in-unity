using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class GroundPhysicsTests
    {
        GameObject slope,skier; SkiPhysicsController controller; SkiPhysicsConfig config; SimulationMode previous;
        [SetUp] public void SetUp()
        {
            previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            config=ScriptableObject.CreateInstance<SkiPhysicsConfig>();config.resetDepth=-100000;
            slope=GameObject.CreatePrimitive(PrimitiveType.Cube);slope.transform.localScale=new Vector3(2000,1,20000);slope.AddComponent<SnowSurface>();
            skier=new GameObject("Test skier");skier.AddComponent<Rigidbody>();skier.AddComponent<SkierInput>().injected=true;skier.AddComponent<SkiContactSystem>();
            controller=skier.AddComponent<SkiPhysicsController>();controller.Initialize(config);
        }
        void Place(float angle,float speed=0)
        {
            slope.transform.rotation=Quaternion.Euler(angle,0,0);
            controller.ResetTo(slope.transform.up*(config.rideHeight+.5f),slope.transform.rotation);
            controller.Body.linearVelocity=slope.transform.forward*speed;Physics.SyncTransforms();
        }
        void Simulate(float seconds){for(int i=0;i<seconds/.01f;i++){controller.Step(.01f);Physics.Simulate(.01f);if(seconds>50 && i%2000==0)TestContext.WriteLine($"t {i*.01f} speed {controller.Speed} grounded {controller.Grounded} height {controller.Contacts.Height} pos {controller.Body.position}");}}
        [TearDown] public void TearDown(){Object.DestroyImmediate(skier);Object.DestroyImmediate(slope);Object.DestroyImmediate(config);Physics.simulationMode=previous;}
        [Test] public void GravityAcceleratesAndUphillSlows()
        {Place(20);Simulate(5);Assert.That(controller.Speed,Is.GreaterThan(10));Assert.That(controller.Grounded,Is.True);Place(-15,15);Simulate(3);Assert.That(controller.Speed,Is.LessThan(15));}
        [Test] public void TerminalSpeedAndTuckAreSane()
        {Place(20);Simulate(100);float normal=controller.Speed;controller.Input.tuck=true;Place(20);Simulate(100);float tuck=controller.Speed;TestContext.WriteLine($"Terminal upright {normal:F2}, tuck {tuck:F2}");Assert.That(normal,Is.InRange(25f,40f));Assert.That(tuck,Is.GreaterThan(normal+5));Assert.That(tuck,Is.LessThan(45));}
        [Test] public void SwitchCarvingUsesTravelDirection()
        {Place(15,15);controller.Body.rotation=Quaternion.AngleAxis(180,slope.transform.up)*slope.transform.rotation;controller.Input.steer=.7f;Simulate(2);Assert.That(controller.Body.linearVelocity.x,Is.GreaterThan(1));}
        [Test] public void BrakeStopsAndPowderSlows()
        {Place(0,15);controller.Input.brake=true;Simulate(3);Assert.That(controller.Speed,Is.LessThan(1));controller.Input.brake=false;Place(15,15);slope.GetComponent<SnowSurface>().type=SurfaceType.Powder;Simulate(5);Assert.That(controller.Speed,Is.LessThan(15));Assert.That(controller.Contacts.Surface,Is.EqualTo(SurfaceType.Powder));}
        [Test] public void CarvingTurnsSmoothlyAndKeepsContact()
        {Place(15,15);controller.Input.steer=.7f;Simulate(3);Assert.That(Mathf.Abs(controller.Body.rotation.eulerAngles.y),Is.GreaterThan(5));Assert.That(controller.Grounded,Is.True);Assert.That(controller.Speed,Is.GreaterThan(8));}
    }
}
