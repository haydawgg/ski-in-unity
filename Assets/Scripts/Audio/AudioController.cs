using UnityEngine;
namespace PowderFlow
{
    public class AudioController:MonoBehaviour
    {
        public GameSystemConfig config;SkiPhysicsController skier;AudioSource snow,wind,rail,impact;
        AudioSource Loop(string name,AudioClip clip){var obj=new GameObject(name);obj.transform.SetParent(transform,false);var source=obj.AddComponent<AudioSource>();source.clip=clip;source.loop=true;source.volume=0;source.Play();return source;}
        void Start()
        {
            skier=GetComponent<SkiPhysicsController>();snow=Loop("Snow Foley",config.snow);wind=Loop("Wind Foley",config.wind);rail=Loop("Rail Foley",config.rail);impact=gameObject.AddComponent<AudioSource>();
            skier.Landed+=r=>Play(r.quality==LandingQuality.Bail?config.crash:config.landing,Mathf.Clamp01(r.impact/15));skier.TookOff+=()=>Play(config.pop,.4f);GetComponent<BailSystem>().Crashed+=()=>Play(config.crash,1);
        }
        void Play(AudioClip clip,float volume){if(clip)impact.PlayOneShot(clip,volume*config.impactVolume*SaveStore.Current.sfx);}
        void Update()
        {
            float ratio=Mathf.Clamp01(skier.Speed/config.maximumAudioSpeed),sfx=SaveStore.Current.sfx;
            snow.volume=Mathf.MoveTowards(snow.volume,skier.Grounded?ratio*config.snowVolume*sfx*(1+skier.Slip*.08f):0,Time.unscaledDeltaTime);snow.pitch=.7f+ratio*.8f;
            wind.volume=ratio*ratio*config.windVolume*sfx;wind.pitch=.8f+ratio*.4f;
            rail.volume=GetComponent<RailSystem>().Riding?ratio*config.railVolume*sfx:0;
        }
    }
}
