using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class RailTests
    {
        [Test]public void KinkPathProjectionAndTangentsStayContinuous()
        {
            var obj=new GameObject();var path=obj.AddComponent<RailPath>();path.points=new[]{Vector3.zero,new Vector3(0,0,5),new Vector3(0,-3,9)};
            Assert.That(path.Length,Is.EqualTo(10).Within(.001));float d=path.Closest(new Vector3(.4f,0,3),out var p,out var tangent);Assert.That(d,Is.EqualTo(3).Within(.001));Assert.That(p.z,Is.EqualTo(3).Within(.001));Assert.That(tangent,Is.EqualTo(Vector3.forward));
            Assert.That(Vector3.Distance(path.Evaluate(4.999f,out _),path.Evaluate(5.001f,out _)),Is.LessThan(.003));Object.DestroyImmediate(obj);
        }
    }
}
