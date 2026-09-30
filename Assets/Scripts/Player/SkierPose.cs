using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    public class SkierPose : MonoBehaviour
    {
        public Transform root,hips,spine,head,leftArm,leftElbow,leftHand,rightArm,rightElbow,rightHand,leftThigh,leftShin,leftFoot,rightThigh,rightShin,rightFoot,leftSki,rightSki,leftPole,rightPole;
        public CharacterVisualConfig config;public CharacterVisualConfig Visuals=>config?config:CharacterVisualConfig.Default;
        SkiPhysicsController skier;GrabSystem grab;float displayedBend,displayedCarve,landingBend,sketchAge=100,sketchDirection;
        public float Bend=>displayedBend;
        public float GrabWeight {get;private set;}
        Dictionary<Transform,Quaternion> neutral=new Dictionary<Transform,Quaternion>();Dictionary<Transform,Vector3> positions=new Dictionary<Transform,Vector3>();
        void Start(){skier=GetComponent<SkiPhysicsController>();grab=GetComponent<GrabSystem>();Cache();if(skier){skier.Landed+=OnLanding;skier.ResetPerformed+=ResetPose;}}
        void OnDestroy(){if(skier){skier.Landed-=OnLanding;skier.ResetPerformed-=ResetPose;}}
        void ResetPose(){displayedBend=displayedCarve=landingBend=GrabWeight=0;sketchAge=100;}
        void OnLanding(LandingResult result){if(result.quality==LandingQuality.Bail)return;landingBend=Mathf.Clamp01(result.impact/skier.trickConfig.cleanImpact);sketchAge=result.quality==LandingQuality.Sketchy?0:100;sketchDirection=Mathf.Sign(skier.Input.steer==0?Vector3.Dot(skier.Body.linearVelocity,skier.transform.right):skier.Input.steer);}
        public void Cache(){neutral.Clear();positions.Clear();if(root)foreach(var t in root.GetComponentsInChildren<Transform>()){neutral[t]=t.localRotation;positions[t]=t.localPosition;}}
        public void Bind(Transform source)
        {
            root=source;var driver=GetComponent<SkiPhysicsController>();if(driver&&driver.config)root.localPosition=Vector3.up*driver.config.visualRideOffset;Transform Find(string n){foreach(var t in source.GetComponentsInChildren<Transform>())if(t.name==n)return t;return null;}
            hips=Find("Hips");spine=Find("Spine");head=Find("Head");leftArm=Find("UpperArm_L");leftElbow=Find("LowerArm_L");leftHand=Find("Hand_L");rightArm=Find("UpperArm_R");rightElbow=Find("LowerArm_R");rightHand=Find("Hand_R");
            leftThigh=Find("Thigh_L");leftShin=Find("Shin_L");leftFoot=Find("Foot_L");rightThigh=Find("Thigh_R");rightShin=Find("Shin_R");rightFoot=Find("Foot_R");leftSki=Find("Ski_L");rightSki=Find("Ski_R");leftPole=Find("Pole_L");rightPole=Find("Pole_R");
            AddGrabTargets(leftSki);AddGrabTargets(rightSki);Cache();
        }
        static void AddGrabTargets(Transform ski)
        {
            if(!ski)return;
            foreach(var name in new[]{"NoseGrab","MidGrab","TailGrab"})if(!ski.Find(name))
            {
                var target=new GameObject(name).transform;target.SetParent(ski,false);
                target.localPosition=new Vector3(0,name=="NoseGrab"?.72f:name=="TailGrab"?-.7f:0,-.06f);
            }
        }
        void Pose(Transform t,Vector3 angles){if(t&&neutral.TryGetValue(t,out var q)){t.localPosition=positions[t];t.localRotation=q*Quaternion.Euler(angles);}}
        void LateUpdate(){ApplyPose();}
        public void ApplyPose(bool immediate=false)
        {
            if(!root||!leftSki||!skier)return;
            var c=Visuals;var motion=skier.Motion;bool ridingRail=TryGetComponent<RailSystem>(out var rail)&&rail.Riding;
            GrabWeight=!ridingRail&&grab?(immediate?(grab.Current!=GrabType.None?1:0):Mathf.SmoothStep(0,1,grab.Blend)):0;
            bool grabbing=GrabWeight>.001f;var grabType=grab?grab.PoseGrab:GrabType.None;
            float dt=Time.deltaTime;sketchAge+=dt;landingBend=Mathf.MoveTowards(landingBend,0,dt*c.landingRecovery);
            float rotation=Mathf.Clamp01(skier.Body.angularVelocity.magnitude/c.rotationPoseSpeed);
            float prepare=motion.landingPrediction;
            float carve=skier.Grounded?motion.carveAmount:0;
            displayedCarve=immediate?carve:Mathf.Lerp(displayedCarve,carve,1-Mathf.Exp(-c.motionResponse*dt));
            float recovery=skier.Grounded?Mathf.Clamp01(1-sketchAge/c.sketchyDuration):0;
            float wanted=skier.Input.crouch?1:skier.Input.tuck?c.tuckBend:ridingRail?c.railBend:skier.Grounded?c.neutralBend:c.airBend;
            if(!skier.Grounded&&!ridingRail){wanted=Mathf.Max(wanted,c.airBend+rotation*c.rotationTuck);wanted=Mathf.Lerp(wanted,c.landingLegBend,prepare);wanted*=1-motion.popAmount*c.popExtension;}
            wanted=Mathf.Max(wanted,Mathf.Max(skier.Compression,landingBend));wanted=Mathf.Lerp(wanted,1,GrabWeight);
            displayedBend=immediate?wanted:Mathf.Lerp(displayedBend,wanted,1-Mathf.Exp(-c.poseResponse*Time.deltaTime));float bend=displayedBend;
            float grabThigh=c.grabThigh,grabShin=c.grabShin,grabFoot=c.grabFoot;
            if(grabType==GrabType.Mute||grabType==GrabType.Japan){grabThigh=c.crossBodyGrabAngles.x;grabShin=c.crossBodyGrabAngles.y;grabFoot=c.crossBodyGrabAngles.z;}
            if(grabType==GrabType.Nose){grabThigh=c.noseGrabAngles.x;grabShin=c.noseGrabAngles.y;grabFoot=c.noseGrabAngles.z;}
            float thighAngle=Mathf.Lerp(-bend*c.thighBend,grabThigh,GrabWeight),shinAngle=Mathf.Lerp(bend*c.shinBend,grabShin,GrabWeight);
            float airShape=!skier.Grounded&&!ridingRail?rotation*(1-prepare)*(1-GrabWeight):0;
            thighAngle-=airShape*c.airKneeLift;
            float footAngle=Mathf.Lerp(-bend*c.footBend,grabFoot,GrabWeight);
            Pose(leftThigh,new Vector3(thighAngle,0,0));Pose(rightThigh,new Vector3(thighAngle,0,0));Pose(leftShin,new Vector3(shinAngle,0,0));Pose(rightShin,new Vector3(shinAngle,0,0));
            Pose(leftFoot,new Vector3(footAngle,0,0));Pose(rightFoot,new Vector3(footAngle,0,0));
            float grabSide=grab&&grab.LeftHand?-1:1;
            Pose(spine,new Vector3(Mathf.Lerp(bend*c.spineBend,c.grabSpine,GrabWeight)+airShape*c.airTorsoBend,GrabWeight*grabSide*c.grabTorsoTwist,displayedCarve*c.carveCounterLean));Pose(head,new Vector3(-bend*c.spineBend*c.headCounterBend,0,-displayedCarve*c.carveCounterLean*.5f));
            float contactDrop=skier.Grounded?Mathf.Max(0,skier.Contacts.Height-skier.config.rideHeight)*motion.groundedAmount:0;
            root.localPosition=Vector3.up*(skier.config.visualRideOffset-c.stanceDrop-bend*c.rootDrop-contactDrop);
            root.localRotation=Quaternion.Euler(0,0,-displayedCarve*skier.config.maximumLean);
            if(hips&&positions.ContainsKey(hips))hips.localPosition=positions[hips]+hips.parent.InverseTransformVector(skier.transform.right*(displayedCarve*c.carveHipShift+grabSide*GrabWeight*c.grabHipShift+Mathf.Sin(sketchAge*18)*recovery*sketchDirection*c.sketchyHipShift));
            float airFold=!skier.Grounded&&!ridingRail?rotation*(1-prepare)*c.airArmFold:0;
            float balance=displayedCarve*c.carveArmBalance+Mathf.Sin(sketchAge*18)*recovery*sketchDirection*c.sketchyArmBalance+(ridingRail?rail.Balance*c.carveArmBalance:0);
            float spread=c.armSpread+prepare*c.landingArmSpread+(ridingRail?c.landingArmSpread:0)+GrabWeight*c.grabCounterArm;
            Pose(leftArm,new Vector3(c.armNeutral-bend*c.armBend+airFold,0,-spread-balance));Pose(rightArm,new Vector3(c.armNeutral-bend*c.armBend+airFold,0,spread-balance));Pose(leftElbow,new Vector3(c.elbowBend-airFold+GrabWeight*20,0,0));Pose(rightElbow,new Vector3(c.elbowBend-airFold+GrabWeight*20,0,0));Pose(leftHand,Vector3.zero);Pose(rightHand,Vector3.zero);
            Pose(leftSki,Vector3.zero);Pose(rightSki,Vector3.zero);
            if(ridingRail)AlignRailFeet(rail);
            else if(skier.Grounded)
            {
                AlignFoot(leftThigh,leftShin,leftFoot,leftSki,skier.Contacts.left);
                AlignFoot(rightThigh,rightShin,rightFoot,rightSki,skier.Contacts.right);
            }
            else if(GrabWeight<1)
            {
                // Knees compress independently while the bindings keep the skis along the rider's heading.
                var forward=Quaternion.AngleAxis(-bend*c.footBend*.2f,skier.transform.right)*skier.transform.forward;
                if(motion.landing.valid&&Vector3.Dot(skier.transform.up,motion.landing.normal)>.55f)forward=Vector3.Slerp(forward,Vector3.ProjectOnPlane(skier.transform.forward,motion.landing.normal).normalized,prepare);
                AlignAirSki(leftFoot,leftSki,forward,1-GrabWeight);AlignAirSki(rightFoot,rightSki,forward,1-GrabWeight);
            }
            bool crossed=!skier.Grounded&&!ridingRail&&(skier.Input.modifierLeft || (grab&&grab.Current==GrabType.CrissCross));
            if(crossed){leftSki.rotation=Quaternion.AngleAxis(c.crossYaw,skier.transform.up)*leftSki.rotation;rightSki.rotation=Quaternion.AngleAxis(-c.crossYaw,skier.transform.up)*rightSki.rotation;}
            if(grabbing)
            {
                PullGrabSki();
                Vector3 target=grab.Target(leftSki,rightSki);
                var hand=grab.LeftHand?leftHand:rightHand;
                SolveArm(grab.LeftHand?leftArm:rightArm,grab.LeftHand?leftElbow:rightElbow,hand,Vector3.Lerp(hand.position,target,GrabWeight),skier.transform.forward);
                if(grabType==GrabType.CrissCross)
                {
                    var otherSki=grab.LeftHand?rightSki:leftSki;var otherArm=grab.LeftHand?rightArm:leftArm;var otherElbow=grab.LeftHand?rightElbow:leftElbow;hand=grab.LeftHand?rightHand:leftHand;
                    PullGrabSki(otherSki,otherArm,otherElbow,hand,otherSki.Find("MidGrab").position);
                    SolveArm(otherArm,otherElbow,hand,Vector3.Lerp(hand.position,otherSki.Find("MidGrab").position,GrabWeight),skier.transform.forward);
                }
            }
            float swing=skier.Grounded||ridingRail?0:(Mathf.Sin(Time.time*c.poleFrequency)*.25f+Mathf.Clamp((motion.rollAngularVelocity+motion.yawAngularVelocity*.35f)/c.rotationPoseSpeed,-1,1))*c.poleSway;
            TrailPole(leftPole,-1,swing);TrailPole(rightPole,1,-swing);
        }
        void PullGrabSki()
        {
            var ski=grab.Ski(leftSki,rightSki);var arm=grab.LeftHand?leftArm:rightArm;var elbow=grab.LeftHand?leftElbow:rightElbow;var hand=grab.LeftHand?leftHand:rightHand;
            PullGrabSki(ski,arm,elbow,hand,grab.Target(leftSki,rightSki));
        }
        void PullGrabSki(Transform ski,Transform arm,Transform elbow,Transform hand,Vector3 target)
        {
            float reach=Vector3.Distance(arm.position,elbow.position)+Vector3.Distance(elbow.position,hand.position)-.015f;
            var toShoulder=arm.position-target;
            float shift=Mathf.Min(Visuals.grabReachAdjustment,Mathf.Max(0,toShoulder.magnitude-reach))*GrabWeight;
            if(shift<.001f)return;
            bool left=ski==leftSki;var foot=left?leftFoot:rightFoot;var oldFoot=foot.position;var rotation=foot.rotation;
            SolveArm(left?leftThigh:rightThigh,left?leftShin:rightShin,foot,oldFoot+toShoulder.normalized*shift,skier.transform.forward);
            // Preserve the boot/binding offset and the independent ski orientation.
            foot.rotation=rotation;ski.position+=foot.position-oldFoot;
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
            var kneeHint=forward+skier.transform.right*displayedCarve*(Visuals.carveKneeBend/45);
            SolveArm(thigh,shin,foot,contact.point+contact.normal*.12f+forward*.03f,kneeHint);
            foot.rotation=Quaternion.FromToRotation(foot.up,forward)*foot.rotation;
            ski.rotation=Quaternion.FromToRotation(ski.up,forward)*ski.rotation;
            ski.position=contact.point+contact.normal*.03f;
            var edge=Quaternion.AngleAxis(-displayedCarve*skier.config.maximumLean*.35f,forward);
            foot.rotation=edge*foot.rotation;ski.rotation=edge*ski.rotation;
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
        void AlignAirSki(Transform foot,Transform ski,Vector3 forward,float weight=1)
        {
            foot.rotation=Quaternion.Slerp(foot.rotation,Quaternion.FromToRotation(foot.up,forward)*foot.rotation,weight);
            ski.rotation=Quaternion.Slerp(ski.rotation,Quaternion.FromToRotation(ski.up,forward)*ski.rotation,weight);
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
