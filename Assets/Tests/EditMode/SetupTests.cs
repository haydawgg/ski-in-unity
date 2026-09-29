using NUnit.Framework;
using UnityEngine;
using UnityEngine.Rendering;

namespace PowderFlow.Tests
{
    public class SetupTests
    {
        [Test] public void LinearUrpAndHundredHertzAreConfigured()
        {
            Assert.That(QualitySettings.activeColorSpace, Is.EqualTo(ColorSpace.Linear));
            Assert.That(GraphicsSettings.defaultRenderPipeline, Is.Not.Null);
            Assert.That(Time.fixedDeltaTime, Is.EqualTo(.01f).Within(.0001f));
        }
    }
}
