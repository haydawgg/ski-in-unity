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
    public class ParkPolishTests:PlayWorldTestBase
    {
#if UNITY_EDITOR
        bool asyncCompilation;
        [SetUp]public void SynchronousReviewShaders(){asyncCompilation=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;}
        [TearDown]public void RestoreReviewShaders(){ShaderUtil.allowAsyncCompilation=asyncCompilation;}
#endif
        [UnityTest]public IEnumerator ConnectedLiftClearApproachesAndParkCaptures()
        {
#if UNITY_EDITOR
            var root=new GameObject("VP4 review");root.SetActive(false);var world=root.AddComponent<MountainWorld>();
            world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");world.graphicsConfig=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");root.SetActive(true);yield return null;
            var props=world.GetComponent<MountainProps>();var towers=new List<Transform>();var cables=new List<Transform>();int chairs=0;
            foreach(Transform child in props.Lift){if(child.name=="LiftTower")towers.Add(child);if(child.name=="LiftCable")cables.Add(child);if(child.name=="LiftChair")chairs++;}
            Assert.That(towers.Count,Is.EqualTo(14));Assert.That(cables.Count,Is.EqualTo(26));Assert.That(chairs,Is.EqualTo(52));Assert.That(props.Signs.childCount,Is.EqualTo(5));
            bool lettersFound=false;foreach(var renderer in props.Signs.GetComponentsInChildren<Renderer>())if(renderer.enabled)foreach(var material in renderer.sharedMaterials)if(material.name=="sign_letter")
            {lettersFound=true;Assert.That(material.shader.name,Is.EqualTo("PowderFlow/Alpine Prop"));Assert.That(material.GetColor("_FillColor").g,Is.EqualTo(.68f).Within(.001f));}
            Assert.That(lettersFound,Is.True,"Region boards must retain their readable lettering material after import");
            foreach(var cable in cables)foreach(var local in new[]{Vector3.zero,Vector3.forward*100})
            {
                var endpoint=cable.TransformPoint(local);float nearest=float.MaxValue;
                foreach(var tower in towers)foreach(float x in new[]{-3f,3f})nearest=Mathf.Min(nearest,Vector3.Distance(endpoint,tower.TransformPoint(new Vector3(x,14.67f,0))));
                Assert.That(nearest,Is.LessThan(.01f),"Cable ends must meet the sheave tops");
            }
            foreach(Transform child in props.Markers)Assert.That(child.GetComponentsInChildren<Collider>(),Is.Empty,"Markers must not add approach collisions");
            Physics.SyncTransforms();foreach(float z in new[]{55f,260,405,780,1040,1350})
            {
                Assert.That(Physics.Raycast(world.OnSnow(0,z,50),Vector3.down,out var hit,60,~0,QueryTriggerInteraction.Ignore),Is.True);
                Assert.That(hit.collider.GetComponentInParent<SnowSurface>().type,Is.EqualTo(SurfaceType.Groomed));Assert.That(hit.point.y,Is.EqualTo(MountainWorld.Height(0,z)).Within(.1f));
            }
            var player=world.player;player.Input.BeginInjected();player.enabled=false;player.GetComponent<RailSystem>().enabled=false;player.Body.isKinematic=true;player.GetComponent<SkierPose>().root.gameObject.SetActive(false);
            Camera camera=null;foreach(var follow in root.GetComponentsInChildren<SkiCameraController>()){follow.enabled=false;camera=follow.GetComponent<Camera>();}Assert.That(camera,Is.Not.Null);
            var output="Documentation/VisualPolish/Phase4";Directory.CreateDirectory(output);var lighting=world.GetComponent<AlpineLighting>();
            Transform Find(string name){foreach(Transform child in root.transform)if(child.name==name)return child;throw new System.Exception(name);}
            foreach(bool day in new[]{false,true})
            {
                lighting.Apply(day);yield return null;string preset=day?"Day":"Sunset";camera.fieldOfView=65;
                camera.transform.position=world.OnSnow(-25,355,3);camera.transform.LookAt(world.OnSnow(10,450,3));Capture(camera,output+"/"+preset+"ParkDetail.png");
                camera.transform.position=world.OnSnow(-65,70,9);camera.transform.LookAt(world.OnSnow(0,1000,12));Capture(camera,output+"/"+preset+"Vista.png");
                var rail=Find("Rail_kink");camera.fieldOfView=52;camera.transform.position=rail.TransformPoint(new Vector3(-5,4,-6));camera.transform.LookAt(rail.TransformPoint(new Vector3(0,.3f,6)));Capture(camera,output+"/"+preset+"Rail.png");
                var box=Find("Box_wide");camera.transform.position=box.TransformPoint(new Vector3(-4,3,-5));camera.transform.LookAt(box.TransformPoint(new Vector3(0,.3f,4)));Capture(camera,output+"/"+preset+"Box.png");
                var jump=Find("JumpMedium");camera.fieldOfView=60;camera.transform.position=jump.TransformPoint(new Vector3(-12,4,-7));camera.transform.LookAt(jump.TransformPoint(new Vector3(0,1,9)));Capture(camera,output+"/"+preset+"Jump.png");
                var hut=Find("Hut");camera.fieldOfView=50;camera.transform.position=hut.TransformPoint(new Vector3(8,5,-12));camera.transform.LookAt(hut.TransformPoint(new Vector3(0,2,0)));Capture(camera,output+"/"+preset+"Hut.png");
                camera.fieldOfView=55;camera.transform.position=world.OnSnow(-107,350,17);camera.transform.LookAt(world.OnSnow(-85,420,13));Capture(camera,output+"/"+preset+"Lift.png");
                var sign=props.Signs.GetChild(1);camera.fieldOfView=48;camera.transform.position=sign.position+new Vector3(4,3.5f,-7);camera.transform.LookAt(sign.position+Vector3.up*2);Capture(camera,output+"/"+preset+"Sign.png");
                camera.fieldOfView=58;camera.transform.position=world.OnSnow(0,6,2.6f);camera.transform.LookAt(world.OnSnow(0,18,3.4f));Capture(camera,output+"/"+preset+"Gate.png");
            }
            Assert.That(ShaderUtil.ShaderHasError(Shader.Find("PowderFlow/Alpine Prop")),Is.False);
            Debug.Log("VP4 REVIEW: 14 towers, 26 connected spans, 52 chairs, five region boards; clear approaches; Day/Sunset captures");Object.Destroy(root);
#else
            yield break;
#endif
        }
        static void Capture(Camera camera,string path)
        {
            var rt=new RenderTexture(1920,1080,24);camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=null;Object.Destroy(image);Object.Destroy(rt);
        }
    }
}
