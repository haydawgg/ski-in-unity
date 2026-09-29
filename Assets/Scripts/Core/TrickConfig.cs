using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Tricks")]
    public class TrickConfig : ScriptableObject
    {
        public float baseInertia=18, tuckInertiaReduction=.35f, yawTorque=60, pitchTorque=48, rollTorque=38, angularDrag=.03f, maximumAngularSpeed=12;
        public float preloadRate=3, maximumPreload=3.8f, takeoffContactLock=.18f;
        public float perfectAlignment=15, cleanAlignment=35, sketchyAlignment=60, uprightBailAngle=65;
        public float perfectImpact=9, cleanImpact=18, bailImpact=34, bailAngularSpeed=13;
        public float assistHeight=2.4f, assistAngle=42, assistResponse=3, landingAngularDamping=4, compressionResponse=8;
        public float perfectRetention=.99f, cleanRetention=.91f, sketchyRetention=.67f, flatImpactScrub=.15f;
        public float rotationTolerance=30, scorePerHalfTurn=180, flipScore=550, grabScorePerSecond=260, switchBonus=1.25f, corkBonus=1.3f, comboTimeout=7, repeatDecay=.22f;
        public float bailRecovery=2, grabBuffer=.14f, railCaptureDistance=.85f, railMinimumSpeed=2, railMaximumVerticalSpeed=7, railFriction=.6f, railBalanceResponse=1.2f, railInstability=.3f, railPop=3.5f, railScorePerSecond=320;
    }
}
