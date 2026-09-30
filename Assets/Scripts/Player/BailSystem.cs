using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    public class BailSystem : MonoBehaviour
    {
        public event System.Action Crashed;
        public Transform CameraTarget {get;private set;}
        SkiPhysicsController skier;SkierPose pose;GameObject ragdoll;float remaining;
        void Start(){skier=GetComponent<SkiPhysicsController>();pose=GetComponent<SkierPose>();skier.Landed+=OnLand;skier.ResetPerformed+=Clear;}
        void OnLand(LandingResult landing){if(landing.quality==LandingQuality.Bail)Crash();}
        public void Crash()
        {
            if(ragdoll)return;Crashed?.Invoke();skier.Bailed=true;remaining=skier.trickConfig.bailRecovery;
            if(pose&&pose.root)
            {
                ragdoll=Instantiate(pose.root.gameObject,pose.root.position,pose.root.rotation);ragdoll.name="Momentum ragdoll";pose.root.gameObject.SetActive(false);
                var bodies=new Dictionary<Transform,Rigidbody>();
                foreach(var t in ragdoll.GetComponentsInChildren<Transform>())
                {
                    if(!(t.name=="Hips"||t.name=="Spine"||t.name=="Chest"||t.name=="Head"||t.name.StartsWith("UpperArm_")||t.name.StartsWith("LowerArm_")||t.name.StartsWith("Hand_")||t.name.StartsWith("Thigh_")||t.name.StartsWith("Shin_")||t.name.StartsWith("Foot_")))continue;
                    var col=t.gameObject.AddComponent<CapsuleCollider>();col.radius=.10f;col.height=.28f;
                    if(t.name=="Hips")CameraTarget=t;
                    var rb=t.gameObject.AddComponent<Rigidbody>();rb.mass=skier.config.mass/16;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;
                    rb.linearVelocity=skier.Body.linearVelocity+Vector3.Cross(skier.Body.angularVelocity,t.position-skier.Body.position);rb.angularVelocity=skier.Body.angularVelocity;bodies[t]=rb;
                    var parent=t.parent;while(parent&&!bodies.ContainsKey(parent))parent=parent.parent;
                    if(parent){var joint=t.gameObject.AddComponent<ConfigurableJoint>();joint.connectedBody=bodies[parent];joint.xMotion=joint.yMotion=joint.zMotion=ConfigurableJointMotion.Locked;joint.angularXMotion=joint.angularYMotion=joint.angularZMotion=ConfigurableJointMotion.Limited;joint.lowAngularXLimit=new SoftJointLimit{limit=-45};joint.highAngularXLimit=new SoftJointLimit{limit=45};joint.angularYLimit=joint.angularZLimit=new SoftJointLimit{limit=35};joint.enableCollision=false;}
                }
            }
            skier.Body.isKinematic=true;
        }
        void Update(){if(!skier.Bailed)return;remaining-=Time.deltaTime;if(remaining<=0)skier.ResetTo(skier.safePosition,skier.safeRotation);}
        void Clear(){CameraTarget=null;if(ragdoll)Destroy(ragdoll);ragdoll=null;if(pose&&pose.root)pose.root.gameObject.SetActive(true);skier.Body.isKinematic=false;}
    }
}
