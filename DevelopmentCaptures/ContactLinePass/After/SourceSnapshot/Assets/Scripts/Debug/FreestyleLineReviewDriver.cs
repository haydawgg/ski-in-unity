using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace PowderFlow
{
    // Input-only diagnostic rider. Never moves the body, edits velocity or
    // rescues an attempt. Human controls remain the normal gameplay authority.
    [DefaultExecutionOrder(-30)]
    public class FreestyleLineReviewDriver:MonoBehaviour
    {
        SkiPhysicsController skier;FreestyleLineDefinition line;Vector3[] route;bool previousRail;float began,exitSnowTime;
        public int Resets {get;private set;}public int Launches {get;private set;}public int Landings {get;private set;}
        public readonly HashSet<string> Rails=new HashSet<string>();
        public bool Complete=>skier&&skier.Body.position.z>=line.exit.z&&exitSnowTime>=.35f;
        public bool Failed=>skier&&(skier.Bailed||Resets>0);
        readonly List<string> events=new List<string>();
        public void Begin(SkiPhysicsController source,FreestyleLineDefinition definition,bool alternate)
        {
            skier=source;line=definition;route=alternate?line.alternateRoute:line.mainRoute;began=Time.time;exitSnowTime=0;previousRail=false;Resets=Launches=Landings=0;Rails.Clear();events.Clear();
            events.Add("time,event,x,y,z,speed,quality");skier.ResetPerformed+=OnReset;skier.TookOff+=OnLaunch;skier.Landed+=OnLanding;Event("start","");
        }
        void Event(string name,string quality)
        {
            var p=skier.Body.position;var rail=skier.GetComponent<RailSystem>();float speed=rail&&rail.Riding?rail.Velocity.magnitude:skier.Speed;string F(float n)=>n.ToString("F4",CultureInfo.InvariantCulture);
            events.Add(string.Join(",",F(Time.time-began),name,F(p.x),F(p.y),F(p.z),F(speed),quality));
        }
        void OnReset(){Resets++;Event("reset","");}void OnLaunch(){Launches++;Event("takeoff","");}void OnLanding(LandingResult result){Landings++;Event("landing",result.quality.ToString());}
        void FixedUpdate()
        {
            if(!skier||Failed||Complete)return;
            exitSnowTime=skier.Grounded&&skier.Body.position.z>=line.exit.z?exitSnowTime+Time.fixedDeltaTime:0;
            var input=skier.Input;input.tuck=true;input.steer=input.flip=input.roll=0;
            var rail=skier.GetComponent<RailSystem>();
            if(rail.Riding)
            {
                if(!previousRail){Rails.Add(rail.Current.name);Event("rail:"+rail.Current.name,"");}
                float at=rail.Current.Closest(skier.Body.position-Vector3.up*skier.config.rideHeight,out _,out _);
                if(at>rail.Current.Length*.86f)input.pop=true;
            }
            else if(previousRail)Event("rail-pop","");
            previousRail=rail.Riding;
            if(!skier.Grounded||rail.Riding)return;
            var position=skier.Body.position;float lookahead=Mathf.Clamp(skier.Speed*.65f,9,16),z=position.z+lookahead,x=route[route.Length-1].x;
            Vector3 prior=line.start;
            foreach(var point in route)
            {
                if(z<=point.z){x=Mathf.Lerp(prior.x,point.x,Mathf.InverseLerp(prior.z,point.z,z));break;}prior=point;
            }
            var desired=new Vector3(x-position.x,0,lookahead).normalized;
            var heading=Vector3.ProjectOnPlane(skier.Body.rotation*Vector3.forward,Vector3.up).normalized;
            float error=Vector3.SignedAngle(heading,desired,Vector3.up)*Mathf.Deg2Rad;
            input.steer=Mathf.Clamp(error*2.8f,-1,1);
        }
        public void Save(string path){Event(Complete?"exit":"incomplete",Failed?"failed":"");Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllLines(path,events);}
        void OnDestroy(){if(skier){skier.ResetPerformed-=OnReset;skier.TookOff-=OnLaunch;skier.Landed-=OnLanding;}}
    }
}
