using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Camera")]
    public class CameraConfig : ScriptableObject
    {
        public float distance=4.6f, height=1.6f, airHeight=1.25f, followDamping=.11f, lookAhead=1.8f, lookHeight=.35f;
        public float normalFov=67, speedFov=76, speedReference=35, speedPullback=.9f, obstructionRadius=.25f, directionResponse=3, cameraSensitivity=1;
        public float portraitPullback=1.35f, landingShake=.07f;
        [Header("Presentation framing")]
        public float airPullback=.45f,airLookAhead=1.5f,railPullback=.35f,railHeight=1.6f,railLookAhead=1.8f,bailPullback=3,bailHeight=3.2f,bailOrbit=10;
        public float nearClip=.08f,obstructionPadding=.12f,landingLookSeconds=.5f,landingAnticipation=.25f,fovResponse=4,shakeRecovery=.28f;
        public Vector3 menuOffset=new Vector3(32,12,58),menuLook=new Vector3(0,-22,180);public float menuFov=58;
    }
}
