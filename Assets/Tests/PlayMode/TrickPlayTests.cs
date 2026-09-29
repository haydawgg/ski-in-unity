using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace PowderFlow.Tests
{
    public class TrickPlayTests : PlayWorldTestBase
    {
        [UnityTest]public IEnumerator GrabsRagdollAndInstantResetAreConnected()
        {
            var root=new GameObject();root.SetActive(false);var world=root.AddComponent<PhysicsTestWorld>();world.physicsConfig=ScriptableObject.CreateInstance<SkiPhysicsConfig>();world.cameraConfig=ScriptableObject.CreateInstance<CameraConfig>();world.trickConfig=ScriptableObject.CreateInstance<TrickConfig>();root.SetActive(true);yield return null;
            var p=world.player;p.Input.BeginInjected();p.ResetTo(new Vector3(100,60,0),Quaternion.identity);yield return new WaitForFixedUpdate();
            var grab=p.GetComponent<GrabSystem>();
            for(int choice=0;choice<8;choice++)
            {
                p.Input.grabLeft=choice%2==0;p.Input.grabRight=!p.Input.grabLeft;p.Input.modifierLeft=choice==2||choice==3||choice>=6;p.Input.modifierRight=choice>=4;
                yield return null;Assert.That(grab.Current,Is.Not.EqualTo(GrabType.None));
            }
            p.Body.linearVelocity=Vector3.forward*10;p.GetComponent<BailSystem>().Crash();yield return null;
            var rag=GameObject.Find("Momentum ragdoll");Assert.That(rag,Is.Not.Null);Assert.That(rag.GetComponentsInChildren<Rigidbody>().Length,Is.GreaterThanOrEqualTo(13));
            float start=Time.realtimeSinceStartup;p.ResetTo(p.startPosition,p.startRotation);yield return null;
            Assert.That(Time.realtimeSinceStartup-start,Is.LessThan(1));Assert.That(p.Bailed,Is.False);Assert.That(p.Body.isKinematic,Is.False);
        }
    }
}
