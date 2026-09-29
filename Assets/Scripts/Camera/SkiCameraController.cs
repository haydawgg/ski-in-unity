using UnityEngine;
namespace PowderFlow
{
    public class SkiCameraController : MonoBehaviour
    {
        public SkiPhysicsController skier; public CameraConfig config;
        Vector3 springVelocity, heading=Vector3.forward;
        Camera lens;float orbitYaw,orbitPitch;
        void Awake() { lens=GetComponent<Camera>(); }
        void Start(){if(skier)skier.ResetPerformed+=Snap;}
        void OnDestroy(){if(skier)skier.ResetPerformed-=Snap;}
        void Snap(){heading=Vector3.ProjectOnPlane(skier.transform.forward,Vector3.up).normalized;springVelocity=Vector3.zero;transform.position=skier.Body.position-heading*config.distance+Vector3.up*config.height;transform.rotation=Quaternion.LookRotation(skier.Body.position+Vector3.up*config.lookHeight-transform.position,Vector3.up);}
        void LateUpdate()
        {
            if(!skier||!config)return;
            var velocity=Vector3.ProjectOnPlane(skier.Body.linearVelocity,Vector3.up);
            if(velocity.sqrMagnitude>4)heading=Vector3.Slerp(heading,velocity.normalized,1-Mathf.Exp(-config.directionResponse*Time.deltaTime));
            else heading=Vector3.Slerp(heading,Vector3.ProjectOnPlane(skier.transform.forward,Vector3.up).normalized,Time.deltaTime);
            float speed=Mathf.Clamp01(skier.Speed/config.speedReference);
            float distance=config.distance+speed*config.speedPullback;
            if(lens.aspect<1)distance*=config.portraitPullback;
            var mouse=UnityEngine.InputSystem.Mouse.current;if(mouse!=null&&mouse.rightButton.isPressed){var delta=mouse.delta.ReadValue()*config.cameraSensitivity*.15f;orbitYaw+=delta.x;orbitPitch=Mathf.Clamp(orbitPitch+delta.y*(SaveStore.Current.invertCamera?-1:1),-30,45);}else {orbitYaw=Mathf.Lerp(orbitYaw,0,Time.deltaTime*2);orbitPitch=Mathf.Lerp(orbitPitch,0,Time.deltaTime*2);}
            var followDirection=Quaternion.Euler(orbitPitch,orbitYaw,0)*heading;
            var focus=skier.transform.position+Vector3.up*config.lookHeight;
            var desired=focus-followDirection*distance+Vector3.up*(skier.Grounded?config.height:config.airHeight);
            if(Physics.SphereCast(focus,config.obstructionRadius,(desired-focus).normalized,out var hit,Vector3.Distance(focus,desired),~(1<<8),QueryTriggerInteraction.Ignore))desired=hit.point+hit.normal*config.obstructionRadius;
            transform.position=Vector3.SmoothDamp(transform.position,desired,ref springVelocity,config.followDamping);
            var aim=focus+heading*config.lookAhead*speed;
            transform.rotation=Quaternion.LookRotation(aim-transform.position,Vector3.up);
            lens.fieldOfView=Mathf.Lerp(config.normalFov,config.speedFov,speed);
        }
    }
}
