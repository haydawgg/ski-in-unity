using UnityEngine;
namespace PowderFlow
{
    public class SkiDebugOverlay : MonoBehaviour
    {
        public SkiPhysicsController skier; public bool visible;bool draw;int slowIndex;
        void Update()
        {
            if(skier.Input.debugToggle)visible=!visible;
            var keyboard=UnityEngine.InputSystem.Keyboard.current;
            if(keyboard?.f2Key.wasPressedThisFrame==true)draw=!draw;
            if(keyboard?.f3Key.wasPressedThisFrame==true&&Time.timeScale>0){slowIndex=(slowIndex+1)%3;Time.timeScale=slowIndex==1?.5f:slowIndex==2?.25f:1;visible=true;}
        }
        void OnDrawGizmos(){if(!draw||!skier)return;Gizmos.color=Color.cyan;Gizmos.DrawRay(skier.Contacts.left.point,skier.Contacts.left.normal);Gizmos.DrawRay(skier.Contacts.right.point,skier.Contacts.right.normal);Gizmos.color=Color.yellow;Gizmos.DrawRay(skier.transform.position,skier.Body.linearVelocity*.25f);}
        void OnGUI()
        {
            if(!skier||!visible)return;
            GUI.Box(new Rect(15,70,440,390),"PHYSICS / F1   SLOW MOTION / F3");
            var c=skier.Contacts;
            GUI.Label(new Rect(30,100,420,360),$"Speed {skier.Speed:F1} m/s / {skier.Speed*3.6f:F0} km/h\nVelocity {skier.Body.linearVelocity}\nGrounded {skier.Grounded}   L {c.left.hit} R {c.right.hit}\nSlope {Vector3.Angle(Vector3.up,skier.SupportNormal):F1}°\nNormal {skier.SupportNormal}\nEdge {skier.Edge:F2}  lateral slip {skier.Slip:F2}\nSurface {c.Surface}\nAngular {skier.Body.angularVelocity}\nAir {skier.Air.AirTime:F2}s  Y/P/R {GetComponent<TrickTracker>()?.current.yaw:F0}/{GetComponent<TrickTracker>()?.current.pitch:F0}/{GetComponent<TrickTracker>()?.current.roll:F0}\nGrab {GetComponent<GrabSystem>()?.Current} Rail {GetComponent<RailSystem>()?.Riding}\nLanding {skier.LastLanding.quality}  Prediction {skier.PredictedLanding.valid}  contact in {skier.PredictedLanding.time:F2}s\nCarve load {skier.Motion.lateralAcceleration:F1}  bend {GetComponent<SkierPose>()?.Bend:F2}\nCompression {skier.Compression:F2}  IK {GetComponent<GrabSystem>()?.Blend:F2}  time {Time.timeScale:F2}x\nFPS {1/Mathf.Max(.001f,Time.smoothDeltaTime):F0}");
        }
    }
}
