using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/Presentation")]
    public class PresentationConfig:ScriptableObject
    {
        public Font regular,bold;
        public Color panel=new Color(.055f,.085f,.12f,.97f),card=new Color(.10f,.145f,.18f,.98f),line=new Color(.23f,.31f,.35f),text=new Color(.94f,.94f,.89f),muted=new Color(.65f,.73f,.76f),focus=new Color(.25f,.74f,.66f),warm=new Color(1,.69f,.36f);
        public Color perfect=new Color(.35f,.87f,.72f),clean=new Color(.52f,.77f,1),sketchy=new Color(1,.74f,.37f),bail=new Color(1,.48f,.47f);
        public float margin=32,settingRow=46,feedbackDuration=1.8f,feedbackFade=.5f,previewYaw=30,previewFov=35;
        public int titleSize=44,bodySize=18,smallSize=13,metricSize=36,hudSize=22;
        public Color hudShadow=new Color(0,.015f,.025f,.8f);public float hudShadowOffset=1.2f;
        static PresentationConfig active;
        public static PresentationConfig Active {get {if(!active)active=Resources.Load<PresentationConfig>("PowderFlowPresentation");if(!active){active=CreateInstance<PresentationConfig>();active.hideFlags=HideFlags.HideAndDontSave;}return active;}}
        public Color LandingColor(LandingQuality quality)=>quality==LandingQuality.Perfect?perfect:quality==LandingQuality.Clean?clean:quality==LandingQuality.Sketchy?sketchy:bail;
        public float FeedbackAlpha(float remaining)=>Mathf.Clamp01((remaining-(4-feedbackDuration))/feedbackFade);
    }
}
