using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Game Systems")]
    public class GameSystemConfig:ScriptableObject
    {
        public float sessionDuration=150,collisionBailSpeed=9,inputSensitivity=1,keyboardSensitivity=1,gamepadSensitivity=1,cameraSensitivity=1;
        public AudioClip snow,wind,rail,landing,crash,pop;
        public float maximumAudioSpeed=40,snowVolume=.25f,windVolume=.3f,railVolume=.25f,impactVolume=.55f;
    }
}
