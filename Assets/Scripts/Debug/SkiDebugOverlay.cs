using UnityEngine;
namespace PowderFlow
{
    public class SkiDebugOverlay : MonoBehaviour
    {
        public SkiPhysicsController skier; public bool visible;
        void Update() { if(skier.Input.debugToggle)visible=!visible; }
        void OnGUI()
        {
            if(!skier||!visible)return;
            GUI.Box(new Rect(15,70,360,245),"PHYSICS / F1");
            var c=skier.Contacts;
            GUI.Label(new Rect(30,100,330,220),$"Speed {skier.Speed:F1} m/s / {skier.Speed*3.6f:F0} km/h\nVelocity {skier.Body.linearVelocity}\nGrounded {skier.Grounded}   L {c.left.hit} R {c.right.hit}\nSlope {Vector3.Angle(Vector3.up,skier.SupportNormal):F1}°\nNormal {skier.SupportNormal}\nEdge {skier.Edge:F2}  lateral slip {skier.Slip:F2}\nSurface {c.Surface}\nFPS {1/Mathf.Max(.001f,Time.smoothDeltaTime):F0}");
        }
    }
}
