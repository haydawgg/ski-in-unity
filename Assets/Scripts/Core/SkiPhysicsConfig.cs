using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Ski Physics")]
    public class SkiPhysicsConfig : ScriptableObject
    {
        public float visualRideOffset=.07f;
        public float mass=80, rideHeight=.75f, probeLift=.3f, probeRange=1.8f, probeRadius=.06f, skiSeparation=.38f, contactHalfLength=.5f;
        public float contactReach=1.05f, supportSpring=180, supportDamping=25, normalResponse=9;
        public float snowFriction=.035f, iceFriction=.012f, powderFriction=.07f, powderDrag=.35f;
        public float flatGrip=2, edgeGrip=11.5f, maximumGrip=20, steerResponse=7, sidecutRadius=12, speedRadiusScale=900;
        public float brakeRotationRate=2,
            brakeDeceleration=9, skidGrip=.65f, airDensity=1.225f, uprightDragArea=.60f, tuckDragArea=.28f;
        public float rotationResponse=9, maximumLean=36, safeSpeed=28, resetDepth=-500, safeRecordInterval=2;
        public float popImpulse=3.8f, popSpeedScale=.025f, popBuffer=.14f, crouchHeight=.12f;
    }
}
