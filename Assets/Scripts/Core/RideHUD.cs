using UnityEngine;
namespace PowderFlow
{
    public class RideHUD:MonoBehaviour
    {
        public SkiPhysicsController skier;ComboSystem score;Presentation ui;GameFlow flow;GUIStyle speedText,rightText,feedbackText,detailText;
        void Start(){score=skier.GetComponent<ComboSystem>();flow=GetComponentInParent<GameFlow>();}
        void OnGUI()
        {
            if(!skier||(flow&&flow.menu))return;
            if(ui==null)
            {
                ui=new Presentation(PresentationConfig.Active);
                speedText=new GUIStyle(ui.metric){fontSize=ui.config.hudSize,alignment=TextAnchor.UpperLeft};
                rightText=new GUIStyle(speedText){alignment=TextAnchor.UpperRight};
                feedbackText=new GUIStyle(ui.body){alignment=TextAnchor.UpperRight};detailText=new GUIStyle(ui.small){alignment=TextAnchor.UpperRight};
            }
            var c=new PresentationCanvas(Screen.width,Screen.height,Screen.safeArea);var old=GUI.matrix;var color=GUI.color;GUI.matrix=c.Matrix;float m=ui.config.margin;
            try
            {
                string units=SaveStore.Current.mph?"mph":"km/h";
                ui.ShadowText(new Rect(m,m,170,34),(skier.Speed*(SaveStore.Current.mph?2.23694f:3.6f)).ToString("F0")+" "+units,speedText,ui.config.text);
                bool session=flow&&GameFlow.ScoreSession;
                if(score&&session)
                {
                    string multiplier=score.Multiplier>1?"  ×"+score.Multiplier:"";
                    ui.ShadowText(new Rect(c.width-m-340,m,340,34),score.Total.ToString("N0")+multiplier,rightText,ui.config.text);
                    ui.ShadowText(new Rect(c.width-m-170,m+32,170,25),Mathf.CeilToInt(Mathf.Max(0,flow.Remaining))+" s",detailText,ui.config.text);
                }
                if(score)
                {
                    float alpha=ui.config.FeedbackAlpha(score.FeedbackTime);
                    if(alpha>0)
                    {
                        GUI.color=new Color(1,1,1,alpha);var r=c.FeedbackRect(session,m);
                        ui.ShadowText(new Rect(r.x,r.y,r.width,46),score.LastName,feedbackText,ui.config.text);
                        ui.ShadowText(new Rect(r.x,r.y+48,r.width,24),score.LastQuality.ToString().ToUpperInvariant()+"  +"+score.LastPoints.ToString("N0"),detailText,ui.config.LandingColor(score.LastQuality));
                    }
                }
            }
            finally{GUI.matrix=old;GUI.color=color;}
        }
    }
}
