using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        public static T Config<T>(string name) where T:ScriptableObject
        {
            var path=$"Assets/Settings/{name}.asset";var asset=AssetDatabase.LoadAssetAtPath<T>(path);
            if(!asset){asset=ScriptableObject.CreateInstance<T>();AssetDatabase.CreateAsset(asset,path);}return asset;
        }
        [MenuItem("PowderFlow/Create Physics Test Scene")]
        public static void CreatePhysicsScene()
        {
            ConfigureProject();Directory.CreateDirectory("Assets/Scenes");
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            var world=new GameObject("Physics test world").AddComponent<PhysicsTestWorld>();
            world.physicsConfig=Config<SkiPhysicsConfig>("SkiPhysicsConfig");world.cameraConfig=Config<CameraConfig>("CameraConfig");
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/PhysicsTest.unity");
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/PhysicsTest.unity",true)};
            AssetDatabase.SaveAssets();Debug.Log("M2 PHYSICS SCENE READY");
        }
    }
}
