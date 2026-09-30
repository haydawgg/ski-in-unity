using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    public class SnowTrackSystem:MonoBehaviour
    {
        public GraphicsConfig config;
        struct TrackPoint { public Vector3 position,normal,travel;public float born,width,depth,distance;public bool connected; }
        readonly List<TrackPoint>[] points={new List<TrackPoint>(),new List<TrackPoint>()};
        readonly Mesh[] meshes=new Mesh[2];readonly GameObject[] objects=new GameObject[2];readonly bool[] breaks={true,true};
        readonly List<Vector3> vertices=new List<Vector3>(),normals=new List<Vector3>();
        readonly List<Vector2> uv=new List<Vector2>(),ageDepth=new List<Vector2>();readonly List<int> triangles=new List<int>();
        SkiPhysicsController skier;RailSystem rail;Material material;float timer,skipUntil;
        public Mesh TrackMesh(int ski)=>meshes[ski];public int SampleCount(int ski)=>points[ski].Count;
        void Start()
        {
            skier=GetComponent<SkiPhysicsController>();rail=GetComponent<RailSystem>();skier.ResetPerformed+=BreakTrail;
            material=new Material(config.trackMaterial){name="Runtime ski grooves"};material.SetFloat("_Lifetime",config.trackLifetime);
            for(int i=0;i<2;i++)
            {
                objects[i]=new GameObject(i==0?"Left ski grooves":"Right ski grooves");objects[i].transform.SetParent(transform.parent,true);
                meshes[i]=new Mesh{name="Fading ski grooves"};meshes[i].MarkDynamic();objects[i].AddComponent<MeshFilter>().sharedMesh=meshes[i];
                var renderer=objects[i].AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
        void BreakTrail(){breaks[0]=breaks[1]=true;skipUntil=Time.time+Time.fixedDeltaTime*2;timer=0;}
        void OnDisable(){BreakTrail();}
        void LateUpdate()
        {
            if(!skier)return;timer+=Time.deltaTime;if(timer<config.trackUpdateInterval)return;timer=0;
            bool snow=skier.Surface==SurfaceType.Groomed||skier.Surface==SurfaceType.Powder||skier.Surface==SurfaceType.Ice;
            bool riding=skier.Grounded&&!skier.Bailed&&!(rail&&rail.Riding)&&snow&&Time.time>=skipUntil;
            for(int i=0;i<2;i++)
            {
                var contact=i==0?skier.Contacts.left:skier.Contacts.right;var list=points[i];
                if(!riding||!contact.hit){breaks[i]=true;}
                else if(skier.Speed>1)
                {
                    var position=contact.point;
                    float width=config.trackWidth*(skier.Surface==SurfaceType.Powder?config.powderTrackWidth:1);
                    float depth=skier.Surface==SurfaceType.Powder?config.powderTrackDepth:skier.Surface==SurfaceType.Ice?.5f:1;
                    if(list.Count==0||Vector3.Distance(position,list[list.Count-1].position)>=config.trackSpacing)
                    {
                        var previous=list.Count>0?list[list.Count-1]:default;float gap=Vector3.Distance(position,previous.position);
                        bool connected=list.Count>0&&!breaks[i]&&gap<config.trackMaximumGap&&Time.time-previous.born<config.trackMaximumInterval;
                        int steps=connected?Mathf.Clamp(Mathf.CeilToInt(gap/config.trackSpacing),1,16):1;
                        var travel=Vector3.ProjectOnPlane(skier.Body.linearVelocity,contact.normal).normalized;
                        for(int step=1;step<=steps;step++)
                        {
                            float t=(float)step/steps;
                            var sample=connected?Vector3.Lerp(previous.position,position,t):position;
                            var normal=connected?Vector3.Slerp(previous.normal,contact.normal,t):contact.normal;
                            // Physics contacts average tip/tail probes; project the visual groove onto the snow between them.
                            if(Physics.Raycast(sample+normal*.4f,-normal,out var hit,.8f,~(1<<8),QueryTriggerInteraction.Ignore))
                            {
                                var surface=hit.collider.GetComponentInParent<SnowSurface>();
                                if(!surface||surface.type==SurfaceType.Groomed||surface.type==SurfaceType.Powder||surface.type==SurfaceType.Ice){sample=hit.point;normal=hit.normal;}
                            }
                            list.Add(new TrackPoint{position=sample,normal=normal,travel=travel,born=connected?Mathf.Lerp(previous.born,Time.time,t):Time.time,width=width,depth=depth,connected=connected,distance=connected?previous.distance+gap*t:0});
                        }
                        breaks[i]=false;
                    }
                }
                while(list.Count>config.trackSamples||(list.Count>0&&Time.time-list[0].born>config.trackLifetime))list.RemoveAt(0);
                Rebuild(i);
            }
        }
        void Rebuild(int ski)
        {
            var list=points[ski];var mesh=meshes[ski];mesh.Clear();if(list.Count<2)return;
            vertices.Clear();normals.Clear();uv.Clear();ageDepth.Clear();triangles.Clear();var root=objects[ski].transform;
            for(int k=0;k<list.Count;k++)
            {
                var point=list[k];Vector3 travel=point.travel;
                if(k>0&&point.connected)travel=point.position-list[k-1].position;
                else if(k+1<list.Count&&list[k+1].connected)travel=list[k+1].position-point.position;
                var side=Vector3.Cross(point.normal,travel.normalized).normalized*point.width*.5f;
                var lifted=point.position+point.normal*config.trackLift;
                vertices.Add(root.InverseTransformPoint(lifted-side));vertices.Add(root.InverseTransformPoint(lifted+side));
                var normal=root.InverseTransformDirection(point.normal);normals.Add(normal);normals.Add(normal);
                uv.Add(new Vector2(0,point.distance));uv.Add(new Vector2(1,point.distance));ageDepth.Add(new Vector2(point.born,point.depth));ageDepth.Add(new Vector2(point.born,point.depth));
                if(k>0&&point.connected){int a=(k-1)*2;triangles.Add(a);triangles.Add(a+2);triangles.Add(a+1);triangles.Add(a+1);triangles.Add(a+2);triangles.Add(a+3);}
            }
            mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetUVs(1,ageDepth);mesh.SetTriangles(triangles,0);mesh.RecalculateBounds();
        }
        void OnDestroy()
        {
            if(skier)skier.ResetPerformed-=BreakTrail;
            for(int i=0;i<2;i++){if(objects[i])Destroy(objects[i]);if(meshes[i])Destroy(meshes[i]);}if(material)Destroy(material);
        }
    }
}
