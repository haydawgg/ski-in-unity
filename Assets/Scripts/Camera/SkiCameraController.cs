using UnityEngine;
namespace PowderFlow
{
    public class SkiCameraController:MonoBehaviour
    {
        public SkiPhysicsController skier;public CameraConfig config;
        Vector3 springVelocity,heading=Vector3.forward;Camera lens;RailSystem rail;BailSystem bail;
        float orbitYaw,orbitPitch,shake,displayFov;bool menu,titleView;
        readonly RaycastHit[] obstructions=new RaycastHit[64];
        void Awake(){lens=GetComponent<Camera>();displayFov=lens.fieldOfView;}
        void Start(){if(skier){rail=skier.GetComponent<RailSystem>();bail=skier.GetComponent<BailSystem>();skier.ResetPerformed+=Snap;skier.Landed+=Landing;Snap();}}
        void OnDestroy(){if(skier){skier.ResetPerformed-=Snap;skier.Landed-=Landing;}}
        public void SetMenu(bool visible,bool title){menu=visible;titleView=visible&&title;}
        void Landing(LandingResult result){if(result.quality!=LandingQuality.Bail)shake=config.landingShake*Mathf.Clamp01(result.impact/15);}
        Vector3 Focus=>skier.Bailed&&bail&&bail.CameraTarget?bail.CameraTarget.position:skier.Body.position;
        void Snap()
        {
            heading=Vector3.ProjectOnPlane(skier.Body.linearVelocity.sqrMagnitude>4?skier.Body.linearVelocity:skier.transform.forward,Vector3.up).normalized;if(heading.sqrMagnitude<.1f)heading=Vector3.forward;
            springVelocity=Vector3.zero;orbitYaw=orbitPitch=shake=0;
            var focus=Focus+Vector3.up*config.lookHeight;transform.position=ClearPosition(focus,focus-heading*config.distance+Vector3.up*config.height);transform.rotation=Quaternion.LookRotation(focus-transform.position,Vector3.up);
        }
        Vector3 ClearPosition(Vector3 focus,Vector3 candidate)
        {
            var delta=candidate-focus;float distance=delta.magnitude;if(distance<.01f)return candidate;
            int count=Physics.SphereCastNonAlloc(focus,config.obstructionRadius,delta/distance,obstructions,distance,~((1<<8)|(1<<30)),QueryTriggerInteraction.Ignore);float nearest=distance;
            for(int i=0;i<count;i++)
            {
                var hit=obstructions[i];if(bail&&bail.CameraTarget&&hit.collider.transform.IsChildOf(bail.CameraTarget.root))continue;
                nearest=Mathf.Min(nearest,Mathf.Max(.05f,hit.distance-config.obstructionPadding));
            }
            return focus+delta/distance*nearest;
        }
        void LateUpdate()
        {
            if(!skier||!config)return;lens.nearClipPlane=config.nearClip;
            if(titleView)
            {
                transform.position=skier.startPosition+config.menuOffset;transform.LookAt(skier.startPosition+config.menuLook,Vector3.up);lens.fieldOfView=config.menuFov;return;
            }
            if(menu)return;
            float dt=Time.deltaTime;var planar=Vector3.ProjectOnPlane(skier.Body.linearVelocity,Vector3.up);
            if(!skier.Bailed&&planar.sqrMagnitude>4)heading=Vector3.Slerp(heading,planar.normalized,1-Mathf.Exp(-config.directionResponse*dt));
            float speed=Mathf.Clamp01(skier.Speed/config.speedReference);bool riding=rail&&rail.Riding,air=!skier.Grounded&&!riding&&!skier.Bailed;
            float distance=config.distance+speed*config.speedPullback+(air?config.airPullback:riding?config.railPullback:skier.Bailed?config.bailPullback:0);
            if(lens.aspect<1)distance*=config.portraitPullback;
            var mouse=UnityEngine.InputSystem.Mouse.current;
            if(mouse!=null&&mouse.rightButton.isPressed){var d=mouse.delta.ReadValue()*config.cameraSensitivity*.15f;orbitYaw+=d.x;orbitPitch=Mathf.Clamp(orbitPitch+d.y*(SaveStore.Current.invertCamera?-1:1),-30,45);}
            else {orbitYaw=Mathf.Lerp(orbitYaw,0,dt*2);orbitPitch=Mathf.Lerp(orbitPitch,0,dt*2);}
            var focus=Focus+Vector3.up*config.lookHeight;var slope=skier.Grounded?Vector3.ProjectOnPlane(heading,skier.SupportNormal).normalized:heading;
            var direction=Quaternion.Euler(orbitPitch,orbitYaw+(skier.Bailed?config.bailOrbit:0),0)*slope;
            float height=skier.Bailed?config.bailHeight:air?config.airHeight:riding?config.railHeight:config.height;
            var desired=ClearPosition(focus,focus-direction*distance+Vector3.up*height);
            var smoothed=Vector3.SmoothDamp(transform.position,desired,ref springVelocity,config.followDamping);
            var clear=ClearPosition(focus,smoothed);if((clear-smoothed).sqrMagnitude>.0025f)springVelocity=Vector3.zero;
            var aim=focus+heading*(air?config.airLookAhead:riding?config.railLookAhead:config.lookAhead)*speed;
            if(air&&Physics.Raycast(skier.Body.position+planar*config.landingLookSeconds+Vector3.up*20,Vector3.down,out var ground,120,~((1<<8)|(1<<30)),QueryTriggerInteraction.Ignore))aim.y+=Mathf.Clamp(ground.point.y-skier.Body.position.y,-2,0)*config.landingAnticipation;
            shake=Mathf.MoveTowards(shake,0,dt*config.shakeRecovery);clear+=Vector3.up*Mathf.Sin(Time.time*43)*shake;transform.position=ClearPosition(focus,clear);
            transform.rotation=Quaternion.LookRotation(aim-transform.position,Vector3.up);
            displayFov=Mathf.Lerp(displayFov,Mathf.Lerp(config.normalFov,config.speedFov,speed),1-Mathf.Exp(-config.fovResponse*dt));lens.fieldOfView=displayFov;
        }
    }
}
