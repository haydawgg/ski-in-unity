using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class KickerTests
    {
        [Test] public void ContinuousKickerLaunchesFromEntryMomentum()
        {
            var prior=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            var snow=GameObject.CreatePrimitive(PrimitiveType.Cube);snow.transform.position=new Vector3(0,-.5f,0);snow.transform.localScale=new Vector3(100,1,100);snow.AddComponent<SnowSurface>();
            var kicker=TestFeatureBuilder.Kicker(Vector3.zero,8,12,4);
            var obj=new GameObject();obj.transform.position=new Vector3(0,.75f,-5);obj.AddComponent<Rigidbody>();obj.AddComponent<SkierInput>().injected=true;obj.AddComponent<SkiContactSystem>();
            var driver=obj.AddComponent<SkiPhysicsController>();var config=ScriptableObject.CreateInstance<SkiPhysicsConfig>();driver.Initialize(config);driver.Body.linearVelocity=Vector3.forward*18;
            Physics.SyncTransforms();bool launched=false;Vector3 launch=Vector3.zero;
            for(int i=0;i<300;i++)
            {
                driver.Step(.01f);Physics.Simulate(.01f);
                if(!driver.Grounded && driver.Body.position.z>11 && driver.Body.position.y>3){launched=true;launch=driver.Body.linearVelocity;break;}
            }
            TestContext.WriteLine($"Kicker launch velocity {launch}");Assert.That(launched,Is.True);Assert.That(launch.y,Is.GreaterThan(3));Assert.That(launch.z,Is.GreaterThan(8));
            Object.DestroyImmediate(obj);Object.DestroyImmediate(kicker);Object.DestroyImmediate(snow);Object.DestroyImmediate(config);Physics.simulationMode=prior;
        }
    }
}
