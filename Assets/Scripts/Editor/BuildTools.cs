using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Prepare Game")]
        public static void PrepareGame()
        {
            var systems=Config<GameSystemConfig>("GameSystemConfig");systems.snow=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Snow.wav");systems.wind=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Wind.wav");systems.rail=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Rail.wav");systems.landing=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Landing.wav");systems.crash=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Crash.wav");systems.pop=AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Pop.wav");EditorUtility.SetDirty(systems);
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/Mountain.unity");var world=UnityEngine.Object.FindFirstObjectByType<MountainWorld>();var flow=world.gameObject.GetComponent<GameFlow>();if(!flow)flow=world.gameObject.AddComponent<GameFlow>();flow.config=systems;flow.startInMenu=false;world.systemConfig=systems;EditorSceneManager.SaveScene(scene);
            flow.startInMenu=true;EditorSceneManager.SaveScene(scene,"Assets/Scenes/MainMenu.unity",true);
            world.physicsConfig.safeSpeed=45;EditorUtility.SetDirty(world.physicsConfig);
            EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/MainMenu.unity",true),new EditorBuildSettingsScene("Assets/Scenes/Mountain.unity",true),new EditorBuildSettingsScene("Assets/Scenes/PhysicsTest.unity",true),new EditorBuildSettingsScene("Assets/Scenes/AssetPreview.unity",true)};
            AssetDatabase.SaveAssets();Debug.Log("M10 GAME PREPARED");
        }
        [MenuItem("PowderFlow/Build Linux Game")]
        public static void BuildGame()
        {
            PrepareGame();Directory.CreateDirectory("Builds/Linux");PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);PlayerSettings.usePlayerLog=true;
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/MainMenu.unity","Assets/Scenes/Mountain.unity","Assets/Scenes/PhysicsTest.unity","Assets/Scenes/AssetPreview.unity"},locationPathName="Builds/Linux/PowderFlow.x86_64",target=BuildTarget.StandaloneLinux64,options=BuildOptions.Development});
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);Debug.Log($"BUILD PASS {report.summary.totalSize} bytes");
        }
    }
}
