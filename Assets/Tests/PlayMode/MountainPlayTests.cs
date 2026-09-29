using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class MountainPlayTests : PlayWorldTestBase
    {
        [UnityTest]public IEnumerator TopToBottomRunTraversesConnectedMountain()
        {
#if UNITY_EDITOR
            var obj=new GameObject();obj.SetActive(false);var world=obj.AddComponent<MountainWorld>();world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");obj.SetActive(true);yield return null;
            world.player.Input.injected=true;world.player.Input.tuck=true;Time.timeScale=12;
            var areas=new HashSet<string>();float max=0;
            for(int i=0;i<140&&max<1380;i++)
            {
                yield return new WaitForSeconds(1);areas.Add(world.Area);max=Mathf.Max(max,world.player.Body.position.z);
                if(i%20==0)Debug.Log($"RUN t={i} z={world.player.Body.position.z:F0} speed={world.player.Speed:F1} bail={world.player.Bailed} pos={world.player.Body.position} ground={world.player.Grounded} reach={world.player.config.contactReach} L={world.player.Contacts.left.hit}");
                Assert.That(float.IsNaN(world.player.Speed),Is.False);
            }
            Time.timeScale=1;Debug.Log($"MOUNTAIN RUN maximum {max:F0}m; areas {areas.Count}");Assert.That(max,Is.GreaterThan(1380));Assert.That(areas.Count,Is.GreaterThanOrEqualTo(5));Object.Destroy(obj);
#endif
        }
    }
}
