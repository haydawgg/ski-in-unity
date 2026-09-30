using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Configure Alpine Graphics")]
        public static void ConfigureGraphics()
        {
            var config=Config<GraphicsConfig>("GraphicsConfig");
            Material Make(string name,string shader){string path="Assets/Art/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find(shader));AssetDatabase.CreateAsset(m,path);}return m;}
            config.snowMaterial=Make("AlpineSnow","PowderFlow/Alpine Snow");config.skyMaterial=Make("AlpineSky","PowderFlow/Alpine Sky");
            config.snowMaterial.enableInstancing=true;
            config.snowMaterial.SetColor("_BaseColor",config.snowTint);config.snowMaterial.SetColor("_RockColor",config.rockTint);
            config.snowMaterial.SetFloat("_RippleStrength",config.rippleStrength);config.snowMaterial.SetFloat("_RippleScale",config.rippleScale);config.snowMaterial.SetFloat("_Variation",config.snowVariation);config.snowMaterial.SetFloat("_Glitter",config.glitter);config.snowMaterial.SetFloat("_DetailDistance",config.detailDistance);config.snowMaterial.SetFloat("_Wrap",config.lightWrap);config.snowMaterial.SetFloat("_RimStrength",config.rimStrength);EditorUtility.SetDirty(config.snowMaterial);
            config.snowMaterial.SetFloat("_SlopeBlend",1);
            config.propSnowMaterial=Make("PackedSnow","PowderFlow/Alpine Snow");config.propSnowMaterial.CopyPropertiesFromMaterial(config.snowMaterial);config.propSnowMaterial.SetFloat("_SlopeBlend",0);EditorUtility.SetDirty(config.propSnowMaterial);
            ConfigureSnowGraphics(config);
            if(!config.postProfile){config.postProfile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(config.postProfile,"Assets/Settings/AlpinePost.asset");}
            var tone=PersistEffect<Tonemapping>(config.postProfile);tone.mode.Override(TonemappingMode.ACES);
            var bloom=PersistEffect<Bloom>(config.postProfile);bloom.threshold.Override(config.bloomThreshold);bloom.intensity.Override(config.bloomIntensity);bloom.scatter.Override(.65f);
            var vignette=PersistEffect<Vignette>(config.postProfile);vignette.intensity.Override(config.vignetteIntensity);vignette.smoothness.Override(.65f);
            var grading=PersistEffect<ColorAdjustments>(config.postProfile);grading.postExposure.Override(config.exposure);grading.contrast.Override(config.contrast);grading.saturation.Override(config.saturation);
            foreach(var effect in config.postProfile.components)EditorUtility.SetDirty(effect);EditorUtility.SetDirty(config.postProfile);
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PowderFlowRenderer.asset");bool has=false;foreach(var f in renderer.rendererFeatures)if(f is ScreenSpaceAmbientOcclusion)has=true;
            renderer.postProcessData=AssetDatabase.LoadAssetAtPath<PostProcessData>("Packages/com.unity.render-pipelines.universal/Runtime/Data/PostProcessData.asset");
            if(!renderer.postProcessData)throw new System.Exception("Missing URP post-process resources");
            if(!has){var ao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ao.name="Alpine SSAO";AssetDatabase.AddObjectToAsset(ao,renderer);renderer.rendererFeatures.Add(ao);var settings=new SerializedObject(ao);settings.FindProperty("m_Settings.Downsample").boolValue=true;settings.FindProperty("m_Settings.Intensity").floatValue=.6f;settings.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(renderer);renderer.SetDirty();}
            foreach(var feature in renderer.rendererFeatures)if(feature is ScreenSpaceAmbientOcclusion){var settings=new SerializedObject(feature);settings.FindProperty("m_Settings.Intensity").floatValue=config.ambientOcclusion;settings.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(feature);}
            EditorUtility.SetDirty(renderer);renderer.SetDirty();
            var pipeline=(UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
            var pipelineSettings=new SerializedObject(pipeline);pipelineSettings.FindProperty("m_SoftShadowsSupported").boolValue=true;pipelineSettings.ApplyModifiedPropertiesWithoutUndo();
            pipeline.supportsCameraDepthTexture=true;pipeline.colorGradingMode=ColorGradingMode.HighDynamicRange;pipeline.mainLightShadowmapResolution=config.shadowResolution;pipeline.shadowDepthBias=config.shadowDepthBias;pipeline.shadowNormalBias=config.shadowNormalBias;EditorUtility.SetDirty(pipeline);
            var graphics=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);graphics.FindProperty("m_FogStripping").intValue=1;graphics.FindProperty("m_FogKeepExp").boolValue=true;graphics.FindProperty("m_FogKeepExp2").boolValue=true;graphics.ApplyModifiedPropertiesWithoutUndo();
            var catalog=Config<AssetCatalog>("AssetCatalog");foreach(var a in catalog.assets)
            {
                var obj=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(a.prefab));
                var snow=a.type=="Mountain"||a.name=="ValleyFloor"?config.snowMaterial:config.propSnowMaterial;
                foreach(var r in obj.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i]&&(mats[i]==config.snowMaterial||mats[i]==config.propSnowMaterial||mats[i].name=="snow"||mats[i].name.StartsWith("snow.")))mats[i]=snow;r.sharedMaterials=mats;}PrefabUtility.SaveAsPrefabAsset(obj,AssetDatabase.GetAssetPath(a.prefab));PrefabUtility.UnloadPrefabContents(obj);
            }
            EditorUtility.SetDirty(config);var scene=EditorSceneManager.OpenScene("Assets/Scenes/Mountain.unity");var world=Object.FindFirstObjectByType<MountainWorld>();world.graphicsConfig=config;RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogDensity=config.fogDensity;RenderSettings.fogColor=config.sunsetFog;EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("ALPINE GRAPHICS CONFIGURED / persistent post + fog + soft shadows");
        }
        static T PersistEffect<T>(VolumeProfile profile) where T:VolumeComponent
        {
            if(!profile.TryGet(out T effect))effect=profile.Add<T>(true);
            if(!AssetDatabase.Contains(effect)){effect.name=typeof(T).Name;AssetDatabase.AddObjectToAsset(effect,profile);}
            effect.active=true;return effect;
        }
        [MenuItem("PowderFlow/Apply Visual Polish Foundation")]
        public static void ConfigureVisualPolish()
        {
            var c=Config<GraphicsConfig>("GraphicsConfig");
            c.sunsetTop=new Color(.16f,.20f,.36f);c.sunsetHorizon=new Color(.76f,.60f,.64f);c.sunsetAmbient=new Color(.37f,.43f,.58f);c.sunsetLight=new Color(1,.84f,.76f);
            c.dayTop=new Color(.035f,.21f,.49f);c.dayHorizon=new Color(.58f,.76f,.91f);c.dayAmbient=new Color(.44f,.52f,.64f);c.dayLight=new Color(1,.98f,.94f);
            c.sunIntensity=1.25f;c.daySunIntensity=1.7f;c.rippleStrength=.022f;c.rippleScale=1.8f;c.fogDensity=.0007f;EditorUtility.SetDirty(c);ConfigureGraphics();
        }
    }
}
