using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Create Mountain")]
        public static void CreateMountain()
        {
            ConfigureProject();var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var world=new GameObject("PowderFlow Mountain").AddComponent<MountainWorld>();
            world.catalog=Config<AssetCatalog>("AssetCatalog");world.physicsConfig=Config<SkiPhysicsConfig>("SkiPhysicsConfig");world.cameraConfig=Config<CameraConfig>("CameraConfig");world.trickConfig=Config<TrickConfig>("TrickConfig");world.worldConfig=Config<WorldConfig>("WorldConfig");
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/Mountain.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Mountain.unity",true),new EditorBuildSettingsScene("Assets/Scenes/PhysicsTest.unity",true)};AssetDatabase.SaveAssets();Debug.Log("M8 MOUNTAIN READY");
        }
    }
}
