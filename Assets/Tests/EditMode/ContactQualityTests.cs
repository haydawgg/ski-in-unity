using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;
namespace PowderFlow.Tests
{
    public class ContactQualityTests
    {
        GameObject root;SkiPhysicsController p;SkiPhysicsConfig config;TrickConfig tricks;SimulationMode previous;
        static string Output=>Environment.GetEnvironmentVariable("POWDERFLOW_CONTACT_OUTPUT")??"DevelopmentCaptures/ContactLinePass/After";
        [SetUp]public void Setup()
        {
            previous=Physics.simulationMode;Physics.simulationMode=SimulationMode.Script;
            config=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset"));config.resetDepth=-100000;
            tricks=UnityEngine.Object.Instantiate(AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset"));
            root=new GameObject("Contact lab");var skier=new GameObject("Measured skier");skier.transform.SetParent(root.transform);
            skier.AddComponent<Rigidbody>();skier.AddComponent<SkierInput>().BeginInjected();skier.AddComponent<SkiContactSystem>();p=skier.AddComponent<SkiPhysicsController>();p.Initialize(config);p.trickConfig=tricks;
        }
        [TearDown]public void Cleanup()
        {
            foreach(var filter in root.GetComponentsInChildren<MeshFilter>())if(filter.sharedMesh)UnityEngine.Object.DestroyImmediate(filter.sharedMesh);
            UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(config);UnityEngine.Object.DestroyImmediate(tricks);Physics.simulationMode=previous;
        }
        void Route(string profile)
        {
            PhysicsTestWorld.ContactQualityRoute(root.transform,new Vector3(4000,0,0),profile);Physics.SyncTransforms();
            Physics.Raycast(new Vector3(4000+(profile=="OneSki"?-.04f:0),10,2),Vector3.down,out var h,20,~(1<<8),QueryTriggerInteraction.Ignore);
            if(profile=="OneSki"){h.point=new Vector3(3999.96f,-.46f,2);h.normal=new Vector3(0,1,.23f).normalized;}
            var f=Vector3.ProjectOnPlane(Vector3.forward,h.normal).normalized;p.ResetTo(h.point+h.normal*config.rideHeight,Quaternion.LookRotation(f,h.normal));p.Body.linearVelocity=f*16;
        }
        static string F(float n)=>n.ToString("F6",CultureInfo.InvariantCulture);
        static int Samples(Vector3 position,Vector3 forward,SkiPhysicsConfig c)
        {
            int count=0;for(int i=-1;i<=1;i+=2)if(Physics.SphereCast(position+forward*c.contactHalfLength*i+Vector3.up*c.probeLift,c.probeRadius,Vector3.down,out var h,c.probeRange,~(1<<8),QueryTriggerInteraction.Ignore))
            {float height=Vector3.Dot(position-h.point,h.normal);if(height<=c.contactReach&&height>=-.3f)count++;}return count;
        }
        [TestCase("Flat",0f)][TestCase("Slope",0f)][TestCase("Slope",.25f)][TestCase("Slope",.8f)]
        [TestCase("Rollers",0f)][TestCase("Ripple",0f)][TestCase("Seam",0f)][TestCase("OneSki",0f)]
        public void RecordDeterministicRoute(string profile,float steer)
        {
            Route(profile);p.Input.steer=steer;
            var lines=new List<string>{"time,x,y,z,left,right,leftSamples,rightSamples,leftHeight,rightHeight,height,rawNx,rawNy,rawNz,supportNx,supportNy,supportNz,supportAcceleration,verticalVelocity,normalVelocity,grounded,transitions,speed,lateralAcceleration"};
            int transitions=0,groundTicks=0,oneTicks=0;bool grounded=false;float lastHeight=0,totalHeightStep=0,minHeight=float.MaxValue,maxHeight=float.MinValue;Vector3 priorVelocity=p.Body.linearVelocity;
            int ticks=steer==0?1000:250;
            for(int i=0;i<ticks;i++)
            {
                p.Step(.01f);var c=p.Contacts;var f=Vector3.ProjectOnPlane(p.Body.rotation*Vector3.forward,Vector3.up).normalized;var side=Vector3.Cross(Vector3.up,f);
                int left=Samples(p.Body.position-side*config.skiSeparation*.5f,f,config),right=Samples(p.Body.position+side*config.skiSeparation*.5f,f,config);
                if(i>0&&grounded!=p.Grounded)transitions++;grounded=p.Grounded;if(grounded)groundTicks++;if(c.left.hit!=c.right.hit)oneTicks++;
                float support=p.Grounded?Mathf.Max(0,(config.rideHeight-c.Height)*config.supportSpring-Vector3.Dot(p.Body.linearVelocity,p.SupportNormal)*config.supportDamping-Vector3.Dot(Physics.gravity,p.SupportNormal)):0;
                var velocity=p.Body.linearVelocity;float lateral=Vector3.Dot((velocity-priorVelocity)/.01f,p.Body.rotation*Vector3.right);priorVelocity=velocity;
                var position=p.Body.position;var n=(c.left.rawHit&&c.right.rawHit?c.left.rawNormal+c.right.rawNormal:c.left.rawHit?c.left.rawNormal:c.right.rawNormal).normalized;var sn=p.SupportNormal;
                lines.Add(string.Join(",",F(i*.01f),F(position.x),F(position.y),F(position.z),c.left.hit?"1":"0",c.right.hit?"1":"0",left,right,F(c.left.height),F(c.right.height),F(c.Height),F(n.x),F(n.y),F(n.z),F(sn.x),F(sn.y),F(sn.z),F(support),F(velocity.y),F(Vector3.Dot(velocity,sn)),grounded?"1":"0",transitions,F(p.Speed),F(lateral)));
                if(i>100){totalHeightStep+=Mathf.Abs(c.Height-lastHeight);minHeight=Mathf.Min(minHeight,c.Height);maxHeight=Mathf.Max(maxHeight,c.Height);}lastHeight=c.Height;
                Physics.Simulate(.01f);Assert.That(float.IsFinite(p.Speed),Is.True);
            }
            Directory.CreateDirectory(Output+"/Telemetry");string name=profile+(steer==0?"":steer<.5f?"GentleCarve":"HardCarve");File.WriteAllLines(Output+"/Telemetry/"+name+".csv",lines);
            TestContext.WriteLine($"{name}: transitions={transitions} ground={groundTicks}/{ticks} oneSki={oneTicks} heightStepSum={totalHeightStep:F5} speed={p.Speed:F2}");
            Assert.That(p.Bailed,Is.False);if(profile=="OneSki")Assert.That(oneTicks,Is.GreaterThan(900));
            if(profile=="Flat"||profile=="Slope")Assert.That(groundTicks,Is.GreaterThan(ticks*.97f));
            if(config.refineContactSurface&&(profile=="Flat"||profile=="Slope"||profile=="OneSki"))Assert.That(maxHeight-minHeight,Is.LessThan(.002f),"Constant surfaces must settle without visible suspension noise");
            if(profile=="Rollers"||profile=="Seam")Assert.That(transitions,Is.Zero,"Shallow smooth shape changes must retain continuous support");
        }
        void Tick(int count){for(int i=0;i<count;i++){p.Step(.01f);Physics.Simulate(.01f);}}
        [Test]public void PopReleasesImmediatelyAndDoesNotRegroundDuringClearAir()
        {
            Route("Slope");Tick(50);int takeoffs=0,landings=0;p.TookOff+=()=>takeoffs++;p.Landed+=r=>landings++;p.Input.pop=true;p.Step(.01f);
            Assert.That(p.Grounded,Is.False);Assert.That(takeoffs,Is.EqualTo(1));Physics.Simulate(.01f);
            for(int i=0;i<300;i++)
            {
                p.Step(.01f);float clearance=(p.Body.position.y+.23f*p.Body.position.z)/Mathf.Sqrt(1+.23f*.23f);
                if(clearance>config.contactReach+.03f)Assert.That(p.Grounded,Is.False,"No held contact during genuine flight");Physics.Simulate(.01f);
            }
            Assert.That(p.Grounded,Is.True);Assert.That(landings,Is.EqualTo(1));Assert.That(takeoffs,Is.EqualTo(1));Assert.That(p.Bailed,Is.False);
        }
        [Test]public void LandingStartsANewSupportFrameWithoutASecondLaunch()
        {
            Route("Slope");var position=p.Body.position;p.ResetTo(position+Vector3.up*8,Quaternion.Euler(-20,0,0));p.Body.linearVelocity=new Vector3(0,0,18);p.EnterAirWithoutRestartingTrick();
            int landings=0,launches=0;p.Landed+=r=>landings++;p.TookOff+=()=>launches++;
            Tick(400);Assert.That(landings,Is.EqualTo(1));Assert.That(launches,Is.Zero);Assert.That(p.Grounded&&!p.Bailed,Is.True);
            Assert.That(Vector3.Angle(p.SupportNormal,p.Contacts.Normal),Is.LessThan(.1f));
        }
        [Test]public void MildTerrainRecapturePreservesMomentumAndOneLanding()
        {
            Route("Slope");var rotation=p.Body.rotation;var position=p.Body.position;p.ResetTo(position+rotation*Vector3.up*.4f,rotation);p.Body.linearVelocity=rotation*Vector3.forward*16;p.EnterAirWithoutRestartingTrick();
            int landings=0;p.Landed+=r=>landings++;Tick(200);Assert.That(p.Bailed,Is.False);Assert.That(p.Grounded,Is.True);Assert.That(landings,Is.EqualTo(1));Assert.That(p.Speed,Is.GreaterThan(15));
        }
        [Test]public void ARealMissingSurfaceReleasesSupportWithoutHysteresis()
        {
            Route("Slope");Tick(40);Assert.That(p.Grounded,Is.True);root.GetComponentInChildren<MeshCollider>().enabled=false;p.Step(.01f);
            Assert.That(p.Grounded,Is.False);Assert.That(p.Contacts.left.hit||p.Contacts.right.hit,Is.False);Assert.That(p.SupportAcceleration,Is.Zero);
        }
    }
}
