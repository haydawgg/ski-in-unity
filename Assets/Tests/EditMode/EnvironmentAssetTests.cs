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
    }
}
