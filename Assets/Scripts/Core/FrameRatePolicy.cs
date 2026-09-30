using UnityEngine;

namespace PowderFlow
{
    public static class FrameRatePolicy
    {
        public const int FramesPerSecond = 144;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Apply()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = FramesPerSecond;
        }
    }
}
