using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class PlayWorldTestBase
    {
        [TearDown]public void RemoveGeneratedTestWorlds()
        {
            foreach(var world in Object.FindObjectsByType<MountainWorld>(FindObjectsSortMode.None))if(world)Object.DestroyImmediate(world.gameObject);
            foreach(var world in Object.FindObjectsByType<PhysicsTestWorld>(FindObjectsSortMode.None))if(world)Object.DestroyImmediate(world.gameObject);
            Time.timeScale=1;
        }
    }
}
