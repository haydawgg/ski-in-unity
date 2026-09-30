using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Camera")]
    public class CameraConfig : ScriptableObject
    {
        public float distance=6.8f, height=2.2f, airHeight=1.8f, followDamping=.14f, lookAhead=1.8f, lookHeight=.35f;
        public float normalFov=73, speedFov=82, speedReference=35, speedPullback=2.5f, obstructionRadius=.25f, directionResponse=3, cameraSensitivity=1;
        public float portraitPullback=1.35f, landingShake=.07f;
        [Header("Presentation framing")]
        public float airPullback=1.4f,airLookAhead=1.5f,railPullback=.5f,railHeight=2.1f,railLookAhead=1.8f,bailPullback=3,bailHeight=3.2f,bailOrbit=10;
        public float nearClip=.08f,obstructionPadding=.12f,landingLookSeconds=.5f,landingAnticipation=.25f,fovResponse=4,shakeRecovery=.28f;
        public Vector3 menuOffset=new Vector3(32,12,58),menuLook=new Vector3(0,-22,180);public float menuFov=58;
    }
}
