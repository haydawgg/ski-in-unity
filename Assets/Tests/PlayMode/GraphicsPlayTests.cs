using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class GraphicsPlayTests : PlayWorldTestBase
    {
        [UnityTest]public IEnumerator SunsetDayAndSceneryRender()
        {
#if UNITY_EDITOR
            var obj=new GameObject();obj.SetActive(false);var world=obj.AddComponent<MountainWorld>();world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");world.graphicsConfig=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");obj.SetActive(true);yield return null;
            var p=world.player;p.Input.BeginInjected();world.Respawn(1);Camera camera=null;foreach(var follow in Object.FindObjectsByType<SkiCameraController>(FindObjectsSortMode.None))if(follow.skier==p)camera=follow.GetComponent<Camera>();
            yield return new WaitForSeconds(.4f);
            camera.transform.position=p.Body.position+new Vector3(0,2.2f,-6);camera.transform.LookAt(p.Body.position+Vector3.up*.4f);camera.fieldOfView=73;
            Capture(camera,"Sunset",1920,1080);world.GetComponent<AlpineLighting>().Apply(true);yield return null;camera.transform.position=p.Body.position+new Vector3(0,2.2f,-6);camera.transform.LookAt(p.Body.position+Vector3.up*.4f);Capture(camera,"Day",1920,1080);
            foreach(bool day in new[]{false,true})
            {
                world.GetComponent<AlpineLighting>().Apply(day);yield return null;camera.fieldOfView=65;
                camera.transform.position=world.OnSnow(-25,355,3);camera.transform.LookAt(world.OnSnow(10,450,3));
                Capture(camera,day?"DayParkDetail":"SunsetParkDetail",1920,1080);
                camera.transform.position=world.OnSnow(-65,70,9);camera.transform.LookAt(world.OnSnow(0,1000,12));
                Capture(camera,day?"DayVista":"SunsetVista",1920,1080);
            }
            Assert.That(p.GetComponent<SnowTrackSystem>(),Is.Not.Null);Assert.That(p.GetComponent<SnowEffectsController>(),Is.Not.Null);Object.Destroy(obj);
#endif
        }
        static void Capture(Camera camera,string name,int width,int height)
        {
            var old=camera.targetTexture;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();Directory.CreateDirectory("Documentation/Screenshots");File.WriteAllBytes("Documentation/Screenshots/"+name+".png",image.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=null;Object.Destroy(image);Object.Destroy(rt);
        }
    }
}
