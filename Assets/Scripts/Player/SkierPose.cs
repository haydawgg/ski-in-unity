using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    public class SkierPose : MonoBehaviour
    {
        public Transform root,hips,spine,head,leftArm,leftElbow,leftHand,rightArm,rightElbow,rightHand,leftThigh,leftShin,leftFoot,rightThigh,rightShin,rightFoot,leftSki,rightSki,leftPole,rightPole;
        public CharacterVisualConfig config;public CharacterVisualConfig Visuals=>config?config:CharacterVisualConfig.Default;
        SkiPhysicsController skier;GrabSystem grab;float displayedBend;
        Dictionary<Transform,Quaternion> neutral=new Dictionary<Transform,Quaternion>();Dictionary<Transform,Vector3> positions=new Dictionary<Transform,Vector3>();
        void Start(){skier=GetComponent<SkiPhysicsController>();grab=GetComponent<GrabSystem>();Cache();}
        public void Cache(){neutral.Clear();positions.Clear();if(root)foreach(var t in root.GetComponentsInChildren<Transform>()){neutral[t]=t.localRotation;positions[t]=t.localPosition;}}
        public void Bind(Transform source)
        {
            root=source;var driver=GetComponent<SkiPhysicsController>();if(driver&&driver.config)root.localPosition=Vector3.up*driver.config.visualRideOffset;Transform Find(string n){foreach(var t in source.GetComponentsInChildren<Transform>())if(t.name==n)return t;return null;}
            hips=Find("Hips");spine=Find("Spine");head=Find("Head");leftArm=Find("UpperArm_L");leftElbow=Find("LowerArm_L");leftHand=Find("Hand_L");rightArm=Find("UpperArm_R");rightElbow=Find("LowerArm_R");rightHand=Find("Hand_R");
            leftThigh=Find("Thigh_L");leftShin=Find("Shin_L");leftFoot=Find("Foot_L");rightThigh=Find("Thigh_R");rightShin=Find("Shin_R");rightFoot=Find("Foot_R");leftSki=Find("Ski_L");rightSki=Find("Ski_R");leftPole=Find("Pole_L");rightPole=Find("Pole_R");Cache();
        }
        void Pose(Transform t,Vector3 angles){if(t&&neutral.TryGetValue(t,out var q)){t.localPosition=positions[t];t.localRotation=q*Quaternion.Euler(angles);}}
        void LateUpdate(){ApplyPose();}
        public void ApplyPose(bool immediate=false)
        {
            if(!root||!leftSki||!skier)return;
            var c=Visuals;bool ridingRail=TryGetComponent<RailSystem>(out var rail)&&rail.Riding;bool grabbing=!ridingRail&&grab&&grab.Current!=GrabType.None;
            float wanted=skier.Input.crouch?1:skier.Input.tuck?c.tuckBend:ridingRail?c.railBend:skier.Grounded?c.neutralBend:c.airBend;
            wanted=Mathf.Max(wanted,skier.Compression);if(grabbing)wanted=1;
            displayedBend=immediate?wanted:Mathf.Lerp(displayedBend,wanted,1-Mathf.Exp(-c.poseResponse*Time.deltaTime));float bend=displayedBend;
            float thighAngle=grabbing?c.grabThigh:-bend*c.thighBend,shinAngle=grabbing?c.grabShin:bend*c.shinBend;
            float footAngle=grabbing?c.grabFoot:-bend*c.footBend;
            if(grabbing&&(grab.Current==GrabType.Mute||grab.Current==GrabType.Japan)){thighAngle=c.crossBodyGrabAngles.x;shinAngle=c.crossBodyGrabAngles.y;footAngle=c.crossBodyGrabAngles.z;}
            if(grabbing&&grab.Current==GrabType.Nose){thighAngle=c.noseGrabAngles.x;shinAngle=c.noseGrabAngles.y;footAngle=c.noseGrabAngles.z;}
            Pose(leftThigh,new Vector3(thighAngle,0,0));Pose(rightThigh,new Vector3(thighAngle,0,0));Pose(leftShin,new Vector3(shinAngle,0,0));Pose(rightShin,new Vector3(shinAngle,0,0));
            Pose(leftFoot,new Vector3(footAngle,0,0));Pose(rightFoot,new Vector3(footAngle,0,0));
            Pose(spine,new Vector3(grabbing?c.grabSpine:bend*c.spineBend,0,0));Pose(head,new Vector3(-bend*c.spineBend*c.headCounterBend,0,0));
            root.localPosition=Vector3.up*(skier.config.visualRideOffset-bend*c.rootDrop);
            root.localRotation=Quaternion.Euler(0,0,-skier.Edge*skier.config.maximumLean*(skier.Grounded?1:.25f));
            Pose(leftArm,new Vector3(c.armNeutral-bend*c.armBend,0,-c.armSpread));Pose(rightArm,new Vector3(c.armNeutral-bend*c.armBend,0,c.armSpread));Pose(leftElbow,new Vector3(c.elbowBend,0,0));Pose(rightElbow,new Vector3(c.elbowBend,0,0));
            Pose(leftSki,Vector3.zero);Pose(rightSki,Vector3.zero);
            if(ridingRail)AlignRailFeet(rail);
            else if(skier.Grounded)
            {
                AlignFoot(leftThigh,leftShin,leftFoot,leftSki,skier.Contacts.left);
                AlignFoot(rightThigh,rightShin,rightFoot,rightSki,skier.Contacts.right);
            }
            else if(!grabbing)
            {
                // Knees compress independently while the bindings keep the skis along the rider's heading.
                var forward=Quaternion.AngleAxis(-bend*c.footBend*.2f,skier.transform.right)*skier.transform.forward;
                AlignAirSki(leftFoot,leftSki,forward);AlignAirSki(rightFoot,rightSki,forward);
            }
            bool crossed=!skier.Grounded&&!ridingRail&&(skier.Input.modifierLeft || (grab&&grab.Current==GrabType.CrissCross));
            if(crossed){leftSki.rotation=Quaternion.AngleAxis(c.crossYaw,skier.transform.up)*leftSki.rotation;rightSki.rotation=Quaternion.AngleAxis(-c.crossYaw,skier.transform.up)*rightSki.rotation;}
            if(grabbing)
            {
                Vector3 target=grab.Target(leftSki,rightSki);
                SolveArm(grab.LeftHand?leftArm:rightArm,grab.LeftHand?leftElbow:rightElbow,grab.LeftHand?leftHand:rightHand,target,skier.transform.forward);
                if(grab.Current==GrabType.CrissCross)SolveArm(grab.LeftHand?rightArm:leftArm,grab.LeftHand?rightElbow:leftElbow,grab.LeftHand?rightHand:leftHand,(grab.LeftHand?rightSki:leftSki).position,skier.transform.forward);
            }
            float swing=skier.Grounded||ridingRail?0:Mathf.Sin(Time.time*c.poleFrequency)*c.poleSway;
            TrailPole(leftPole,-1,swing);TrailPole(rightPole,1,-swing);
        }
        void TrailPole(Transform pole,float side,float swing)
        {
            if(!pole)return;Pose(pole,Vector3.zero);
            float handSide=Vector3.Dot(pole.position-hips.position,skier.transform.right);if(Mathf.Abs(handSide)>.05f)side=Mathf.Sign(handSide);
            Vector3 trail=Visuals.poleTrail;Vector3 direction=skier.transform.forward*trail.z+skier.transform.up*trail.y+skier.transform.right*side*trail.x;
            direction=Quaternion.AngleAxis(swing,skier.transform.forward)*direction;
            pole.rotation=Quaternion.FromToRotation(pole.up,direction.normalized)*pole.rotation;
        }
        void AlignFoot(Transform thigh,Transform shin,Transform foot,Transform ski,SkiContact contact)
        {
            if(!contact.hit)return;
            var forward=Vector3.ProjectOnPlane(skier.Body.rotation*Vector3.forward,contact.normal).normalized;
            SolveArm(thigh,shin,foot,contact.point+contact.normal*.12f+forward*.03f,forward);
            foot.rotation=Quaternion.FromToRotation(foot.up,forward)*foot.rotation;
            ski.rotation=Quaternion.FromToRotation(ski.up,forward)*ski.rotation;
            ski.position=contact.point+contact.normal*.03f;
        }
        void AlignRailFeet(RailSystem rail)
        {
            // Rail physics follows its centerline; visual bindings sit on the actual riding surface.
            rail.Current.Closest(skier.transform.position-Vector3.up*skier.config.rideHeight,out var center,out var tangent);
            var normal=Vector3.ProjectOnPlane(Vector3.up,tangent).normalized;
            var heading=Vector3.ProjectOnPlane(skier.Body.rotation*Vector3.forward,normal).normalized;var side=Vector3.Cross(normal,heading);
            if(Physics.Raycast(center+normal*.5f,-normal,out var hit,1,~(1<<8),QueryTriggerInteraction.Ignore)&&hit.collider.transform.IsChildOf(rail.Current.transform))center=hit.point;
            float narrow=Mathf.Min(skier.config.skiSeparation,Mathf.Max(.22f,rail.Current.width+.1f));
            float stance=Mathf.Lerp(skier.config.skiSeparation,narrow,Mathf.Abs(Vector3.Dot(heading,tangent)));
            var contact=new SkiContact{hit=true,point=center-side*stance*.5f,normal=normal,surface=SurfaceType.Rail};
            AlignFoot(leftThigh,leftShin,leftFoot,leftSki,contact);contact.point=center+side*stance*.5f;AlignFoot(rightThigh,rightShin,rightFoot,rightSki,contact);
        }
        void AlignAirSki(Transform foot,Transform ski,Vector3 forward)
        {
            foot.rotation=Quaternion.FromToRotation(foot.up,forward)*foot.rotation;
            ski.rotation=Quaternion.FromToRotation(ski.up,forward)*ski.rotation;
        }
        public static void SolveArm(Transform upper,Transform lower,Transform hand,Vector3 target,Vector3 bendHint=default)
        {
            if(!upper||!lower||!hand)return;
            float a=Vector3.Distance(upper.position,lower.position),b=Vector3.Distance(lower.position,hand.position);
            Vector3 dir=(target-upper.position).normalized;float d=Mathf.Clamp(Vector3.Distance(upper.position,target),.05f,a+b-.001f);
            float x=(a*a-b*b+d*d)/(2*d),y=Mathf.Sqrt(Mathf.Max(0,a*a-x*x));
            Vector3 bend=Vector3.ProjectOnPlane(bendHint.sqrMagnitude>.01f?bendHint:upper.root.forward,dir).normalized;
            Vector3 elbow=upper.position+dir*x+bend*y;
            upper.rotation=Quaternion.FromToRotation(lower.position-upper.position,elbow-upper.position)*upper.rotation;
            lower.rotation=Quaternion.FromToRotation(hand.position-lower.position,target-lower.position)*lower.rotation;
        }
    }
}
