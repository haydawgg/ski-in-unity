using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace PowderFlow
{
    public class StandaloneSmokeTest:MonoBehaviour
    {
        public GameFlow flow;
        IEnumerator Start()
        {
            if(Array.Exists(Environment.GetCommandLineArgs(),a=>a=="--menu-capture"))
            {
                yield return new WaitForSecondsRealtime(2);ScreenCapture.CaptureScreenshot(Application.dataPath+"/../smoke-menu.png");yield return new WaitForSecondsRealtime(.5f);Application.Quit(0);yield break;
            }
            QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;var world=GetComponent<MountainWorld>();var p=world.player;p.Input.BeginInjected();p.Input.tuck=true;
            yield return new WaitForSeconds(2);double began=Time.realtimeSinceStartupAsDouble,previous=began;int frames=0;float maxSpeed=0,startZ=p.Body.position.z;var frameTimes=new List<double>();
            while(Time.realtimeSinceStartupAsDouble-began<12){double now=Time.realtimeSinceStartupAsDouble;if(frames>0)frameTimes.Add((now-previous)*1000);previous=now;frames++;maxSpeed=Mathf.Max(maxSpeed,p.Speed);yield return null;}
            double fps=frames/(Time.realtimeSinceStartupAsDouble-began);var path=Application.dataPath+"/../smoke-report.json";
            frameTimes.Sort();double percentile(float q)=>frameTimes[Mathf.Clamp(Mathf.CeilToInt((frameTimes.Count-1)*q),0,frameTimes.Count-1)];string number(double v)=>v.ToString("F2",System.Globalization.CultureInfo.InvariantCulture);
            File.WriteAllText(path,$"{{\"averageFps\":{number(fps)},\"averageFrameMs\":{number(1000/fps)},\"p95FrameMs\":{number(percentile(.95f))},\"p99FrameMs\":{number(percentile(.99f))},\"width\":{Screen.width},\"height\":{Screen.height},\"quality\":{SaveStore.Current.quality},\"maxSpeed\":{number(maxSpeed)},\"distance\":{number(p.Body.position.z-startZ)},\"gpu\":\"{SystemInfo.graphicsDeviceName}\"}}");
            ScreenCapture.CaptureScreenshot(Application.dataPath+"/../smoke-gameplay.png");yield return new WaitForSeconds(.2f);Application.Quit(fps>60&&p.Body.position.z>startZ+30?0:2);
        }
    }
}
