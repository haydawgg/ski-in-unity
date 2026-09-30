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
    public class EnvironmentPolishTests:PlayWorldTestBase
    {
        [UnityTest]public IEnumerator RegionCompositionClearanceAndMovingLods()
        {
#if UNITY_EDITOR
            var obj=new GameObject("VP3 review world");obj.SetActive(false);var world=obj.AddComponent<MountainWorld>();
            world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");world.graphicsConfig=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");obj.SetActive(true);yield return null;
            var scenery=world.GetComponent<MountainScenery>();var config=world.worldConfig;var regionCounts=new Dictionary<string,int>();
            Assert.That(scenery.Trees.childCount,Is.EqualTo(config.treeCount));Assert.That(scenery.Ranges.childCount,Is.EqualTo(5));
            int rocks=0,shrubs=0;foreach(Transform accent in scenery.Accents){if(accent.name.StartsWith("Rock"))rocks++;else shrubs++;Assert.That(MountainScenery.IsProtected(config,accent.position.x,accent.position.z,accent.name.StartsWith("Rock")?3:1.5f),Is.False,accent.name);}
            Assert.That(rocks,Is.EqualTo(config.rockCount));Assert.That(shrubs,Is.EqualTo(config.shrubCount));
            foreach(Transform tree in scenery.Trees)
            {
                Assert.That(MountainScenery.IsProtected(config,tree.position.x,tree.position.z,5),Is.False,tree.name);string region=tree.name.Split(' ')[0];regionCounts.TryGetValue(region,out int n);regionCounts[region]=n+1;
                foreach(Transform other in scenery.Trees)if(tree!=other)Assert.That(Vector2.Distance(new Vector2(tree.position.x,tree.position.z),new Vector2(other.position.x,other.position.z)),Is.GreaterThanOrEqualTo(config.treeSpacing-.001f));
            }
            Assert.That(regionCounts["Park"],Is.GreaterThan(regionCounts["Easy"]));Assert.That(regionCounts["Freeride"],Is.GreaterThan(regionCounts["BigAir"]));
            Physics.SyncTransforms();foreach(float z in new[]{1050f,1100,1160,1220,1260}){Assert.That(Physics.Raycast(new Vector3(75,500,z),Vector3.down,out var hit,700,~0,QueryTriggerInteraction.Ignore),Is.True);Assert.That(hit.collider.GetComponentInParent<SnowSurface>(),Is.Not.Null);Assert.That(hit.point.y,Is.EqualTo(MountainWorld.Height(75,z)).Within(.25f));}
            var player=world.player;player.Input.BeginInjected();player.enabled=false;player.GetComponent<RailSystem>().enabled=false;player.Body.isKinematic=true;player.GetComponent<SkierPose>().root.gameObject.SetActive(false);
            Camera camera=null;foreach(var follow in obj.GetComponentsInChildren<SkiCameraController>()){follow.enabled=false;camera=follow.GetComponent<Camera>();}Assert.That(camera,Is.Not.Null);camera.fieldOfView=65;
            var output="Documentation/VisualPolish/Phase3";Directory.CreateDirectory(output);Directory.CreateDirectory("Logs/VegetationFrames");var lighting=world.GetComponent<AlpineLighting>();
            var starts=new[]{new Vector2(0,55),new Vector2(0,380),new Vector2(0,845),new Vector2(75,1080),new Vector2(0,1350)};string[] labels={"Easy","Park","BigAir","Freeride","Lower"};
            foreach(bool day in new[]{false,true})
            {
                lighting.Apply(day);yield return null;
                for(int i=0;i<5;i++){var p=starts[i];camera.transform.position=world.OnSnow(p.x,p.y,5);camera.transform.LookAt(world.OnSnow(p.x,p.y+170,4));Capture(camera,output+"/"+(day?"Day":"Sunset")+labels[i]+".png",1920,1080);}
            }
            lighting.Apply(true);yield return null;
            var sheet=new Texture2D(1920,1440,TextureFormat.RGB24,false);
            for(int i=0;i<4;i++)
            {
                var tree=world.Place("Pine"+i,world.OnSnow(0,60),Quaternion.identity);tree.GetComponent<LODGroup>().ForceLOD(0);
                camera.transform.position=tree.transform.position+new Vector3(10,8,-19);camera.transform.LookAt(tree.transform.position+Vector3.up*(i==2?6:3.5f));camera.fieldOfView=38;
                var image=Read(camera,960,720);sheet.SetPixels((i%2)*960,(1-i/2)*720,960,720,image.GetPixels());Object.Destroy(image);Object.Destroy(tree);yield return null;
            }
            sheet.Apply();File.WriteAllBytes(output+"/PineVariants.jpg",sheet.EncodeToJPG(92));Object.Destroy(sheet);
            var review=world.Place("Pine2",world.OnSnow(0,300),Quaternion.Euler(0,20,0));var group=review.GetComponent<LODGroup>();var lodSheet=new Texture2D(2880,720,TextureFormat.RGB24,false);
            camera.fieldOfView=42;camera.transform.position=review.transform.position+new Vector3(12,8,-20);camera.transform.LookAt(review.transform.position+Vector3.up*6);
            for(int i=0;i<3;i++){group.ForceLOD(i);var image=Read(camera,960,720);lodSheet.SetPixels(i*960,0,960,720,image.GetPixels());Object.Destroy(image);}lodSheet.Apply();File.WriteAllBytes(output+"/PineLods.jpg",lodSheet.EncodeToJPG(92));Object.Destroy(lodSheet);group.ForceLOD(-1);camera.fieldOfView=65;
            for(int i=0;i<72;i++)
            {
                float distance=Mathf.Lerp(220,12,i/71f);camera.transform.position=review.transform.position+new Vector3(0,6+distance*.30f,-distance);camera.transform.LookAt(review.transform.position+Vector3.up*6);
                var image=Read(camera,1280,720);File.WriteAllBytes($"Logs/VegetationFrames/{i:000}.jpg",image.EncodeToJPG(90));Object.Destroy(image);yield return new WaitForSecondsRealtime(1f/12);
            }
            Object.Destroy(review);Debug.Log("VP3 COMPOSITION "+string.Join(", ",regionCounts)+$"; {rocks} rocks; {shrubs} shrubs; protected corridors clear");Object.Destroy(obj);
#else
            yield break;
#endif
        }
        static void Capture(Camera camera,string path,int width,int height){var image=Read(camera,width,height);File.WriteAllBytes(path,image.EncodeToPNG());Object.Destroy(image);}
        static Texture2D Read(Camera camera,int width,int height)
        {
            var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;for(int warm=0;warm<2;warm++)camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();camera.targetTexture=null;RenderTexture.active=null;Object.Destroy(rt);return image;
        }
    }
}
