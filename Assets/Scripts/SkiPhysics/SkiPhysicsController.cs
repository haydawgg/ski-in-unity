using UnityEngine;
namespace PowderFlow
{
    [RequireComponent(typeof(Rigidbody),typeof(SkiContactSystem),typeof(SkierInput))]
    public class SkiPhysicsController : MonoBehaviour
    {
        public SkiPhysicsConfig config;
        public TrickConfig trickConfig; public AirControlSystem Air { get; private set; }
        public event System.Action TookOff; public event System.Action<LandingResult> Landed;
        public LandingResult LastLanding { get; private set; }
        public bool Bailed { get; set; } public float Compression { get; private set; }
        float contactLock,airTime; Vector3 preload; bool previousGrounded;
        public Rigidbody Body { get; private set; }
        public SkiContactSystem Contacts { get; private set; }
        public SkierInput Input { get; private set; }
        public Transform visual;
        public bool Grounded { get; private set; }
        public float Speed => Body ? Body.linearVelocity.magnitude : 0;
        public float Edge { get; private set; }
        public float Slip { get; private set; }
        public Vector3 SupportNormal { get; private set; }=Vector3.up;
        public Vector3 startPosition, safePosition;
        public Quaternion startRotation, safeRotation;
        float safeTime;
        void Awake() { Initialize(config); }
        public void Initialize(SkiPhysicsConfig settings)
        {
            config=settings; Body=GetComponent<Rigidbody>(); Contacts=GetComponent<SkiContactSystem>(); Input=GetComponent<SkierInput>();
            Air=GetComponent<AirControlSystem>(); if(!Air)Air=gameObject.AddComponent<AirControlSystem>();
            if(!config) return;
            gameObject.layer=8;
            Body.mass=config.mass; Body.interpolation=RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            Body.linearDamping=0; Body.angularDamping=0; Body.maxAngularVelocity=25;
            startPosition=safePosition=Body.position; startRotation=safeRotation=Body.rotation;
        }
        void FixedUpdate() { if(config) Step(Time.fixedDeltaTime); }
        public void Step(float dt)
        {
            Contacts.Sample(Body,config);
            contactLock=Mathf.Max(0,contactLock-dt);
            Grounded=Contacts.Grounded && contactLock<=0 && !Bailed;
            if(Bailed)return;
            if(Input.pop && Grounded)
            {
                Body.AddForce(SupportNormal*(config.popImpulse+Speed*config.popSpeedScale),ForceMode.VelocityChange);
                contactLock=trickConfig?trickConfig.takeoffContactLock:.18f; Grounded=false; Input.pop=false;
            }
            if(trickConfig)
            {
                if(previousGrounded && !Grounded){Air.Begin(Body,Input,trickConfig,preload);preload=Vector3.zero;airTime=0;TookOff?.Invoke();}
                if(!previousGrounded && Grounded && airTime>.1f)
                {
                    LastLanding=LandingSystem.Evaluate(Body.rotation,Body.linearVelocity,Body.angularVelocity,Contacts.Normal,Contacts.left.hit&&Contacts.right.hit,trickConfig);
                    Compression=Mathf.Clamp01(LastLanding.impact/trickConfig.cleanImpact);
                    Bailed=LastLanding.quality==LandingQuality.Bail;
                    if(!Bailed)Body.linearVelocity=Vector3.ProjectOnPlane(Body.linearVelocity,Contacts.Normal)*LastLanding.retention;
                    Landed?.Invoke(LastLanding); if(Bailed)return;
                }
                if(!Grounded)
                {
                    airTime+=dt;Air.Step(Body,Input,trickConfig,dt);
                    if(Physics.Raycast(Body.position,Vector3.down,out var landingHit,trickConfig.assistHeight,~(1<<8),QueryTriggerInteraction.Ignore)){LandingSystem.Assist(Body,landingHit.normal,landingHit.distance,trickConfig,dt);Air.SetMomentum(Body.angularVelocity*Air.Inertia);}
                }
                else
                {
                    var desired=Input.modifierRight ? Vector3.up*Input.steer*trickConfig.preloadRate+Body.rotation*Vector3.right*Input.flip*trickConfig.preloadRate : Vector3.zero;
                    preload=Vector3.Lerp(preload,Vector3.ClampMagnitude(desired,trickConfig.maximumPreload),dt*config.steerResponse);
                }
            }
            previousGrounded=Grounded;
            Compression=Mathf.MoveTowards(Compression,0,dt*(trickConfig?trickConfig.compressionResponse:8));
            if(Grounded)Body.AddForce(CarvingSystem.Drag(Body.linearVelocity,Input.tuck,config),ForceMode.Acceleration);
            Edge=Mathf.Lerp(Edge,Input.steer,1-Mathf.Exp(-config.steerResponse*dt));
            if(Grounded)
            {
                SupportNormal=Vector3.Slerp(SupportNormal,Contacts.Normal,1-Mathf.Exp(-config.normalResponse*dt));
                var normal=SupportNormal;
                var forward=Vector3.ProjectOnPlane(Body.rotation*Vector3.forward,normal).normalized;
                if(forward.sqrMagnitude<.1f)forward=Vector3.ProjectOnPlane(Vector3.forward,normal).normalized;
                var side=Vector3.Cross(normal,forward);
                float longitudinal=Vector3.Dot(Body.linearVelocity,forward), lateral=Vector3.Dot(Body.linearVelocity,side);
                float support=(config.rideHeight-Contacts.Height)*config.supportSpring-Vector3.Dot(Body.linearVelocity,normal)*config.supportDamping-Vector3.Dot(Physics.gravity,normal);
                Body.AddForce(normal*Mathf.Max(0,support),ForceMode.Acceleration);
                float grip=Mathf.Lerp(config.flatGrip,config.edgeGrip,Mathf.Abs(Edge));
                if(Input.brake)grip=config.skidGrip;
                if(Contacts.Surface==SurfaceType.Powder)grip*=.65f;
                if(Contacts.Surface==SurfaceType.Ice)grip*=.4f;
                float lateralAccel=Mathf.Clamp(-lateral*grip,-config.maximumGrip,config.maximumGrip);
                Body.AddForce(side*lateralAccel,ForceMode.Acceleration);
                float friction=CarvingSystem.Friction(Contacts.Surface,config)*Physics.gravity.magnitude;
                if(Mathf.Abs(longitudinal)>.05f)Body.AddForce(-forward*Mathf.Sign(longitudinal)*Mathf.Min(friction,Mathf.Abs(longitudinal)/dt),ForceMode.Acceleration);
                if(Contacts.Surface==SurfaceType.Powder)Body.AddForce(-Vector3.ProjectOnPlane(Body.linearVelocity,normal)*config.powderDrag,ForceMode.Acceleration);
                if(Input.brake && Speed>.05f)Body.AddForce(-Body.linearVelocity.normalized*Mathf.Min(config.brakeDeceleration,Speed/dt),ForceMode.Acceleration);
                float yawRate=Mathf.Abs(Edge)<.02f ? 0 : longitudinal/CarvingSystem.Radius(Mathf.Abs(longitudinal),Edge,config)*Mathf.Sign(Edge);
                var target=Quaternion.AngleAxis(yawRate*Mathf.Rad2Deg*dt,normal)*Quaternion.LookRotation(forward,normal);
                Body.MoveRotation(target);
                Body.angularVelocity=Vector3.zero;
                Slip=Mathf.Abs(lateral);
                safeTime+=dt;
                if(safeTime>config.safeRecordInterval && Speed<config.safeSpeed) { safeTime=0; safePosition=Body.position; safeRotation=Body.rotation; }
            }
            if(visual)visual.localRotation=Quaternion.Euler(0,0,-Edge*config.maximumLean);
            if(Body.position.y<config.resetDepth)ResetTo(startPosition,startRotation);
        }
        public void ResetTo(Vector3 position,Quaternion rotation)
        {
            Body.position=position; Body.rotation=rotation; Body.linearVelocity=Body.angularVelocity=Vector3.zero;
            SupportNormal=rotation*Vector3.up; Edge=0;Bailed=false;contactLock=0;airTime=0;preload=Vector3.zero;previousGrounded=false;
        }
        void Update() { if(Input && Input.reset)ResetTo(safePosition,safeRotation); }
    }
}
