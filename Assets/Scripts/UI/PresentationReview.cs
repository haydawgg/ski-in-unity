using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
namespace PowderFlow
{
    // Explicit development diagnostic; screenshots use the actual native window and IMGUI controls.
    public class PresentationReview:MonoBehaviour
    {
        public GameFlow flow;string output;
        [Serializable]class Shot {public string name;public int width,height;}
        [Serializable]class Review {public int frameRateCap;public bool pointerAdjustment,pointerSave;public List<Shot> captures=new List<Shot>();}
        IEnumerator Click(Vector2 point)
        {
            flow.reviewPointer=new Event{type=EventType.MouseDown,mousePosition=point,button=0};yield return new WaitForSecondsRealtime(.12f);
            flow.reviewPointer=new Event{type=EventType.MouseUp,mousePosition=point,button=0};yield return new WaitForSecondsRealtime(.12f);
        }
        IEnumerator Capture(string name,Review report)
        {
            yield return new WaitForSecondsRealtime(.35f);string path=Path.Combine(output,name+".png");ScreenCapture.CaptureScreenshot(path);yield return new WaitForSecondsRealtime(.2f);
            if(!File.Exists(path)){Debug.LogError("Missing presentation capture "+path);Application.Quit(3);yield break;}
            report.captures.Add(new Shot{name=name,width=Screen.width,height=Screen.height});
        }
        IEnumerator Start()
        {
            output=Path.GetFullPath("Logs/VP6-native-ui");foreach(var arg in Environment.GetCommandLineArgs())if(arg.StartsWith("--presentation-output="))output=Path.GetFullPath(arg.Substring("--presentation-output=".Length));Directory.CreateDirectory(output);
            var prior=SaveStore.Current;var previousPath=SaveStore.OverridePath;SaveStore.Current=new SavedGame();SaveStore.OverridePath=Path.Combine(output,"review-save.json");var world=GetComponent<MountainWorld>();world.player.Input.BeginInjected();var report=new Review{frameRateCap=Application.targetFrameRate};
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(1080,1920)})
            {
                SaveStore.Current.width=size.x;SaveStore.Current.height=size.y;Screen.SetResolution(size.x,size.y,FullScreenMode.Windowed);
                yield return new WaitForSecondsRealtime(.7f);
                if(Screen.width!=size.x||Screen.height!=size.y){Debug.LogError($"Requested {size}; actual {Screen.width}x{Screen.height}");Application.Quit(3);yield break;}
                string suffix=size.x+"x"+size.y;flow.startInMenu=true;world.GetComponent<AlpineLighting>().Apply(false);
                foreach(var page in new[]{"main","settings","controls","outfit","results"}){flow.ShowPage(page);yield return Capture(page+suffix,report);}
                if(size.x==1280)
                {
                    flow.ShowPage("settings");yield return null;var canvas=new PresentationCanvas(Screen.width,Screen.height,Screen.safeArea);var row=flow.SettingRectangles(canvas)[0];float before=SaveStore.Current.master;
                    yield return Click(new Vector2(row.xMax-24,row.center.y));report.pointerAdjustment=SaveStore.Current.master>before;
                    yield return Click(new Vector2(canvas.width*.75f,canvas.height-PresentationConfig.Active.margin-58));report.pointerSave=flow.Page=="main"&&File.Exists(SaveStore.OverridePath);
                    if(!report.pointerAdjustment||!report.pointerSave){Debug.LogError("Native IMGUI pointer adjustment/save failed");Application.Quit(3);yield break;}
                }
                world.GetComponent<AlpineLighting>().Apply(true);flow.ShowPage("main");yield return Capture("DayMain"+suffix,report);
                flow.ShowPage("outfit");SaveStore.Current.outfit=5;yield return Capture("DayOutfit"+suffix,report);
                flow.startInMenu=false;flow.menu=false;Time.timeScale=1;world.Respawn(1);world.player.Input.tuck=true;GameFlow.ScoreSession=true;yield return new WaitForSeconds(.7f);
                world.player.GetComponent<ComboSystem>().SendMessage("Score",new TrickRecord{yaw=720,grab="Safety",grabTime=1,landing=LandingQuality.Clean});yield return Capture("RideHUD"+suffix,report);
                flow.ShowPage("main");yield return Capture("Pause"+suffix,report);GameFlow.ScoreSession=false;
            }
            File.WriteAllText(Path.Combine(output,"native-ui.json"),JsonUtility.ToJson(report,true));SaveStore.Current=prior;SaveStore.OverridePath=previousPath;Debug.Log("PRESENTATION NATIVE REVIEW PASS / responsive pages, pointer adjustment/save, HUD and pause");Application.Quit(0);
        }
    }
}
