using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    public class SnowTrackSystem:MonoBehaviour
    {
        public GraphicsConfig config;SkiPhysicsController skier;
        struct TrackPoint{public Vector3 position,normal;public float time;public bool connected;}
        readonly List<TrackPoint>[] points={new List<TrackPoint>(),new List<TrackPoint>()};Mesh[] meshes=new Mesh[2];bool[] breaks={true,true};float timer;
        void Start()
        {
            skier=GetComponent<SkiPhysicsController>();skier.ResetPerformed+=BreakTrail;for(int i=0;i<2;i++){var obj=new GameObject(i==0?"Left ski grooves":"Right ski grooves");obj.transform.SetParent(transform.parent,true);meshes[i]=new Mesh{name="Persistent ski tracks"};obj.AddComponent<MeshFilter>().sharedMesh=meshes[i];obj.AddComponent<MeshRenderer>().sharedMaterial=config.trackMaterial;}
        }
        void BreakTrail(){breaks[0]=breaks[1]=true;}
        void LateUpdate()
        {
            timer+=Time.deltaTime;if(timer<.04f)return;timer=0;
            for(int i=0;i<2;i++)
            {
                var list=points[i];var contact=i==0?skier.Contacts.left:skier.Contacts.right;
                if(!skier.Grounded)breaks[i]=true;
                if(skier.Grounded&&contact.hit&&skier.Speed>1)
                {
                    Vector3 p=contact.point+contact.normal*config.trackLift;
                    if(list.Count==0||Vector3.Distance(p,list[list.Count-1].position)>config.trackSpacing){list.Add(new TrackPoint{position=p,normal=contact.normal,time=Time.time,connected=!breaks[i]});breaks[i]=false;}
                }
                while(list.Count>config.trackSamples||(list.Count>0&&Time.time-list[0].time>config.trackLifetime))list.RemoveAt(0);
                if(list.Count<2)continue;
                var verts=new Vector3[list.Count*2];var colors=new Color[list.Count*2];var tris=new List<int>((list.Count-1)*6);
                for(int k=0;k<list.Count;k++)
                {
                    Vector3 travel=k==0?list[1].position-list[0].position:list[k].position-list[k-1].position;Vector3 side=Vector3.Cross(list[k].normal,travel.normalized)*config.trackWidth*.5f;
                    verts[k*2]=list[k].position-side;verts[k*2+1]=list[k].position+side;colors[k*2]=colors[k*2+1]=new Color(1,1,1,Mathf.Clamp01(1-(Time.time-list[k].time)/config.trackLifetime));
                    if(k>0&&list[k].connected && Vector3.Distance(list[k].position,list[k-1].position)<8 && list[k].time-list[k-1].time<.25f){int a=(k-1)*2;tris.AddRange(new[]{a,a+2,a+1,a+1,a+2,a+3});}
                }
                meshes[i].Clear();meshes[i].vertices=verts;meshes[i].colors=colors;meshes[i].SetTriangles(tris,0);meshes[i].RecalculateNormals();meshes[i].RecalculateBounds();
            }
        }
    }
}
