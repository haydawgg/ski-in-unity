using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Tricks")]
    public class TrickConfig : ScriptableObject
    {
        public float baseInertia=18, tuckInertiaReduction=.35f, yawTorque=78, pitchTorque=62, rollTorque=48, angularDrag=.03f, maximumAngularSpeed=12;
        public float preloadRate=3, maximumPreload=3.8f, takeoffContactLock=.18f;
        public float perfectAlignment=15, cleanAlignment=35, sketchyAlignment=60, uprightBailAngle=65;
        public float perfectImpact=10.5f, cleanImpact=21, bailImpact=34, bailAngularSpeed=13;
        public float assistHeight=2.4f, assistAngle=42, assistResponse=3, landingAngularDamping=4, compressionResponse=8;
        [Header("Trajectory and spotting")]
        public float predictionHorizon=.8f, predictionStep=.05f, landingPrepareTime=.46f;
        public float spottingTime=.42f, spottingAngularDamping=6, rotationReleaseThreshold=.08f;
        [Header("Rail approach")]
        public float railCaptureResponse=14;
        public float terrainRecaptureImpact=2.5f;
        public float perfectRetention=.99f, cleanRetention=.91f, sketchyRetention=.76f, flatImpactScrub=.15f;
        public float rotationTolerance=30, scorePerHalfTurn=180, flipScore=550, grabScorePerSecond=260, switchBonus=1.25f, corkBonus=1.3f, comboTimeout=7, repeatDecay=.22f;
        public float bailRecovery=2, grabBuffer=.14f, railCaptureDistance=.85f, railMinimumSpeed=2, railMaximumVerticalSpeed=7, railFriction=.6f, railBalanceResponse=1.2f, railInstability=.3f, railPop=3.5f, railScorePerSecond=320;
    }
}
