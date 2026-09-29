using System.IO;
using NUnit.Framework;
namespace PowderFlow.Tests
{
    public class SaveTests
    {
        [Test]public void SettingsAndHighScoreRoundTripAndReplaceAtomically()
        {
            string dir=Path.Combine(Path.GetTempPath(),"powderflow-tests-"+System.Guid.NewGuid());string path=Path.Combine(dir,"save.json");
            var s=new SavedGame{master=.45f,day=true,mph=true,highScore=4840,outfit=4,width=1080,height=1920};SaveStore.Save(s,path);var r=SaveStore.Load(path);Assert.That(r.highScore,Is.EqualTo(4840));Assert.That(r.day,Is.True);Assert.That(r.width,Is.EqualTo(1080));Assert.That(r.master,Is.EqualTo(.45f));s.highScore=9000;SaveStore.Save(s,path);Assert.That(SaveStore.Load(path).highScore,Is.EqualTo(9000));Directory.Delete(dir,true);
        }
    }
}
