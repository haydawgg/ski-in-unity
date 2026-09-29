using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PowderFlow.Tests
{
    public class RotationAcceptanceTests:PlayWorldTestBase
    {
        [UnityTest]public IEnumerator ActualSpinsAndForwardBackwardFlipsCanLand()
        {
            var plane=GameObject.CreatePrimitive(PrimitiveType.Cube);plane.transform.position=new Vector3(1000,0,0);plane.transform.localScale=new Vector3(100,1,200);plane.AddComponent<SnowSurface>();
            for(int trick=0;trick<7;trick++)
            {
                var obj=new GameObject();obj.transform.position=new Vector3(1000,18,0);obj.AddComponent<Rigidbody>();obj.AddComponent<SkierInput>().injected=true;obj.AddComponent<SkiContactSystem>();var p=obj.AddComponent<SkiPhysicsController>();var ski=ScriptableObject.CreateInstance<SkiPhysicsConfig>();var config=ScriptableObject.CreateInstance<TrickConfig>();config.angularDrag=0;p.Initialize(ski);p.trickConfig=config;
                float airtime=Mathf.Sqrt(2*(18-.5f-ski.contactReach)/Physics.gravity.magnitude);int degrees=trick<5?new[]{180,360,540,720,1080}[trick]:360;
                p.Body.linearVelocity=Vector3.forward*15;p.Air.SetMomentum((trick<5?Vector3.up:Vector3.right*(trick==5?1:-1))*degrees*Mathf.Deg2Rad/airtime*config.baseInertia);
                yield return new WaitForSeconds(airtime+.3f);Assert.That(p.Bailed,Is.False,$"rotation {trick} {degrees}");Assert.That(p.Grounded,Is.True);Assert.That(p.LastLanding.alignment,Is.LessThan(config.cleanAlignment));
                Object.Destroy(obj);Object.Destroy(ski);Object.Destroy(config);yield return null;
            }
            Object.Destroy(plane);
        }
    }
}
