using UnityEditor;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        public static void DiagnoseTerrain()
        {
            var catalog=Config<AssetCatalog>("AssetCatalog");var obj=Object.Instantiate(catalog.Find("Mountain00"));Physics.SyncTransforms();
            foreach(var collider in obj.GetComponentsInChildren<Collider>())Debug.Log("TERRAIN COLLIDER "+collider.bounds+" rotation "+collider.transform.rotation+" scale "+collider.transform.lossyScale);
            foreach(float z in new[]{-12f,12,80,149}){bool hit=Physics.Raycast(new Vector3(0,1000,z),Vector3.down,out var info,2000);Debug.Log($"RAY z={z} hit={hit} point={info.point} normal={info.normal} expected={MountainWorld.Height(0,z)}");}
            Object.DestroyImmediate(obj);
        }
    }
}
