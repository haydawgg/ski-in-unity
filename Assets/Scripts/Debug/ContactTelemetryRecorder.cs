using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UnityEngine;
namespace PowderFlow
{
    // Opt-in recorder for the lab and development reviews; absent from ordinary runs.
    [DefaultExecutionOrder(100)]
    public class ContactTelemetryRecorder:MonoBehaviour
    {
        public SkiPhysicsController skier;
        readonly List<string> rows=new List<string>();float time;Vector3 previousVelocity;
        public void Begin(SkiPhysicsController source)
        {
            skier=source;time=0;previousVelocity=source.Body.linearVelocity;rows.Clear();
            rows.Add("time,x,y,z,leftRaw,rightRaw,left,right,leftSamples,rightSamples,leftRawHeight,rightRawHeight,leftHeight,rightHeight,height,rawNx,rawNy,rawNz,filteredNx,filteredNy,filteredNz,supportNx,supportNy,supportNz,supportAcceleration,verticalVelocity,normalVelocity,grounded,transitions,speed,lateralAcceleration,rail,bailed");
        }
        static string F(float value)=>value.ToString("F6",CultureInfo.InvariantCulture);
        void FixedUpdate(){if(skier)Record(Time.fixedDeltaTime);}
        public void Record(float dt)
        {
            var c=skier.Contacts;var a=c.left;var b=c.right;var pos=skier.Body.position;var velocity=skier.Body.isKinematic&&skier.TryGetComponent<RailSystem>(out var rail)?rail.Velocity:skier.Body.linearVelocity;
            var raw=(a.rawHit&&b.rawHit?a.rawNormal+b.rawNormal:a.rawHit?a.rawNormal:b.rawNormal).normalized;var n=c.Normal;var sn=skier.SupportNormal;
            float lateral=Vector3.Dot((velocity-previousVelocity)/dt,skier.Body.rotation*Vector3.right);previousVelocity=velocity;
            rows.Add(string.Join(",",F(time),F(pos.x),F(pos.y),F(pos.z),a.rawHit?1:0,b.rawHit?1:0,a.hit?1:0,b.hit?1:0,a.sampleCount,b.sampleCount,F(a.rawHeight),F(b.rawHeight),F(a.height),F(b.height),F(c.Height),F(raw.x),F(raw.y),F(raw.z),F(n.x),F(n.y),F(n.z),F(sn.x),F(sn.y),F(sn.z),F(skier.SupportAcceleration),F(velocity.y),F(Vector3.Dot(velocity,sn)),skier.Grounded?1:0,skier.GroundedTransitions,F(velocity.magnitude),F(lateral),skier.Body.isKinematic?1:0,skier.Bailed?1:0));time+=dt;
        }
        public void Save(string path){Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllLines(path,rows);skier=null;}
    }
}
