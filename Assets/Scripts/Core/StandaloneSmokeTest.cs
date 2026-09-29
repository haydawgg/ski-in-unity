using System;
using System.Collections;
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
            QualitySettings.vSyncCount=0;Application.targetFrameRate=-1;var world=GetComponent<MountainWorld>();var p=world.player;p.Input.injected=true;p.Input.tuck=true;
            yield return new WaitForSeconds(2);double began=Time.realtimeSinceStartupAsDouble;int frames=0;float maxSpeed=0,startZ=p.Body.position.z;
            while(Time.realtimeSinceStartupAsDouble-began<12){frames++;maxSpeed=Mathf.Max(maxSpeed,p.Speed);yield return null;}
            double fps=frames/(Time.realtimeSinceStartupAsDouble-began);var path=Application.dataPath+"/../smoke-report.json";
            File.WriteAllText(path,$"{{\"averageFps\":{fps.ToString("F1",System.Globalization.CultureInfo.InvariantCulture)},\"maxSpeed\":{maxSpeed.ToString("F2",System.Globalization.CultureInfo.InvariantCulture)},\"distance\":{(p.Body.position.z-startZ).ToString("F1",System.Globalization.CultureInfo.InvariantCulture)},\"gpu\":\"{SystemInfo.graphicsDeviceName}\"}}");
            ScreenCapture.CaptureScreenshot(Application.dataPath+"/../smoke-gameplay.png");yield return new WaitForSeconds(.2f);Application.Quit(fps>60&&p.Body.position.z>startZ+30?0:2);
        }
    }
}
