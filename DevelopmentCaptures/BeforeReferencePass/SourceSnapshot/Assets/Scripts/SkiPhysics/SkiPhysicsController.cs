using UnityEngine;
namespace PowderFlow
{
    [RequireComponent(typeof(Rigidbody),typeof(SkiContactSystem),typeof(SkierInput))]
    public class SkiPhysicsController : MonoBehaviour
    {
        public SkiPhysicsConfig config;
        public TrickConfig trickConfig; public AirControlSystem Air { get; private set; }
        public event System.Action ResetPerformed; public event System.Action TookOff; public event System.Action<LandingResult> Landed;
        public LandingResult LastLanding { get; private set; }
        public bool PowderOverride;public SurfaceType Surface=>PowderOverride?SurfaceType.Powder:Contacts.Surface;
        public bool Bailed { get; set; } public float Compression { get; private set; }
        public SkierMotionState Motion { get; private set; }
        public LandingPrediction PredictedLanding { get; private set; }
        Vector3 lastVelocity;bool hasVelocitySample;float lateralLoad,motionGrounded,popAge=100,balanceRecovery;
        float bufferedPop,sinceGrounded=100;float contactLock,airTime; Vector3 preload; bool previousGrounded;
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
            Body.centerOfMass=Input.crouch?Vector3.down*config.crouchHeight:Vector3.zero;
            Body.mass=config.mass; Body.interpolation=RigidbodyInterpolation.Interpolate;
            Body.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
            Body.linearDamping=0; Body.angularDamping=0; Body.maxAngularVelocity=25;
            startPosition=safePosition=Body.position; startRotation=safeRotation=Body.rotation;
        }
        void FixedUpdate() { if(config) Step(Time.fixedDeltaTime); }
        public void Step(float dt)
        {
            Body.centerOfMass=Input.crouch?Vector3.down*config.crouchHeight:Vector3.zero;
            popAge+=dt;
            if(TryGetComponent<RailSystem>(out var rail)&&rail.Riding){Grounded=false;PredictedLanding=default;UpdateMotion(dt,rail);return;}
            Contacts.Sample(Body,config);
            contactLock=Mathf.Max(0,contactLock-dt);
            Grounded=Contacts.Grounded && contactLock<=0 && !Bailed;
            if(Bailed){PredictedLanding=default;UpdateMotion(dt,rail);return;}
            if(Input.pop){bufferedPop=config.popBuffer;Input.pop=false;}
            bufferedPop=Mathf.Max(0,bufferedPop-dt);sinceGrounded=Grounded?0:sinceGrounded+dt;
            if(bufferedPop>0 && (Grounded||sinceGrounded<=config.popBuffer))
            {
                Body.AddForce(SupportNormal*(config.popImpulse+Speed*config.popSpeedScale),ForceMode.VelocityChange);
                contactLock=trickConfig?trickConfig.takeoffContactLock:.18f; Grounded=false; bufferedPop=0;
                popAge=0;
            }
            if(trickConfig)
            {
                if(previousGrounded && !Grounded){Air.Begin(Body,Input,trickConfig,preload);preload=Vector3.zero;airTime=0;TookOff?.Invoke();}
                if(!previousGrounded && Grounded && airTime>.1f)
                {
                    LastLanding=LandingSystem.Evaluate(Body.rotation,Body.linearVelocity,Body.angularVelocity,Contacts.Normal,Contacts.left.hit&&Contacts.right.hit,trickConfig);
                    Compression=Mathf.Clamp01(LastLanding.impact/trickConfig.cleanImpact);
                    balanceRecovery=LastLanding.quality==LandingQuality.Sketchy?1:0;
                    Bailed=LastLanding.quality==LandingQuality.Bail;
                    if(!Bailed)Body.linearVelocity=Vector3.ProjectOnPlane(Body.linearVelocity,Contacts.Normal)*LastLanding.retention;
                    Landed?.Invoke(LastLanding); if(Bailed){UpdateMotion(dt,rail);return;}
                }
                if(!Grounded)
                {
                    airTime+=dt;Air.Step(Body,Input,trickConfig,dt);
                    PredictedLanding=LandingSystem.Predict(Body.position,Body.linearVelocity,config.contactReach,config.probeRadius,trickConfig);
                    if(Physics.Raycast(Body.position,Vector3.down,out var landingHit,trickConfig.assistHeight,~(1<<8),QueryTriggerInteraction.Ignore)){LandingSystem.Assist(Body,landingHit.normal,landingHit.distance,trickConfig,dt);Air.SetMomentum(Body.angularVelocity*Air.Inertia);}
                    Air.SpotLanding(Body,Input,PredictedLanding,trickConfig,dt);
                }
                else
                {
                    PredictedLanding=default;
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
                if(Input.brake&&Speed>2){var travel=Vector3.ProjectOnPlane(Body.linearVelocity,normal).normalized;var stopped=Quaternion.AngleAxis(80*(Input.steer<0?-1:1),normal)*travel;forward=Vector3.RotateTowards(forward,stopped,config.brakeRotationRate*dt,0);}
                var side=Vector3.Cross(normal,forward);
                float longitudinal=Vector3.Dot(Body.linearVelocity,forward), lateral=Vector3.Dot(Body.linearVelocity,side);
                float support=(config.rideHeight-Contacts.Height)*config.supportSpring-Vector3.Dot(Body.linearVelocity,normal)*config.supportDamping-Vector3.Dot(Physics.gravity,normal);
                Body.AddForce(normal*Mathf.Max(0,support),ForceMode.Acceleration);
                float grip=Mathf.Lerp(config.flatGrip,config.edgeGrip,Mathf.Abs(Edge));
                if(Input.brake)grip=config.skidGrip;
                if(Surface==SurfaceType.Powder)grip*=.65f;
                if(Surface==SurfaceType.Ice)grip*=.4f;
                float lateralAccel=Mathf.Clamp(-lateral*grip,-config.maximumGrip,config.maximumGrip);
                Body.AddForce(side*lateralAccel,ForceMode.Acceleration);
                float friction=CarvingSystem.Friction(Surface,config)*Physics.gravity.magnitude;
                if(Mathf.Abs(longitudinal)>.05f)Body.AddForce(-forward*Mathf.Sign(longitudinal)*Mathf.Min(friction,Mathf.Abs(longitudinal)/dt),ForceMode.Acceleration);
                if(Surface==SurfaceType.Powder)Body.AddForce(-Vector3.ProjectOnPlane(Body.linearVelocity,normal)*config.powderDrag,ForceMode.Acceleration);
                if(Input.brake && Speed>.05f)Body.AddForce(-Body.linearVelocity.normalized*Mathf.Min(config.brakeDeceleration,Speed/dt),ForceMode.Acceleration);
                float yawRate=Mathf.Abs(Edge)<.02f ? 0 : Mathf.Abs(longitudinal)/CarvingSystem.Radius(Mathf.Abs(longitudinal),Edge,config)*Mathf.Sign(Edge);
                var target=Quaternion.AngleAxis(yawRate*Mathf.Rad2Deg*dt,normal)*Quaternion.LookRotation(forward,normal);
                Body.MoveRotation(target);
                Body.angularVelocity=Vector3.zero;
                Slip=Mathf.Abs(lateral);
                safeTime+=dt;
                if(safeTime>config.safeRecordInterval && Speed<config.safeSpeed) { safeTime=0; safePosition=Body.position; safeRotation=Body.rotation; }
            }
            if(visual)visual.localRotation=Quaternion.Euler(0,0,-Edge*config.maximumLean);
            if(Body.position.y<config.resetDepth)ResetTo(startPosition,startRotation);
            UpdateMotion(dt,rail);
        }
        void UpdateMotion(float dt,RailSystem rail)
        {
            float response=1-Mathf.Exp(-config.normalResponse*dt);
            var velocity=Body.isKinematic&&rail&&rail.Riding?rail.Velocity:Body.linearVelocity;
            float load=hasVelocitySample?Vector3.Dot((velocity-lastVelocity)/Mathf.Max(.001f,dt),Body.rotation*Vector3.right):0;
            lateralLoad=Mathf.Lerp(lateralLoad,Grounded?Mathf.Clamp(load,-config.maximumGrip,config.maximumGrip):0,response);
            lastVelocity=velocity;hasVelocitySample=true;
            motionGrounded=Mathf.Lerp(motionGrounded,Grounded?1:0,response);
            balanceRecovery=Mathf.MoveTowards(balanceRecovery,0,dt);
            var angular=Body.angularVelocity;
            float preparation=PredictedLanding.valid&&trickConfig?1-Mathf.Clamp01(PredictedLanding.time/Mathf.Max(.01f,trickConfig.landingPrepareTime)):0;
            Motion=new SkierMotionState{
                speed=velocity.magnitude,normalizedSpeed=Mathf.Clamp01(velocity.magnitude/Mathf.Sqrt(config.speedRadiusScale)),verticalSpeed=velocity.y,
                groundedAmount=motionGrounded,slopeAngle=Vector3.Angle(SupportNormal,Vector3.up),lateralAcceleration=lateralLoad,
                carveAmount=Grounded?Edge*Mathf.Lerp(.45f,1,Mathf.Clamp01(Mathf.Abs(lateralLoad)/(config.maximumGrip*.4f))):0,
                edgeAmount=Edge,skidAmount=Grounded?Mathf.Max(Input.brake?1:0,Mathf.Clamp01(Slip/Mathf.Max(2,Speed)*3)):0,
                crouchAmount=Input.crouch?1:Input.tuck?1:0,airborneAmount=!Grounded&&!(rail&&rail.Riding)?1:0,airTime=airTime,
                yawAngularVelocity=Vector3.Dot(angular,Vector3.up),pitchAngularVelocity=Vector3.Dot(angular,Body.rotation*Vector3.right),rollAngularVelocity=Vector3.Dot(angular,Body.rotation*Vector3.forward),
                grabAmount=TryGetComponent<GrabSystem>(out var grab)&&grab.Current!=GrabType.None?1:0,railAmount=rail&&rail.Riding?1:0,
                landingPrediction=preparation,landing=PredictedLanding,landingCompression=Compression,impactStrength=LastLanding.impact,
                switchAmount=Vector3.Dot(Body.rotation*Vector3.forward,velocity)<0?1:0,balanceAmount=rail&&rail.Riding?rail.Balance:balanceRecovery,
                popAmount=TryGetComponent<SkierPose>(out var pose)?1-Mathf.Clamp01(popAge/pose.Visuals.popDuration):0
            };
        }
        public void EnterAirWithoutRestartingTrick(){contactLock=trickConfig.takeoffContactLock;previousGrounded=false;Grounded=false;airTime=0;popAge=0;Air.Begin(Body,Input,trickConfig,Vector3.zero);}
        public void ResetTo(Vector3 position,Quaternion rotation)
        {
            Body.isKinematic=false;Body.position=position; Body.rotation=rotation; Body.linearVelocity=Body.angularVelocity=Vector3.zero;
            SupportNormal=rotation*Vector3.up; Edge=Slip=Compression=0;Grounded=false;Bailed=false;PowderOverride=false;bufferedPop=0;sinceGrounded=100;contactLock=0;airTime=0;preload=Vector3.zero;previousGrounded=false;
            PredictedLanding=default;Motion=default;hasVelocitySample=false;lateralLoad=motionGrounded=balanceRecovery=0;popAge=100;Air.ResetState();ResetPerformed?.Invoke();
        }
        void OnCollisionEnter(Collision collision){if(!Bailed&&collision.relativeVelocity.magnitude>(GetComponentInParent<GameFlow>()?.config.collisionBailSpeed??9)&&collision.contactCount>0&&Mathf.Abs(collision.GetContact(0).normal.y)<.45f)GetComponent<BailSystem>()?.Crash();}
        void Update() { if(Input && Input.reset)ResetTo(safePosition,safeRotation); }
    }
}
