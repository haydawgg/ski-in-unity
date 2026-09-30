using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace PowderFlow
{
    [Serializable] public class SurfaceFingerprint { public string asset, mesh, sha256; }
    [Serializable] public class SurfaceFingerprints { public SurfaceFingerprint[] surfaces; }
    public static partial class EditorTools
    {
        public static string CollisionFingerprint(MeshCollider collider,Transform root)
        {
            var vertices=collider.sharedMesh.vertices;var indices=collider.sharedMesh.triangles;var triangles=new List<string>();
            string Point(int index)
            {
                var p=root.InverseTransformPoint(collider.transform.TransformPoint(vertices[index]));
                return Mathf.RoundToInt(p.x*10000).ToString(CultureInfo.InvariantCulture)+","+Mathf.RoundToInt(p.y*10000).ToString(CultureInfo.InvariantCulture)+","+Mathf.RoundToInt(p.z*10000).ToString(CultureInfo.InvariantCulture);
            }
            for(int i=0;i<indices.Length;i+=3){var corners=new[]{Point(indices[i]),Point(indices[i+1]),Point(indices[i+2])};Array.Sort(corners,StringComparer.Ordinal);triangles.Add(string.Join(";",corners));}
            triangles.Sort(StringComparer.Ordinal);using var hash=SHA256.Create();return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(string.Join("\n",triangles)))).Replace("-","").ToLowerInvariant();
        }
        public static void CaptureParkSurfaceBaseline()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");var surfaces=new List<SurfaceFingerprint>();
            foreach(var asset in catalog.assets)if(asset.type=="Jumps")foreach(var collider in asset.prefab.GetComponentsInChildren<MeshCollider>())
                surfaces.Add(new SurfaceFingerprint{asset=asset.name,mesh=collider.name.Replace("Collision_",""),sha256=CollisionFingerprint(collider,asset.prefab.transform)});
            Directory.CreateDirectory("Documentation/VisualPolish/Phase4");File.WriteAllText("Documentation/VisualPolish/Phase4/riding-surface-baseline.json",JsonUtility.ToJson(new SurfaceFingerprints{surfaces=surfaces.ToArray()},true));
            Debug.Log("PARK SURFACE BASELINE "+surfaces.Count);
        }
    }
}
