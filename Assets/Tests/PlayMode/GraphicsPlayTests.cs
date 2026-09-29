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
        [UnityTest]public IEnumerator SunsetDayPortraitAndPerformanceRender()
        {
#if UNITY_EDITOR
            var obj=new GameObject();obj.SetActive(false);var world=obj.AddComponent<MountainWorld>();world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");world.graphicsConfig=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");obj.SetActive(true);yield return null;
            var p=world.player;p.Input.injected=true;world.Respawn(1);Camera camera=null;foreach(var follow in Object.FindObjectsByType<SkiCameraController>(FindObjectsSortMode.None))if(follow.skier==p)camera=follow.GetComponent<Camera>();
            yield return new WaitForSeconds(.4f);
            camera.transform.position=p.Body.position+new Vector3(0,2.2f,-6);camera.transform.LookAt(p.Body.position+Vector3.up*.4f);camera.fieldOfView=73;
            var benchmark=new RenderTexture(1920,1080,24);camera.targetTexture=benchmark;var pixel=new Texture2D(1,1,TextureFormat.RGB24,false);
            for(int i=0;i<10;i++){camera.Render();RenderTexture.active=benchmark;pixel.ReadPixels(new Rect(0,0,1,1),0,0);}
            double start=Time.realtimeSinceStartupAsDouble;for(int i=0;i<90;i++){camera.Render();RenderTexture.active=benchmark;pixel.ReadPixels(new Rect(0,0,1,1),0,0);}
            double fps=90/(Time.realtimeSinceStartupAsDouble-start);Debug.Log($"M9 SYNCHRONOUS 1080P RENDER FPS {fps:F1}");camera.targetTexture=null;RenderTexture.active=null;Object.Destroy(pixel);Object.Destroy(benchmark);
            Capture(camera,"Sunset",1920,1080);world.GetComponent<AlpineLighting>().Apply(true);yield return null;camera.transform.position=p.Body.position+new Vector3(0,2.2f,-6);camera.transform.LookAt(p.Body.position+Vector3.up*.4f);Capture(camera,"Day",1920,1080);
            var rt=new RenderTexture(720,1280,24);camera.targetTexture=rt;yield return null;camera.transform.position=p.Body.position+new Vector3(0,2.2f,-8.1f);camera.transform.LookAt(p.Body.position+Vector3.up*.4f);Capture(camera,"Portrait",720,1280);camera.targetTexture=null;Object.Destroy(rt);
            Assert.That(p.GetComponent<SnowTrackSystem>(),Is.Not.Null);Assert.That(p.GetComponent<SnowEffectsController>(),Is.Not.Null);Assert.That(fps,Is.GreaterThan(60));Object.Destroy(obj);
#endif
        }
        static void Capture(Camera camera,string name,int width,int height)
        {
            var old=camera.targetTexture;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();Directory.CreateDirectory("Documentation/Screenshots");File.WriteAllBytes("Documentation/Screenshots/"+name+".png",image.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=null;Object.Destroy(image);Object.Destroy(rt);
        }
    }
}
