using NUnit.Framework;
using UnityEngine;
namespace PowderFlow.Tests
{
    public class PresentationTests
    {
        [Test]public void SettingsAndFeedbackStayInsideSupportedSafeAreas()
        {
            foreach(var size in new[]{new Vector2(1280,720),new Vector2(1920,1080)})
            foreach(bool inset in new[]{false,true})
            {
                var safe=inset?new Rect(24,40,size.x-48,size.y-80):new Rect(0,0,size.x,size.y);var canvas=new PresentationCanvas(size.x,size.y,safe);var config=PresentationConfig.Active;
                var rows=canvas.SettingsRows(config.margin,config.settingRow);
                foreach(var r in rows){Assert.That(r.xMin,Is.GreaterThanOrEqualTo(config.margin));Assert.That(r.xMax,Is.LessThanOrEqualTo(canvas.width-config.margin+.1f));Assert.That(r.yMax,Is.LessThan(canvas.height-config.margin-82));}
                for(int i=0;i<rows.Length;i++)for(int j=i+1;j<rows.Length;j++)Assert.That(rows[i].Overlaps(rows[j]),Is.False,"Setting controls must not overlap");
                foreach(bool session in new[]{false,true}){var feedback=canvas.FeedbackRect(session,config.margin);Assert.That(feedback.xMin,Is.GreaterThanOrEqualTo(canvas.width*.5f));Assert.That(feedback.yMax,Is.LessThan(canvas.height*.25f));Assert.That(feedback.xMax,Is.LessThanOrEqualTo(canvas.width-config.margin+.1f));}
                Assert.That(canvas.origin.x+canvas.width*canvas.scale,Is.EqualTo(safe.xMax).Within(.1f));
            }
        }
        [Test]public void ThemeHasFontsAndFeedbackFadesWithDistinctLandingTiers()
        {
            var c=PresentationConfig.Active;Assert.That(c.regular,Is.Not.Null);Assert.That(c.bold,Is.Not.Null);
            Assert.That(c.FeedbackAlpha(4),Is.EqualTo(1));Assert.That(c.FeedbackAlpha(0),Is.Zero);Assert.That(c.FeedbackAlpha(2.4f),Is.LessThan(c.FeedbackAlpha(3.5f)));
            Assert.That(c.LandingColor(LandingQuality.Perfect),Is.Not.EqualTo(c.LandingColor(LandingQuality.Bail)));
        }
    }
}
