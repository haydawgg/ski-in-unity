using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class ContactLineReviewTests:PlayWorldTestBase
    {
        public static string Output=>Environment.GetEnvironmentVariable("POWDERFLOW_CONTACT_OUTPUT")??"DevelopmentCaptures/ContactLinePass/After";
        public static void Capture(Camera camera,string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));var old=camera.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1280,720,24);
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=active;UnityEngine.Object.Destroy(image);UnityEngine.Object.Destroy(rt);
        }
        public static void Spawn(MountainWorld w,float x,float z,float speed,float lift=0)
        {
            var p=w.player;p.Input.BeginInjected();Assert.That(Physics.Raycast(w.OnSnow(x,z,40),Vector3.down,out var h,80,~(1<<8),QueryTriggerInteraction.Ignore),Is.True);
            var f=Vector3.ProjectOnPlane(Vector3.forward,h.normal).normalized;p.ResetTo(h.point+h.normal*(p.config.rideHeight+lift),Quaternion.LookRotation(f,h.normal));p.Body.linearVelocity=f*speed;
            if(lift>0)p.EnterAirWithoutRestartingTrick();
        }
        [UnityTest]public IEnumerator MainAuthoredLineUsesRealPhysics(){yield return ReviewLine(false);}
        [UnityTest]public IEnumerator AlternateBoxBranchReconnects(){yield return ReviewLine(true);}
        IEnumerator ReviewLine(bool alternate)
        {
#if UNITY_EDITOR
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;float capture=Time.captureDeltaTime;Time.captureDeltaTime=1f/30;
            var w=PresentationPolishTests.CreateWorld();yield return null;var p=w.player;var line=w.worldConfig.freestyleLine;Spawn(w,line.start.x,line.start.z,alternate?22:0);
            var pilot=p.gameObject.AddComponent<FreestyleLineReviewDriver>();pilot.Begin(p,line,alternate);var telemetry=p.gameObject.AddComponent<ContactTelemetryRecorder>();telemetry.Begin(p);
            var camera=w.GetComponentInChildren<Camera>();camera.aspect=16f/9;string name=alternate?"AlternateLine":"AuthoredLine";int frame=0;float began=Time.time;var framing=new List<string>{"time,z,viewportX,viewportY,cameraDistance,horizonRoll"};
            string sequence=Output+"/Sequences/"+name;if(Directory.Exists(sequence))Directory.Delete(sequence,true);
            try
            {
                while(Time.time-began<45&&!pilot.Complete&&!pilot.Failed)
                {
                    yield return null;
                    if(frame%3==0)
                    {
                        Capture(camera,Output+"/Sequences/"+name+"/"+(frame/3).ToString("D4")+".png");var v=camera.WorldToViewportPoint(p.Body.position);float roll=Mathf.Abs(Vector3.Dot(camera.transform.right,Vector3.up));
                        framing.Add(string.Join(",",(Time.time-began).ToString("F4",CultureInfo.InvariantCulture),p.Body.position.z.ToString("F4",CultureInfo.InvariantCulture),v.x.ToString("F4",CultureInfo.InvariantCulture),v.y.ToString("F4",CultureInfo.InvariantCulture),Vector3.Distance(camera.transform.position,p.Body.position).ToString("F4",CultureInfo.InvariantCulture),roll.ToString("F5",CultureInfo.InvariantCulture)));
                        Assert.That(v.z,Is.GreaterThan(1));Assert.That(v.x,Is.InRange(.05f,.95f));Assert.That(v.y,Is.InRange(.05f,.95f));Assert.That(roll,Is.LessThan(.015f));
                    }
                    frame++;
                }
                pilot.Save(Output+"/"+name+"-events.csv");telemetry.Save(Output+"/Telemetry/"+name+".csv");File.WriteAllLines(Output+"/"+name+"-camera.csv",framing);Capture(camera,Output+"/States/"+name+"Exit.png");
                TestContext.WriteLine($"{name}: time={Time.time-began:F2} z={p.Body.position.z:F2} x={p.Body.position.x:F2} speed={p.Speed:F2} jumps={pilot.Launches} landings={pilot.Landings} rails={string.Join("/",pilot.Rails)} resets={pilot.Resets} bail={p.Bailed}");
                Assert.That(pilot.Complete&&!pilot.Failed,Is.True,"Continuous input-only line must reach its exit without reset or bail");
                Assert.That(p.Body.position.z,Is.LessThan(800),"Final snow landing and recovery stay in the terrain park");Assert.That(p.Grounded,Is.True);Assert.That(pilot.Launches,Is.GreaterThanOrEqualTo(2));Assert.That(pilot.Landings,Is.GreaterThanOrEqualTo(2));Assert.That(pilot.Rails.Count,Is.GreaterThanOrEqualTo(1));
                Assert.That(pilot.Rails.Contains(alternate?"Box_wide":"Rail_down"),Is.True,"Reach the selected branch from the first landing");
            }
            finally{Time.captureDeltaTime=capture;ShaderUtil.allowAsyncCompilation=async;UnityEngine.Object.Destroy(w.cameraConfig);UnityEngine.Object.Destroy(w.gameObject);}
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator CaptureSmoothSlopeAndRollerContact()
        {
#if UNITY_EDITOR
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;float capture=Time.captureDeltaTime;Time.captureDeltaTime=1f/30;
            try
            {
            foreach(string profile in new[]{"Slope","Rollers"})
            {
                var obj=new GameObject("Contact capture / "+profile);PhysicsTestWorld.FactoryOnly=true;var w=obj.AddComponent<PhysicsTestWorld>();PhysicsTestWorld.FactoryOnly=false;w.enabled=false;
                w.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");w.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");w.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");w.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");
                PhysicsTestWorld.ContactQualityRoute(obj.transform,new Vector3(270,80,0),profile);Physics.SyncTransforms();var normal=new Vector3(0,1,.23f).normalized;var forward=Vector3.ProjectOnPlane(Vector3.forward,normal).normalized;
                w.SpawnPlayer(new Vector3(270,79.54f,2)+normal*w.physicsConfig.rideHeight,Quaternion.LookRotation(forward,normal));var p=w.player;p.transform.SetParent(obj.transform,true);w.followCamera.transform.SetParent(obj.transform,true);p.Input.BeginInjected();p.Body.linearVelocity=forward*16;
                var sun=new GameObject("Lab sun").AddComponent<Light>();sun.transform.SetParent(obj.transform);sun.type=LightType.Directional;sun.intensity=1.5f;sun.transform.rotation=Quaternion.Euler(35,-25,0);RenderSettings.ambientLight=new Color(.55f,.6f,.7f);
                var telemetry=p.gameObject.AddComponent<ContactTelemetryRecorder>();telemetry.Begin(p);float began=Time.time;int frame=0,ground=0;
                try
                {
                    while(Time.time-began<8)
                    {
                        yield return null;if(p.Grounded)ground++;if(frame%3==0)Capture(w.followCamera,Output+"/Sequences/Lab"+profile+"/"+(frame/3).ToString("D4")+".png");frame++;
                    }
                    telemetry.Save(Output+"/Telemetry/Lab"+profile+".csv");Assert.That(ground,Is.GreaterThan(frame*.97f));Assert.That(p.Bailed,Is.False);
                }
                finally{UnityEngine.Object.Destroy(obj);}
            }
            }
            finally{Time.captureDeltaTime=capture;ShaderUtil.allowAsyncCompilation=async;PhysicsTestWorld.FactoryOnly=false;}
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator RecordProductionContactBaseline()
        {
#if UNITY_EDITOR
            bool async=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
            var w=PresentationPolishTests.CreateWorld();yield return null;var p=w.player;var recorder=p.gameObject.AddComponent<ContactTelemetryRecorder>();var camera=w.GetComponentInChildren<Camera>();camera.aspect=16f/9;
            try
            {
                foreach(string scenario in new[]{"Straight","GentleCarve","HardCarve","RoughTerrain","Undulations","SmallJump","LargeJump","ExistingParkLine","ExistingRailLine"})
                {
                    float x=scenario=="RoughTerrain"?75:scenario=="ExistingRailLine"?-17:10,z=scenario=="RoughTerrain"?1080:48,speed=16,duration=3;
                    if(scenario=="SmallJump"){x=0;z=93;speed=18;duration=5;}
                    if(scenario=="LargeJump"){x=0;z=888;speed=24;duration=7;}
                    if(scenario=="ExistingParkLine"){x=0;z=290;speed=22;duration=12;}
                    if(scenario=="Undulations"){x=22;z=350;duration=6;}
                    if(scenario=="ExistingRailLine"){z=185;speed=12;duration=5;}
                    Spawn(w,x,z,speed);p.Input.tuck=true;p.Input.steer=scenario=="GentleCarve"?.25f:scenario=="HardCarve"?.8f:0;recorder.Begin(p);
                    int ticks=Mathf.RoundToInt(duration/Time.fixedDeltaTime);int frame=0;
                    for(int i=0;i<ticks;i++)
                    {
                        yield return new WaitForFixedUpdate();
                        if(i%20==0)Capture(camera,Output+"/Sequences/"+scenario+"/"+(frame++).ToString("D4")+".png");
                        if(i==100||i==ticks-1)Capture(camera,Output+"/States/"+scenario+(i==100?"Start":"End")+".png");
                    }
                    recorder.Save(Output+"/Telemetry/Production"+scenario+".csv");Assert.That(float.IsFinite(p.Speed),Is.True);
                }
            }
            finally{ShaderUtil.allowAsyncCompilation=async;UnityEngine.Object.Destroy(w.cameraConfig);UnityEngine.Object.Destroy(w.gameObject);}
#else
            yield break;
#endif
        }
    }
}
