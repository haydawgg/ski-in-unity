using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace PowderFlow
{
    public class GeneratedAssetImporter : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if(!assetPath.StartsWith("Assets/Art/Generated/"))return;
            var importer=(ModelImporter)assetImporter;importer.globalScale=1;importer.useFileUnits=true;importer.importNormals=ModelImporterNormals.Import;importer.importTangents=ModelImporterTangents.CalculateMikk;importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;importer.animationType=assetPath.Contains("/Character/")?ModelImporterAnimationType.Generic:ModelImporterAnimationType.None;importer.importAnimation=false;
        }
    }
    [Serializable]public class AssetManifest {public ManifestAsset[] assets;}
    [Serializable]public class ManifestAsset {public string name,type,fbxPath,previewPath;public int triangleCount;}
    [Serializable]public class RailMetadata {public string railId;public float width;public Vector3[] points;}
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Import Generated Assets")]
        public static void ImportGeneratedAssets()
        {
            Directory.CreateDirectory("Assets/Prefabs/Generated");Directory.CreateDirectory("Assets/Art/Materials");
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var manifest=JsonUtility.FromJson<AssetManifest>(File.ReadAllText("Assets/Art/Generated/asset_manifest.json"));var records=new List<GeneratedAsset>();
            foreach(var asset in manifest.assets)
            {
                var model=AssetDatabase.LoadAssetAtPath<GameObject>(asset.fbxPath);if(!model)throw new Exception("Missing imported model "+asset.name);
                var instance=UnityEngine.Object.Instantiate(model);instance.name=asset.name;
                var anim=instance.GetComponent<Animator>();if(anim)anim.enabled=false;
                foreach(var renderer in instance.GetComponentsInChildren<Renderer>())
                {
                    var list=renderer.sharedMaterials;
                    for(int i=0;i<list.Length;i++)
                    {
                        if(!list[i])continue;
                        string materialPath="Assets/Art/Materials/"+list[i].name.Replace(" ","_")+".mat";
                        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                        if(!material){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=list[i].color;material.SetFloat("_Smoothness",.18f);material.enableInstancing=true;AssetDatabase.CreateAsset(material,materialPath);}
                        list[i]=material;
                    }
                    renderer.sharedMaterials=list;
                }
                string metadataPath=Path.ChangeExtension(asset.fbxPath,".json"),metadata=File.Exists(metadataPath)?File.ReadAllText(metadataPath):"";
                if(asset.type!="Character" && asset.type!="Skis" && asset.type!="EnvironmentDistant")
                {
                    foreach(var filter in instance.GetComponentsInChildren<MeshFilter>())
                    {
                        if(filter.name.Contains("LOD")&&!filter.name.Contains("LOD0"))continue;
                        var collider=filter.gameObject.AddComponent<MeshCollider>();collider.sharedMesh=filter.sharedMesh;
                    }
                    instance.AddComponent<SnowSurface>().type=asset.type=="Rails"||asset.type=="Boxes"?SurfaceType.Rail:SurfaceType.Groomed;
                    GameObjectUtility.SetStaticEditorFlags(instance,StaticEditorFlags.BatchingStatic);
                }
                if(asset.type=="Rails"||asset.type=="Boxes")
                {
                    var data=JsonUtility.FromJson<RailMetadata>(metadata);var rail=instance.AddComponent<RailPath>();rail.railId=data.railId;rail.points=data.points;rail.width=data.width;
                }
                var lodRenderers=new List<Renderer[]>();
                for(int lod=0;lod<3;lod++){var collection=new List<Renderer>();foreach(var renderer in instance.GetComponentsInChildren<Renderer>())if(renderer.name.Contains("LOD"+lod))collection.Add(renderer);lodRenderers.Add(collection.ToArray());}
                if(lodRenderers[0].Length>0){var group=instance.AddComponent<LODGroup>();group.SetLODs(new[]{new LOD(.2f,lodRenderers[0]),new LOD(.08f,lodRenderers[1]),new LOD(.015f,lodRenderers[2])});group.RecalculateBounds();}
                var prefab=PrefabUtility.SaveAsPrefabAsset(instance,"Assets/Prefabs/Generated/"+asset.name+".prefab");UnityEngine.Object.DestroyImmediate(instance);
                records.Add(new GeneratedAsset{name=asset.name,type=asset.type,prefab=prefab,metadata=metadata});
            }
            var catalog=Config<AssetCatalog>("AssetCatalog");catalog.assets=records.ToArray();EditorUtility.SetDirty(catalog);
            var scene=EditorSceneManager.OpenScene("Assets/Scenes/PhysicsTest.unity");var world=UnityEngine.Object.FindFirstObjectByType<PhysicsTestWorld>();world.catalog=catalog;EditorSceneManager.SaveScene(scene);
            CreateAssetPreview(catalog);AssetDatabase.SaveAssets();Debug.Log("GENERATED IMPORT PASS "+records.Count);
        }
        public static void CreateAssetPreview(AssetCatalog catalog)
        {
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);var preview=new GameObject("Asset preview").AddComponent<AssetPreviewWorld>();preview.catalog=catalog;
            EditorSceneManager.SaveScene(scene,"Assets/Scenes/AssetPreview.unity");
        }
    }
}
