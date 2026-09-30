using UnityEngine;

namespace PowderFlow
{
    // A physics snapshot shared by pose/debug consumers; it never moves the body.
    public struct SkierMotionState
    {
        public float speed, normalizedSpeed, verticalSpeed, groundedAmount, slopeAngle;
        public float carveAmount, lateralAcceleration, edgeAmount, skidAmount, crouchAmount;
        public float airborneAmount, airTime, yawAngularVelocity, pitchAngularVelocity, rollAngularVelocity;
        public float grabAmount, railAmount, landingPrediction, landingCompression, impactStrength;
        public float switchAmount, balanceAmount, popAmount;
        public LandingPrediction landing;
    }
}
