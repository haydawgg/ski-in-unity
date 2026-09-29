using UnityEngine;
namespace PowderFlow
{
    public class PowderZone : MonoBehaviour
    {
        void OnTriggerEnter(Collider collider){var p=collider.GetComponent<SkiPhysicsController>();if(p)p.PowderOverride=true;}
        void OnTriggerExit(Collider collider){var p=collider.GetComponent<SkiPhysicsController>();if(p)p.PowderOverride=false;}
    }
}
