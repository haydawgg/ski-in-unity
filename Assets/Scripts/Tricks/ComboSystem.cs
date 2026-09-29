using UnityEngine;
namespace PowderFlow
{
    public class ComboSystem : MonoBehaviour
    {
        public int Total { get; private set; } public int Combo { get; private set; } public int Multiplier { get; private set; }=1;
        public string LastName { get; private set; }="";public int LastPoints { get; private set; }public float FeedbackTime { get; private set; }
        public LandingQuality LastQuality { get; private set; }
        float idle;string previousName="";int repeats;
        SkiPhysicsController skier;
        void Start(){skier=GetComponent<SkiPhysicsController>();GetComponent<TrickTracker>().Completed+=Score;}
        public static int Calculate(TrickRecord r,TrickConfig c)
        {
            if(r.landing==LandingQuality.Bail)return 0;
            float points=100+TrickTracker.Quantize(r.yaw,c.rotationTolerance)/180f*c.scorePerHalfTurn+Mathf.Floor((Mathf.Abs(r.pitch)+c.rotationTolerance)/360)*c.flipScore+r.grabTime*c.grabScorePerSecond+r.railTime*c.railScorePerSecond+r.tweak*80;
            if(r.switchTakeoff||r.switchLanding)points*=c.switchBonus;if(Mathf.Abs(r.roll)>90)points*=c.corkBonus;
            points*=r.landing==LandingQuality.Perfect?1.25f:r.landing==LandingQuality.Sketchy?.6f:1;
            return Mathf.RoundToInt(points);
        }
        void Score(TrickRecord r)
        {
            LastName=TrickTracker.Name(r,skier.trickConfig);LastQuality=r.landing;FeedbackTime=4;
            if(r.landing==LandingQuality.Bail){Combo=0;Multiplier=1;LastPoints=0;return;}
            repeats=LastName==previousName?repeats+1:0;previousName=LastName;
            LastPoints=Mathf.RoundToInt(Calculate(r,skier.trickConfig)*Mathf.Pow(1-skier.trickConfig.repeatDecay,repeats));
            Combo+=LastPoints*Multiplier;Total+=LastPoints*Multiplier;Multiplier=Mathf.Min(8,Multiplier+1);idle=0;
        }
        void Update(){FeedbackTime-=Time.deltaTime;idle+=Time.deltaTime;if(idle>skier.trickConfig.comboTimeout || (skier.Speed<.5f&&idle>2)){Combo=0;Multiplier=1;}}
        public void Restart(){Total=Combo=0;Multiplier=1;}
    }
}
