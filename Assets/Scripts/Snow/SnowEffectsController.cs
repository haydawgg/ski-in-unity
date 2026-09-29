using UnityEngine;
namespace PowderFlow
{
    public class SnowEffectsController:MonoBehaviour
    {
        public GraphicsConfig config;SkiPhysicsController skier;ParticleSystem spray;
        void Start()
        {
            skier=GetComponent<SkiPhysicsController>();var obj=new GameObject("Pooled snow flakes");obj.transform.SetParent(transform,false);spray=obj.AddComponent<ParticleSystem>();var main=spray.main;main.loop=false;main.playOnAwake=false;main.startLifetime=new ParticleSystem.MinMaxCurve(.4f,1.4f);main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.035f,.12f);main.startColor=new Color(.88f,.91f,1,.85f);main.maxParticles=config.maxParticles;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=.7f;var emission=spray.emission;emission.enabled=false;var shape=spray.shape;shape.enabled=false;var renderer=spray.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=config.particleMaterial;renderer.renderMode=ParticleSystemRenderMode.Billboard;
            skier.Landed+=r=>Burst((int)(config.landingBurst*Mathf.Clamp01(r.impact/15)),false);GetComponent<BailSystem>().Crashed+=()=>Burst((int)config.crashBurst,true);
        }
        void LateUpdate()
        {
            if(!skier.Grounded||skier.Speed<2)return;
            float rate=(Mathf.Abs(skier.Edge)*skier.Speed+skier.Slip*4)*config.sprayRate*.08f;if(skier.Surface==SurfaceType.Powder)rate*=2;
            int count=Mathf.Min(30,Mathf.FloorToInt(rate*Time.deltaTime));
            for(int i=0;i<count;i++){var contact=i%2==0?skier.Contacts.left:skier.Contacts.right;if(!contact.hit)continue;var p=new ParticleSystem.EmitParams{position=contact.point,velocity=contact.normal*Random.Range(.2f,config.sprayLift)+skier.transform.right*Random.Range(-2,2)-skier.Body.linearVelocity*.12f};spray.Emit(p,1);}
        }
        void Burst(int count,bool crash)
        {
            if(!spray)return;for(int i=0;i<count;i++){var p=new ParticleSystem.EmitParams{position=skier.transform.position-Vector3.up*.4f,velocity=Random.insideUnitSphere*(crash?5:3)+Vector3.up*3,startSize=crash?Random.Range(.10f,.35f):Random.Range(.05f,.15f),startLifetime=crash?2:1};spray.Emit(p,1);}
        }
    }
}
