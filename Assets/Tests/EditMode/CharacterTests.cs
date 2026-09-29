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
    }
}
