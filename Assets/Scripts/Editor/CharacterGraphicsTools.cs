using UnityEditor;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Configure Character Visuals")]
        public static void ConfigureCharacterGraphics()
        {
            var config=Config<CharacterVisualConfig>("CharacterVisualConfig");
            EditorUtility.SetDirty(config);
            var catalog=Config<AssetCatalog>("AssetCatalog");catalog.characterVisuals=config;EditorUtility.SetDirty(catalog);
            var shader=Shader.Find("PowderFlow/Skier Surface");if(!shader)throw new System.Exception("Missing skier surface shader");
            foreach(var name in new[]{"skin","jacket","jacket_accent","jacket_trim","pants","pants_panel","helmet","helmet_accent","goggles","gloves","boots","ski","ski_graphic","ski_ink","equipment_trim","edges"})
            {
                var m=AssetDatabase.LoadAssetAtPath<Material>("Assets/Art/Materials/"+name+".mat");if(!m)continue;
                bool fabric=name.StartsWith("jacket")||name.StartsWith("pants")||name=="gloves";
                m.shader=shader;m.SetColor("_FillColor",config.ambientFill);m.SetFloat("_Rim",fabric?config.fabricRim:config.equipmentRim);m.SetFloat("_Weave",fabric?config.fabricWeave:0);
                m.SetFloat("_Smoothness",fabric?.18f:name=="goggles"?.85f:name=="edges"?.72f:.42f);m.SetFloat("_Metallic",name=="edges"?.75f:name=="goggles"?.3f:0);
                var p=config.outfits[0];if(name=="jacket")m.color=p.jacket;else if(name=="jacket_accent"||name=="helmet_accent")m.color=p.accent;else if(name=="pants")m.color=p.pants;else if(name=="pants_panel")m.color=p.panels;else if(name=="ski")m.color=p.skis;else if(name=="ski_graphic")m.color=p.graphics;else if(name=="helmet")m.color=new Color(.12f,.18f,.23f);
                EditorUtility.SetDirty(m);
            }
            AssetDatabase.SaveAssets();Debug.Log("VP2 CHARACTER GRAPHICS CONFIGURED");
        }
    }
}
