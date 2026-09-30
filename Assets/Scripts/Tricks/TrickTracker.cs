using System;
using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    [Serializable] public class TrickRecord
    {
        public float yaw,pitch,roll,airtime,takeoffSpeed,grabTime,railTime,tweak;
        public bool switchTakeoff,switchLanding;public string grab="",rail="";public LandingQuality landing;
    }
    [DefaultExecutionOrder(100)]
    public class TrickTracker : MonoBehaviour
    {
        public TrickRecord current=new TrickRecord(); public bool tracking;
        SkiPhysicsController skier;Quaternion previous;
        public event Action<TrickRecord> Completed;
        void Start()
        {
            skier=GetComponent<SkiPhysicsController>();skier.TookOff+=Begin;skier.Landed+=Finish;
            skier.ResetPerformed+=Clear;
        }
        void Clear(){tracking=false;current=new TrickRecord();previous=skier.Body.rotation;}
        public void Begin()
        {
            current=new TrickRecord{takeoffSpeed=skier.Speed,switchTakeoff=Vector3.Dot(skier.transform.forward,skier.Body.linearVelocity)<0};tracking=true;previous=skier.Body.rotation;
        }
        void FixedUpdate()
        {
            if(!tracking)return;
            TrackRotation(skier.Body.rotation);current.airtime+=Time.fixedDeltaTime;
            if(skier.Input.modifierLeft||skier.Input.modifierRight)current.tweak+=Time.fixedDeltaTime;
        }
        public void TrackRotation(Quaternion rotation)
        {
            var delta=rotation*Quaternion.Inverse(previous);delta.ToAngleAxis(out float angle,out Vector3 axis);
            if(angle>180)angle-=360;
            if(!float.IsNaN(axis.x)&&Mathf.Abs(angle)<90)
            {
                current.yaw+=angle*Vector3.Dot(axis,Vector3.up);
                current.pitch+=angle*Vector3.Dot(axis,previous*Vector3.right);
                current.roll+=angle*Vector3.Dot(axis,previous*Vector3.forward);
            }
            previous=rotation;
        }
        void Finish(LandingResult result)
        {
            if(!tracking)return;tracking=false;current.landing=result.quality;current.switchLanding=result.switched;Completed?.Invoke(current);
        }
        public static int Quantize(float degrees,float tolerance,float step=180) => Mathf.FloorToInt((Mathf.Abs(degrees)+tolerance)/step)*Mathf.RoundToInt(step);
        public static string Name(TrickRecord r,TrickConfig c)
        {
            int spin=Quantize(r.yaw,c.rotationTolerance),flips=Quantize(r.pitch,c.rotationTolerance,360)/360;
            bool cork=Mathf.Abs(r.roll)>90 || (Mathf.Abs(r.pitch)>90&&spin>=180);
            var words=new List<string>();if(r.switchTakeoff)words.Add("Switch");
            if(cork)words.Add("Cork");else if(flips>0)words.Add((flips>1?$"{flips}x ":"")+(r.pitch<0?"Backflip":"Frontflip"));
            if(spin>0)words.Add(spin.ToString());
            if(r.railTime>0)words.Add(r.rail+" Slide");
            if(!string.IsNullOrEmpty(r.grab)&&r.grabTime>.05f)words.Add(r.grab);
            if(words.Count==0)words.Add("Straight Air");if(r.switchLanding&&!r.switchTakeoff)words.Add("to Switch");
            return string.Join(" ",words);
        }
    }
}
