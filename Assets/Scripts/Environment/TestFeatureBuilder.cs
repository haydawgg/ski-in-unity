using UnityEngine;
namespace PowderFlow
{
    public static class TestFeatureBuilder
    {
        public static GameObject Kicker(Vector3 position,float width,float length,float height)
        {
            const int sections=40;var vertices=new Vector3[(sections+1)*2];var triangles=new int[sections*6];
            for(int i=0;i<=sections;i++){float t=(float)i/sections;vertices[i*2]=new Vector3(-width*.5f,height*t*t,length*t);vertices[i*2+1]=new Vector3(width*.5f,height*t*t,length*t);}
            for(int i=0;i<sections;i++){int a=i*2,j=i*6;triangles[j]=a;triangles[j+1]=a+2;triangles[j+2]=a+1;triangles[j+3]=a+1;triangles[j+4]=a+2;triangles[j+5]=a+3;}
            var mesh=new Mesh();mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
            var obj=new GameObject("Temporary continuous kicker");obj.transform.position=position;obj.AddComponent<MeshFilter>().sharedMesh=mesh;obj.AddComponent<MeshRenderer>().sharedMaterial=PhysicsTestWorld.Material(new Color(.85f,.92f,1));obj.AddComponent<MeshCollider>().sharedMesh=mesh;obj.AddComponent<SnowSurface>();return obj;
        }
    }
}
