using UnityEditor;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Configure Environment Visuals")]
        public static void ConfigureEnvironmentGraphics()
        {
            void Surface(string name,Color color)
            {
                var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/"+name+".mat");if(!m)return;
                m.color=color;m.SetFloat("_Smoothness",.08f);m.enableInstancing=true;EditorUtility.SetDirty(m);
            }
            Surface("pine_dark",new Color(.075f,.16f,.145f));Surface("bark",new Color(.16f,.115f,.085f));
            Surface("rock",new Color(.19f,.22f,.28f));Surface("distant_rock",new Color(.22f,.25f,.32f));
            void Hardware(string name,Color color,float smoothness,float metallic=0)
            {
                var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/"+name+".mat");if(!m)return;
                m.shader=Shader.Find("PowderFlow/Alpine Prop");m.SetColor("_FillColor",new Color(.65f,.68f,.74f));m.SetColor("_EmissionColor",Color.black);
                m.color=color;m.SetFloat("_Smoothness",smoothness);m.SetFloat("_Metallic",metallic);m.enableInstancing=true;EditorUtility.SetDirty(m);
            }
            Hardware("steel",new Color(.035f,.055f,.075f),.30f,.7f);Hardware("galvanized",new Color(.43f,.52f,.58f),.40f,.8f);
            Hardware("park_trim",new Color(.16f,.38f,.40f),.22f);Hardware("park_accent",new Color(.92f,.34f,.18f),.16f);
            Hardware("box_top",new Color(.22f,.42f,.44f),.35f,.15f);Hardware("sign_ink",new Color(.035f,.085f,.11f),.10f);
            Hardware("sign_letter",new Color(.93f,.91f,.80f),.10f);Hardware("wood",new Color(.46f,.31f,.21f),.10f);
            Hardware("roof",new Color(.13f,.22f,.25f),.18f);Hardware("window",new Color(.11f,.26f,.32f),.55f,.2f);
            Hardware("easy_marker",new Color(.16f,.53f,.34f),.12f);Hardware("medium_marker",new Color(.10f,.37f,.72f),.16f);
            Hardware("lamp",new Color(.98f,.87f,.65f),.28f);var lamp=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/lamp.mat");if(lamp){lamp.SetColor("_EmissionColor",new Color(.15f,.10f,.04f));EditorUtility.SetDirty(lamp);}
            var catalog=Config<AssetCatalog>("AssetCatalog");
            foreach(var asset in catalog.assets)if(asset.type=="EnvironmentDistant")
            {
                var path=AssetDatabase.GetAssetPath(asset.prefab);var obj=PrefabUtility.LoadPrefabContents(path);
                foreach(var renderer in obj.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                PrefabUtility.SaveAsPrefabAsset(obj,path);PrefabUtility.UnloadPrefabContents(obj);
            }
            AssetDatabase.SaveAssets();Debug.Log("ENVIRONMENT MATERIALS CONFIGURED");
        }
    }
}
