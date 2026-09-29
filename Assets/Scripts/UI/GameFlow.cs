using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
namespace PowderFlow
{
    [DefaultExecutionOrder(500)]
    public class GameFlow:MonoBehaviour
    {
        public GameSystemConfig config;public bool startInMenu;
        public static bool ScoreSession;public bool menu;public float Remaining {get;private set;}
        string page="main";int selected;MountainWorld world;GUIStyle title,body,button;Texture2D shade;
        void Start()
        {
            world=GetComponent<MountainWorld>();Remaining=config.sessionDuration;menu=startInMenu;Time.timeScale=menu?0:1;
            world.player.ResetTo(world.player.startPosition,world.player.startRotation);ApplySettings();
            if(System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a=="--smoke-test")){menu=false;Time.timeScale=1;gameObject.AddComponent<StandaloneSmokeTest>().flow=this;}
            else if(System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a=="--menu-capture"))gameObject.AddComponent<StandaloneSmokeTest>().flow=this;
        }
        public void ApplySettings()
        {
            var saved=SaveStore.Current;AudioListener.volume=saved.master;
            world.GetComponent<AlpineLighting>()?.Apply(saved.day);world.player.GetComponent<OutfitSystem>().Apply(saved.outfit);
            var camera=world.cameraConfig;camera.normalFov=saved.fov;camera.cameraSensitivity=saved.cameraSensitivity;
            var pipeline=(UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
            if(pipeline){pipeline.renderScale=saved.quality==0?.7f:saved.quality==1?.85f:1;pipeline.shadowDistance=saved.quality==0?50:saved.quality==1?85:world.graphicsConfig.shadowDistance;pipeline.shadowCascadeCount=saved.quality<2?2:4;}
            if(!Application.isEditor)Screen.SetResolution(saved.width,saved.height,saved.fullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
            Cursor.visible=menu;
        }
        void Update()
        {
            if(world.player.Input.pause){menu=!menu;page="main";selected=0;Time.timeScale=menu?0:1;Cursor.visible=menu;}
            if(!menu&&ScoreSession){Remaining-=Time.deltaTime;if(Remaining<=0){SaveStore.Current.highScore=Mathf.Max(SaveStore.Current.highScore,world.player.GetComponent<ComboSystem>().Total);SaveStore.Save();menu=true;page="results";Time.timeScale=0;}}
            if(!menu)return;
            var k=Keyboard.current;var p=Gamepad.current;
            bool up=(k?.upArrowKey.wasPressedThisFrame??false)||(p?.dpad.up.wasPressedThisFrame??false),down=(k?.downArrowKey.wasPressedThisFrame??false)||(p?.dpad.down.wasPressedThisFrame??false);
            int count=page=="main"?(startInMenu?5:6):page=="settings"?15:1;
            if(up)selected=(selected+count-1)%count;if(down)selected=(selected+1)%count;
            bool enter=(k?.enterKey.wasPressedThisFrame??false)||(p?.buttonSouth.wasPressedThisFrame??false);
            if(enter&&page=="main")Activate(selected);else if(enter&&(page=="controls"||page=="results")){if(page=="results")ScoreSession=false;page="main";selected=0;}
            if(page=="settings")
            {
                int delta=((k?.rightArrowKey.wasPressedThisFrame??false)||(p?.dpad.right.wasPressedThisFrame??false)?1:0)-((k?.leftArrowKey.wasPressedThisFrame??false)||(p?.dpad.left.wasPressedThisFrame??false)?1:0);
                if(delta!=0)Adjust(selected,delta);if(enter&&selected==14){SaveStore.Save();ApplySettings();page="main";selected=0;}
            }
        }
        void Activate(int option)
        {
            if(startInMenu)
            {
                if(option<2){ScoreSession=option==1;Time.timeScale=1;SceneManager.LoadScene("Mountain");}
                else if(option==2){page="settings";selected=0;}else if(option==3){page="controls";selected=0;}else Application.Quit();
            }
            else
            {
                if(option==0){menu=false;Time.timeScale=1;Cursor.visible=false;}
                else if(option==1){world.Respawn(0);world.player.GetComponent<ComboSystem>().Restart();Remaining=config.sessionDuration;menu=false;Time.timeScale=1;}
                else if(option==2){world.Respawn(1);menu=false;Time.timeScale=1;}
                else if(option==3){page="settings";selected=0;}
                else if(option==4){Time.timeScale=1;SceneManager.LoadScene("MainMenu");}else Application.Quit();
            }
        }
        void Adjust(int item,int delta)
        {
            var s=SaveStore.Current;
            switch(item)
            {
                case 0:s.master=Mathf.Clamp01(s.master+delta*.05f);break;case 1:s.sfx=Mathf.Clamp01(s.sfx+delta*.05f);break;case 2:s.music=Mathf.Clamp01(s.music+delta*.05f);break;
                case 3:s.keyboardSensitivity=Mathf.Clamp(s.keyboardSensitivity+delta*.1f,.3f,2);break;case 4:s.gamepadSensitivity=Mathf.Clamp(s.gamepadSensitivity+delta*.1f,.3f,2);break;case 5:s.cameraSensitivity=Mathf.Clamp(s.cameraSensitivity+delta*.1f,.3f,2);break;
                case 6:s.fov=Mathf.Clamp(s.fov+delta,60,85);break;case 7:s.quality=(s.quality+delta+4)%4;break;case 8:s.day=!s.day;break;case 9:s.outfit=(s.outfit+delta+6)%6;break;case 10:s.fullscreen=!s.fullscreen;break;
                case 11:
                    int[] widths={1920,1080,1280},heights={1080,1920,720};int resolution=System.Array.IndexOf(widths,s.width);resolution=(Mathf.Max(0,resolution)+delta+3)%3;s.width=widths[resolution];s.height=heights[resolution];break;
                case 12:s.mph=!s.mph;break;case 13:s.invertCamera=!s.invertCamera;break;
            }
        }
        void OnGUI()
        {
            if(title==null){title=new GUIStyle(GUI.skin.label){fontSize=42,fontStyle=FontStyle.Bold};body=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true};button=new GUIStyle(GUI.skin.button){fontSize=19};shade=new Texture2D(1,1);shade.SetPixel(0,0,new Color(.035f,.045f,.075f,.88f));shade.Apply();}
            if(!menu){if(ScoreSession)GUI.Label(new Rect(Screen.width*.5f-80,20,160,40),$"{Mathf.Max(0,Remaining):F0}s",body);return;}
            float width=Mathf.Min(560,Screen.width-36),x=(Screen.width-width)/2,y=Mathf.Max(20,(Screen.height-640)/2);GUI.DrawTexture(new Rect(x-14,y-14,width+28,Mathf.Min(650,Screen.height-30)),shade);GUI.Label(new Rect(x,y,width,60),startInMenu?"POWDERFLOW":"PAUSED",title);GUI.Label(new Rect(x,y+60,width,35),"Find your line. Keep the flow.",body);y+=110;
            if(page=="main")
            {
                var labels=startInMenu?new[]{"FREE RIDE","SCORE SESSION","SETTINGS / OUTFIT","CONTROLS","QUIT"}:new[]{"RESUME","RESTART AT TOP","RESTART AT PARK","SETTINGS / OUTFIT","RETURN TO MENU","QUIT"};
                for(int i=0;i<labels.Length;i++){GUI.backgroundColor=i==selected?new Color(.45f,.69f,.65f):Color.white;if(GUI.Button(new Rect(x,y+i*58,width,48),labels[i],button))Activate(i);}GUI.backgroundColor=Color.white;GUI.Label(new Rect(x,y+labels.Length*58+5,width,60),$"HIGH SCORE  {SaveStore.Current.highScore:N0}\nD-pad / arrows to navigate · A / Enter to select",body);
            }
            else if(page=="settings")
            {
                var s=SaveStore.Current;var labels=new[]{ $"Master volume  {s.master:P0}",$"SFX volume  {s.sfx:P0}",$"Music volume  {s.music:P0}",$"Keyboard sensitivity  {s.keyboardSensitivity:F1}",$"Gamepad sensitivity  {s.gamepadSensitivity:F1}",$"Camera sensitivity  {s.cameraSensitivity:F1}",$"FOV  {s.fov:F0}",$"Quality  {new[]{"Low","Medium","High","Epic"}[s.quality]}",$"Lighting  {(s.day?"Day":"Sunset")}",$"Outfit  {s.outfit+1}/6",$"Fullscreen  {s.fullscreen}",$"Resolution  {s.width}×{s.height}",s.mph?"Units  MPH":"Units  KM/H",s.invertCamera?"Invert camera  ON":"Invert camera  OFF","SAVE & BACK"};
                for(int i=0;i<labels.Length;i++){float row=y+i*30;GUI.Label(new Rect(x+40,row,width-80,30),(i==selected?"› ":"")+labels[i],body);if(i<14){if(GUI.Button(new Rect(x,row,32,27),"−"))Adjust(i,-1);if(GUI.Button(new Rect(x+width-32,row,32,27),"+"))Adjust(i,1);}else if(GUI.Button(new Rect(x,row,width,30),labels[i])){SaveStore.Save();ApplySettings();page="main";selected=0;}}
            }
            else if(page=="controls")
            {
                GUI.Label(new Rect(x,y,width,370),"GROUND: A/D carve, W tuck, S brake\nAIR: A/D spin, Up/Down flip, Left/Right roll\nSPACE: hold crouch, release pop\nQ/E: Safety/Mute; Shift: Tail/Nose\nCtrl: Stale/Method; both: Japan/Blunt\nQ+E: cross skis; Ctrl: off-axis/preload\nR: quick retry · T: marker · Y: retry marker\nL: lighting · F1: telemetry · F2: traces\nGamepad: sticks steer/rotate, A pop, Y tuck, X brake, LT/RT grabs, LB/RB modifiers, B reset, D-pad up/down marker. Start pauses.\nRight mouse held: look around",body);
                if(GUI.Button(new Rect(x,y+380,width,40),"BACK")){page="main";selected=0;}
            }
            else {GUI.Label(new Rect(x,y,width,140),$"SESSION COMPLETE\n{world.player.GetComponent<ComboSystem>().Total:N0} points\nBest: {SaveStore.Current.highScore:N0}",title);if(GUI.Button(new Rect(x,y+160,width,45),"CONTINUE")){ScoreSession=false;page="main";selected=0;}}
        }
        void OnDestroy(){Time.timeScale=1;}
    }
}
