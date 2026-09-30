using UnityEngine;
namespace PowderFlow
{
    public class AudioController:MonoBehaviour
    {
        public GameSystemConfig config;SkiPhysicsController skier;RailSystem railSystem;AudioSource snow,wind,rail,impact;
        AudioSource Loop(string name,AudioClip clip){var obj=new GameObject(name);obj.transform.SetParent(transform,false);var source=obj.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=0;source.Play();return source;}
        void Start()
        {
            skier=GetComponent<SkiPhysicsController>();railSystem=GetComponent<RailSystem>();snow=Loop("Snow Foley",config.snow);wind=Loop("Wind Foley",config.wind);rail=Loop("Rail Foley",config.rail);impact=gameObject.AddComponent<AudioSource>();
            skier.Landed+=r=>Play(r.quality==LandingQuality.Bail?config.crash:config.landing,Mathf.Clamp01(r.impact/15));skier.TookOff+=()=>Play(config.pop,.4f);GetComponent<BailSystem>().Crashed+=()=>Play(config.crash,1);
        }
        void Play(AudioClip clip,float volume){if(clip)impact.PlayOneShot(clip,volume*config.impactVolume*SaveStore.Current.sfx);}
        void Update()
        {
            float speed=railSystem.Riding?railSystem.Velocity.magnitude:skier.Speed;
            float ratio=Mathf.Clamp01(speed/config.maximumAudioSpeed),sfx=SaveStore.Current.sfx;
            float load=Mathf.Clamp01(Mathf.Abs(skier.Motion.lateralAcceleration)/Mathf.Max(1,skier.config.maximumGrip));
            float snowEnergy=1+skier.Slip*.08f+load*.45f+(skier.Input.brake?.3f:0);
            float snowTarget=skier.Grounded&&!skier.Bailed&&!railSystem.Riding?Mathf.Clamp01(ratio*config.snowVolume*sfx*snowEnergy):0;
            snow.volume=Mathf.MoveTowards(snow.volume,snowTarget,Time.unscaledDeltaTime);snow.pitch=.7f+ratio*.8f+load*.12f;
            wind.volume=ratio*ratio*config.windVolume*sfx;wind.pitch=.8f+ratio*.4f;
            rail.volume=Mathf.MoveTowards(rail.volume,railSystem.Riding?ratio*config.railVolume*sfx:0,Time.unscaledDeltaTime);rail.pitch=.85f+ratio*.5f;
        }
    }
}
