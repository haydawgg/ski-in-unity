using System.Collections;
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
        GameFlow Create(bool menu,float duration=150)
        {
            var obj=new GameObject();obj.SetActive(false);var world=obj.AddComponent<MountainWorld>();world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");world.graphicsConfig=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");
            var flow=obj.AddComponent<GameFlow>();flow.startInMenu=menu;flow.config=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameSystemConfig>("Assets/Settings/GameSystemConfig.asset"));flow.config.sessionDuration=duration;obj.SetActive(true);return flow;
        }
        [UnityTest]public IEnumerator GamepadCanLaunchFreeRideFromMainMenu()
        {
            var flow=Create(true);var pad=InputSystem.AddDevice<Gamepad>();yield return null;Assert.That(flow.menu,Is.True);
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(GamepadButton.South));InputSystem.Update();yield return null;yield return null;
            InputSystem.RemoveDevice(pad);Assert.That(SceneManager.GetActiveScene().name,Is.EqualTo("Mountain"));Assert.That(Object.FindFirstObjectByType<GameFlow>().menu,Is.False);
        }
        [UnityTest]public IEnumerator SessionEndsAndPreservesSavedHighScore()
        {
            var prior=SaveStore.Current;string folder=Path.Combine(Path.GetTempPath(),"powderflow-session-"+System.Guid.NewGuid());SaveStore.OverridePath=Path.Combine(folder,"save.json");SaveStore.Current=new SavedGame{highScore=1234};GameFlow.ScoreSession=true;
            var flow=Create(false,.5f);yield return new WaitForSecondsRealtime(1);
            Assert.That(flow.menu,Is.True);Assert.That(flow.Remaining,Is.LessThanOrEqualTo(0));Assert.That(SaveStore.Load().highScore,Is.EqualTo(1234));Assert.That(File.Exists(SaveStore.OverridePath),Is.True);
            SaveStore.OverridePath=null;SaveStore.Current=prior;GameFlow.ScoreSession=false;Directory.Delete(folder,true);Time.timeScale=1;
        }
        IEnumerator Press(Gamepad pad,GamepadButton button)
        {
            InputSystem.QueueStateEvent(pad,new GamepadState().WithButton(button));InputSystem.Update();yield return null;
            InputSystem.QueueStateEvent(pad,new GamepadState());InputSystem.Update();yield return null;
        }
        [UnityTest]public IEnumerator RightStickControlsRailBalanceWithoutChangingSteering()
        {
            var obj=new GameObject("Input fixture");var input=obj.AddComponent<SkierInput>();var pad=InputSystem.AddDevice<Gamepad>();
            try
            {
                InputSystem.QueueStateEvent(pad,new GamepadState{rightStick=new Vector2(.8f,0)});InputSystem.Update();yield return null;
                Assert.That(input.roll,Is.GreaterThan(.5f));Assert.That(input.steer,Is.EqualTo(0).Within(.01f));
            }
            finally{InputSystem.RemoveDevice(pad);Object.Destroy(obj);}
        }
        [UnityTest]public IEnumerator GamepadSettingsCanSavePortraitResolution()
        {
            var prior=SaveStore.Current;string folder=Path.Combine(Path.GetTempPath(),"powderflow-settings-"+System.Guid.NewGuid());SaveStore.OverridePath=Path.Combine(folder,"save.json");SaveStore.Current=new SavedGame();var pad=InputSystem.AddDevice<Gamepad>();
            try
            {
                Create(true);yield return null;yield return Press(pad,GamepadButton.DpadDown);yield return Press(pad,GamepadButton.DpadDown);yield return Press(pad,GamepadButton.South);
                for(int i=0;i<11;i++)yield return Press(pad,GamepadButton.DpadDown);
                yield return Press(pad,GamepadButton.DpadRight);Assert.That(SaveStore.Current.width,Is.EqualTo(1080));Assert.That(SaveStore.Current.height,Is.EqualTo(1920));
                for(int i=0;i<3;i++)yield return Press(pad,GamepadButton.DpadDown);yield return Press(pad,GamepadButton.South);
                Assert.That(File.Exists(SaveStore.OverridePath),Is.True);Assert.That(SaveStore.Load().width,Is.EqualTo(1080));
            }
            finally {InputSystem.RemoveDevice(pad);SaveStore.OverridePath=null;SaveStore.Current=prior;if(Directory.Exists(folder))Directory.Delete(folder,true);Time.timeScale=1;}
        }
#endif
    }
}
