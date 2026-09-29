using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PowderFlow.Tests
{
    public class WorldSmokeTests
    {
        [UnityTest] public IEnumerator DownhillSceneKeepsSkierStableDuringCarving()
        {
            var root=new GameObject("Test world");root.SetActive(false);
            var world=root.AddComponent<PhysicsTestWorld>();world.physicsConfig=ScriptableObject.CreateInstance<SkiPhysicsConfig>();world.cameraConfig=ScriptableObject.CreateInstance<CameraConfig>();world.trickConfig=ScriptableObject.CreateInstance<TrickConfig>();
            root.SetActive(true);yield return null;
            world.player.Input.injected=true;
            Time.timeScale=10;
            yield return new WaitForSeconds(4);
            Assert.That(world.player.Speed,Is.GreaterThan(6));
            world.player.Input.steer=.4f;
            yield return new WaitForSeconds(2);
            Assert.That(world.player.Grounded,Is.True);
            Assert.That(float.IsNaN(world.player.Speed),Is.False);
            world.player.Input.brake=true;
            yield return new WaitForSeconds(3);
            Assert.That(world.player.Speed,Is.LessThan(3));
            Time.timeScale=1;
        }
    }
}
