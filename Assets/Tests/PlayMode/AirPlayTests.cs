using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PowderFlow.Tests
{
    public class AirPlayTests
    {
        [UnityTest] public IEnumerator FullSpinCanLandWithForwardMomentum()
        {
            var plane=GameObject.CreatePrimitive(PrimitiveType.Cube);plane.transform.position=new Vector3(1000,0,0);plane.transform.localScale=new Vector3(100,1,200);plane.AddComponent<SnowSurface>();
            var obj=new GameObject();obj.transform.position=new Vector3(1000,8,0);obj.AddComponent<Rigidbody>();obj.AddComponent<SkierInput>().injected=true;obj.AddComponent<SkiContactSystem>();
            var driver=obj.AddComponent<SkiPhysicsController>();var ski=ScriptableObject.CreateInstance<SkiPhysicsConfig>();var tricks=ScriptableObject.CreateInstance<TrickConfig>();driver.Initialize(ski);driver.trickConfig=tricks;
            driver.Body.linearVelocity=Vector3.forward*15;
            float airtime=Mathf.Sqrt(2*(8-.5f-ski.contactReach)/Physics.gravity.magnitude);
            driver.Air.SetMomentum(Vector3.up*(2*Mathf.PI/airtime)*tricks.baseInertia);
            yield return new WaitForSeconds(airtime+.25f);
            Assert.That(driver.Bailed,Is.False);Assert.That(driver.LastLanding.alignment,Is.LessThan(tricks.cleanAlignment));Assert.That(driver.Speed,Is.GreaterThan(10));Assert.That(driver.Grounded,Is.True);
            Object.Destroy(obj);Object.Destroy(plane);Object.Destroy(ski);Object.Destroy(tricks);
        }
    }
}
