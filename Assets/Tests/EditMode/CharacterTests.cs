using NUnit.Framework;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class CharacterTests
    {
        [Test] public void GeneratedCharacterHasRequiredBonesAndSeparateSkis()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Skier.prefab");Assert.That(prefab,Is.Not.Null);
            var body=new GameObject();var pose=body.AddComponent<SkierPose>();var character=Object.Instantiate(prefab,body.transform);pose.Bind(character.transform);
            Assert.That(pose.hips,Is.Not.Null);Assert.That(pose.leftArm,Is.Not.Null);Assert.That(pose.rightHand,Is.Not.Null);Assert.That(pose.leftSki,Is.Not.Null);Assert.That(pose.rightSki,Is.Not.Null);Assert.That(pose.leftSki,Is.Not.SameAs(pose.rightSki));Assert.That(pose.leftPole,Is.Not.Null);Assert.That(pose.leftSki.position.x,Is.LessThan(pose.rightSki.position.x));
            float height=0;Bounds bounds=default;bool first=true;foreach(var r in character.GetComponentsInChildren<Renderer>()){if(first){bounds=r.bounds;first=false;}else bounds.Encapsulate(r.bounds);}height=bounds.size.y;Assert.That(height,Is.InRange(1.5f,3));
            Object.DestroyImmediate(body);
        }
        [Test]public void PolishedGarmentsHaveBlendedWeightsAndBoundedGeometry()
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Skier.prefab");
            var renderers=prefab.GetComponentsInChildren<SkinnedMeshRenderer>();int triangles=0,blended=0;
            Assert.That(renderers.Length,Is.InRange(3,8),"Keep independent equipment without dozens of primitive renderers");
            foreach(var r in renderers)
            {
                triangles+=r.sharedMesh.triangles.Length/3;
                foreach(var w in r.sharedMesh.boneWeights)if(w.weight0>.01f&&w.weight1>.01f)blended++;
                foreach(var m in r.sharedMaterials)Assert.That(m.shader.name,Is.EqualTo("PowderFlow/Skier Surface"));
            }
            Assert.That(triangles,Is.InRange(10000,15000));Assert.That(blended,Is.GreaterThan(100),"Garments need continuous joint deformation");
            Assert.That(ShaderUtil.ShaderHasError(Shader.Find("PowderFlow/Skier Surface")),Is.False);
            var config=AssetDatabase.LoadAssetAtPath<CharacterVisualConfig>("Assets/Settings/CharacterVisualConfig.asset");Assert.That(config.outfits.Length,Is.EqualTo(6));
        }
    }
}
