using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Configure Project")]
        public static void ConfigureProject()
        {
            Directory.CreateDirectory("Assets/Settings");
            PlayerSettings.productName = "PowderFlow";
            PlayerSettings.companyName = "PowderFlow Studio";
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.defaultScreenWidth = 1920;
            PlayerSettings.defaultScreenHeight = 1080;
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/PowderFlowURP.asset");
            if (!pipeline)
            {
                var renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, "Assets/Settings/PowderFlowRenderer.asset");
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.supportsHDR = true;
                pipeline.msaaSampleCount = 4;
                pipeline.shadowDistance = 150;
                pipeline.shadowCascadeCount = 4;
                AssetDatabase.CreateAsset(pipeline, "Assets/Settings/PowderFlowURP.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            Time.fixedDeltaTime = .01f;
            var timeSettings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TimeManager.asset")[0]);
            var fixedStep = timeSettings.FindProperty("Fixed Timestep");
            if (fixedStep.FindPropertyRelative("m_Count") != null)
                fixedStep.FindPropertyRelative("m_Count").longValue = 1411200;
            else fixedStep.floatValue = .01f;
            timeSettings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssets();
            Debug.Log("M1 SETUP PASS: Linear URP/HDR, Input System, 100 Hz physics.");
        }
    }
}
