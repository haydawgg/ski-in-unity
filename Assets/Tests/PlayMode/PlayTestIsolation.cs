using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    [SetUpFixture]public class PlayTestIsolation
    {
        [OneTimeSetUp]public void ClearOpenGameLevelBeforeTestFixtures()
        {
            foreach(var world in Object.FindObjectsByType<MountainWorld>(FindObjectsSortMode.None))if(world)Object.DestroyImmediate(world.gameObject);
            foreach(var world in Object.FindObjectsByType<PhysicsTestWorld>(FindObjectsSortMode.None))if(world)Object.DestroyImmediate(world.gameObject);
            Time.timeScale=1;GameFlow.ScoreSession=false;
        }
    }
}
