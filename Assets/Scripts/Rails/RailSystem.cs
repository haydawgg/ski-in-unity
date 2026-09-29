using UnityEngine;
namespace PowderFlow
{
    [DefaultExecutionOrder(-20)]
    public class RailSystem : MonoBehaviour
    {
        public bool Riding { get; private set; }public float Balance { get; private set; } public RailPath Current { get; private set; }
        SkiPhysicsController skier;TrickTracker tracker;float distance,speed,direction,cooldown,yawVelocity;
        void Start(){skier=GetComponent<SkiPhysicsController>();tracker=GetComponent<TrickTracker>();skier.ResetPerformed+=Clear;}
        void FixedUpdate()
        {
            if(!skier||!skier.trickConfig||skier.Bailed)return;
            var c=skier.trickConfig;float dt=Time.fixedDeltaTime;cooldown-=dt;
            if(!Riding)
            {
                if(cooldown>0||skier.Speed<c.railMinimumSpeed||skier.Body.linearVelocity.y>c.railMaximumVerticalSpeed)return;
                foreach(var path in RailPath.Active)
                {
                    var sample=skier.Body.position-Vector3.up*skier.config.rideHeight;
                    float at=path.Closest(sample,out var point,out var captureTangent);float d=Vector3.Distance(sample,point);
                    if(d>c.railCaptureDistance+path.width*.5f || sample.y<point.y-.3f || Vector3.Angle(skier.transform.up,Vector3.up)>60)continue;
                    float along=Vector3.Dot(skier.Body.linearVelocity,captureTangent);if(Mathf.Abs(along)<c.railMinimumSpeed)continue;
                    Current=path;distance=at;direction=Mathf.Sign(along);speed=Mathf.Abs(along);Balance=0;Riding=true;skier.Body.isKinematic=true;
                    if(!tracker.tracking)tracker.Begin();tracker.current.rail=path.railId;break;
                }
                return;
            }
            speed=Mathf.Max(0,speed+(Vector3.Dot(Physics.gravity,Current.Evaluate(distance,out var tangent)-Current.Evaluate(distance-.01f,out _))/.01f*direction-c.railFriction)*dt);
            distance+=speed*direction*dt;
            Current.Evaluate(distance,out tangent);
            float mismatch=Mathf.Abs(Vector3.Dot(skier.transform.right,tangent));
            Balance+=((mismatch-.5f)*c.railInstability-skier.Input.roll*c.railBalanceResponse)*dt;
            Balance=Mathf.Clamp(Balance,-1.2f,1.2f);
            yawVelocity=Mathf.Lerp(yawVelocity,skier.Input.steer*3,dt*3);
            skier.Body.MoveRotation(Quaternion.AngleAxis(yawVelocity*Mathf.Rad2Deg*dt,Vector3.up)*skier.Body.rotation);
            var pointOnRail=Current.Evaluate(distance,out tangent)+Vector3.up*skier.config.rideHeight;
            skier.Body.MovePosition(pointOnRail);tracker.current.railTime+=dt;
            if(skier.Input.pop || distance<=0 || distance>=Current.Length || speed<c.railMinimumSpeed)
            {
                skier.Input.pop=false;Exit(tangent*speed*direction+Vector3.up*c.railPop);
            }
            else if(Mathf.Abs(Balance)>1){Exit(tangent*speed*direction);skier.GetComponent<BailSystem>().Crash();}
        }
        void Exit(Vector3 velocity)
        {
            Riding=false;cooldown=.5f;Current=null;skier.Body.isKinematic=false;skier.Body.linearVelocity=velocity;skier.Body.angularVelocity=Vector3.up*yawVelocity;
            skier.EnterAirWithoutRestartingTrick();
        }
        void Clear(){Riding=false;Current=null;cooldown=0;if(skier)skier.Body.isKinematic=false;}
    }
}
