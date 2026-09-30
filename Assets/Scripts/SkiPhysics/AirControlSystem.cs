using UnityEngine;
namespace PowderFlow
{
    public class AirControlSystem : MonoBehaviour
    {
        public Vector3 AngularMomentum { get; private set; }
        public float AirTime { get; private set; }
        public float Inertia { get; private set; }
        public bool HadRotationInput { get; private set; }
        public void Begin(Rigidbody body,SkierInput input,TrickConfig config,Vector3 preload)
        {
            AirTime=0;Inertia=config.baseInertia;HadRotationInput=preload.sqrMagnitude>.01f;
            AngularMomentum=(body.angularVelocity+preload)*Inertia;
        }
        public void SetMomentum(Vector3 momentum) { AngularMomentum=momentum; }
        public void ResetState(){AngularMomentum=Vector3.zero;AirTime=0;HadRotationInput=false;}
        public void SpotLanding(Rigidbody body,SkierInput input,LandingPrediction prediction,TrickConfig c,float dt)
        {
            // Only a player who actually released rotational input asks to spot a landing.
            if(!HadRotationInput||!prediction.valid||prediction.time>c.spottingTime)return;
            if(Mathf.Max(Mathf.Abs(input.steer),Mathf.Abs(input.flip),Mathf.Abs(input.roll))>c.rotationReleaseThreshold)return;
            if(Vector3.Angle(body.rotation*Vector3.up,prediction.normal)>c.assistAngle)return;
            var travel=Vector3.ProjectOnPlane(body.linearVelocity,prediction.normal);
            float facing=Vector3.Angle(body.rotation*Vector3.forward,travel);
            if(Mathf.Min(facing,180-facing)>c.assistAngle)return;
            float weight=1-Mathf.Clamp01(prediction.time/Mathf.Max(.01f,c.spottingTime));
            AngularMomentum*=Mathf.Exp(-c.spottingAngularDamping*weight*dt);
            body.angularVelocity=AngularMomentum/Mathf.Max(.01f,Inertia);
        }
        public void Step(Rigidbody body,SkierInput input,TrickConfig c,float dt)
        {
            AirTime+=dt;
            float tucked=input.tuck||input.crouch||input.grabLeft||input.grabRight ? 1:0;
            Inertia=c.baseInertia*(1-c.tuckInertiaReduction*tucked);
            Vector3 torque=Vector3.up*input.steer*c.yawTorque + body.rotation*Vector3.right*input.flip*c.pitchTorque + body.rotation*Vector3.forward*(input.roll+(input.modifierRight?input.steer*.55f:0))*c.rollTorque;
            if(torque.sqrMagnitude>.01f)HadRotationInput=true;
            AngularMomentum=(AngularMomentum+torque*dt)*Mathf.Exp(-c.angularDrag*dt);
            AngularMomentum=Vector3.ClampMagnitude(AngularMomentum,c.maximumAngularSpeed*Inertia);
            body.angularVelocity=AngularMomentum/Inertia;
        }
    }
}
