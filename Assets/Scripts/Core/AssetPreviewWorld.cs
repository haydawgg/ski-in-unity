using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
namespace PowderFlow
{
    public class AssetPreviewWorld : MonoBehaviour
    {
        public AssetCatalog catalog;
        Transform character;OutfitSystem outfit;float yaw=30;
        void Awake()
        {
            var host=new GameObject("Character review");host.transform.SetParent(transform);character=Instantiate(catalog.Find("Skier"),host.transform).transform;
            var pose=host.AddComponent<SkierPose>();pose.config=catalog.characterVisuals;pose.Bind(character);outfit=host.AddComponent<OutfitSystem>();outfit.Apply(0);
            void Light(string name,Vector3 angles,Color color,float intensity){var light=new GameObject(name).AddComponent<UnityEngine.Light>();light.transform.SetParent(transform);light.type=LightType.Directional;light.intensity=intensity;light.color=color;light.transform.rotation=Quaternion.Euler(angles);}
            Light("Studio key",new Vector3(32,-35,0),new Color(1,.92f,.83f),1.4f);Light("Studio fill",new Vector3(15,130,0),new Color(.58f,.72f,1),.65f);
            RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.45f,.5f,.6f);RenderSettings.fog=false;
            var camera=new GameObject("Preview camera").AddComponent<Camera>();camera.transform.SetParent(transform);camera.tag="MainCamera";camera.transform.position=new Vector3(0,.6f,3.5f);camera.transform.LookAt(new Vector3(0,.15f,0));camera.fieldOfView=42;camera.backgroundColor=new Color(.055f,.075f,.105f);camera.clearFlags=CameraClearFlags.SolidColor;camera.GetUniversalAdditionalCameraData().antialiasing=AntialiasingMode.FastApproximateAntialiasing;
        }
        void Update()
        {
            var k=Keyboard.current;if(k==null)return;
            if(k.leftArrowKey.isPressed)yaw-=Time.deltaTime*70;if(k.rightArrowKey.isPressed)yaw+=Time.deltaTime*70;character.localRotation=Quaternion.Euler(0,yaw,0);
            var keys=new[]{k.digit1Key,k.digit2Key,k.digit3Key,k.digit4Key,k.digit5Key,k.digit6Key};for(int i=0;i<keys.Length;i++)if(keys[i].wasPressedThisFrame)outfit.Apply(i);
        }
        void OnGUI(){GUI.Label(new Rect(20,20,500,50),$"CHARACTER REVIEW · {catalog.characterVisuals.outfits[outfit.preset].name}\n1–6: outfit · Left/Right: rotate");}
    }
}
