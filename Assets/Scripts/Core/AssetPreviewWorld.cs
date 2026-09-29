using UnityEngine;
namespace PowderFlow
{
    public class AssetPreviewWorld : MonoBehaviour
    {
        public AssetCatalog catalog;
        void Awake()
        {
            var character=Instantiate(catalog.Find("Skier"),Vector3.zero,Quaternion.identity);
            var light=new GameObject("Studio light").AddComponent<Light>();light.type=LightType.Directional;light.intensity=2;light.transform.rotation=Quaternion.Euler(30,-35,0);
            RenderSettings.ambientLight=new Color(.45f,.5f,.6f);
            var camera=new GameObject("Preview camera").AddComponent<Camera>();camera.tag="MainCamera";camera.transform.position=new Vector3(2,1,3);camera.transform.LookAt(new Vector3(0,.2f,0));camera.backgroundColor=new Color(.15f,.2f,.29f);camera.clearFlags=CameraClearFlags.SolidColor;
        }
    }
}
