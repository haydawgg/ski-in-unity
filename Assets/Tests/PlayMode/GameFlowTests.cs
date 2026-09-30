using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class GameFlowTests:PlayWorldTestBase
    {
#if UNITY_EDITOR
        readonly List<InputDevice> suspended=new List<InputDevice>();
        readonly List<CameraConfig> cameraConfigs=new List<CameraConfig>();
        readonly List<Gamepad> testPads=new List<Gamepad>();
        CameraConfig sourceCamera;float priorNormalFov,priorSpeedFov,priorSensitivity;
        Keyboard testKeyboard;InputSettings.UpdateMode previousUpdateMode;InputSettings.BackgroundBehavior previousBackground;InputSettings.EditorInputBehaviorInPlayMode previousEditorInput;
        [SetUp]public void IsolateVirtualInput()
        {
            sourceCamera=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");priorNormalFov=sourceCamera.normalFov;priorSpeedFov=sourceCamera.speedFov;priorSensitivity=sourceCamera.cameraSensitivity;
            previousUpdateMode=InputSystem.settings.updateMode;
            previousBackground=InputSystem.settings.backgroundBehavior;previousEditorInput=InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.backgroundBehavior=InputSettings.BackgroundBehavior.IgnoreFocus;
            InputSystem.settings.editorInputBehaviorInPlayMode=InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            InputSystem.settings.updateMode=InputSettings.UpdateMode.ProcessEventsManually;
            foreach(var device in InputSystem.devices)if(device.enabled){suspended.Add(device);InputSystem.DisableDevice(device);}
            testKeyboard=InputSystem.AddDevice<Keyboard>();testKeyboard.MakeCurrent();
        }
        [TearDown]public void RestoreInputDevices()
        {
            sourceCamera.normalFov=priorNormalFov;sourceCamera.speedFov=priorSpeedFov;sourceCamera.cameraSensitivity=priorSensitivity;
            foreach(var camera in cameraConfigs)Object.Destroy(camera);cameraConfigs.Clear();
            foreach(var pad in testPads)if(pad.added)InputSystem.RemoveDevice(pad);testPads.Clear();
            if(testKeyboard!=null&&testKeyboard.added)InputSystem.RemoveDevice(testKeyboard);
            foreach(var device in suspended)if(device.added)InputSystem.EnableDevice(device);suspended.Clear();
            InputSystem.settings.updateMode=previousUpdateMode;
            InputSystem.settings.backgroundBehavior=previousBackground;InputSystem.settings.editorInputBehaviorInPlayMode=previousEditorInput;
        }
        Gamepad CreatePad(){var pad=InputSystem.AddDevice<Gamepad>();testPads.Add(pad);pad.MakeCurrent();return pad;}
        GameFlow Create(bool menu,float duration=150)
        {
            var obj=new GameObject();obj.SetActive(false);var world=obj.AddComponent<MountainWorld>();world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.cameraConfig=Object.Instantiate(AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset"));cameraConfigs.Add(world.cameraConfig);world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");world.graphicsConfig=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");
            var flow=obj.AddComponent<GameFlow>();flow.startInMenu=menu;flow.config=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameSystemConfig>("Assets/Settings/GameSystemConfig.asset"));flow.config.sessionDuration=duration;obj.SetActive(true);return flow;
        }
        [UnityTest]public IEnumerator GamepadCanLaunchFreeRideFromMainMenu()
        {
            var flow=Create(true);var pad=CreatePad();yield return null;Assert.That(flow.menu,Is.True);
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();yield return null;yield return null;
            InputSystem.RemoveDevice(pad);Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Mountain"));Assert.That(Object.FindFirstObjectByType<GameFlow>().menu,Is.False);
        }
        [UnityTest]public IEnumerator SessionEndsAndPreservesSavedHighScore()
        {
            var prior=SaveStore.Current;string folder=Path.Combine(Path.GetTempPath(),"powderflow-session-"+System.Guid.NewGuid());SaveStore.OverridePath=Path.Combine(folder,"save.json");SaveStore.Current=new SavedGame{highScore=1234};GameFlow.ScoreSession=true;
            var flow=Create(false,.5f);yield return new WaitForSecondsRealtime(1);
            Assert.That(flow.menu,Is.True);Assert.That(flow.Remaining,Is.LessThanOrEqualTo(0));Assert.That(SaveStore.Load().highScore,Is.EqualTo(1234));Assert.That(File.Exists(SaveStore.OverridePath),Is.True);
            var pad=CreatePad();yield return Press(pad,GamepadButton.East);Assert.That(flow.Page,Is.EqualTo("main"));Assert.That(GameFlow.ScoreSession,Is.False);
            yield return Press(pad,GamepadButton.East);yield return null;Assert.That(flow.menu,Is.False,"Back from completed results must allow untimed riding without reopening results");
            SaveStore.OverridePath=null;SaveStore.Current=prior;GameFlow.ScoreSession=false;Directory.Delete(folder,true);Time.timeScale=1;
        }
        IEnumerator Press(Gamepad pad,GamepadButton button)
        {
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button));InputSystem.Update();yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();yield return null;
        }
        [UnityTest]public IEnumerator RightStickControlsRailBalanceWithoutChangingSteering()
        {
            var obj=new GameObject("Input fixture");var input=obj.AddComponent<SkierInput>();var pad=CreatePad();
            try
            {
                InputSystem.QueueStateEvent(pad,new GamepadState{rightStick=new Vector2(.8f,0)});InputSystem.Update();yield return null;
                Debug.Log($"VIRTUAL INPUT focused={Application.isFocused} enabled={pad.enabled} current={Gamepad.current==pad} value={pad.rightStick.ReadValue()} injected={input.injected} roll={input.roll}");
                Assert.That(input.roll,Is.GreaterThan(.5f));Assert.That(input.steer,Is.EqualTo(0).Within(.01f));
            }
            finally{InputSystem.RemoveDevice(pad);Object.Destroy(obj);}
        }
        [UnityTest]public IEnumerator GamepadSettingsCanSaveDesktopResolution()
        {
            var prior=SaveStore.Current;string folder=Path.Combine(Path.GetTempPath(),"powderflow-settings-"+System.Guid.NewGuid());SaveStore.OverridePath=Path.Combine(folder,"save.json");SaveStore.Current=new SavedGame{width=1280,height=720};var pad=CreatePad();
            try
            {
                var flow=Create(true);yield return null;
                var camera=flow.GetComponent<MountainWorld>().cameraConfig;float span=camera.speedFov-camera.normalFov;SaveStore.Current.fov=79;flow.ApplySettings();flow.ApplySettings();
                Assert.That(camera.normalFov,Is.EqualTo(79));Assert.That(camera.speedFov-camera.normalFov,Is.EqualTo(span).Within(.001f));
                yield return Press(pad,GamepadButton.DpadDown);yield return Press(pad,GamepadButton.DpadDown);yield return Press(pad,GamepadButton.South);
                for(int i=0;i<11;i++)yield return Press(pad,GamepadButton.DpadDown);
                yield return Press(pad,GamepadButton.DpadRight);Assert.That(SaveStore.Current.width,Is.EqualTo(1920));Assert.That(SaveStore.Current.height,Is.EqualTo(1080));
                for(int i=0;i<3;i++)yield return Press(pad,GamepadButton.DpadDown);yield return Press(pad,GamepadButton.South);
                Assert.That(File.Exists(SaveStore.OverridePath),Is.True);Assert.That(SaveStore.Load().width,Is.EqualTo(1920));Assert.That(SaveStore.Load().height,Is.EqualTo(1080));
            }
            finally {InputSystem.RemoveDevice(pad);SaveStore.OverridePath=null;SaveStore.Current=prior;if(Directory.Exists(folder))Directory.Delete(folder,true);Time.timeScale=1;}
        }
        IEnumerator PressKey(Key key)
        {
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState(key));InputSystem.Update();yield return null;
            InputSystem.QueueStateEvent(testKeyboard,new KeyboardState());InputSystem.Update();yield return null;
        }
        [UnityTest]public IEnumerator KeyboardOutfitPreviewBackAndSaveRemainUsable()
        {
            var prior=SaveStore.Current;var previousPath=SaveStore.OverridePath;string folder=Path.Combine(Path.GetTempPath(),"powderflow-outfit-"+System.Guid.NewGuid());SaveStore.OverridePath=Path.Combine(folder,"save.json");SaveStore.Current=new SavedGame();
            try
            {
                var flow=Create(true);yield return null;yield return PressKey(Key.DownArrow);yield return PressKey(Key.DownArrow);yield return PressKey(Key.Enter);Assert.That(flow.Page,Is.EqualTo("settings"));
                yield return PressKey(Key.O);Assert.That(flow.Page,Is.EqualTo("outfit"));yield return PressKey(Key.RightArrow);Assert.That(SaveStore.Current.outfit,Is.EqualTo(1));
                yield return PressKey(Key.Escape);Assert.That(flow.Page,Is.EqualTo("settings"));Assert.That(flow.Selected,Is.EqualTo(9));
                for(int i=0;i<5;i++)yield return PressKey(Key.DownArrow);yield return PressKey(Key.Enter);Assert.That(flow.Page,Is.EqualTo("main"));Assert.That(SaveStore.Load().outfit,Is.EqualTo(1));
                yield return PressKey(Key.Escape);Assert.That(flow.menu,Is.True,"Back must not unpause the title scene");
            }
            finally{SaveStore.Current=prior;SaveStore.OverridePath=previousPath;if(Directory.Exists(folder))Directory.Delete(folder,true);Time.timeScale=1;}
        }
#endif
    }
}
