using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PowderFlow.Tests
{
    public class RailPlayTests
    {
        [UnityTest] public IEnumerator FlatDownAndKinkRailsCaptureAndExitWithMomentum()
        {
            for(int variant=0;variant<3;variant++)
            {
                var root=new GameObject();root.SetActive(false);var world=root.AddComponent<PhysicsTestWorld>();world.physicsConfig=ScriptableObject.CreateInstance<SkiPhysicsConfig>();world.cameraConfig=ScriptableObject.CreateInstance<CameraConfig>();world.trickConfig=ScriptableObject.CreateInstance<TrickConfig>();root.SetActive(true);yield return null;
                var path=new GameObject("Test rail").AddComponent<RailPath>();path.transform.position=new Vector3(1000,10,0);path.railId=variant.ToString();path.points=variant==2?new[]{Vector3.zero,new Vector3(0,-1,5),new Vector3(0,-3,10)}:new[]{Vector3.zero,new Vector3(0,variant==0?0:-2,10)};
                var p=world.player;p.Input.injected=true;p.ResetTo(new Vector3(1000,10.9f,1),Quaternion.identity);p.Body.linearVelocity=new Vector3(0,-1,8);
                yield return new WaitForSeconds(.1f);var rail=p.GetComponent<RailSystem>();Assert.That(rail.Riding,Is.True);
                yield return new WaitForSeconds(1.8f);Assert.That(rail.Riding,Is.False);Assert.That(p.Speed,Is.GreaterThan(6));
                Object.Destroy(path.gameObject);Object.Destroy(p.gameObject);Object.Destroy(root);yield return null;
            }
        }
    }
}
