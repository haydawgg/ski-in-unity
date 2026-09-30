using System.IO;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class ParkAssetTests
    {
        [Test]public void ImportedParkRetainsSurfacesAndUsesOnlyExplicitCollision()
        {
            var catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");
            Assert.That(ShaderUtil.ShaderHasError(Shader.Find("PowderFlow/Alpine Prop")),Is.False);
            var baseline=JsonUtility.FromJson<SurfaceFingerprints>(File.ReadAllText("Documentation/VisualPolish/Phase4/riding-surface-baseline.json"));Assert.That(baseline.surfaces.Length,Is.EqualTo(12));
            foreach(var surface in baseline.surfaces)
            {
                var prefab=catalog.Find(surface.asset);MeshCollider found=null;foreach(var collider in prefab.GetComponentsInChildren<MeshCollider>())if(collider.name=="Collision_"+surface.mesh)found=collider;
                Assert.That(found,Is.Not.Null,surface.asset+" "+surface.mesh);Assert.That(EditorTools.CollisionFingerprint(found,prefab.transform),Is.EqualTo(surface.sha256),"Preserve the validated jump collision triangles");
            }
            foreach(var asset in catalog.assets)
            {
                bool explicitCollision=false;foreach(var filter in asset.prefab.GetComponentsInChildren<MeshFilter>())if(filter.name.StartsWith("Collision_"))explicitCollision=true;
                if(explicitCollision)foreach(var collider in asset.prefab.GetComponentsInChildren<Collider>()){Assert.That(collider.name,Does.StartWith("Collision_"));Assert.That(collider.GetComponent<Renderer>().enabled,Is.False,"Collision meshes must not double-render");}
                if(asset.type=="EnvironmentAccent")Assert.That(asset.prefab.GetComponentsInChildren<Collider>(),Is.Empty,asset.name);
            }
            foreach(string prefix in new[]{"Rail_","Box_"})foreach(string kind in prefix=="Rail_"?new[]{"flat","down","kink","rainbow","wide"}:new[]{"flat","down","kink","narrow","wide"})
            {
                var obj=Object.Instantiate(catalog.Find(prefix+kind));obj.transform.position=new Vector3(5000,100,0);var rail=obj.GetComponent<RailPath>();Physics.SyncTransforms();
                foreach(float z in new[]{1.5f,3,7.5f,9})
                {
                    Vector3 center=Vector3.zero;float slope=0;
                    for(int i=1;i<rail.points.Length;i++)if(z<=rail.points[i].z){var a=rail.points[i-1];var b=rail.points[i];center=Vector3.Lerp(a,b,(z-a.z)/(b.z-a.z));slope=(b.y-a.y)/(b.z-a.z);break;}
                    var position=obj.transform.TransformPoint(center);Assert.That(Physics.Raycast(position+Vector3.up*3,Vector3.down,out var hit,4,~0,QueryTriggerInteraction.Ignore),Is.True,prefix+kind);
                    float radius=prefix=="Box_"?0:kind=="wide"?.13f:.07f;
                    Assert.That(hit.point.y,Is.EqualTo(position.y+radius*Mathf.Sqrt(1+slope*slope)).Within(.015f),prefix+kind+" visual/path contact");
                    Assert.That(hit.collider.GetComponentInParent<SnowSurface>().type,Is.EqualTo(SurfaceType.Rail));
                }
                Object.DestroyImmediate(obj);
            }
        }
    }
}
