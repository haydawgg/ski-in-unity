using UnityEngine;
namespace PowderFlow
{
    public struct SkiContact
    {
        public bool hit; public Vector3 point, normal; public float height; public SurfaceType surface;
    }
    public class SkiContactSystem : MonoBehaviour
    {
        public SkiContact left, right;
        public bool Grounded => left.hit||right.hit;
        public Vector3 Normal { get; private set; }=Vector3.up;
        public float Height { get; private set; }
        public SurfaceType Surface => left.hit ? left.surface : right.surface;
        public void Sample(Rigidbody body, SkiPhysicsConfig c)
        {
            var forward=Vector3.ProjectOnPlane(body.rotation*Vector3.forward,Vector3.up).normalized;
            if(forward.sqrMagnitude<.1f) forward=Vector3.forward;
            var side=Vector3.Cross(Vector3.up,forward);
            left=Probe(body.position-side*c.skiSeparation*.5f,forward,c);
            right=Probe(body.position+side*c.skiSeparation*.5f,forward,c);
            if(Grounded)
            {
                Normal=(left.hit&&right.hit ? left.normal+right.normal : left.hit?left.normal:right.normal).normalized;
                Height=left.hit&&right.hit ? (left.height+right.height)*.5f : left.hit?left.height:right.height;
            }
        }
        SkiContact Probe(Vector3 position,Vector3 forward,SkiPhysicsConfig c)
        {
            SkiContact result=default; int count=0;
            for(int i=-1;i<=1;i+=2)
            {
                var origin=position+forward*c.contactHalfLength*i+Vector3.up*c.probeLift;
                if(Physics.SphereCast(origin,c.probeRadius,Vector3.down,out var hit,c.probeRange,~(1<<8),QueryTriggerInteraction.Ignore))
                {
                    float height=Vector3.Dot(position-hit.point,hit.normal);
                    if(height>c.contactReach || height<-.3f) continue;
                    result.point+=hit.point; result.normal+=hit.normal; result.height+=height; count++;
                    var surface=hit.collider.GetComponentInParent<SnowSurface>();
                    result.surface=surface ? surface.type : SurfaceType.Groomed;
                }
            }
            if(count>0) { result.hit=true; result.point/=count; result.normal.Normalize(); result.height/=count; }
            return result;
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color=Color.cyan;
            if(left.hit) { Gizmos.DrawSphere(left.point,.07f); Gizmos.DrawRay(left.point,left.normal); }
            if(right.hit) { Gizmos.DrawSphere(right.point,.07f); Gizmos.DrawRay(right.point,right.normal); }
        }
    }
}
