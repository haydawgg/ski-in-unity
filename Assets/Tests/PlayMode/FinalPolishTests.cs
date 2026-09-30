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
    public class FinalPolishTests:PlayWorldTestBase
    {
#if UNITY_EDITOR
        const string Output="Documentation/VisualPolish/Phase7";
        bool asyncShaders;
        [SetUp]public void PrepareReview(){asyncShaders=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;}
        [TearDown]public void RestoreShaders(){ShaderUtil.allowAsyncCompilation=asyncShaders;}
        static MountainWorld CreateWorld()=>PresentationPolishTests.CreateWorld();
        static void Spawn(MountainWorld world,float x,float z,float speed=14,float lift=0)
        {
            var p=world.player;p.Input.BeginInjected();
            Assert.That(Physics.Raycast(world.OnSnow(x,z,40),Vector3.down,out var hit,80,~(1<<8),QueryTriggerInteraction.Ignore),Is.True);
            var forward=Vector3.ProjectOnPlane(Vector3.forward,hit.normal).normalized;
            p.ResetTo(hit.point+hit.normal*(p.config.rideHeight+lift),Quaternion.LookRotation(forward,hit.normal));p.Body.linearVelocity=forward*speed;
            if(lift>0)p.EnterAirWithoutRestartingTrick();
        }
        static Camera ReviewCamera(MountainWorld world,bool follow=false)
        {
            var driver=world.GetComponentInChildren<SkiCameraController>();driver.enabled=follow;var camera=driver.GetComponent<Camera>();camera.aspect=16f/9;camera.fieldOfView=58;return camera;
        }
        static void Capture(Camera camera,string path,int width=1920,int height=1080)
        {
            var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());
            camera.targetTexture=old;RenderTexture.active=active;Object.Destroy(image);Object.Destroy(rt);
        }
        static RailPath FindRail(MountainWorld world,string name)
        {
            foreach(var path in world.GetComponentsInChildren<RailPath>())if(path.name==name)return path;
            Assert.Fail("Missing rail "+name);return null;
        }
        static void EnterRail(MountainWorld world,RailPath path,float yaw=0,float distance=2)
        {
            var p=world.player;p.Input.BeginInjected();var point=path.Evaluate(distance,out var tangent);var normal=Vector3.ProjectOnPlane(Vector3.up,tangent).normalized;
            p.ResetTo(point+Vector3.up*(p.config.rideHeight+.08f),Quaternion.AngleAxis(yaw,normal)*Quaternion.LookRotation(tangent,normal));p.Body.linearVelocity=tangent*10-Vector3.up*.3f;
        }
        static void Detail(Camera camera,SkiPhysicsController p)
        {
            camera.transform.position=p.Body.position+new Vector3(-4,3.1f,-6);camera.transform.LookAt(p.Body.position+new Vector3(0,-.3f,0));
        }
#endif
        [UnityTest]public IEnumerator RailSkiContactYawAndExitStayConsistent()
        {
#if UNITY_EDITOR
            bool baseline=System.Environment.GetEnvironmentVariable("POWDERFLOW_FINAL_BASELINE")=="1";string folder=Output+(baseline?"/Before":"/Rails");
            var world=CreateWorld();yield return null;var p=world.player;p.Input.BeginInjected();var pose=p.GetComponent<SkierPose>();var rail=p.GetComponent<RailSystem>();var camera=ReviewCamera(world);var stats=new List<string>();
            string[] names={"Rail_flat","Rail_down","Rail_kink","Rail_rainbow","Rail_wide","Box_flat","Box_down","Box_kink","Rail_flat","Rail_flat","Rail_flat","Rail_flat"};
            string[] labels={"Flat","Down","Kink","Rainbow","Wide","BoxFlat","BoxDown","BoxKink","Switch","Sideways","Tuck","HeldStyle"};
            for(int i=0;i<(baseline?10:names.Length);i++)
            {
                world.GetComponent<AlpineLighting>().Apply(true);var path=FindRail(world,names[i]);float yaw=i==8?180:i==9?90:0;
                EnterRail(world,path,yaw,names[i]=="Rail_kink"?6.3f:2);if(i==10)p.Input.tuck=true;if(i==11)p.Input.grabLeft=p.Input.grabRight=p.Input.modifierLeft=true;
                yield return new WaitForSeconds(.2f);Assert.That(rail.Riding,Is.True,labels[i]);yield return null;pose.ApplyPose(true);
                path.Closest(p.Body.position-Vector3.up*p.config.rideHeight,out var center,out var tangent);var normal=Vector3.ProjectOnPlane(Vector3.up,tangent).normalized;var heading=Vector3.ProjectOnPlane(p.Body.rotation*Vector3.forward,normal).normalized;
                Assert.That(Physics.Raycast(center+Vector3.up*.5f,Vector3.down,out var hit,1,~(1<<8),QueryTriggerInteraction.Ignore),Is.True);Assert.That(hit.collider.transform.IsChildOf(path.transform),Is.True);
                float leftGap=Vector3.Dot(pose.leftSki.position-hit.point,normal),rightGap=Vector3.Dot(pose.rightSki.position-hit.point,normal);
                stats.Add($"{labels[i]}: leftAxis={Vector3.Dot(pose.leftSki.up,heading):F3}, rightAxis={Vector3.Dot(pose.rightSki.up,heading):F3}, leftGap={leftGap:F3}, rightGap={rightGap:F3}, stance={Vector3.Distance(pose.leftSki.position,pose.rightSki.position):F3}, footSki={Vector3.Distance(pose.leftFoot.position,pose.leftSki.position):F3}, skiForwardNormal={Vector3.Dot(pose.leftSki.forward,normal):F3}");
                Detail(camera,p);Capture(camera,folder+"/"+labels[i]+".png");
                if(!baseline)
                {
                    Assert.That(Vector3.Dot(pose.leftSki.up,heading),Is.GreaterThan(.98f),labels[i]+" left ski follows rider yaw on rail plane");Assert.That(Vector3.Dot(pose.rightSki.up,heading),Is.GreaterThan(.98f),labels[i]+" right ski follows rider yaw on rail plane");
                    Assert.That(leftGap,Is.InRange(.015f,.075f),labels[i]+" ski contact");Assert.That(rightGap,Is.InRange(.015f,.075f));Assert.That(Vector3.Distance(pose.leftFoot.position,pose.leftSki.position),Is.LessThan(.18f));
                }
            }
            if(!baseline)
            {
                p.Input.BeginInjected();p.Input.pop=true;yield return new WaitForFixedUpdate();yield return new WaitForFixedUpdate();Assert.That(rail.Riding,Is.False);Assert.That(p.Speed,Is.GreaterThan(8));p.Input.grabLeft=true;
                yield return new WaitForSeconds(.12f);pose.ApplyPose(true);Assert.That(p.GetComponent<GrabSystem>().Current,Is.EqualTo(GrabType.Safety));Assert.That(Vector3.Distance(pose.leftHand.position,p.GetComponent<GrabSystem>().Target(pose.leftSki,pose.rightSki)),Is.LessThan(.14f));Detail(camera,p);Capture(camera,folder+"/PopSafety.png");
                Spawn(world,10,50,8);yield return new WaitForSeconds(.35f);Assert.That(p.Grounded,Is.True);pose.ApplyPose(true);Assert.That(Vector3.Dot(pose.leftSki.up,Vector3.ProjectOnPlane(p.Body.rotation*Vector3.forward,p.Contacts.left.normal).normalized),Is.GreaterThan(.98f));
                stats.Add($"Ground transition: skiForwardNormal={Vector3.Dot(pose.leftSki.forward,p.Contacts.left.normal):F3}, footSki={Vector3.Distance(pose.leftFoot.position,pose.leftSki.position):F3}");Detail(camera,p);Capture(camera,folder+"/GroundTransition.png");
            }
            Directory.CreateDirectory(folder);File.WriteAllLines(folder+"/rail-review.txt",stats);Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);Debug.Log("FINAL RAIL POSE REVIEW: captured slides, yaw/contact and pop/ground transitions");
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator RotatingGrabsKeepReachAndIndependentEquipment()
        {
#if UNITY_EDITOR
            var world=CreateWorld();yield return null;var p=world.player;Spawn(world,10,68,0,8);yield return null;p.enabled=false;p.GetComponent<RailSystem>().enabled=false;p.Body.isKinematic=true;
            var pose=p.GetComponent<SkierPose>();var grab=p.GetComponent<GrabSystem>();var camera=ReviewCamera(world);var stats=new List<string>();
            foreach(float yaw in new[]{0f,90f,180f})for(int i=0;i<9;i++)
            {
                p.Body.rotation=Quaternion.Euler(13,yaw,yaw==90?22:0);p.Input.BeginInjected();
                switch(i){case 0:p.Input.grabLeft=true;break;case 1:p.Input.grabRight=true;break;case 2:p.Input.grabLeft=p.Input.modifierLeft=p.Input.modifierRight=true;break;case 3:p.Input.grabLeft=p.Input.modifierLeft=true;break;case 4:p.Input.grabRight=p.Input.modifierLeft=true;break;case 5:p.Input.grabLeft=p.Input.modifierRight=true;break;case 6:p.Input.grabRight=p.Input.modifierLeft=p.Input.modifierRight=true;break;case 7:p.Input.grabRight=p.Input.modifierRight=true;break;case 8:p.Input.grabLeft=p.Input.grabRight=true;break;}
                yield return null;yield return null;pose.ApplyPose(true);var hand=grab.LeftHand?pose.leftHand:pose.rightHand;float error=Vector3.Distance(hand.position,grab.Target(pose.leftSki,pose.rightSki));
                Assert.That(error,Is.LessThan(.14f),grab.Current+" wrist at yaw "+yaw);Assert.That(float.IsFinite(pose.leftSki.position.x),Is.True);
                if(i==8)Assert.That(Vector3.Dot(pose.leftSki.up,pose.rightSki.up),Is.LessThan(.9f));stats.Add($"{grab.Current} yaw {yaw}: wrist error={error:F3} m");
                if(yaw==90){camera.transform.position=p.Body.position+new Vector3(3,1,4);camera.transform.LookAt(p.Body.position+Vector3.up*.45f);Capture(camera,Output+"/Grabs/"+grab.Current+".png",1280,720);}
            }
            File.WriteAllLines(Output+"/grab-reach.txt",stats);Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);Debug.Log("FINAL GRAB REVIEW: nine selections at three body orientations, including off-axis rotation");
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator PlainAirSkisStayAttachedThroughTuckAndRotation()
        {
#if UNITY_EDITOR
            bool baseline=System.Environment.GetEnvironmentVariable("POWDERFLOW_FINAL_AIR_BASELINE")=="1";string folder=Output+(baseline?"/BeforeAir":"/Air");
            var world=CreateWorld();yield return null;var p=world.player;Spawn(world,10,68,0,8);yield return null;p.enabled=false;p.GetComponent<RailSystem>().enabled=false;p.Body.isKinematic=true;
            var pose=p.GetComponent<SkierPose>();var camera=ReviewCamera(world);var stats=new List<string>();
            foreach(float yaw in new[]{0f,90f,180f})foreach(bool tuck in new[]{false,true})
            {
                p.Body.rotation=Quaternion.Euler(13,yaw,yaw==90?22:0);p.Input.BeginInjected();p.Input.tuck=tuck;yield return null;yield return null;pose.ApplyPose(true);
                float left=Vector3.Dot(pose.leftSki.up,p.Body.rotation*Vector3.forward),right=Vector3.Dot(pose.rightSki.up,p.Body.rotation*Vector3.forward);
                stats.Add($"yaw={yaw}, tuck={tuck}: leftAxis={left:F3}, rightAxis={right:F3}, leftBinding={Vector3.Distance(pose.leftFoot.position,pose.leftSki.position):F3}, rightBinding={Vector3.Distance(pose.rightFoot.position,pose.rightSki.position):F3}");
                if(!baseline){Assert.That(left,Is.GreaterThan(.98f));Assert.That(right,Is.GreaterThan(.98f));Assert.That(Vector3.Distance(pose.leftFoot.position,pose.leftSki.position),Is.LessThan(.12f));Assert.That(Vector3.Distance(pose.rightFoot.position,pose.rightSki.position),Is.LessThan(.12f));}
                Detail(camera,p);Capture(camera,folder+"/"+(tuck?"Tuck":"Glide")+yaw+".png",1280,720);
            }
            File.WriteAllLines(folder+"/air-review.txt",stats);Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);Debug.Log("FINAL PLAIN AIR REVIEW: glide/tuck at three body orientations with attached bindings");
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator DesktopRegionsLightingAndOutfitsRemainCoherent()
        {
#if UNITY_EDITOR
            var world=CreateWorld();yield return null;var p=world.player;var camera=ReviewCamera(world,true);var stats=new List<string>();
            string[] labels={"Easy","Park","BigAir","Freeride","Lower"};float[] xs={10,10,10,75,5},zs={40,360,920,1120,1350};int[] outfits={0,1,5,1,0};
            foreach(bool day in new[]{false,true})for(int i=0;i<labels.Length;i++)
            {
                world.GetComponent<AlpineLighting>().Apply(day);int outfit=day?(outfits[i]+5)%6:outfits[i];p.GetComponent<OutfitSystem>().Apply(outfit);Spawn(world,xs[i],zs[i],12);yield return new WaitForSeconds(.3f);
                var viewport=camera.WorldToViewportPoint(p.Body.position);Assert.That(viewport.z,Is.GreaterThan(1));Assert.That(viewport.x,Is.InRange(.15f,.85f));Assert.That(viewport.y,Is.InRange(.15f,.8f));Assert.That(p.Bailed,Is.False);Assert.That(Mathf.Abs(Vector3.Dot(camera.transform.right,Vector3.up)),Is.LessThan(.015f));
                string preset=day?"Day":"Sunset";Capture(camera,Output+"/Regions/"+preset+labels[i]+".png");stats.Add($"{preset} {labels[i]}: outfit={world.catalog.characterVisuals.outfits[outfit].name}, x={p.Body.position.x:F2}, z={p.Body.position.z:F2}, surface={p.Surface}, viewport={viewport}");
            }
            foreach(string name in new[]{"Alpine Snow","Alpine Sky","Alpine Prop","Skier Surface","Ski Grooves","Snow Particle"}){var shader=Shader.Find("PowderFlow/"+name);Assert.That(shader,Is.Not.Null);Assert.That(ShaderUtil.ShaderHasError(shader),Is.False,name);}
            File.WriteAllLines(Output+"/region-review.txt",stats);Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);Debug.Log("FINAL REGION REVIEW: five areas, Day/Sunset, four coordinated outfits, 1920x1080 landscape");
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator DesktopActionSequencesUseActualTransitions()
        {
#if UNITY_EDITOR
            var world=CreateWorld();yield return null;var p=world.player;var camera=ReviewCamera(world,true);var pose=p.GetComponent<SkierPose>();var rail=p.GetComponent<RailSystem>();var tracker=p.GetComponent<TrickTracker>();
            int takeoffs=0,landings=0,crashes=0;p.TookOff+=()=>takeoffs++;p.Landed+=r=>{if(r.quality!=LandingQuality.Bail)landings++;};p.GetComponent<BailSystem>().Crashed+=()=>crashes++;
            var stats=new List<string>();float previousCaptureDelta=Time.captureDeltaTime;
            try
            {
                foreach(bool day in new[]{false,true})
                {
                    world.GetComponent<AlpineLighting>().Apply(day);p.GetComponent<OutfitSystem>().Apply(day?5:0);Spawn(world,10,40,14);p.Input.tuck=true;yield return new WaitForSeconds(.15f);
                    string preset=day?"Day":"Sunset",frames="Logs/VP7-"+preset+"-frames";Directory.CreateDirectory(frames);int beforeTakeoffs=takeoffs,beforeLandings=landings,beforeCrashes=crashes,railFrames=0,grabFrames=0,previousBeat=-1;float maxYaw=0;Time.captureDeltaTime=1f/24;
                    for(int frame=0;frame<288;frame++)
                    {
                        int beat=frame*10/24;bool changed=beat!=previousBeat;
                        p.Input.steer=beat>=4&&beat<12?.6f:beat>=18&&beat<27?1:beat>=91?.4f:0;p.Input.modifierRight=beat>=18&&beat<20;p.Input.brake=beat>=12&&beat<16;p.Input.tuck=beat<18||beat>=90;p.Input.grabLeft=(beat>=22&&beat<27)||(beat>=54&&beat<60);p.Input.grabRight=false;
                        if(changed&&beat==18){Spawn(world,10,68,14);p.Input.crouch=p.Input.modifierRight=true;p.Input.steer=1;}
                        if(changed&&beat==20){p.Input.crouch=false;p.Input.pop=true;}
                        if(changed&&beat==44)EnterRail(world,FindRail(world,"Rail_flat"),0,1);
                        if(changed&&beat==52)p.Input.pop=true;
                        if(changed&&beat==75)p.GetComponent<BailSystem>().Crash();
                        if(changed&&beat==81)Spawn(world,-8,42,13);
                        if(changed&&beat==90)Spawn(world,75,1120,13);
                        yield return null;
                        if(rail.Riding)railFrames++;if(p.GetComponent<GrabSystem>().Current!=GrabType.None)grabFrames++;if(tracker.tracking&&!rail.Riding)maxYaw=Mathf.Max(maxYaw,Mathf.Abs(tracker.current.yaw));
                        Assert.That(float.IsFinite(p.Speed),Is.True);Assert.That(p.GetComponent<SnowEffectsController>().LiveParticles,Is.LessThanOrEqualTo(world.graphicsConfig.maxParticles));
                        Capture(camera,frames+"/"+frame.ToString("D4")+".png",1280,720);
                        if(changed)
                        {
                            string label=beat==9?"Carve":beat==15?"Brake":beat==25?"SpinGrab":beat==48?"Rail":beat==57?"RailPopGrab":beat==69?"Landing":beat==77?"Bail":beat==82?"Reset":beat==104?"Powder":null;
                            if(label!=null){Capture(camera,Output+"/Actions/"+preset+label+".png",1280,720);stats.Add($"{preset} {label}: frame={frame}, grounded={p.Grounded}, rail={rail.Riding}, grab={p.GetComponent<GrabSystem>().Current}, bail={p.Bailed}, yaw={tracker.current.yaw:F1}, particles={p.GetComponent<SnowEffectsController>().LiveParticles}");}
                        }
                        previousBeat=beat;
                    }
                    Time.captureDeltaTime=previousCaptureDelta;Assert.That(takeoffs,Is.GreaterThan(beforeTakeoffs));Assert.That(landings,Is.GreaterThan(beforeLandings));Assert.That(crashes,Is.GreaterThan(beforeCrashes));Assert.That(railFrames,Is.GreaterThan(5));Assert.That(grabFrames,Is.GreaterThan(5));Assert.That(maxYaw,Is.GreaterThan(45));
                    stats.Add($"{preset} sequence: actual takeoffs={takeoffs-beforeTakeoffs}, successful landings={landings-beforeLandings}, crashes={crashes-beforeCrashes}, rail samples={railFrames}, grab samples={grabFrames}, max tracked air yaw={maxYaw:F1}; 288 samples / 12 simulated seconds / 1280x720");
                }
            }
            finally{Time.captureDeltaTime=previousCaptureDelta;Directory.CreateDirectory(Output);File.WriteAllLines(Output+"/action-review.txt",stats);}
            Object.Destroy(world.cameraConfig);Object.Destroy(world.gameObject);Debug.Log("FINAL ACTION REVIEW: Day/Sunset actual takeoff, spin/grab, rail/pop, landing, bail, reset and powder; desktop landscape");
#else
            yield break;
#endif
        }
    }
}
