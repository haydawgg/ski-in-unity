using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.TestTools;
namespace PowderFlow.Tests
{
    public class CharacterPolishTests:PlayWorldTestBase
    {
        [UnityTest]public IEnumerator CaptureGarmentsOutfitsAndGrabReach()
        {
#if UNITY_EDITOR
            var obj=new GameObject("Character review fixture");obj.SetActive(false);var world=obj.AddComponent<PhysicsTestWorld>();world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");obj.SetActive(true);yield return null;
            var p=world.player;p.Input.BeginInjected();p.ResetTo(new Vector3(100,60,0),Quaternion.identity);yield return new WaitForFixedUpdate();p.enabled=false;p.GetComponent<RailSystem>().enabled=false;p.Body.isKinematic=true;
            var pose=p.GetComponent<SkierPose>();var grab=p.GetComponent<GrabSystem>();var outfit=p.GetComponent<OutfitSystem>();var camera=world.followCamera;camera.GetComponent<SkiCameraController>().enabled=false;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.055f,.075f,.105f);camera.fieldOfView=42;
            camera.allowHDR=true;camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
            var volume=obj.AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/AlpinePost.asset");RenderSettings.fog=false;
            var output="Documentation/VisualPolish/Phase2";Directory.CreateDirectory(output);var results=new List<string>();
            camera.transform.position=p.transform.position+new Vector3(0,.5f,4.4f);camera.transform.LookAt(p.transform.position+Vector3.up*.85f);
            for(int warm=0;warm<4;warm++){var image=Capture(camera,960,720);Object.Destroy(image);yield return null;}
            var sheets=new[]{new Texture2D(960*3,720*3,TextureFormat.RGB24,false),new Texture2D(960*3,720*3,TextureFormat.RGB24,false),new Texture2D(960*3,720*3,TextureFormat.RGB24,false)};
            Vector3[] angles={new Vector3(0,.5f,4.4f),new Vector3(4.4f,.5f,0),new Vector3(0,.5f,-4.4f)};
            for(int i=0;i<9;i++)
            {
                p.Input.BeginInjected();switch(i){case 0:p.Input.grabLeft=true;break;case 1:p.Input.grabRight=true;break;case 2:p.Input.grabLeft=p.Input.modifierLeft=p.Input.modifierRight=true;break;case 3:p.Input.grabLeft=p.Input.modifierLeft=true;break;case 4:p.Input.grabRight=p.Input.modifierLeft=true;break;case 5:p.Input.grabLeft=p.Input.modifierRight=true;break;case 6:p.Input.grabRight=p.Input.modifierLeft=p.Input.modifierRight=true;break;case 7:p.Input.grabRight=p.Input.modifierRight=true;break;case 8:p.Input.grabLeft=p.Input.grabRight=true;break;}
                yield return null;yield return null;pose.ApplyPose(true);
                var hand=grab.LeftHand?pose.leftHand:pose.rightHand;float error=Vector3.Distance(hand.position,grab.Target(pose.leftSki,pose.rightSki));
                results.Add($"{grab.Current}: {error:F3}m");Debug.Log($"VP2 GRAB {grab.Current} reach error {error:F3}m");
                Assert.That(float.IsFinite(error),Is.True);
                Assert.That(error,Is.LessThan(.14f),grab.Current+" wrist should reach within a glove length of the grip");
                for(int view=0;view<3;view++)
                {
                    camera.transform.position=p.transform.position+angles[view];camera.transform.LookAt(p.transform.position+Vector3.up*.85f);
                    var image=Capture(camera,960,720);sheets[view].SetPixels((i%3)*960,(2-i/3)*720,960,720,image.GetPixels());Object.Destroy(image);
                }
            }
            string[] labels={"Front","Side","Rear"};for(int view=0;view<3;view++){sheets[view].Apply();File.WriteAllBytes(output+"/Grabs"+labels[view]+".jpg",sheets[view].EncodeToJPG(90));Object.Destroy(sheets[view]);}
            File.WriteAllLines(output+"/grab-reach.txt",results);
            p.Input.BeginInjected();yield return null;yield return null;pose.ApplyPose(true);
            var palettes=new Texture2D(960*3,720*2,TextureFormat.RGB24,false);
            for(int i=0;i<6;i++)
            {
                outfit.Apply(i);camera.transform.position=p.transform.position+new Vector3(1.8f,.5f,4);camera.transform.LookAt(p.transform.position+Vector3.up*.65f);
                var image=Capture(camera,960,720);palettes.SetPixels((i%3)*960,(1-i/3)*720,960,720,image.GetPixels());Object.Destroy(image);
                Color? ski=null,graphic=null;foreach(var renderer in pose.root.GetComponentsInChildren<Renderer>())foreach(var m in renderer.materials){if(m.name=="ski (Instance)")ski=m.GetColor("_BaseColor");if(m.name=="ski_graphic (Instance)")graphic=m.GetColor("_BaseColor");}
                Assert.That(ski.HasValue&&graphic.HasValue,Is.True);Assert.That(Vector4.Distance(ski.Value,graphic.Value),Is.GreaterThan(.1f),"Ski graphics must retain distinct color");
            }
            palettes.Apply();File.WriteAllBytes(output+"/Outfits.jpg",palettes.EncodeToJPG(90));Object.Destroy(palettes);outfit.Apply(0);
            foreach(bool tucked in new[]{false,true})
            {
                p.Input.tuck=tucked;pose.ApplyPose(true);camera.transform.position=p.transform.position+new Vector3(2.3f,.45f,-3);camera.transform.LookAt(p.transform.position+Vector3.up*.85f);
                var image=Capture(camera,1280,960);File.WriteAllBytes(output+(tucked?"/Tuck.png":"/Airborne.png"),image.EncodeToPNG());Object.Destroy(image);
            }
            Object.Destroy(obj);
#else
            yield break;
#endif
        }
        static Texture2D Capture(Camera camera,int width,int height)
        {
            var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();camera.targetTexture=null;RenderTexture.active=null;Object.Destroy(rt);return image;
        }
    }
}
