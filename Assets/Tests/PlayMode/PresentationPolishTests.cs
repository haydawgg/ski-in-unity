using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class PresentationPolishTests:PlayWorldTestBase
    {
#if UNITY_EDITOR
        public static MountainWorld CreateWorld()
        {
            var obj=new GameObject("Presentation review");obj.SetActive(false);var w=obj.AddComponent<MountainWorld>();
            w.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");w.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");w.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");w.cameraConfig=Object.Instantiate(AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset"));w.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");w.graphicsConfig=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");obj.SetActive(true);return w;
        }
        static void Spawn(MountainWorld w,float x,float z,float speed,float lift=0)
        {
            var p=w.player;p.Input.steer=0;p.Input.brake=false;p.Input.tuck=false;p.Input.grabLeft=p.Input.grabRight=false;
            Physics.Raycast(w.OnSnow(x,z,40),Vector3.down,out var hit,80,~(1<<8),QueryTriggerInteraction.Ignore);
            var forward=Vector3.ProjectOnPlane(Vector3.forward,hit.normal).normalized;
            p.ResetTo(hit.point+hit.normal*(p.config.rideHeight+lift),Quaternion.LookRotation(forward,hit.normal));p.Body.linearVelocity=forward*speed;if(lift>0)p.EnterAirWithoutRestartingTrick();
        }
        static void Capture(Camera camera,string path,int width,int height)
        {
            var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;Object.Destroy(image);Object.Destroy(rt);
        }
#endif
        [UnityTest]public IEnumerator CameraStatesAtDesktopSizes()
        {
#if UNITY_EDITOR
            bool baseline=System.Environment.GetEnvironmentVariable("POWDERFLOW_PRESENTATION_BASELINE")=="1";
            string output="Documentation/VisualPolish/Phase7/Camera"+(baseline?"/Before":"");Directory.CreateDirectory(output);
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            var w=CreateWorld();yield return null;var p=w.player;p.Input.BeginInjected();var camera=w.GetComponentInChildren<SkiCameraController>().GetComponent<Camera>();var stats=new List<string>();
            foreach(int width in new[]{1280,1920})
            {
                int height=width*9/16;camera.aspect=(float)width/height;string suffix=width==1280?"Desktop720":"Desktop1080";
                void Shot(string name)
                {
                    Capture(camera,output+"/"+name+suffix+".png",width,height);var v=camera.WorldToViewportPoint(p.Body.position);
                    stats.Add($"{name} {suffix}: player viewport={v}, camera={camera.transform.position}, horizon roll={camera.transform.eulerAngles.z:F2}, grounded={p.Grounded}, fov={camera.fieldOfView:F2}");
                    if(!baseline){Assert.That(v.z,Is.GreaterThan(1));Assert.That(v.x,Is.InRange(.15f,.85f));Assert.That(v.y,Is.InRange(.15f,.8f));Assert.That(Mathf.Abs(Vector3.Dot(camera.transform.right,Vector3.up)),Is.LessThan(.015f));Assert.That(Physics.CheckSphere(camera.transform.position,.16f,~(1<<8),QueryTriggerInteraction.Ignore),Is.False,"Camera must stay outside collision geometry");}
                }
                Spawn(w,0,360,18);yield return new WaitForSeconds(.6f);Shot("ParkCruise");
                Spawn(w,10,395,5);p.Input.steer=.65f;yield return new WaitForSeconds(.4f);Shot("SlowCarve");
                Spawn(w,-17,208,12);yield return new WaitForSeconds(.15f);Shot("RailEntry");
                Spawn(w,8,855,22,7);p.Input.grabLeft=true;p.Body.angularVelocity=new Vector3(1.5f,3,0);yield return new WaitForSeconds(.25f);Assert.That(p.Grounded,Is.False);Shot("BigAir");
                Spawn(w,10,395,12);yield return new WaitForSeconds(.2f);p.GetComponent<BailSystem>().Crash();yield return new WaitForSeconds(.2f);Shot("Bail");
            }
            File.WriteAllLines(output+"/camera-review.txt",stats);ShaderUtil.allowAsyncCompilation=async;Object.Destroy(w.cameraConfig);Object.Destroy(w.gameObject);Debug.Log("PRESENTATION CAMERA REVIEW: five actual states / desktop landscape 720p and 1080p");
#else
            yield break;
#endif
        }
    }
}
