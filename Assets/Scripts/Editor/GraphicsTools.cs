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
            config.trackMaterial=Make("SkiGrooves","PowderFlow/Ski Grooves");config.trackMaterial.shader=Shader.Find("PowderFlow/Ski Grooves");config.trackMaterial.SetColor("_BaseColor",new Color(.24f,.28f,.4f,.35f));
            config.particleMaterial=Make("SnowFlakes","Universal Render Pipeline/Particles/Unlit");config.particleMaterial.SetColor("_BaseColor",Color.white);config.particleMaterial.SetFloat("_Surface",1);config.particleMaterial.SetFloat("_SrcBlend",5);config.particleMaterial.SetFloat("_DstBlend",10);config.particleMaterial.SetFloat("_ZWrite",0);config.particleMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");config.particleMaterial.renderQueue=3000;
            Directory.CreateDirectory("Assets/Art/Textures");var texture=new Texture2D(32,32,TextureFormat.RGBA32,false);for(int y=0;y<32;y++)for(int x=0;x<32;x++){float d=Vector2.Distance(new Vector2(x,y),new Vector2(15.5f,15.5f))/15.5f;texture.SetPixel(x,y,new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-d),1.3f)));}texture.Apply();File.WriteAllBytes("Assets/Art/Textures/SnowFlake.png",texture.EncodeToPNG());AssetDatabase.ImportAsset("Assets/Art/Textures/SnowFlake.png");config.particleMaterial.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Art/Textures/SnowFlake.png"));Object.DestroyImmediate(texture);
            if(!config.postProfile){var profile=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(profile,"Assets/Settings/AlpinePost.asset");var tone=profile.Add<Tonemapping>(true);tone.mode.Override(TonemappingMode.ACES);var bloom=profile.Add<Bloom>(true);bloom.threshold.Override(1);bloom.intensity.Override(.55f);var vignette=profile.Add<Vignette>(true);vignette.intensity.Override(.12f);config.postProfile=profile;}
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/PowderFlowRenderer.asset");bool has=false;foreach(var f in renderer.rendererFeatures)if(f is ScreenSpaceAmbientOcclusion)has=true;
            if(!has){var ao=ScriptableObject.CreateInstance<ScreenSpaceAmbientOcclusion>();ao.name="Alpine SSAO";AssetDatabase.AddObjectToAsset(ao,renderer);renderer.rendererFeatures.Add(ao);var settings=new SerializedObject(ao);settings.FindProperty("m_Settings.Downsample").boolValue=true;settings.FindProperty("m_Settings.Intensity").floatValue=.6f;settings.ApplyModifiedPropertiesWithoutUndo();EditorUtility.SetDirty(renderer);renderer.SetDirty();}
            var catalog=Config<AssetCatalog>("AssetCatalog");foreach(var a in catalog.assets)
            {
                var obj=PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(a.prefab));foreach(var r in obj.GetComponentsInChildren<Renderer>()){var mats=r.sharedMaterials;for(int i=0;i<mats.Length;i++)if(mats[i]&&mats[i].name=="snow")mats[i]=config.snowMaterial;r.sharedMaterials=mats;}PrefabUtility.SaveAsPrefabAsset(obj,AssetDatabase.GetAssetPath(a.prefab));PrefabUtility.UnloadPrefabContents(obj);
            }
            EditorUtility.SetDirty(config);var scene=EditorSceneManager.OpenScene("Assets/Scenes/Mountain.unity");var world=Object.FindFirstObjectByType<MountainWorld>();world.graphicsConfig=config;EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();Debug.Log("M9 GRAPHICS CONFIGURED");
        }
    }
}
