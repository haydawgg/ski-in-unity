using UnityEngine;
namespace PowderFlow
{
    public struct SkiContact
    {
        public bool hit,rawHit; public int sampleCount; public Vector3 point, normal,rawNormal; public float height,rawHeight,confidence; public SurfaceType surface;
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
                    // Sphere sweeps return a separation normal at triangle edges.
                    // Read the same collider's face beneath the probe, without
                    // extending reach or retaining a previous tick's contact.
                    var rawNormal=hit.normal;float rawHeight=Vector3.Dot(position-hit.point,rawNormal);
                    if(c.refineContactSurface&&SurfaceFace(hit,origin,forward,c,out var face))
                    {hit.point=face.point;hit.normal=face.normal;}
                    float height=Vector3.Dot(position-hit.point,hit.normal);
                    if(height>c.contactReach || height<-.3f) continue;
                    result.point+=hit.point; result.normal+=hit.normal; result.height+=height;result.rawHeight+=rawHeight;result.rawNormal+=rawNormal;count++;
                    var surface=hit.collider.GetComponentInParent<SnowSurface>();
                    result.surface=surface ? surface.type : SurfaceType.Groomed;
                }
            }
            result.sampleCount=count;
            if(count>0) { result.hit=result.rawHit=true; result.point/=count; result.normal.Normalize(); result.height/=count;result.rawHeight/=count;result.rawNormal.Normalize();result.confidence=count*.5f; }
            return result;
        }
        static bool ValidFace(RaycastHit face,RaycastHit sweep,SkiPhysicsConfig c)=>Vector3.Distance(face.point,sweep.point)<c.probeRadius*3&&face.normal.y>.1f;
        static bool SurfaceFace(RaycastHit sweep,Vector3 origin,Vector3 forward,SkiPhysicsConfig c,out RaycastHit face)
        {
            if(sweep.collider.Raycast(new Ray(origin,Vector3.down),out face,c.probeRange+c.probeRadius)&&ValidFace(face,sweep,c))return true;
            // At an open lip the sphere still touches the edge while its center
            // has passed it. Query the face at that existing contact, then just
            // inside the edge. This never creates or prolongs a sweep hit.
            for(int i=0;i<3;i++)
            {
                float inset=i==0?0:i==1?-c.probeRadius*.5f:c.probeRadius*.5f;
                if(sweep.collider.Raycast(new Ray(sweep.point+Vector3.up*c.probeLift+forward*inset,Vector3.down),out face,c.probeLift+c.probeRadius*3)&&ValidFace(face,sweep,c))return true;
            }
            return false;
        }
        void OnDrawGizmosSelected()
        {
            Gizmos.color=Color.cyan;
            if(left.hit) { Gizmos.DrawSphere(left.point,.07f); Gizmos.DrawRay(left.point,left.normal); }
            if(right.hit) { Gizmos.DrawSphere(right.point,.07f); Gizmos.DrawRay(right.point,right.normal); }
        }
    }
}
