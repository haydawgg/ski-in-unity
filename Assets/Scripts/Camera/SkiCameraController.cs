using UnityEngine;
namespace PowderFlow
{
    public class SkiCameraController : MonoBehaviour
    {
        public SkiPhysicsController skier; public CameraConfig config;
        Vector3 springVelocity, heading=Vector3.forward;
        Camera lens;
        void Awake() { lens=GetComponent<Camera>(); }
        void LateUpdate()
        {
            if(!skier||!config)return;
            var velocity=Vector3.ProjectOnPlane(skier.Body.linearVelocity,Vector3.up);
            if(velocity.sqrMagnitude>4)heading=Vector3.Slerp(heading,velocity.normalized,1-Mathf.Exp(-config.directionResponse*Time.deltaTime));
            else heading=Vector3.Slerp(heading,Vector3.ProjectOnPlane(skier.transform.forward,Vector3.up).normalized,Time.deltaTime);
            float speed=Mathf.Clamp01(skier.Speed/config.speedReference);
            float distance=config.distance+speed*config.speedPullback;
            if(lens.aspect<1)distance*=config.portraitPullback;
            var focus=skier.transform.position+Vector3.up*config.lookHeight;
            var desired=focus-heading*distance+Vector3.up*(skier.Grounded?config.height:config.airHeight);
            if(Physics.SphereCast(focus,config.obstructionRadius,(desired-focus).normalized,out var hit,Vector3.Distance(focus,desired),~(1<<8),QueryTriggerInteraction.Ignore))desired=hit.point+hit.normal*config.obstructionRadius;
            transform.position=Vector3.SmoothDamp(transform.position,desired,ref springVelocity,config.followDamping);
            var aim=focus+heading*config.lookAhead*speed;
            transform.rotation=Quaternion.LookRotation(aim-transform.position,Vector3.up);
            lens.fieldOfView=Mathf.Lerp(config.normalFov,config.speedFov,speed);
        }
    }
}
