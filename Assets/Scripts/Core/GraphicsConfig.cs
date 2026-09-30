using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Graphics")]
    public class GraphicsConfig:ScriptableObject
    {
        public Material snowMaterial,propSnowMaterial,skyMaterial,particleMaterial,powderMaterial,trackMaterial;
        public UnityEngine.Rendering.VolumeProfile postProfile;
        public Color sunsetTop=new Color(.19f,.19f,.39f),sunsetHorizon=new Color(.88f,.56f,.65f),sunsetAmbient=new Color(.44f,.35f,.57f),sunsetLight=new Color(1,.75f,.68f);
        public Color dayTop=new Color(.08f,.3f,.68f),dayHorizon=new Color(.64f,.81f,.94f),dayAmbient=new Color(.45f,.55f,.69f),dayLight=new Color(1,.98f,.94f);
        public float sunsetElevation=13,dayElevation=42,sunAzimuth=160,sunIntensity=1.6f,fogDensity=.0008f,shadowDistance=150;
        public float trackSpacing=.18f,trackWidth=.11f,trackLift=.008f,trackLifetime=100;
        public int trackSamples=1100,maxParticles=1000;
        public float sprayRate=50,sprayLift=1.6f,landingBurst=80,crashBurst=180;
        [Header("Snow interaction")]
        public float trackUpdateInterval=.025f,trackMaximumGap=2.5f,trackMaximumInterval=.2f,powderTrackWidth=1.5f,powderTrackDepth=1.35f;
        public Color grooveColor=new Color(.28f,.36f,.48f,.40f),grooveRidgeColor=new Color(.85f,.92f,1,.18f);
        public float grooveSoftness=.22f,powderSprayMultiplier=2.2f,brakeCloudRate=22,railShavingRate=18,particleSoftDistance=.3f;
        public float flakeSize=.07f,puffSize=.42f,puffOpacity=.25f,contactAccent=14;
        [Header("Visual polish / atmosphere")]
        public Color sunsetFog=new Color(.49f,.51f,.66f),dayFog=new Color(.58f,.73f,.86f);
        public Color sunsetCloud=new Color(.73f,.66f,.77f),dayCloud=new Color(.94f,.96f,.99f);
        public float daySunIntensity=1.7f,dayFogDensity=.00055f,cloudScale=2.8f,cloudCoverage=.48f,cloudSoftness=.22f,sunRadius=.55f;
        [Header("Visual polish / surface")]
        public Color snowTint=new Color(.82f,.88f,.95f),rockTint=new Color(.16f,.19f,.25f);
        public float rippleStrength=.05f,rippleScale=3f,snowVariation=.065f,glitter=.16f,detailDistance=80,lightWrap=.16f,rimStrength=.055f;
        [Header("Visual polish / post and shadows")]
        public float exposure=.1f,contrast=5,saturation=-4,bloomIntensity=.28f,bloomThreshold=1.1f,vignetteIntensity=.10f;
        public float shadowDepthBias=.6f,shadowNormalBias=.35f,ambientOcclusion=.38f;
        public int shadowResolution=4096;
    }
}
