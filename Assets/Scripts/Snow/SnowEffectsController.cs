using UnityEngine;
namespace PowderFlow
{
    public class SnowEffectsController:MonoBehaviour
    {
        public GraphicsConfig config;
        SkiPhysicsController skier;RailSystem rail;BailSystem bail;ParticleSystem flakes,powder,shavings;
        float flakeCarry,puffCarry,railCarry,skipUntil;Vector3 previousPosition;
        public int GroundParticlesEmitted {get;private set;}public int RailParticlesEmitted {get;private set;}
        public int LandingBursts {get;private set;}public int CrashBursts {get;private set;}
        public int LiveParticles=>(flakes?flakes.particleCount:0)+(powder?powder.particleCount:0)+(shavings?shavings.particleCount:0);
        public static int Accumulate(ref float carry,float rate,float dt,int limit)
        {
            carry=Mathf.Min(carry+Mathf.Max(0,rate)*dt,limit+1);int count=Mathf.Min(limit,Mathf.FloorToInt(carry));carry-=count;return count;
        }
        void Start()
        {
            skier=GetComponent<SkiPhysicsController>();rail=GetComponent<RailSystem>();bail=GetComponent<BailSystem>();
            int cap=Mathf.Max(3,config.maxParticles),flakeCap=Mathf.Max(1,cap*55/100),puffCap=Mathf.Max(1,cap*35/100);
            flakes=CreatePool("Fine snow spray",config.particleMaterial,flakeCap,.7f);
            powder=CreatePool("Soft powder clouds",config.powderMaterial,puffCap,.16f);
            shavings=CreatePool("Rail frost shavings",config.particleMaterial,cap-flakeCap-puffCap,.8f);
            skier.Landed+=OnLanding;skier.ResetPerformed+=ResetEffects;if(bail)bail.Crashed+=OnCrash;previousPosition=skier.Body.position;
        }
        ParticleSystem CreatePool(string name,Material material,int cap,float gravity)
        {
            var obj=new GameObject(name);obj.transform.SetParent(transform,false);var pool=obj.AddComponent<ParticleSystem>();pool.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
            var main=pool.main;main.loop=true;main.playOnAwake=false;main.startSpeed=0;main.maxParticles=cap;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=gravity;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;
            var emission=pool.emission;emission.enabled=false;var shape=pool.shape;shape.enabled=false;
            var renderer=pool.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            var fade=pool.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.08f),new GradientAlphaKey(.65f,.55f),new GradientAlphaKey(0,1)});fade.color=gradient;
            var size=pool.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.65f,1,1.3f));pool.Play();return pool;
        }
        bool Emit(ParticleSystem pool,Vector3 position,Vector3 velocity,float size,float lifetime,float alpha)
        {
            if(pool.particleCount>=pool.main.maxParticles||LiveParticles>=config.maxParticles)return false;
            pool.Emit(new ParticleSystem.EmitParams{position=position,velocity=velocity,startSize=size,startLifetime=lifetime,startColor=new Color(.93f,.96f,1,alpha),rotation=Random.Range(0,360)},1);return true;
        }
        void LateUpdate()
        {
            if(!skier)return;float dt=Time.deltaTime;var position=skier.Body.position;
            float railSpeed=dt>0?Mathf.Min(40,(position-previousPosition).magnitude/dt):0;previousPosition=position;
            if(dt<=0||Time.time<skipUntil||skier.Bailed){flakeCarry=puffCarry=railCarry=0;return;}
            if(rail&&rail.Riding)
            {
                flakeCarry=puffCarry=0;int count=Accumulate(ref railCarry,config.railShavingRate*Mathf.Clamp(railSpeed/12,.3f,2),dt,14);
                Vector3 point=rail.Current.Evaluate(rail.Current.Closest(position-Vector3.up*skier.config.rideHeight,out _,out _),out var tangent);
                for(int i=0;i<count;i++)if(Emit(shavings,point+Vector3.up*.025f,Vector3.up*Random.Range(.3f,.85f)+skier.transform.right*Random.Range(-.6f,.6f)-tangent*railSpeed*.08f,config.flakeSize*Random.Range(.25f,.65f),Random.Range(.22f,.5f),.65f))RailParticlesEmitted++;
                return;
            }
            railCarry=0;
            bool snow=skier.Surface==SurfaceType.Groomed||skier.Surface==SurfaceType.Powder||skier.Surface==SurfaceType.Ice;
            if(!skier.Grounded||skier.Speed<2||!snow){flakeCarry=puffCarry=0;return;}
            var normal=skier.SupportNormal;var velocity=Vector3.ProjectOnPlane(skier.Body.linearVelocity,normal);var travel=velocity.normalized;
            var right=Vector3.Cross(normal,Vector3.ProjectOnPlane(skier.transform.forward,normal).normalized).normalized;
            float slip=Vector3.Dot(velocity,right),edge=skier.Edge;var outward=right*(Mathf.Abs(slip)>.5f?-Mathf.Sign(slip):Mathf.Abs(edge)>.05f?-Mathf.Sign(edge):0);
            float factor=skier.Surface==SurfaceType.Powder?config.powderSprayMultiplier:skier.Surface==SurfaceType.Ice?.35f:1;
            float load=Mathf.Clamp01(Mathf.Abs(skier.Motion.lateralAcceleration)/Mathf.Max(1,skier.config.maximumGrip));
            float rate=config.sprayRate*(.18f+Mathf.Abs(edge)*skier.Speed*.055f+skier.Slip*.12f+load*config.carveLoadSpray+(skier.Input.brake?.65f:0))*factor;
            int flakesNow=Accumulate(ref flakeCarry,Mathf.Min(rate,220),dt,36);
            int puffsNow=Accumulate(ref puffCarry,(skier.Input.brake?config.brakeCloudRate*factor:rate*(.1f+load*.12f))*Mathf.Clamp01(skier.Speed/10),dt,12);
            for(int i=0;i<flakesNow+puffsNow;i++)
            {
                bool mist=i>=flakesNow;var contact=(i%2==0)?skier.Contacts.left:skier.Contacts.right;if(!contact.hit)continue;
                var point=contact.point+contact.normal*.035f-travel*.25f;
                float sideways=Random.Range(.25f,1.1f)+Mathf.Min(skier.Slip,6)*.2f+load*config.carveFanSpeed;
                var motion=outward*sideways+right*Random.Range(-.25f,.25f)-velocity*(mist?.035f:.07f)+contact.normal*Random.Range(mist?.15f:.35f,mist?.55f:config.sprayLift);
                bool emitted=Emit(mist?powder:flakes,point,motion,mist?config.puffSize*Random.Range(.65f,1.2f)*(skier.Input.brake?1.35f:1):config.flakeSize*Random.Range(.5f,1.25f),Random.Range(mist?.4f:.3f,mist?.9f:.8f),mist?config.puffOpacity:.75f);
                if(emitted)GroundParticlesEmitted++;
            }
        }
        Vector3 ContactOrigin(out Vector3 normal)
        {
            var left=skier.Contacts.left;var right=skier.Contacts.right;normal=left.hit?left.normal:right.hit?right.normal:skier.SupportNormal;
            return left.hit&&right.hit?(left.point+right.point)*.5f:left.hit?left.point:right.hit?right.point:skier.Body.position-normal*skier.config.rideHeight;
        }
        void OnLanding(LandingResult result)
        {
            if(result.quality==LandingQuality.Bail||result.impact<.8f||skier.Surface==SurfaceType.Rail||skier.Surface==SurfaceType.Rock)return;
            LandingBursts++;var origin=ContactOrigin(out var normal);float strength=Mathf.Clamp01(result.impact/15);
            var right=Vector3.Cross(normal,skier.transform.forward).normalized;var forward=Vector3.Cross(right,normal);
            int count=Mathf.RoundToInt(config.landingBurst*strength);
            for(int i=0;i<count;i++)
            {
                var radial=(right*Random.Range(-1,1)+forward*Random.Range(-1,1)).normalized;
                Emit(flakes,origin+radial*Random.Range(.1f,.45f)+normal*.06f,radial*Random.Range(.7f,2.5f)+normal*Random.Range(.5f,1.7f),config.flakeSize*Random.Range(.7f,1.3f),Random.Range(.35f,.85f),.8f);
            }
            int accent=Mathf.RoundToInt(config.contactAccent*Mathf.Lerp(.4f,1,strength));
            for(int i=0;i<accent;i++)
            {
                float angle=2*Mathf.PI*i/accent;var radial=right*Mathf.Cos(angle)+forward*Mathf.Sin(angle);
                Emit(powder,origin+radial*.38f+normal*.08f,radial*(.8f+strength)+normal*.25f,config.puffSize*1.3f,.65f,config.puffOpacity*.9f);
            }
        }
        void OnCrash()
        {
            CrashBursts++;var origin=ContactOrigin(out var normal);int count=Mathf.RoundToInt(config.crashBurst);
            for(int i=0;i<count;i++)Emit(flakes,skier.Body.position-normal*.25f,Random.insideUnitSphere*2.7f+normal*1.5f,config.flakeSize*Random.Range(.8f,2),Random.Range(.5f,1.3f),.75f);
            if(skier.Contacts.Grounded)for(int i=0;i<18;i++)Emit(powder,origin+normal*.1f,Vector3.ProjectOnPlane(Random.insideUnitSphere,normal)*2+normal*.45f,config.puffSize*1.4f,.8f,config.puffOpacity);
        }
        void ResetEffects()
        {
            foreach(var pool in new[]{flakes,powder,shavings})if(pool)pool.Clear(true);
            flakeCarry=puffCarry=railCarry=0;skipUntil=Time.time+Time.fixedDeltaTime*2;if(skier)previousPosition=skier.Body.position;
        }
        void OnDisable(){ResetEffects();}
        void OnEnable(){if(skier)ResetEffects();}
        void OnDestroy(){if(skier){skier.Landed-=OnLanding;skier.ResetPerformed-=ResetEffects;}if(bail)bail.Crashed-=OnCrash;}
    }
}
