using System.IO;
using UnityEditor;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Configure Snow Interaction Visuals")]
        public static void ConfigureSnowVisuals()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);var config=Config<GraphicsConfig>("GraphicsConfig");ConfigureSnowGraphics(config);EditorUtility.SetDirty(config);AssetDatabase.SaveAssets();Debug.Log("SNOW VISUALS CONFIGURED / soft grooves + pooled spray");
        }
        static void ConfigureSnowGraphics(GraphicsConfig config)
        {
            Material Make(string name,string shader)
            {
                var path="Assets/Art/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);var s=Shader.Find(shader);if(!s)throw new System.Exception("Missing shader "+shader);
                if(!m){m=new Material(s);AssetDatabase.CreateAsset(m,path);}m.shader=s;EditorUtility.SetDirty(m);return m;
            }
            config.trackMaterial=Make("SkiGrooves","PowderFlow/Ski Grooves");config.trackMaterial.SetColor("_BaseColor",config.grooveColor);config.trackMaterial.SetColor("_RidgeColor",config.grooveRidgeColor);config.trackMaterial.SetFloat("_Softness",config.grooveSoftness);config.trackMaterial.SetFloat("_Lifetime",config.trackLifetime);config.trackMaterial.renderQueue=2990;
            config.particleMaterial=Make("SnowFlakes","PowderFlow/Snow Particle");config.powderMaterial=Make("PowderPuffs","PowderFlow/Snow Particle");
            foreach(var m in new[]{config.particleMaterial,config.powderMaterial}){m.SetColor("_BaseColor",Color.white);m.SetFloat("_SoftDistance",config.particleSoftDistance);m.SetFloat("_NearFade",.7f);m.shaderKeywords=new string[0];m.renderQueue=3000;}
            Texture2D Shape(string name,int size,bool puff)
            {
                var texture=new Texture2D(size,size,TextureFormat.RGBA32,false);var colors=new Color[size*size];
                for(int y=0;y<size;y++)for(int x=0;x<size;x++)
                {
                    float dx=(x+.5f)/size*2-1,dy=(y+.5f)/size*2-1,alpha;
                    if(puff)
                    {
                        float Lobe(float cx,float cy,float radius)=>Mathf.Exp(-((dx-cx)*(dx-cx)+(dy-cy)*(dy-cy))/radius);
                        alpha=Mathf.Max(Lobe(-.26f,-.1f,.21f),Mathf.Max(Lobe(.24f,.12f,.26f),Lobe(0,.30f,.18f)));
                        alpha=Mathf.Clamp01((alpha-.06f)/.94f)*(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.65f,1,Mathf.Sqrt(dx*dx+dy*dy))));
                    }
                    else alpha=Mathf.Pow(Mathf.Clamp01(1-Mathf.Sqrt(dx*dx*.75f+dy*dy*1.4f)),1.45f);
                    colors[y*size+x]=new Color(1,1,1,alpha);
                }
                texture.SetPixels(colors);texture.Apply();Directory.CreateDirectory("Assets/Art/Textures");var path="Assets/Art/Textures/"+name+".png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
                var importer=(TextureImporter)AssetImporter.GetAtPath(path);importer.textureType=TextureImporterType.Default;importer.alphaIsTransparency=true;importer.wrapMode=TextureWrapMode.Clamp;importer.filterMode=FilterMode.Bilinear;importer.mipmapEnabled=true;importer.isReadable=true;importer.SaveAndReimport();return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            }
            config.particleMaterial.SetTexture("_BaseMap",Shape("SnowFlake",64,false));config.powderMaterial.SetTexture("_BaseMap",Shape("PowderPuff",128,true));EditorUtility.SetDirty(config);
        }
    }
}
