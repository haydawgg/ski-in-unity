using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace PowderFlow.Tests
{
    public class MotionQualityEditTests
    {
        [Test]public void PredictionFollowsVelocityToDownhillContactAndIgnoresTriggers()
        {
            var c=ScriptableObject.CreateInstance<TrickConfig>();var slope=GameObject.CreatePrimitive(PrimitiveType.Cube);
            slope.transform.SetPositionAndRotation(new Vector3(1000,0,0),Quaternion.Euler(20,0,0));slope.transform.localScale=new Vector3(80,1,100);
            var trigger=GameObject.CreatePrimitive(PrimitiveType.Cube);trigger.transform.position=new Vector3(1000,2,0);trigger.GetComponent<Collider>().isTrigger=true;Physics.SyncTransforms();
            var prediction=LandingSystem.Predict(new Vector3(1000,3.4f,0),new Vector3(0,-7,15),1.05f,.06f,c);
            Assert.That(prediction.valid,Is.True);Assert.That(prediction.time,Is.InRange(.05f,.7f));Assert.That(prediction.point.z,Is.GreaterThan(1));Assert.That(Vector3.Dot(prediction.normal,slope.transform.up),Is.GreaterThan(.98f));
            Assert.That(LandingSystem.Predict(new Vector3(1000,3.4f,0),new Vector3(0,5,15),1.05f,.06f,c).valid,Is.False);
            Object.DestroyImmediate(slope);Object.DestroyImmediate(trigger);Object.DestroyImmediate(c);
        }
        [Test]public void SpottingPreservesFreeAirAndHeldInputButDampsReleasedRotation()
        {
            var c=ScriptableObject.CreateInstance<TrickConfig>();var obj=new GameObject();var body=obj.AddComponent<Rigidbody>();var input=obj.AddComponent<SkierInput>();input.injected=true;var air=obj.AddComponent<AirControlSystem>();
            air.Begin(body,input,c,Vector3.zero);input.steer=1;air.Step(body,input,c,.1f);input.steer=0;
            var prediction=new LandingPrediction{valid=true,time=.12f,normal=Vector3.up};var momentum=air.AngularMomentum;body.linearVelocity=Vector3.forward*15;
            air.SpotLanding(body,input,default,c,.02f);Assert.That(air.AngularMomentum,Is.EqualTo(momentum));
            input.steer=1;air.SpotLanding(body,input,prediction,c,.02f);Assert.That(air.AngularMomentum,Is.EqualTo(momentum));input.steer=0;
            air.SpotLanding(body,input,prediction,c,.02f);Assert.That(air.AngularMomentum.magnitude,Is.LessThan(momentum.magnitude));Assert.That(air.AngularMomentum.magnitude,Is.GreaterThan(momentum.magnitude*.8f));
            body.rotation=Quaternion.Euler(120,0,0);momentum=air.AngularMomentum;air.SpotLanding(body,input,prediction,c,.02f);Assert.That(air.AngularMomentum,Is.EqualTo(momentum));
            Object.DestroyImmediate(obj);Object.DestroyImmediate(c);
        }
        [Test]public void EveryProductionJumpHasAnUpwardRidingCollider()
        {
            foreach(string name in new[]{"JumpSmall","JumpMedium","JumpLarge","Tabletop","Hip","QuarterPipe"})
            {
                var obj=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/"+name+".prefab"));obj.transform.position=new Vector3(1000,100,0);Physics.SyncTransforms();
                MeshCollider top=null;foreach(var c in obj.GetComponentsInChildren<MeshCollider>())if(c.name=="Collision_ContinuousSnowSurface")top=c;
                Assert.That(top,Is.Not.Null,name);var middle=top.bounds.center;middle.y=top.bounds.max.y+10;
                Assert.That(top.Raycast(new Ray(middle,Vector3.down),out var hit,30),Is.True,name+" cast from riding side");Assert.That(hit.normal.y,Is.GreaterThan(.35f),name);
                Object.DestroyImmediate(obj);
            }
        }
        [Test]public void ShallowAlignedTerrainRecapturePreservesSpeedButBadLandingsStillFail()
        {
            var c=ScriptableObject.CreateInstance<TrickConfig>();var velocity=new Vector3(0,-.8f,22);
            var gentle=LandingSystem.Evaluate(Quaternion.identity,velocity,Vector3.zero,Vector3.up,false,c);
            Assert.That(gentle.quality,Is.EqualTo(LandingQuality.Clean));Assert.That(gentle.retention,Is.GreaterThan(.96f));
            var hard=LandingSystem.Evaluate(Quaternion.identity,new Vector3(0,-14,22),Vector3.zero,Vector3.up,false,c);
            Assert.That(hard.quality,Is.EqualTo(LandingQuality.Sketchy));Assert.That(hard.retention,Is.LessThan(.8f));
            Assert.That(LandingSystem.Evaluate(Quaternion.Euler(90,0,0),velocity,Vector3.zero,Vector3.up,false,c).quality,Is.EqualTo(LandingQuality.Bail));
            Assert.That(LandingSystem.Evaluate(Quaternion.Euler(0,80,0),velocity,Vector3.zero,Vector3.up,false,c).quality,Is.EqualTo(LandingQuality.Bail));
            Object.DestroyImmediate(c);
        }
        [Test]public void ReleasedPreloadCanSpotButDoesNotStopInstantly()
        {
            var c=ScriptableObject.CreateInstance<TrickConfig>();var obj=new GameObject();var body=obj.AddComponent<Rigidbody>();var input=obj.AddComponent<SkierInput>();input.injected=true;var air=obj.AddComponent<AirControlSystem>();
            air.Begin(body,input,c,Vector3.up*3);air.Step(body,input,c,.02f);body.linearVelocity=Vector3.forward*15;float before=body.angularVelocity.magnitude;
            air.SpotLanding(body,input,new LandingPrediction{valid=true,time=.24f,normal=Vector3.up},c,.1f);
            Assert.That(body.angularVelocity.magnitude,Is.LessThan(before*.9f));Assert.That(body.angularVelocity.magnitude,Is.GreaterThan(before*.5f));
            Object.DestroyImmediate(obj);Object.DestroyImmediate(c);
        }
        [Test]public void FoundationClipsKeepTheImportedRigInBindPose()
        {
            var model=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Art/Generated/Character/Skier.fbx");Assert.That(model,Is.Not.Null);
            foreach(var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var binds=renderer.sharedMesh.bindposes;
                for(int i=0;i<binds.Length;i++)
                {
                    var skin=renderer.transform.worldToLocalMatrix*renderer.bones[i].localToWorldMatrix*binds[i];
                    Assert.That(((Vector3)skin.GetColumn(3)).magnitude,Is.LessThan(.002f),renderer.name+" "+renderer.bones[i].name+" bind translation");
                    Assert.That(Quaternion.Angle(skin.rotation,Quaternion.identity),Is.LessThan(.2f),renderer.name+" "+renderer.bones[i].name+" bind rotation");
                }
            }
            var assets=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Generated/Character/Skier.fbx");
            foreach(var name in new[]{"Ski_Neutral","Ski_Crouch","Ski_Tuck","Ski_Carve_Left","Ski_Carve_Right","Ski_Pop","Ski_Airborne","Ski_Land","Ski_Rail","Ski_Bail_Start"})
            {
                AnimationClip found=null;foreach(var a in assets)if(a is AnimationClip clip&&clip.name.Contains(name))found=clip;
                Assert.That(found,Is.Not.Null,name);Assert.That(found.length,Is.GreaterThan(.25f),name+" temporal foundation");
            }
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Generated/Skier.prefab");var animator=prefab.GetComponentInChildren<Animator>();if(animator)Assert.That(animator.enabled,Is.False,"Procedural pose remains authoritative");
        }
    }
}
