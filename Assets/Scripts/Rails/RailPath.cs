using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    public class RailPath : MonoBehaviour
    {
        public static readonly List<RailPath> Active=new List<RailPath>();
        public string railId="Flat"; public float width=.12f;public Vector3[] points={Vector3.zero,Vector3.forward*10};
        public float Length { get { float total=0;for(int i=1;i<points.Length;i++)total+=Vector3.Distance(points[i-1],points[i]);return total; } }
        void OnEnable(){Active.Add(this);}void OnDisable(){Active.Remove(this);}
        public Vector3 Evaluate(float distance,out Vector3 tangent)
        {
            tangent=transform.forward;
            for(int i=1;i<points.Length;i++){var a=transform.TransformPoint(points[i-1]);var b=transform.TransformPoint(points[i]);float length=Vector3.Distance(a,b);tangent=(b-a).normalized;if(distance<=length)return a+tangent*Mathf.Max(0,distance);distance-=length;}
            return transform.TransformPoint(points[points.Length-1]);
        }
        public float Closest(Vector3 position,out Vector3 point,out Vector3 tangent)
        {
            float best=float.MaxValue,bestDistance=0,cumulative=0;point=transform.position;tangent=transform.forward;
            for(int i=1;i<points.Length;i++)
            {
                Vector3 a=transform.TransformPoint(points[i-1]),b=transform.TransformPoint(points[i]),delta=b-a;
                float length=delta.magnitude,t=Mathf.Clamp01(Vector3.Dot(position-a,delta)/Mathf.Max(.0001f,delta.sqrMagnitude));var p=a+delta*t;float d=(position-p).sqrMagnitude;
                if(d<best){best=d;point=p;tangent=delta.normalized;bestDistance=cumulative+t*length;}cumulative+=length;
            }
            return bestDistance;
        }
        void OnDrawGizmosSelected(){Gizmos.color=Color.yellow;for(int i=1;i<points.Length;i++)Gizmos.DrawLine(transform.TransformPoint(points[i-1]),transform.TransformPoint(points[i]));}
    }
}
