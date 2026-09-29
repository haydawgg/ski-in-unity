using System;
using UnityEngine;
namespace PowderFlow
{
    [Serializable] public class GeneratedAsset {public string name,type;public GameObject prefab;public string metadata;}
    [CreateAssetMenu(menuName="PowderFlow/Asset Catalog")]
    public class AssetCatalog : ScriptableObject
    {
        public GeneratedAsset[] assets=new GeneratedAsset[0];
        public GameObject Find(string name){foreach(var a in assets)if(a.name==name)return a.prefab;return null;}
    }
}
