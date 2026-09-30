using UnityEditor;
using UnityEngine;
namespace PowderFlow
{
    public static partial class EditorTools
    {
        [MenuItem("PowderFlow/Configure Presentation")]
        public static void ConfigurePresentation()
        {
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            const string path="Assets/Resources/PowderFlowPresentation.asset";var style=AssetDatabase.LoadAssetAtPath<PresentationConfig>(path);
            bool created=!style;if(created){style=ScriptableObject.CreateInstance<PresentationConfig>();AssetDatabase.CreateAsset(style,path);}
            style.regular=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/DejaVuSans.ttf");style.bold=AssetDatabase.LoadAssetAtPath<Font>("Assets/Art/Fonts/DejaVuSans-Bold.ttf");EditorUtility.SetDirty(style);
            if(created){var camera=Config<CameraConfig>("CameraConfig");camera.distance=6.8f;camera.airHeight=1.8f;camera.lookAhead=1.8f;camera.lookHeight=.35f;camera.followDamping=.14f;camera.portraitPullback=1.35f;EditorUtility.SetDirty(camera);}AssetDatabase.SaveAssets();Debug.Log("PRESENTATION CONFIGURED / responsive theme and camera framing");
        }
    }
}
