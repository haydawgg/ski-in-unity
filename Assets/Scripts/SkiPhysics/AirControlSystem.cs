using UnityEngine;
namespace PowderFlow
{
    public class AirControlSystem : MonoBehaviour
    {
        public Vector3 AngularMomentum { get; private set; }
        public float AirTime { get; private set; }
        public float Inertia { get; private set; }
        public void Begin(Rigidbody body,SkierInput input,TrickConfig config,Vector3 preload)
        {
            AirTime=0;Inertia=config.baseInertia;
            AngularMomentum=(body.angularVelocity+preload)*Inertia;
        }
        public void SetMomentum(Vector3 momentum) { AngularMomentum=momentum; }
        public void Step(Rigidbody body,SkierInput input,TrickConfig c,float dt)
        {
            AirTime+=dt;
            float tucked=input.tuck||input.crouch||input.grabLeft||input.grabRight ? 1:0;
            Inertia=c.baseInertia*(1-c.tuckInertiaReduction*tucked);
            Vector3 torque=Vector3.up*input.steer*c.yawTorque + body.rotation*Vector3.right*input.flip*c.pitchTorque + body.rotation*Vector3.forward*(input.roll+(input.modifierRight?input.steer*.55f:0))*c.rollTorque;
            AngularMomentum=(AngularMomentum+torque*dt)*Mathf.Exp(-c.angularDrag*dt);
            AngularMomentum=Vector3.ClampMagnitude(AngularMomentum,c.maximumAngularSpeed*Inertia);
            body.angularVelocity=AngularMomentum/Inertia;
        }
    }
}
