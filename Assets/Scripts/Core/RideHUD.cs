using UnityEngine;
namespace PowderFlow
{
    public class RideHUD : MonoBehaviour
    {
        public SkiPhysicsController skier;ComboSystem score;GUIStyle big,small;
        void Start(){score=skier.GetComponent<ComboSystem>();}
        void OnGUI()
        {
            if(!skier)return;
            if(big==null){big=new GUIStyle(GUI.skin.label){fontSize=30,fontStyle=FontStyle.Bold};small=new GUIStyle(GUI.skin.label){fontSize=16};}
            GUI.Label(new Rect(22,18,240,50),$"{skier.Speed*3.6f:F0} km/h",big);
            if(score){GUI.Label(new Rect(Screen.width-230,20,220,40),$"{score.Total:N0}   ×{score.Multiplier}",big);
                if(score.FeedbackTime>0){float width=Mathf.Min(600,Screen.width-40);var x=(Screen.width-width)/2;GUI.Label(new Rect(x,Screen.height*.74f,width,45),score.LastName,big);GUI.Label(new Rect(x,Screen.height*.74f+42,width,35),$"{score.LastQuality.ToString().ToUpper()}  +{score.LastPoints:N0}",small);}}
            GUI.Label(new Rect(20,Screen.height-34,Screen.width-40,30),"SPACE pop   Q/E grabs   SHIFT/CTRL styles   R retry   T set marker   Y marker   F1 debug",small);
        }
    }
}
