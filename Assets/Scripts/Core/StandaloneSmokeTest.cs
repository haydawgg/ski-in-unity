using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
namespace PowderFlow
{
    public class StandaloneSmokeTest:MonoBehaviour
    {
        public GameFlow flow;
        bool railReviewFailed;
        [Serializable]class RailReviewReport {public bool captured,landed;public float leftAxis,rightAxis,popSpeed,displaySpeed;}
        [Serializable]class PerformanceReport
        {
            public string gpu,cpu;public int width,height,quality,frameRateCap,samples;
            public double averageFps,meanFrameMs,p50FrameMs,p95FrameMs,p99FrameMs,meanMainThreadMs,meanGcBytes;
        }
        IEnumerator CaptureRail(MountainWorld world)
        {
            var p=world.player;RailPath path=null;foreach(var candidate in world.GetComponentsInChildren<RailPath>())if(candidate.name=="Rail_flat"){path=candidate;break;}
            if(!path){railReviewFailed=true;Debug.LogError("Native rail review missing flat rail");yield break;}
            p.Input.BeginInjected();p.Input.tuck=true;var point=path.Evaluate(1,out var tangent);p.ResetTo(point+Vector3.up*(p.config.rideHeight+.08f),Quaternion.LookRotation(tangent));p.Body.linearVelocity=tangent*10-Vector3.up*.3f;
            yield return new WaitForSeconds(.25f);var rail=p.GetComponent<RailSystem>();var pose=p.GetComponent<SkierPose>();pose.ApplyPose(true);
            var report=new RailReviewReport{captured=rail.Riding,leftAxis=Vector3.Dot(pose.leftSki.up,p.Body.rotation*Vector3.forward),rightAxis=Vector3.Dot(pose.rightSki.up,p.Body.rotation*Vector3.forward),displaySpeed=p.Speed};
            var output=Application.dataPath+"/../";ScreenCapture.CaptureScreenshot(output+"smoke-rail.png");yield return new WaitForSeconds(.2f);
            p.Input.pop=true;yield return new WaitForSeconds(.12f);report.popSpeed=p.Speed;ScreenCapture.CaptureScreenshot(output+"smoke-rail-exit.png");yield return new WaitForSeconds(.2f);
            float limit=Time.time+3;while(!p.Grounded&&!p.Bailed&&Time.time<limit)yield return null;report.landed=p.Grounded&&!p.Bailed;
            ScreenCapture.CaptureScreenshot(output+"smoke-rail-landing.png");yield return new WaitForSeconds(.2f);File.WriteAllText(output+"smoke-rail-report.json",JsonUtility.ToJson(report,true));
            railReviewFailed=!report.captured||!report.landed||report.leftAxis<.98f||report.rightAxis<.98f||report.popSpeed<8;
            if(railReviewFailed)Debug.LogError("Native rail capture/pose/pop/landing failed");else Debug.Log("NATIVE RAIL REVIEW PASS / actual captured slide, aligned tucked skis, pop and landing");
        }
        IEnumerator Start()
        {
            if(Array.Exists(Environment.GetCommandLineArgs(),a=>a=="--menu-capture"))
            {
                yield return new WaitForSecondsRealtime(2);ScreenCapture.CaptureScreenshot(Application.dataPath+"/../smoke-menu.png");yield return new WaitForSecondsRealtime(.5f);Application.Quit(0);yield break;
            }
            var world=GetComponent<MountainWorld>();var p=world.player;
            if(Array.Exists(Environment.GetCommandLineArgs(),a=>a=="--smoke-rail")){yield return CaptureRail(world);if(railReviewFailed){Application.Quit(3);yield break;}}
            int area=0;float initialSpeed=0,runDuration=12;foreach(var arg in Environment.GetCommandLineArgs())
            {
                if(arg.StartsWith("--smoke-area="))int.TryParse(arg.Substring("--smoke-area=".Length),out area);
                if(arg.StartsWith("--smoke-speed="))float.TryParse(arg.Substring("--smoke-speed=".Length),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out initialSpeed);
                if(arg.StartsWith("--smoke-duration="))float.TryParse(arg.Substring("--smoke-duration=".Length),System.Globalization.NumberStyles.Float,System.Globalization.CultureInfo.InvariantCulture,out runDuration);
            }
            area=Mathf.Clamp(area,0,4);initialSpeed=float.IsFinite(initialSpeed)?Mathf.Clamp(initialSpeed,0,25):0;
            runDuration=float.IsFinite(runDuration)?Mathf.Clamp(runDuration,4,30):12;
            var starts=new[]{new Vector2(0,12),new Vector2(0,360),new Vector2(0,830),new Vector2(75,1120),new Vector2(0,1330)};
            var point=starts[area];p.ResetTo(world.OnSnow(point.x,point.y,p.config.rideHeight),Quaternion.Euler(13,0,0));p.Body.linearVelocity=Vector3.ProjectOnPlane(Vector3.forward,p.SupportNormal).normalized*initialSpeed;p.Input.BeginInjected();p.Input.tuck=true;
            yield return new WaitForSeconds(2);double began=Time.realtimeSinceStartupAsDouble;float maxSpeed=0,startZ=p.Body.position.z;
            bool profile=Array.Exists(Environment.GetCommandLineArgs(),a=>a=="--profile");var intervals=new List<double>();double previousFrame=began,mainTotal=0,gcTotal=0;
            var main=profile?ProfilerRecorder.StartNew(ProfilerCategory.Internal,"Main Thread",1):default;
            var gc=profile?ProfilerRecorder.StartNew(ProfilerCategory.Memory,"GC Allocated In Frame",1):default;
            while(Time.realtimeSinceStartupAsDouble-began<runDuration)
            {
                maxSpeed=Mathf.Max(maxSpeed,p.Speed);yield return null;
                if(profile){double now=Time.realtimeSinceStartupAsDouble;intervals.Add((now-previousFrame)*1000);previousFrame=now;if(main.Valid)mainTotal+=main.LastValue/1000000.0;if(gc.Valid)gcTotal+=gc.LastValue;}
            }
            if(profile&&intervals.Count>0)
            {
                double total=0;foreach(double interval in intervals)total+=interval;intervals.Sort();double percentile(float q)=>intervals[Mathf.Clamp(Mathf.CeilToInt(intervals.Count*q)-1,0,intervals.Count-1)];
                var performance=new PerformanceReport{gpu=SystemInfo.graphicsDeviceName,cpu=SystemInfo.processorType,width=Screen.width,height=Screen.height,quality=SaveStore.Current.quality,frameRateCap=Application.targetFrameRate,samples=intervals.Count,averageFps=intervals.Count*1000/total,meanFrameMs=total/intervals.Count,p50FrameMs=percentile(.5f),p95FrameMs=percentile(.95f),p99FrameMs=percentile(.99f),meanMainThreadMs=main.Valid?mainTotal/intervals.Count:-1,meanGcBytes=gc.Valid?gcTotal/intervals.Count:-1};
                File.WriteAllText(Application.dataPath+"/../performance-report.json",JsonUtility.ToJson(performance,true));main.Dispose();gc.Dispose();
            }
            float distance=p.Body.position.z-startZ;var path=Application.dataPath+"/../smoke-report.json";
            string number(double v)=>v.ToString("F2",System.Globalization.CultureInfo.InvariantCulture);
            File.WriteAllText(path,$"{{\"area\":{area},\"day\":{world.GetComponent<AlpineLighting>().day.ToString().ToLowerInvariant()},\"initialSpeed\":{number(initialSpeed)},\"runSeconds\":{number(runDuration)},\"startX\":{number(point.x)},\"startZ\":{number(point.y)},\"runStartZ\":{number(startZ)},\"frameRateCap\":{Application.targetFrameRate},\"vSyncCount\":{QualitySettings.vSyncCount},\"width\":{Screen.width},\"height\":{Screen.height},\"quality\":{SaveStore.Current.quality},\"maxSpeed\":{number(maxSpeed)},\"distance\":{number(distance)}}}");
            ScreenCapture.CaptureScreenshot(Application.dataPath+"/../smoke-gameplay.png");yield return new WaitForSeconds(.2f);Application.Quit(distance>30?0:2);
        }
    }
}
