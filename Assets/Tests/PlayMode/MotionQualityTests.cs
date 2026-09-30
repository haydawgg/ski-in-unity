using System.Collections;
using System.IO;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class MotionQualityTests:PlayWorldTestBase
    {
#if UNITY_EDITOR
        static string MotionOutput=>(System.Environment.GetEnvironmentVariable("POWDERFLOW_REVIEW_OUTPUT")??"DevelopmentCaptures/Current")+"/Motion";
        static void Capture(Camera camera,string name)
        {
            string output=MotionOutput;Directory.CreateDirectory(output);
            var active=RenderTexture.active;var previous=camera.targetTexture;var rt=new RenderTexture(1280,720,24);camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(output+"/"+name+".png",image.EncodeToPNG());
            camera.targetTexture=previous;RenderTexture.active=active;Object.Destroy(image);Object.Destroy(rt);
        }
        static void Spawn(MountainWorld world,float x,float z,float speed=14,float lift=0)
        {
            var p=world.player;p.Input.BeginInjected();Assert.That(Physics.Raycast(world.OnSnow(x,z,40),Vector3.down,out var hit,80,~(1<<8),QueryTriggerInteraction.Ignore),Is.True);
            var heading=Vector3.ProjectOnPlane(Vector3.forward,hit.normal).normalized;
            p.ResetTo(hit.point+hit.normal*(p.config.rideHeight+lift),Quaternion.LookRotation(heading,hit.normal));p.Body.linearVelocity=heading*speed;
            if(lift>0)p.EnterAirWithoutRestartingTrick();
        }
        static Camera Detail(MountainWorld world)
        {
            var driver=world.GetComponentInChildren<SkiCameraController>();driver.enabled=false;var camera=driver.GetComponent<Camera>();camera.aspect=16f/9;camera.fieldOfView=42;
            camera.transform.position=world.player.Body.position+new Vector3(2.8f,1.25f,-4);camera.transform.LookAt(world.player.Body.position+Vector3.up*.1f);return camera;
        }
        [UnityTest]public IEnumerator CarvePopAirPreparationAndImpactDriveBody()
        {
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            var world=PresentationPolishTests.CreateWorld();yield return null;var p=world.player;var pose=p.GetComponent<SkierPose>();var camera=Detail(world);var notes=new List<string>();
            foreach(float sign in new[]{-1f,1f})
            {
                Spawn(world,10,48,16);p.Input.steer=sign*.8f;yield return new WaitForSeconds(.6f);pose.ApplyPose(true);camera=Detail(world);
                Assert.That(p.Grounded,Is.True);Assert.That(p.Motion.carveAmount*sign,Is.GreaterThan(.15f));Assert.That(Mathf.Abs(p.Motion.lateralAcceleration),Is.GreaterThan(.5f));Assert.That(p.Speed,Is.GreaterThan(8));
                Assert.That(Vector3.Dot(pose.leftSki.up,p.transform.forward),Is.GreaterThan(.98f));
                float kneeLeft=Vector3.Angle(pose.leftThigh.position-pose.leftShin.position,pose.leftFoot.position-pose.leftShin.position),kneeRight=Vector3.Angle(pose.rightThigh.position-pose.rightShin.position,pose.rightFoot.position-pose.rightShin.position);
                notes.Add($"carve {sign}: amount={p.Motion.carveAmount:F3} load={p.Motion.lateralAcceleration:F2} knees={kneeLeft:F1}/{kneeRight:F1} speed={p.Speed:F2}");Capture(camera,sign<0?"CarveLeft":"CarveRight");
            }
            Spawn(world,10,48,14);yield return new WaitForSeconds(.12f);p.Input.crouch=true;yield return new WaitForSeconds(.25f);float crouch=pose.Bend;Assert.That(crouch,Is.GreaterThan(.9f));camera=Detail(world);Capture(camera,"Crouch");
            p.Input.crouch=false;p.Input.pop=true;yield return new WaitForSeconds(.065f);Assert.That(p.Grounded,Is.False);Assert.That(p.Motion.popAmount,Is.GreaterThan(.3f));Assert.That(pose.Bend,Is.LessThan(crouch-.15f));camera=Detail(world);Capture(camera,"PopExtension");
            notes.Add($"crouch={crouch:F3} pop bend={pose.Bend:F3} vertical={p.Body.linearVelocity.y:F2}");
            Spawn(world,10,50,14,10);yield return new WaitForSeconds(.22f);p.Air.SetMomentum(Vector3.up*6*p.trickConfig.baseInertia);yield return new WaitForSeconds(.12f);float fastBend=pose.Bend;camera=Detail(world);Capture(camera,"FastSpin");
            Assert.That(fastBend,Is.GreaterThan(pose.Visuals.airBend+.1f));p.Air.SetMomentum(Vector3.zero);p.Body.angularVelocity=Vector3.zero;p.Body.rotation=Quaternion.Euler(13,0,0);
            bool prepared=false;LandingResult impact=default;int contacts=0;p.Landed+=r=>{impact=r;contacts++;};
            float until=Time.time+3;while(!p.Grounded&&!p.Bailed&&Time.time<until)
            {
                yield return new WaitForFixedUpdate();if(!prepared&&p.Motion.landingPrediction>.25f){prepared=true;camera=Detail(world);Capture(camera,"LandingPrepare");notes.Add($"prepare={p.Motion.landingPrediction:F3} time={p.PredictedLanding.time:F3} predicted impact={p.PredictedLanding.impact:F2}");}
            }
            Assert.That(prepared,Is.True);Assert.That(p.Bailed,Is.False);Assert.That(contacts,Is.GreaterThan(0));yield return new WaitForSeconds(.08f);float compressed=pose.Bend;camera=Detail(world);Capture(camera,"ImpactCompression");
            Assert.That(compressed,Is.GreaterThan(pose.Visuals.neutralBend+.12f));yield return new WaitForSeconds(.65f);Assert.That(pose.Bend,Is.LessThan(compressed));camera=Detail(world);Capture(camera,"Recovery");
            notes.Add($"impact={impact.impact:F2} quality={impact.quality} compressed={compressed:F3} recovered={pose.Bend:F3} fast air={fastBend:F3}");
            p.ResetTo(p.startPosition,p.startRotation);Assert.That(p.Motion.landingCompression,Is.Zero);Assert.That(p.Compression,Is.Zero);Assert.That(p.GetComponent<TrickTracker>().tracking,Is.False);Assert.That(p.GetComponent<GrabSystem>().Blend,Is.Zero);
            File.WriteAllLines(MotionOutput+"/motion-review.txt",notes);ShaderUtil.allowAsyncCompilation=async;Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);
        }
        [UnityTest]public IEnumerator RailEnvelopeConvergesWithoutCenterlineTeleport()
        {
            var world=PresentationPolishTests.CreateWorld();yield return null;var p=world.player;RailPath path=null;foreach(var candidate in world.GetComponentsInChildren<RailPath>())if(candidate.name=="Rail_flat"){path=candidate;break;}
            var rail=p.GetComponent<RailSystem>();var start=path.Evaluate(1,out var tangent);p.Input.BeginInjected();p.ResetTo(start+Vector3.up*(p.config.rideHeight+.08f)+Vector3.right*.7f,Quaternion.LookRotation(tangent));p.Body.linearVelocity=tangent*10-Vector3.up*.3f;
            float before=p.Body.position.x;yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();
            Assert.That(rail.Riding,Is.True);float first=Mathf.Abs(p.Body.position.x-before);Assert.That(first,Is.LessThan(.3f),"capture correction should blend across multiple steps");
            yield return new WaitForSeconds(.35f);path.Closest(p.Body.position-Vector3.up*p.config.rideHeight,out var center,out _);Assert.That(Mathf.Abs(p.Body.position.x-center.x),Is.LessThan(.02f));Assert.That(rail.Velocity.magnitude,Is.GreaterThan(9));
            Capture(Detail(world),"RailCapture");File.WriteAllText(MotionOutput+"/rail-capture.txt",$"entry offset=.7m first correction={first:F3}m settled={Mathf.Abs(p.Body.position.x-center.x):F3}m speed={rail.Velocity.magnitude:F2}m/s\n");
            p.Input.steer=1;yield return new WaitForSeconds(.15f);p.ResetTo(p.startPosition,p.startRotation);Assert.That(rail.Balance,Is.Zero);Assert.That(rail.Riding,Is.False);
            Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);
        }
        [UnityTest]public IEnumerator GeneratedSmallAndLargeLipsKeepPhysicalLaunchAndLanding()
        {
            var world=PresentationPolishTests.CreateWorld();yield return null;var p=world.player;var notes=new List<string>();
            foreach(var entry in new[]{new Vector2(105,18),new Vector2(900,24)})
            {
                Spawn(world,0,entry.x-12,entry.y);p.Input.tuck=true;
                int takeoffs=0,landings=0;float began=0,airtime=0,highest=0,launchSpeed=0;bool airCaptured=false;
                System.Action takeoff=()=>{takeoffs++;began=Time.time;launchSpeed=p.Speed;};System.Action<LandingResult> landing=r=>{landings++;airtime=Time.time-began;};p.TookOff+=takeoff;p.Landed+=landing;
                float end=Time.time+7;while(Time.time<end&&(takeoffs==0||landings==0)&&!p.Bailed){yield return new WaitForFixedUpdate();if(!airCaptured&&p.Motion.airTime>.3f&&!p.Grounded){airCaptured=true;Capture(Detail(world),entry.x<200?"SmallJumpAir":"LargeJumpAir");}if(!p.Grounded)highest=Mathf.Max(highest,p.Body.position.y-world.OnSnow(p.Body.position.x,p.Body.position.z).y);}
                Assert.That(takeoffs,Is.GreaterThan(0),entry.x+" actual generated lip takeoff");Assert.That(landings,Is.GreaterThan(0));Assert.That(p.Bailed,Is.False);Assert.That(p.Speed,Is.GreaterThan(8));
                notes.Add($"lip {entry.x}: launch={launchSpeed:F2}m/s airtime={airtime:F2}s terrain clearance={highest:F2}m landing={p.LastLanding.quality} speed={p.Speed:F2}");Capture(Detail(world),entry.x<200?"SmallJumpLanding":"LargeJumpLanding");p.TookOff-=takeoff;p.Landed-=landing;
            }
            File.WriteAllLines(MotionOutput+"/jump-review.txt",notes);Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);
        }

        [UnityTest]public IEnumerator GrabsBlendBothHandsToTheSkisAndReleaseSmoothly()
        {
            var world=PresentationPolishTests.CreateWorld();yield return null;var p=world.player;Spawn(world,10,68,0,8);yield return new WaitForFixedUpdate();p.enabled=false;p.GetComponent<RailSystem>().enabled=false;p.Body.isKinematic=true;
            var grab=p.GetComponent<GrabSystem>();var pose=p.GetComponent<SkierPose>();p.Input.grabLeft=true;yield return null;
            Assert.That(grab.Blend,Is.InRange(.001f,.99f));yield return new WaitForSeconds(.2f);Assert.That(grab.Blend,Is.EqualTo(1));
            Assert.That(Vector3.Distance(pose.leftHand.position,grab.Target(pose.leftSki,pose.rightSki)),Is.LessThan(.03f));
            p.Input.grabRight=true;yield return new WaitForSeconds(.2f);pose.ApplyPose(true);
            Assert.That(Vector3.Distance(pose.leftHand.position,pose.leftSki.Find("MidGrab").position),Is.LessThan(.03f));
            Assert.That(Vector3.Distance(pose.rightHand.position,pose.rightSki.Find("MidGrab").position),Is.LessThan(.03f));Capture(Detail(world),"BothHandGrab");
            p.Input.grabLeft=p.Input.grabRight=false;yield return null;Assert.That(grab.Blend,Is.GreaterThan(0));yield return new WaitForSeconds(.2f);Assert.That(grab.Blend,Is.Zero);
            Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);
        }
        [UnityTest,Explicit]public IEnumerator TuneGrabReach()
        {
            var world=PresentationPolishTests.CreateWorld();yield return null;var p=world.player;p.Input.BeginInjected();p.ResetTo(world.OnSnow(10,68,8),Quaternion.Euler(13,0,0));yield return new WaitForFixedUpdate();p.enabled=false;p.GetComponent<RailSystem>().enabled=false;p.Body.isKinematic=true;
            var pose=p.GetComponent<SkierPose>();pose.config=Object.Instantiate(pose.config);var grab=p.GetComponent<GrabSystem>();var camera=world.GetComponentInChildren<Camera>();camera.GetComponent<SkiCameraController>().enabled=false;camera.transform.position=p.Body.position+new Vector3(3,1,4);camera.transform.LookAt(p.Body.position+Vector3.up*.2f);
            var lines=new List<string>();
            foreach(float spine in new[]{0f,18,30,45,60})foreach(float thigh in new[]{-110f,-130,-150})
            {
                pose.config.grabSpine=spine;pose.config.grabThigh=thigh;p.Input.grabLeft=true;yield return null;pose.ApplyPose(true);
                float error=Vector3.Distance(pose.leftHand.position,grab.Target(pose.leftSki,pose.rightSki));
                lines.Add($"spine={spine} thigh={thigh} error={error:F3} upper={pose.leftArm.position-p.Body.position} target={grab.Target(pose.leftSki,pose.rightSki)-p.Body.position} scale={pose.leftSki.lossyScale}");
                Capture(camera,$"Tune-{spine}-{thigh}");
            }
            File.WriteAllLines("Logs/grab-tuning.txt",lines);Object.Destroy(pose.config);Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);
        }
#endif
    }
}
