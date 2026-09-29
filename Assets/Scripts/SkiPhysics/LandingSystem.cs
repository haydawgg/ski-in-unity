using UnityEngine;
namespace PowderFlow
{
    public enum LandingQuality { Perfect,Clean,Sketchy,Bail }
    public struct LandingResult { public LandingQuality quality; public float impact,alignment,upright,slope,retention; public bool switched; }
    public static class LandingSystem
    {
        public static LandingResult Evaluate(Quaternion rotation,Vector3 velocity,Vector3 angularVelocity,Vector3 normal,bool twoContacts,TrickConfig c)
        {
            Vector3 forward=Vector3.ProjectOnPlane(rotation*Vector3.forward,normal).normalized;
            Vector3 travel=Vector3.ProjectOnPlane(velocity,normal).normalized;
            float facing=Vector3.Angle(forward,travel),alignment=Mathf.Min(facing,180-facing),upright=Vector3.Angle(rotation*Vector3.up,normal);
            float impact=Mathf.Max(0,-Vector3.Dot(velocity,normal));
            var q=LandingQuality.Clean;
            if(upright>c.uprightBailAngle || alignment>c.sketchyAlignment || impact>c.bailImpact || angularVelocity.magnitude>c.bailAngularSpeed)q=LandingQuality.Bail;
            else if(alignment<c.perfectAlignment && upright<c.perfectAlignment && impact<c.perfectImpact && twoContacts)q=LandingQuality.Perfect;
            else if(alignment>c.cleanAlignment || upright>c.cleanAlignment || impact>c.cleanImpact || !twoContacts)q=LandingQuality.Sketchy;
            float slope=Vector3.Angle(Vector3.up,normal);
            float retention=q==LandingQuality.Perfect?c.perfectRetention:q==LandingQuality.Clean?c.cleanRetention:q==LandingQuality.Sketchy?c.sketchyRetention:0;
            retention*=1-c.flatImpactScrub*Mathf.Clamp01(impact/c.cleanImpact)*Mathf.Clamp01(1-slope/30);
            return new LandingResult{quality=q,impact=impact,alignment=alignment,upright=upright,slope=slope,retention=retention,switched=facing>90};
        }
        public static void Assist(Rigidbody body,Vector3 normal,float height,TrickConfig c,float dt)
        {
            if(height>c.assistHeight || Vector3.Dot(body.linearVelocity,normal)>0)return;
            if(Vector3.Angle(body.rotation*Vector3.up,normal)>c.assistAngle)return;
            Vector3 travel=Vector3.ProjectOnPlane(body.linearVelocity,normal).normalized;
            if(travel.sqrMagnitude<.1f)return;
            if(Vector3.Dot(body.rotation*Vector3.forward,travel)<0)travel=-travel;
            var target=Quaternion.LookRotation(travel,normal);
            if(Quaternion.Angle(body.rotation,target)>c.assistAngle)return;
            body.MoveRotation(Quaternion.Slerp(body.rotation,target,c.assistResponse*dt));
            body.angularVelocity*=Mathf.Exp(-c.landingAngularDamping*dt);
        }
    }
}
