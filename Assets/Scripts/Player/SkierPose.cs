using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    public class SkierPose : MonoBehaviour
    {
        public Transform root,hips,spine,head,leftArm,leftElbow,leftHand,rightArm,rightElbow,rightHand,leftThigh,leftShin,leftFoot,rightThigh,rightShin,rightFoot,leftSki,rightSki,leftPole,rightPole;
        SkiPhysicsController skier;GrabSystem grab;Dictionary<Transform,Quaternion> neutral=new Dictionary<Transform,Quaternion>();
        void Start(){skier=GetComponent<SkiPhysicsController>();grab=GetComponent<GrabSystem>();Cache();}
        public void Cache(){neutral.Clear();if(root)foreach(var t in root.GetComponentsInChildren<Transform>())neutral[t]=t.localRotation;}
        public void Bind(Transform source)
        {
            root=source;var driver=GetComponent<SkiPhysicsController>();if(driver&&driver.config)root.localPosition=Vector3.up*driver.config.visualRideOffset;Transform Find(string n){foreach(var t in source.GetComponentsInChildren<Transform>())if(t.name==n)return t;return null;}
            hips=Find("Hips");spine=Find("Spine");head=Find("Head");leftArm=Find("UpperArm_L");leftElbow=Find("LowerArm_L");leftHand=Find("Hand_L");rightArm=Find("UpperArm_R");rightElbow=Find("LowerArm_R");rightHand=Find("Hand_R");
            leftThigh=Find("Thigh_L");leftShin=Find("Shin_L");leftFoot=Find("Foot_L");rightThigh=Find("Thigh_R");rightShin=Find("Shin_R");rightFoot=Find("Foot_R");leftSki=Find("Ski_L");rightSki=Find("Ski_R");leftPole=Find("Pole_L");rightPole=Find("Pole_R");Cache();
        }
        void Pose(Transform t,Vector3 angles){if(t&&neutral.TryGetValue(t,out var q))t.localRotation=q*Quaternion.Euler(angles);}
        void LateUpdate()
        {
            if(!root||!leftSki||!skier)return;
            float bend=skier.Input.crouch?1:skier.Input.tuck?.75f:skier.Compression;
            if(grab&&grab.Current!=GrabType.None)bend=1;
            float thighAngle=grab&&grab.Current!=GrabType.None ? -130 : -bend*55;
            Pose(leftThigh,new Vector3(thighAngle,0,0));Pose(rightThigh,new Vector3(thighAngle,0,0));Pose(leftShin,new Vector3(bend*95,0,0));Pose(rightShin,new Vector3(bend*95,0,0));
            Pose(leftFoot,new Vector3(-bend*40,0,0));Pose(rightFoot,new Vector3(-bend*40,0,0));Pose(spine,new Vector3(bend*25,0,0));
            root.localPosition=Vector3.up*(skier.config.visualRideOffset-bend*.20f);
            root.localRotation=Quaternion.Euler(0,0,-skier.Edge*skier.config.maximumLean*(skier.Grounded?1:.25f));
            Pose(leftArm,new Vector3(-20-bend*25,0,-15));Pose(rightArm,new Vector3(-20-bend*25,0,15));Pose(leftElbow,new Vector3(-25,0,0));Pose(rightElbow,new Vector3(-25,0,0));
            if(skier.Grounded)
            {
                AlignFoot(leftThigh,leftShin,leftFoot,leftSki,skier.Contacts.left);
                AlignFoot(rightThigh,rightShin,rightFoot,rightSki,skier.Contacts.right);
            }
            bool crossed=!skier.Grounded&&(skier.Input.modifierLeft || (grab&&grab.Current==GrabType.CrissCross));
            if(!skier.Grounded){Pose(leftSki,Vector3.zero);Pose(rightSki,Vector3.zero);}
            if(crossed){leftSki.rotation=Quaternion.AngleAxis(30,skier.transform.up)*leftSki.rotation;rightSki.rotation=Quaternion.AngleAxis(-30,skier.transform.up)*rightSki.rotation;}
            float swing=skier.Grounded?0:Mathf.Sin(Time.time*4)*25;
            Pose(leftPole,new Vector3(-55+swing,0,15));Pose(rightPole,new Vector3(-55-swing,0,-15));
            if(grab&&grab.Current!=GrabType.None)
            {
                Vector3 target=grab.Target(leftSki,rightSki);
                SolveArm(grab.LeftHand?leftArm:rightArm,grab.LeftHand?leftElbow:rightElbow,grab.LeftHand?leftHand:rightHand,target);
                if(grab.Current==GrabType.CrissCross)SolveArm(grab.LeftHand?rightArm:leftArm,grab.LeftHand?rightElbow:leftElbow,grab.LeftHand?rightHand:leftHand,(grab.LeftHand?rightSki:leftSki).position);
            }
        }
        void AlignFoot(Transform thigh,Transform shin,Transform foot,Transform ski,SkiContact contact)
        {
            if(!contact.hit)return;
            var forward=Vector3.ProjectOnPlane(skier.Body.rotation*Vector3.forward,contact.normal).normalized;
            SolveArm(thigh,shin,foot,contact.point+contact.normal*.12f+forward*.03f);
            foot.rotation=Quaternion.FromToRotation(foot.up,forward)*foot.rotation;
            ski.rotation=Quaternion.FromToRotation(ski.up,forward)*ski.rotation;
            ski.position=contact.point+contact.normal*.03f;
        }
        public static void SolveArm(Transform upper,Transform lower,Transform hand,Vector3 target)
        {
            if(!upper||!lower||!hand)return;
            float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,hand.position);
            Vector3 dir=(target-upper.position).normalized;float d=Mathf.Clamp(Vector3.Distance(upper.position,target),.05f,a+b-.001f);
            float x=(a*a-b*b+d*d)/(2*d),y=Mathf.Sqrt(Mathf.Max(0,a*a-x*x));
            Vector3 bend=Vector3.ProjectOnPlane(upper.root.forward,dir).normalized;
            Vector3 elbow=upper.position+dir*x+bend*y;
            upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
        }
    }
}
