using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PowderFlow.Tests
{
    public class GraphicsFoundationTests
    {
        [Test]
        public void PostEffectsArePersistentSubassetsAndRendererHasResources()
        {
            var config=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");
            var profile=config.postProfile;
            Assert.That(profile.TryGet(out Tonemapping tone),Is.True);
            Assert.That(tone.mode.value,Is.EqualTo(TonemappingMode.ACES));
            Assert.That(profile.TryGet(out Bloom bloom),Is.True);
            Assert.That(bloom.intensity.value,Is.EqualTo(config.bloomIntensity));
            Assert.That(profile.TryGet(out Vignette vignette),Is.True);
            Assert.That(profile.TryGet(out ColorAdjustments grading),Is.True);
            foreach(var effect in profile.components)
            {
                Assert.That(EditorUtility.IsPersistent(effect),Is.True,effect.name+" must survive editor reload");
                Assert.That(AssetDatabase.GetAssetPath(effect),Is.EqualTo(AssetDatabase.GetAssetPath(profile)),effect.name);
                Assert.That(AssetDatabase.TryGetGUIDAndLocalFileIdentifier(effect,out string guid,out long localId),Is.True,effect.name);
                Assert.That(localId,Is.Not.EqualTo(0));
            }
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PowderFlowRenderer.asset");
            Assert.That(renderer.postProcessData,Is.Not.Null);
        }

        [Test]
        public void SoftShadowsDepthAndRuntimeFogAreRetained()
        {
            var pipeline=(UniversalRenderPipelineAsset)GraphicsSettings.defaultRenderPipeline;
            Assert.That(pipeline.supportsSoftShadows,Is.True);
            Assert.That(pipeline.supportsCameraDepthTexture,Is.True);
            Assert.That(pipeline.colorGradingMode,Is.EqualTo(ColorGradingMode.HighDynamicRange));
            var settings=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/GraphicsSettings.asset")[0]);
            Assert.That(settings.FindProperty("m_FogStripping").intValue,Is.EqualTo(1));
            Assert.That(settings.FindProperty("m_FogKeepExp").boolValue,Is.True);
            var catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");
            for(int i=0;i<10;i++)
                Assert.That(catalog.Find($"Mountain{i:00}").GetComponentInChildren<Renderer>().sharedMaterial.shader.name,Is.EqualTo("PowderFlow/Alpine Snow"));
            foreach(var renderer in catalog.Find("Pine0").GetComponentsInChildren<Renderer>())
                if(renderer.sharedMaterial.shader.name=="PowderFlow/Alpine Snow")Assert.That(renderer.sharedMaterial.GetFloat("_SlopeBlend"),Is.EqualTo(0),"Tree snow caps must not turn into rock");
            foreach(string path in new[]{"Assets/Art/Shaders/AlpineSnow.shader","Assets/Art/Shaders/AlpineSky.shader"})
                Assert.That(ShaderUtil.ShaderHasError(AssetDatabase.LoadAssetAtPath<Shader>(path)),Is.False,path);
        }
    }
}
