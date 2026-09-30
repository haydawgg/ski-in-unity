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
            var catalog=Config<AssetCatalog>("AssetCatalog");
            foreach(var asset in catalog.assets)if(asset.type=="EnvironmentDistant")
            {
                var path=AssetDatabase.GetAssetPath(asset.prefab);var obj=PrefabUtility.LoadPrefabContents(path);
                foreach(var renderer in obj.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                PrefabUtility.SaveAsPrefabAsset(obj,path);PrefabUtility.UnloadPrefabContents(obj);
            }
            AssetDatabase.SaveAssets();Debug.Log("VP3 ENVIRONMENT MATERIALS CONFIGURED");
        }
    }
}
