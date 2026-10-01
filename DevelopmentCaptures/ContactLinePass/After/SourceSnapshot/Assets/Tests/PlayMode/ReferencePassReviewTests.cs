using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    // Matched review fixtures use the actual world, pose, camera and physics.
    // Each sequence is continuous; relocating between named scenarios is explicit.
    public class ReferencePassReviewTests:PlayWorldTestBase
    {
#if UNITY_EDITOR
        static string Output=>Environment.GetEnvironmentVariable("POWDERFLOW_REFERENCE_OUTPUT")??"DevelopmentCaptures/AfterReferencePass";
        [Serializable]class Metric
        {
            public string name,landing;
            public float speed,bodyScreenHeight,fov,cameraDistance,cameraRise,bend,kneeLeft,kneeRight,load,carve,slip,particles,yaw,pitch,roll,angular,compression,prediction,wristError;
            public bool grounded,rail,bailed;
        }
        [Serializable]class Review {public List<Metric> samples=new List<Metric>();public List<string> notes=new List<string>();}
        static void Spawn(MountainWorld w,float x,float z,float speed,float lift=0,float yaw=0)
        {
            var p=w.player;p.Input.BeginInjected();
            Assert.That(Physics.Raycast(w.OnSnow(x,z,40),Vector3.down,out var hit,80,~(1<<8),QueryTriggerInteraction.Ignore),Is.True);
            var forward=Vector3.ProjectOnPlane(Vector3.forward,hit.normal).normalized;
            p.ResetTo(hit.point+hit.normal*(p.config.rideHeight+lift),Quaternion.AngleAxis(yaw,hit.normal)*Quaternion.LookRotation(forward,hit.normal));p.Body.linearVelocity=forward*speed;
            if(lift>0)p.EnterAirWithoutRestartingTrick();
        }
        static void Capture(Camera camera,string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(rt);
        }
        static void Shot(MountainWorld w,Camera camera,Review review,string name)
        {
            var p=w.player;var pose=p.GetComponent<SkierPose>();var tracker=p.GetComponent<TrickTracker>();var grab=p.GetComponent<GrabSystem>();
            var head=camera.WorldToViewportPoint(pose.head.position);var left=camera.WorldToViewportPoint(pose.leftFoot.position);var right=camera.WorldToViewportPoint(pose.rightFoot.position);
            var delta=camera.transform.position-p.Body.position;
            review.samples.Add(new Metric{name=name,speed=p.Speed,bodyScreenHeight=Mathf.Abs(head.y-Mathf.Min(left.y,right.y)),fov=camera.fieldOfView,cameraDistance=Vector3.ProjectOnPlane(delta,Vector3.up).magnitude,cameraRise=delta.y,
                bend=pose.Bend,kneeLeft=Vector3.Angle(pose.leftThigh.position-pose.leftShin.position,pose.leftFoot.position-pose.leftShin.position),kneeRight=Vector3.Angle(pose.rightThigh.position-pose.rightShin.position,pose.rightFoot.position-pose.rightShin.position),
                load=p.Motion.lateralAcceleration,carve=p.Motion.carveAmount,slip=p.Slip,particles=p.GetComponent<SnowEffectsController>().LiveParticles,
                yaw=tracker.current.yaw,pitch=tracker.current.pitch,roll=tracker.current.roll,angular=p.Body.angularVelocity.magnitude,compression=p.Compression,prediction=p.Motion.landingPrediction,
                wristError=grab.Current!=GrabType.None?Vector3.Distance((grab.LeftHand?pose.leftHand:pose.rightHand).position,grab.Target(pose.leftSki,pose.rightSki)):0,
                grounded=p.Grounded,rail=p.GetComponent<RailSystem>().Riding,bailed=p.Bailed,landing=p.LastLanding.quality.ToString()});
            Capture(camera,Output+"/States/"+name+".png");Assert.That(float.IsFinite(p.Speed),Is.True);Assert.That(p.GetComponent<SnowEffectsController>().LiveParticles,Is.LessThanOrEqualTo(w.graphicsConfig.maxParticles));
        }
        static IEnumerator RailSpinOff(MountainWorld w,Camera camera,Review review)
        {
            var p=w.player;RailPath path=null;foreach(var item in w.GetComponentsInChildren<RailPath>())if(item.name=="Rail_flat")path=item;Assert.That(path,Is.Not.Null);
            var point=path.Evaluate(1,out var tangent);p.Input.BeginInjected();p.ResetTo(point+Vector3.up*(p.config.rideHeight+.08f)+Vector3.right*.6f,Quaternion.AngleAxis(90,Vector3.up)*Quaternion.LookRotation(tangent));p.Body.linearVelocity=tangent*12-Vector3.up*.3f;
            yield return new WaitForFixedUpdate();Shot(w,camera,review,"RailEntry");yield return new WaitForSeconds(.35f);Assert.That(p.GetComponent<RailSystem>().Riding,Is.True);Shot(w,camera,review,"RailSlide");
            p.Input.pop=true;float limit=Time.time+3;bool shot=false;
            do
            {
                // Input-only review rider aims for a 90 degree pop to switch.
                float error=Vector3.SignedAngle(Vector3.ProjectOnPlane(p.Body.rotation*Vector3.forward,Vector3.up),-Vector3.ProjectOnPlane(tangent,Vector3.up),Vector3.up)*Mathf.Deg2Rad;
                float remaining=p.PredictedLanding.valid?Mathf.Max(.12f,p.PredictedLanding.time):Mathf.Max(.2f,1.8f-p.Motion.airTime);
                float desired=error/(remaining*.8f);p.Input.steer=Mathf.Abs(error)<.12f?0:Mathf.Clamp((desired-p.Body.angularVelocity.y)*2,-1,1);
                yield return null;if(!shot&&p.Motion.airTime>.12f){Shot(w,camera,review,"RailExit");shot=true;}
            }while((p.GetComponent<RailSystem>().Riding||!p.Grounded)&&!p.Bailed&&Time.time<limit);
            p.Input.steer=0;yield return new WaitForSeconds(.06f);Shot(w,camera,review,"RailExitLanding");Assert.That(p.Grounded&&!p.Bailed,Is.True,"Input-driven sideways rail pop must return to snow");
        }
        [UnityTest]public IEnumerator CaptureRailSpinOff()
        {
            var w=PresentationPolishTests.CreateWorld();yield return null;var camera=w.GetComponentInChildren<SkiCameraController>().GetComponent<Camera>();camera.aspect=16f/9;
            var review=File.Exists(Output+"/scenario-metrics.json")?JsonUtility.FromJson<Review>(File.ReadAllText(Output+"/scenario-metrics.json")):new Review();review.samples.RemoveAll(m=>m.name.StartsWith("Rail"));
            yield return RailSpinOff(w,camera,review);Directory.CreateDirectory(Output);File.WriteAllText(Output+"/scenario-metrics.json",JsonUtility.ToJson(review,true));UnityEngine.Object.Destroy(w.cameraConfig);UnityEngine.Object.Destroy(w.gameObject);
        }
        [UnityTest]public IEnumerator CaptureReferenceScenarios()
        {
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;float previousCapture=Time.captureDeltaTime;
            var w=PresentationPolishTests.CreateWorld();yield return null;var p=w.player;var camera=w.GetComponentInChildren<SkiCameraController>().GetComponent<Camera>();camera.aspect=16f/9;
            var review=new Review();Directory.CreateDirectory(Output);
            if(Directory.Exists(Output+"/Sequences"))Directory.Delete(Output+"/Sequences",true);
            try
            {
                foreach(var scenario in new[]{"Straight","CarveLeft","CarveRight","HardSkid"})
                {
                    Spawn(w,10,48,16);p.Input.steer=scenario=="CarveLeft"?-.8f:scenario=="CarveRight"?.8f:0;yield return new WaitForSeconds(.8f);
                    if(scenario=="HardSkid"){p.Input.brake=true;yield return new WaitForSeconds(.25f);}Shot(w,camera,review,scenario);
                }
                foreach(var entry in new[]{new Vector2(105,18),new Vector2(900,24)})
                {
                    string name=entry.x<200?"SmallJump":"LargeJump";Spawn(w,0,entry.x-12,entry.y);p.Input.tuck=true;yield return new WaitForSeconds(.1f);Shot(w,camera,review,name+"Approach");
                    int contacts=0;LandingResult landing=default;Action<LandingResult> handler=r=>{contacts++;landing=r;};p.Landed+=handler;
                    float end=Time.time+7;bool shot=false;int frame=0;Time.captureDeltaTime=1f/24;
                    while(Time.time<end&&contacts==0&&!p.Bailed)
                    {
                        yield return null;
                        if(frame++%2==0)Capture(camera,Output+"/Sequences/"+name+"/"+(frame/2-1).ToString("D4")+".png");
                        if(!shot&&p.Motion.airTime>.3f&&!p.Grounded){shot=true;Shot(w,camera,review,name+"Air");}
                    }
                    Time.captureDeltaTime=previousCapture;Assert.That(shot&&contacts>0,Is.True,name+" uses a production lip and real contact");
                    yield return new WaitForSeconds(.06f);Shot(w,camera,review,name+"Landing");review.notes.Add(name+" contact "+landing.quality+" impact "+landing.impact.ToString("F2"));p.Landed-=handler;
                }
                // Known initial angular momentum gives comparable whole rotations;
                // control responsiveness is measured separately below.
                for(int trick=0;trick<4;trick++)
                {
                    string name=new[]{"Spin360","Spin720","Flip","Cork"}[trick];Spawn(w,10,70,18,18);yield return null;
                    p.GetComponent<TrickTracker>().Begin();
                    float time=Mathf.Sqrt(2*(18+p.config.rideHeight-p.config.contactReach)/(-Vector3.Dot(Physics.gravity,p.SupportNormal)));
                    var axis=trick<2?Vector3.up:trick==2?p.Body.rotation*Vector3.right:(Vector3.up*.65f+p.Body.rotation*Vector3.right*.7f+p.Body.rotation*Vector3.forward*.3f).normalized;
                    float degrees=trick==1?720:360;p.Air.SetMomentum(axis*degrees*Mathf.Deg2Rad/time*p.trickConfig.baseInertia*1.08f);
                    Time.captureDeltaTime=1f/24;int frame=0;bool shot=false;float end=Time.time+3;
                    while(Time.time<end&&!p.Bailed&&(!p.Grounded||frame<3))
                    {
                        yield return null;
                        if(frame++%2==0)Capture(camera,Output+"/Sequences/"+name+"/"+(frame/2-1).ToString("D4")+".png");
                        if(!shot&&frame>=18){shot=true;Shot(w,camera,review,name);}
                    }
                    Time.captureDeltaTime=previousCapture;Shot(w,camera,review,name+"Result");review.notes.Add(name+" seeded angular momentum; captured physical rotation, result "+p.LastLanding.quality);
                }
                Spawn(w,10,68,16,8);p.Input.grabLeft=true;yield return new WaitForSeconds(.35f);Shot(w,camera,review,"Grab");
                Spawn(w,10,50,16,3,45);yield return new WaitForSeconds(.9f);Shot(w,camera,review,"SketchyLanding");
                yield return RailSpinOff(w,camera,review);
                // Uninterrupted center park jump line, no inter-feature relocation.
                Spawn(w,0,290,22);p.Input.tuck=true;Time.captureDeltaTime=1f/24;
                for(int frame=0;frame<144;frame++){yield return null;if(frame%2==0)Capture(camera,Output+"/Sequences/ParkRun/"+(frame/2).ToString("D4")+".png");}
                Time.captureDeltaTime=previousCapture;Shot(w,camera,review,"ParkRun");
                Spawn(w,0,390,0);yield return new WaitForSeconds(.35f);Shot(w,camera,review,"MountainSunset");w.GetComponent<AlpineLighting>().Apply(true);yield return null;Shot(w,camera,review,"MountainDay");
                File.WriteAllText(Output+"/scenario-metrics.json",JsonUtility.ToJson(review,true));
            }
            finally{Time.captureDeltaTime=previousCapture;ShaderUtil.allowAsyncCompilation=async;UnityEngine.Object.Destroy(w.cameraConfig);UnityEngine.Object.Destroy(w.gameObject);}
        }
        [UnityTest]public IEnumerator ContinuousParkLineRetainsMomentum()
        {
            var w=PresentationPolishTests.CreateWorld();w.player.gameObject.AddComponent<AudioController>().config=AssetDatabase.LoadAssetAtPath<GameSystemConfig>("Assets/Settings/GameSystemConfig.asset");
            yield return null;var p=w.player;Spawn(w,0,290,22);p.Input.tuck=true;
            var notes=new List<string>();int launches=0,landings=0;float start=Time.time;
            p.TookOff+=()=>{launches++;notes.Add($"launch t={Time.time-start:F2} z={p.Body.position.z:F2} speed={p.Speed:F2}");};
            p.Landed+=r=>{landings++;notes.Add($"landing t={Time.time-start:F2} z={p.Body.position.z:F2} quality={r.quality} impact={r.impact:F2} alignment={r.alignment:F2} upright={r.upright:F2} contacts={p.Contacts.left.hit}/{p.Contacts.right.hit} speed={p.Speed:F2}");};
            var camera=w.GetComponentInChildren<SkiCameraController>().GetComponent<Camera>();float prior=Time.captureDeltaTime;Time.captureDeltaTime=1f/24;
            try
            {
                string sequence=Output+"/Sequences/ContinuousParkLine";if(Directory.Exists(sequence))Directory.Delete(sequence,true);
                for(int frame=0;frame<288&&!p.Bailed;frame++)
                {
                    yield return null;if(frame%4==0)Capture(camera,sequence+"/"+(frame/4).ToString("D4")+".png");
                    foreach(var audio in p.GetComponentsInChildren<AudioSource>())Assert.That(float.IsFinite(audio.volume)&&float.IsFinite(audio.pitch),Is.True);
                }
                notes.Add($"end z={p.Body.position.z:F2} speed={p.Speed:F2} launches={launches} landings={landings} bail={p.Bailed}");
                Directory.CreateDirectory(Output);File.WriteAllLines(Output+"/continuous-park-line.txt",notes);
                Assert.That(!p.Bailed&&p.Body.position.z>460&&launches>=2&&landings>=2,Is.True,"Two production jumps must flow into successive recoverable landings without relocation");
            }
            finally{Time.captureDeltaTime=prior;UnityEngine.Object.Destroy(w.cameraConfig);UnityEngine.Object.Destroy(w.gameObject);}
        }
        [UnityTest]public IEnumerator MeasureInputResponseAndRelease()
        {
            var w=PresentationPolishTests.CreateWorld();yield return null;var p=w.player;Spawn(w,10,70,16,20);yield return null;
            var notes=new List<string>();p.Input.steer=1;yield return new WaitForSeconds(.25f);notes.Add("yaw velocity after .25s input="+p.Body.angularVelocity.magnitude.ToString("F3"));
            yield return new WaitForSeconds(.75f);notes.Add("yaw velocity after 1s input="+p.Body.angularVelocity.magnitude.ToString("F3"));
            p.Input.steer=0;yield return new WaitForSeconds(.3f);notes.Add("free-air yaw after .3s release="+p.Body.angularVelocity.magnitude.ToString("F3"));
            // Replay identical released, aligned near-contact state through real air control.
            p.Body.rotation=Quaternion.Euler(13,0,0);p.Air.SetMomentum(Vector3.up*5*p.trickConfig.baseInertia);
            var prediction=new LandingPrediction{valid=true,time=.24f,normal=Quaternion.Euler(13,0,0)*Vector3.up};
            p.Air.SpotLanding(p.Body,p.Input,prediction,p.trickConfig,.1f);notes.Add("near-contact released yaw from 5rad/s="+p.Body.angularVelocity.magnitude.ToString("F3"));
            Assert.That(p.Body.angularVelocity.magnitude,Is.GreaterThan(0));Directory.CreateDirectory(Output);File.WriteAllLines(Output+"/control-response.txt",notes);
            UnityEngine.Object.Destroy(w.cameraConfig);UnityEngine.Object.Destroy(w.gameObject);
        }
#endif
    }
}
