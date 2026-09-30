using NUnit.Framework;
using UnityEditor;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class SnowVisualTests
    {
        [Test]public void ContinuousEmissionSurvivesShortRenderStepsAndRejectsCatchupSpikes()
        {
            foreach(int steps in new[]{30,60,144})
            {
                float carry=0;int emitted=0;for(int i=0;i<steps*4;i++)emitted+=SnowEffectsController.Accumulate(ref carry,23,1f/steps,36);
                Assert.That(emitted,Is.InRange(91,92),"Same four seconds of spray must survive different render intervals");
            }
            float debt=0;Assert.That(SnowEffectsController.Accumulate(ref debt,500,3,12),Is.EqualTo(12));Assert.That(debt,Is.LessThanOrEqualTo(1));
        }
        [Test]public void SoftShapeMaterialsAndShadersSurviveReload()
        {
            var config=AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");
            foreach(var material in new[]{config.trackMaterial,config.particleMaterial,config.powderMaterial}){Assert.That(material,Is.Not.Null);Assert.That(ShaderUtil.ShaderHasError(material.shader),Is.False);}
            Assert.That(config.particleMaterial.shader.name,Is.EqualTo("PowderFlow/Snow Particle"));Assert.That(config.powderMaterial.shader.name,Is.EqualTo("PowderFlow/Snow Particle"));
            Assert.That(config.trackMaterial.GetFloat("_Lifetime"),Is.EqualTo(config.trackLifetime));Assert.That(config.trackMaterial.GetFloat("_Softness"),Is.GreaterThan(0));
            foreach(var material in new[]{config.particleMaterial,config.powderMaterial})
            {
                var texture=(Texture2D)material.mainTexture;Assert.That(texture,Is.Not.Null);Assert.That(texture.GetPixel(0,0).a,Is.LessThan(.02f));Assert.That(texture.GetPixel(texture.width/2,texture.height/2).a,Is.GreaterThan(.3f));
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture));Assert.That(importer.wrapMode,Is.EqualTo(TextureWrapMode.Clamp));Assert.That(importer.alphaIsTransparency,Is.True);
            }
        }
    }
}
