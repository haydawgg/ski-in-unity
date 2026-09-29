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
    public class CharacterPlayTests : PlayWorldTestBase
    {
        [UnityTest]public IEnumerator GeneratedCharacterPoseUsesIndependentSkisAndPoleLag()
        {
#if UNITY_EDITOR
            var root=new GameObject();root.SetActive(false);var world=root.AddComponent<PhysicsTestWorld>();world.physicsConfig=ScriptableObject.CreateInstance<SkiPhysicsConfig>();world.cameraConfig=ScriptableObject.CreateInstance<CameraConfig>();world.trickConfig=ScriptableObject.CreateInstance<TrickConfig>();world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");root.SetActive(true);yield return null;
            var p=world.player;p.Input.injected=true;p.Input.tuck=true;yield return new WaitForSeconds(.15f);var groundedPose=p.GetComponent<SkierPose>();Assert.That(Vector3.Dot(groundedPose.leftSki.up,p.transform.forward),Is.GreaterThan(.95f));p.Input.tuck=false;p.ResetTo(new Vector3(100,60,0),Quaternion.identity);yield return new WaitForFixedUpdate();
            var pose=p.GetComponent<SkierPose>();var old=pose.leftSki.localRotation;p.Input.modifierLeft=true;p.Input.grabLeft=true;p.Input.grabRight=true;yield return null;yield return null;
            Assert.That(Quaternion.Angle(old,pose.leftSki.localRotation),Is.GreaterThan(10));Assert.That(Vector3.Dot(pose.leftSki.up,pose.rightSki.up),Is.LessThan(.9f));Assert.That(p.GetComponent<GrabSystem>().Current,Is.EqualTo(GrabType.CrissCross));
            if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)
            {
                var camera=Camera.main;camera.transform.position=p.transform.position+new Vector3(2,1.5f,-4);camera.transform.LookAt(p.transform.position);var rt=new RenderTexture(960,540,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(960,540,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,960,540),0,0);image.Apply();Directory.CreateDirectory("Documentation/Screenshots");File.WriteAllBytes("Documentation/Screenshots/M6-character.png",image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.Destroy(image);Object.Destroy(rt);
            }
#endif
        }
    }
}
