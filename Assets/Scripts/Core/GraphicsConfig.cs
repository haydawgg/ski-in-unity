using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Graphics")]
    public class GraphicsConfig:ScriptableObject
    {
        public Material snowMaterial,skyMaterial,particleMaterial,trackMaterial;
        public UnityEngine.Rendering.VolumeProfile postProfile;
        public Color sunsetTop=new Color(.19f,.19f,.39f),sunsetHorizon=new Color(.88f,.56f,.65f),sunsetAmbient=new Color(.44f,.35f,.57f),sunsetLight=new Color(1,.75f,.68f);
        public Color dayTop=new Color(.08f,.3f,.68f),dayHorizon=new Color(.64f,.81f,.94f),dayAmbient=new Color(.45f,.55f,.69f),dayLight=new Color(1,.98f,.94f);
        public float sunsetElevation=13,dayElevation=42,sunAzimuth=160,sunIntensity=1.6f,fogDensity=.0008f,shadowDistance=150;
        public float trackSpacing=.22f,trackWidth=.07f,trackLift=.015f,trackLifetime=100;
        public int trackSamples=1100,maxParticles=1000;
        public float sprayRate=50,sprayLift=1.6f,landingBurst=80,crashBurst=180;
    }
}
