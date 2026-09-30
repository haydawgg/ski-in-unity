using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace PowderFlow.Tests
{
    public class EnvironmentAssetTests
    {
        [Test]public void GeneratedCatalogHasLodsPathsAndSeamlessTerrain()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");Assert.That(catalog.assets.Length,Is.GreaterThanOrEqualTo(47));
            Assert.That(catalog.Find("Pine2").GetComponent<LODGroup>().GetLODs().Length,Is.EqualTo(3));
            Assert.That(catalog.Find("Rail_kink").GetComponent<RailPath>().points.Length,Is.EqualTo(3));
            var a=Object.Instantiate(catalog.Find("Mountain00"));var b=Object.Instantiate(catalog.Find("Mountain01"));Physics.SyncTransforms();
            Assert.That(Physics.Raycast(new Vector3(0,1000,149.99f),Vector3.down,out var one,2000),Is.True);
            Assert.That(Physics.Raycast(new Vector3(0,1000,150.01f),Vector3.down,out var two,2000),Is.True);
            Assert.That(one.point.y,Is.EqualTo(MountainWorld.Height(0,149.99f)).Within(.1f));
            Assert.That(Mathf.Abs(one.point.y-two.point.y),Is.LessThan(.02f));Assert.That(Vector3.Dot(one.normal,two.normal),Is.GreaterThan(.99f));
            Object.DestroyImmediate(a);Object.DestroyImmediate(b);var freeride=Object.Instantiate(catalog.Find("Mountain07"));Physics.SyncTransforms();Assert.That(Physics.Raycast(new Vector3(90,1000,1100),Vector3.down,out var offCenter,2000),Is.True);Assert.That(offCenter.point.y,Is.EqualTo(MountainWorld.Height(90,1100)).Within(.15f));Object.DestroyImmediate(freeride);
        }
        [Test]public void SceneryLodsAreBoundedAndAccentsAreNonColliding()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");
            for(int variant=0;variant<4;variant++)
            {
                var prefab=catalog.Find("Pine"+variant);var group=prefab.GetComponent<LODGroup>();var lods=group.GetLODs();Assert.That(lods.Length,Is.EqualTo(3));
                Assert.That(group.fadeMode,Is.EqualTo(LODFadeMode.CrossFade));Assert.That(group.animateCrossFading,Is.True);
                int previous=int.MaxValue;
                foreach(var lod in lods)
                {
                    Assert.That(lod.renderers.Length,Is.EqualTo(1),"Consolidate branch primitives into one mesh per LOD");int triangles=0;
                    foreach(var renderer in lod.renderers)triangles+=renderer.GetComponent<MeshFilter>().sharedMesh.triangles.Length/3;
                    Assert.That(triangles,Is.LessThan(previous));Assert.That(triangles,Is.LessThan(6000));previous=triangles;
                }
                Assert.That(prefab.GetComponentsInChildren<Collider>().Length,Is.EqualTo(1));Assert.That(prefab.GetComponent<Collider>(),Is.TypeOf<CapsuleCollider>());
            }
            foreach(string name in new[]{"Shrub0","Shrub1","Ridge0","Ridge1","Ridge2","Ridge3","Ridge4","ValleyFloor"})Assert.That(catalog.Find(name).GetComponentsInChildren<Collider>(),Is.Empty,name);
            foreach(string name in new[]{"Ridge0","Ridge1","Ridge2","Ridge3","Ridge4","ValleyFloor"})foreach(var renderer in catalog.Find(name).GetComponentsInChildren<Renderer>())Assert.That(renderer.shadowCastingMode,Is.EqualTo(UnityEngine.Rendering.ShadowCastingMode.Off),"Backdrop ranges must not darken the riding corridor");
            var ridge=catalog.Find("Ridge2").GetComponentInChildren<MeshFilter>().sharedMesh;Assert.That(ridge.bounds.min.y,Is.LessThan(-40),"Ridge skirts must sink below the valley rather than form a wall");
            Assert.That(ShaderUtil.ShaderHasError(Shader.Find("PowderFlow/Alpine Snow")),Is.False);
        }
    }
}
