using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace PowderFlow
{
    public class AlpineLighting:MonoBehaviour
    {
        public GraphicsConfig config;public bool day;Light sun;
        void Start()
        {
            foreach(var light in GetComponentsInChildren<Light>())if(light.type==LightType.Directional){sun=light;break;}
            if(!sun){sun=new GameObject("Alpine sun").AddComponent<Light>();sun.type=LightType.Directional;sun.transform.SetParent(transform);}
            var volume=gameObject.AddComponent<Volume>();volume.isGlobal=true;volume.sharedProfile=config.postProfile;
            foreach(var camera in Camera.allCameras){var data=camera.GetUniversalAdditionalCameraData();data.renderPostProcessing=true;data.antialiasing=AntialiasingMode.FastApproximateAntialiasing;camera.allowHDR=true;}
            Apply(false);
        }
        public void Apply(bool clearDay)
        {
            if(!sun)return;day=clearDay;sun.transform.rotation=Quaternion.Euler(day?config.dayElevation:config.sunsetElevation,config.sunAzimuth,0);sun.color=day?config.dayLight:config.sunsetLight;sun.intensity=config.sunIntensity;sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=day?config.dayAmbient:config.sunsetAmbient;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogDensity=config.fogDensity;RenderSettings.fogColor=day?config.dayHorizon:new Color(.56f,.51f,.68f);
            var sky=new Material(config.skyMaterial);sky.SetColor("_Top",day?config.dayTop:config.sunsetTop);sky.SetColor("_Horizon",day?config.dayHorizon:config.sunsetHorizon);sky.SetVector("_SunDirection",-sun.transform.forward);sky.SetColor("_SunColor",sun.color);sky.SetFloat("_Clouds",day?1:.35f);RenderSettings.skybox=sky;DynamicGI.UpdateEnvironment();
        }
        void Update(){var player=GetComponent<MountainWorld>()?.player;if(player&&player.Input.lighting)Apply(!day);}
    }
}
