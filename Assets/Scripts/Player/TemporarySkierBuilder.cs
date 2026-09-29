using UnityEngine;
namespace PowderFlow
{
    public static class TemporarySkierBuilder
    {
        public static Transform Build(Transform parent)
        {
            var root=new GameObject("Temporary articulated skier").transform;root.SetParent(parent,false);
            Transform Bone(string name,Transform p,Vector3 pos,Vector3 size,Color color)
            {
                var bone=new GameObject(name).transform;bone.SetParent(p,false);bone.localPosition=pos;
                var part=GameObject.CreatePrimitive(PrimitiveType.Capsule);Object.Destroy(part.GetComponent<Collider>());part.transform.SetParent(bone,false);part.transform.localScale=size;part.GetComponent<Renderer>().sharedMaterial=PhysicsTestWorld.Material(color);return bone;
            }
            Color jacket=new Color(.24f,.55f,.43f),pants=new Color(.12f,.13f,.2f);
            var hips=Bone("Hips",root,Vector3.zero,new Vector3(.4f,.12f,.3f),pants);var spine=Bone("Spine",hips,new Vector3(0,.25f,0),new Vector3(.48f,.2f,.3f),jacket);var chest=Bone("Chest",spine,new Vector3(0,.23f,0),new Vector3(.5f,.15f,.33f),jacket);Bone("Head",chest,new Vector3(0,.36f,0),new Vector3(.32f,.19f,.32f),Color.black);
            for(int side=-1;side<=1;side+=2)
            {
                string s=side<0?"L":"R";
                var arm=Bone("UpperArm_"+s,chest,new Vector3(side*.28f,0,0),new Vector3(.16f,.16f,.16f),jacket);
                var elbow=Bone("LowerArm_"+s,arm,new Vector3(0,-.3f,0),new Vector3(.14f,.14f,.14f),jacket);
                var hand=Bone("Hand_"+s,elbow,new Vector3(0,-.28f,0),new Vector3(.12f,.08f,.12f),Color.black);
                var thigh=Bone("Thigh_"+s,hips,new Vector3(side*.14f,-.1f,0),new Vector3(.23f,.19f,.23f),pants);
                var shin=Bone("Shin_"+s,thigh,new Vector3(0,-.34f,0),new Vector3(.20f,.16f,.20f),pants);
                var foot=Bone("Foot_"+s,shin,new Vector3(0,-.28f,.02f),new Vector3(.18f,.12f,.3f),Color.black);
                var ski=new GameObject("Ski_"+s).transform;ski.SetParent(foot,false);ski.localPosition=new Vector3(0,-.1f,0);
                var mesh=PhysicsTestWorld.Solid("Ski visual",Vector3.zero,new Vector3(.12f,.04f,1.7f),Quaternion.identity,new Color(.1f,.5f,.55f));Object.Destroy(mesh.GetComponent<Collider>());mesh.transform.SetParent(ski,false);
                var pole=new GameObject("Pole_"+s).transform;pole.SetParent(hand,false);
                var poleMesh=PhysicsTestWorld.Solid("Pole visual",Vector3.zero,new Vector3(.02f,1.15f,.02f),Quaternion.identity,Color.black);Object.Destroy(poleMesh.GetComponent<Collider>());poleMesh.transform.SetParent(pole,false);poleMesh.transform.localPosition=Vector3.down*.57f;
            }
            return root;
        }
    }
}
