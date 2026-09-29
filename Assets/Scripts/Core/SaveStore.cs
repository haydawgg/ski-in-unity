using System;
using System.IO;
using UnityEngine;
namespace PowderFlow
{
    [Serializable] public class SavedGame
    {
        public float master=.8f,sfx=.8f,music=.5f,keyboardSensitivity=1,gamepadSensitivity=1,cameraSensitivity=1,fov=73;
        public bool invertCamera,day,mph,fullscreen;public int quality=2,width=1920,height=1080,outfit,highScore;
    }
    public static class SaveStore
    {
        public static SavedGame Current=Load();
        public static string OverridePath;
        public static string PathName=>OverridePath??Path.Combine(Application.persistentDataPath,"powderflow.json");
        public static SavedGame Load(string path=null)
        {
            path=path??PathName;
            try{return File.Exists(path)?JsonUtility.FromJson<SavedGame>(File.ReadAllText(path))??new SavedGame():new SavedGame();}
            catch(Exception e){Debug.LogWarning("Save could not be read: "+e.Message);return new SavedGame();}
        }
        public static void Save(SavedGame state=null,string path=null)
        {
            path=path??PathName;Directory.CreateDirectory(Path.GetDirectoryName(path));string temp=path+".tmp";File.WriteAllText(temp,JsonUtility.ToJson(state??Current,true));if(File.Exists(path))File.Replace(temp,path,null);else File.Move(temp,path);
        }
    }
}
