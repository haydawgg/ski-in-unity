using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
namespace PowderFlow
{
    [DefaultExecutionOrder(500)]
    public partial class GameFlow:MonoBehaviour
    {
        public GameSystemConfig config;public bool startInMenu;
        public static bool ScoreSession;public bool menu;public float Remaining {get;private set;}
        string page="main";int selected;MountainWorld world;SkiCameraController follow;bool pointerMode;float nextStick,cameraSpeedFovSpan;
        void Awake()
        {
            var args=System.Environment.GetCommandLineArgs();
            if(System.Array.Exists(args,a=>a=="--smoke-test"||a=="--menu-capture"||a=="--presentation-review"))Application.runInBackground=true;
        }
        void Start()
        {
            world=GetComponent<MountainWorld>();follow=world.GetComponentInChildren<SkiCameraController>();Remaining=config.sessionDuration;menu=startInMenu;Time.timeScale=menu?0:1;
            world.player.ResetTo(world.player.startPosition,world.player.startRotation);
            var args=System.Environment.GetCommandLineArgs();bool diagnostic=System.Array.Exists(args,a=>a=="--smoke-test");
            if(diagnostic||System.Array.Exists(args,a=>a=="--visual-day"))SaveStore.Current.day=System.Array.Exists(args,a=>a=="--visual-day");
            cameraSpeedFovSpan=world.cameraConfig.speedFov-world.cameraConfig.normalFov;
            ApplySettings();
            if(System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a=="--smoke-test")){menu=false;Time.timeScale=1;gameObject.AddComponent<StandaloneSmokeTest>().flow=this;}
            else if(System.Array.Exists(System.Environment.GetCommandLineArgs(),a=>a=="--menu-capture"))gameObject.AddComponent<StandaloneSmokeTest>().flow=this;
            else if(System.Array.Exists(args,a=>a=="--presentation-review"))gameObject.AddComponent<PresentationReview>().flow=this;
        }
        public void ApplySettings()
        {
            var saved=SaveStore.Current;AudioListener.volume=saved.master;
            world.GetComponent<AlpineLighting>()?.Apply(saved.day);world.player.GetComponent<OutfitSystem>().Apply(saved.outfit);
            var camera=world.cameraConfig;camera.normalFov=saved.fov;camera.speedFov=saved.fov+cameraSpeedFovSpan;camera.cameraSensitivity=saved.cameraSensitivity;
            var pipeline=(UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline;
            if(pipeline){pipeline.renderScale=saved.quality==0?.7f:saved.quality==1?.85f:1;pipeline.shadowDistance=saved.quality==0?50:saved.quality==1?85:world.graphicsConfig.shadowDistance;pipeline.shadowCascadeCount=saved.quality<2?2:4;}
            if(!Application.isEditor)Screen.SetResolution(saved.width,saved.height,saved.fullscreen?FullScreenMode.FullScreenWindow:FullScreenMode.Windowed);
            Cursor.visible=menu;follow?.SetMenu(menu,startInMenu);
        }
        public string Page=>page;public int Selected=>selected;
        public void ShowPage(string value){page=value;selected=0;menu=true;Time.timeScale=0;Cursor.visible=true;follow?.SetMenu(true,startInMenu);}
        void Back()
        {
            if(page=="results")ScoreSession=false;
            if(page=="settings"){SaveStore.Save();ApplySettings();}
            if(page!="main"){page=page=="outfit"?"settings":"main";selected=page=="settings"?9:0;return;}
            if(!startInMenu){menu=false;Time.timeScale=1;Cursor.visible=false;}
        }
        void Update()
        {
            var k=Keyboard.current;var p=Gamepad.current;
            if(world.player.Input.pause){if(menu)Back();else ShowPage("main");world.player.Input.pause=false;}
            if(!menu&&ScoreSession){Remaining-=Time.deltaTime;if(Remaining<=0){SaveStore.Current.highScore=Mathf.Max(SaveStore.Current.highScore,world.player.GetComponent<ComboSystem>().Total);SaveStore.Save();ShowPage("results");}}
            follow?.SetMenu(menu,startInMenu);if(!menu)return;
            if((p?.buttonEast.wasPressedThisFrame??false)){Back();return;}
            bool up=(k?.upArrowKey.wasPressedThisFrame??false)||(p?.dpad.up.wasPressedThisFrame??false),down=(k?.downArrowKey.wasPressedThisFrame??false)||(p?.dpad.down.wasPressedThisFrame??false);
            float stick=p?.leftStick.y.ReadValue()??0;if(Mathf.Abs(stick)>.65f&&Time.unscaledTime>=nextStick){up|=stick>0;down|=stick<0;nextStick=Time.unscaledTime+.22f;}if(Mathf.Abs(stick)<.3f)nextStick=0;
            int count=page=="main"?(startInMenu?5:6):page=="settings"?15:1;
            if(up)selected=(selected+count-1)%count;if(down)selected=(selected+1)%count;
            bool enter=(k?.enterKey.wasPressedThisFrame??false)||(p?.buttonSouth.wasPressedThisFrame??false);
            int delta=((k?.rightArrowKey.wasPressedThisFrame??false)||(p?.dpad.right.wasPressedThisFrame??false)?1:0)-((k?.leftArrowKey.wasPressedThisFrame??false)||(p?.dpad.left.wasPressedThisFrame??false)?1:0);
            if(up||down||enter||delta!=0)pointerMode=false;else if((Mouse.current?.delta.ReadValue().sqrMagnitude??0)>1)pointerMode=true;
            if(enter&&page=="main")Activate(selected);else if(enter&&(page=="controls"||page=="results"||page=="outfit")){if(page=="results")ScoreSession=false;Back();}
            else if(page=="settings")
            {
                if(delta!=0)Adjust(selected,delta);if(enter&&selected==14)Back();
                if((k?.oKey.wasPressedThisFrame??false)||(p?.buttonNorth.wasPressedThisFrame??false)){page="outfit";selected=0;}
            }
            else if(page=="outfit"&&delta!=0)Adjust(9,delta);
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
        public void Adjust(int item,int delta)
        {
            var s=SaveStore.Current;
            switch(item)
            {
                case 0:s.master=Mathf.Clamp01(s.master+delta*.05f);break;case 1:s.sfx=Mathf.Clamp01(s.sfx+delta*.05f);break;case 2:s.music=Mathf.Clamp01(s.music+delta*.05f);break;
                case 3:s.keyboardSensitivity=Mathf.Clamp(s.keyboardSensitivity+delta*.1f,.3f,2);break;case 4:s.gamepadSensitivity=Mathf.Clamp(s.gamepadSensitivity+delta*.1f,.3f,2);break;case 5:s.cameraSensitivity=Mathf.Clamp(s.cameraSensitivity+delta*.1f,.3f,2);break;
                case 6:s.fov=Mathf.Clamp(s.fov+delta,60,85);break;case 7:s.quality=(s.quality+delta+4)%4;break;case 8:s.day=!s.day;break;case 9:s.outfit=(s.outfit+delta+6)%6;world.player.GetComponent<OutfitSystem>().Apply(s.outfit);break;case 10:s.fullscreen=!s.fullscreen;break;
                case 11:
                    int[] widths={1920,1080,1280},heights={1080,1920,720};int resolution=System.Array.IndexOf(widths,s.width);resolution=(Mathf.Max(0,resolution)+delta+3)%3;s.width=widths[resolution];s.height=heights[resolution];break;
                case 12:s.mph=!s.mph;break;case 13:s.invertCamera=!s.invertCamera;break;
            }
        }
        void OnDestroy(){Time.timeScale=1;}
    }
}
