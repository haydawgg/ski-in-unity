using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace PowderFlow
{
    public class AlpineLighting:MonoBehaviour
    {
        public GraphicsConfig config;public bool day;Light sun;Material runtimeSky;
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
            if(!sun)return;day=clearDay;sun.transform.rotation=Quaternion.Euler(day?config.dayElevation:config.sunsetElevation,config.sunAzimuth,0);sun.color=day?config.dayLight:config.sunsetLight;sun.intensity=day?config.daySunIntensity:config.sunIntensity;sun.shadows=LightShadows.Soft;
            RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=day?config.dayAmbient:config.sunsetAmbient;
            RenderSettings.fog=true;RenderSettings.fogMode=FogMode.Exponential;RenderSettings.fogDensity=day?config.dayFogDensity:config.fogDensity;RenderSettings.fogColor=day?config.dayFog:config.sunsetFog;
            if(runtimeSky)Destroy(runtimeSky);var sky=new Material(config.skyMaterial);runtimeSky=sky;sky.SetColor("_Top",day?config.dayTop:config.sunsetTop);sky.SetColor("_Horizon",day?config.dayHorizon:config.sunsetHorizon);sky.SetVector("_SunDirection",-sun.transform.forward);sky.SetColor("_SunColor",sun.color);sky.SetFloat("_Clouds",day?.7f:.38f);sky.SetColor("_CloudColor",day?config.dayCloud:config.sunsetCloud);sky.SetFloat("_CloudScale",config.cloudScale);sky.SetFloat("_CloudCoverage",config.cloudCoverage);sky.SetFloat("_CloudSoftness",config.cloudSoftness);sky.SetFloat("_SunRadius",config.sunRadius);RenderSettings.skybox=sky;DynamicGI.UpdateEnvironment();
        }
        void OnDestroy(){if(runtimeSky)Destroy(runtimeSky);}
        void Update(){var player=GetComponent<MountainWorld>()?.player;if(player&&player.Input.lighting){Apply(!day);SaveStore.Current.day=day;SaveStore.Save();}}
    }
}
