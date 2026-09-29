using UnityEngine;
namespace PowderFlow
{
    public class SkiDebugOverlay : MonoBehaviour
    {
        public SkiPhysicsController skier; public bool visible;bool draw;
        void Update() { if(skier.Input.debugToggle)visible=!visible;if(UnityEngine.InputSystem.Keyboard.current?.f2Key.wasPressedThisFrame==true)draw=!draw; }
        void OnDrawGizmos(){if(!draw||!skier)return;Gizmos.color=Color.cyan;Gizmos.DrawRay(skier.Contacts.left.point,skier.Contacts.left.normal);Gizmos.DrawRay(skier.Contacts.right.point,skier.Contacts.right.normal);Gizmos.color=Color.yellow;Gizmos.DrawRay(skier.transform.position,skier.Body.linearVelocity*.25f);}
        void OnGUI()
        {
            if(!skier||!visible)return;
            GUI.Box(new Rect(15,70,410,320),"PHYSICS / F1");
            var c=skier.Contacts;
            GUI.Label(new Rect(30,100,390,290),$"Speed {skier.Speed:F1} m/s / {skier.Speed*3.6f:F0} km/h\nVelocity {skier.Body.linearVelocity}\nGrounded {skier.Grounded}   L {c.left.hit} R {c.right.hit}\nSlope {Vector3.Angle(Vector3.up,skier.SupportNormal):F1}°\nNormal {skier.SupportNormal}\nEdge {skier.Edge:F2}  lateral slip {skier.Slip:F2}\nSurface {c.Surface}\nAngular {skier.Body.angularVelocity}\nAir {skier.Air.AirTime:F2}s  Y/P/R {GetComponent<TrickTracker>()?.current.yaw:F0}/{GetComponent<TrickTracker>()?.current.pitch:F0}/{GetComponent<TrickTracker>()?.current.roll:F0}\nGrab {GetComponent<GrabSystem>()?.Current} Rail {GetComponent<RailSystem>()?.Riding}\nLanding {skier.LastLanding.quality}  FPS {1/Mathf.Max(.001f,Time.smoothDeltaTime):F0}");
        }
    }
}
