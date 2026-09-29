using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Camera")]
    public class CameraConfig : ScriptableObject
    {
        public float distance=6, height=2.2f, airHeight=1.2f, followDamping=.18f, lookAhead=3, lookHeight=.55f;
        public float normalFov=73, speedFov=82, speedReference=35, speedPullback=2.5f, obstructionRadius=.25f, directionResponse=3, cameraSensitivity=1;
        public float portraitPullback=1.35f, landingShake=.07f;
    }
}
