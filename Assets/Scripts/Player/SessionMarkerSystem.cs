using UnityEngine;
namespace PowderFlow
{
    public class SessionMarkerSystem : MonoBehaviour
    {
        public bool HasMarker { get; private set; }public Vector3 Position { get; private set; }public Quaternion Rotation { get; private set; }
        SkiPhysicsController skier;
        void Start(){skier=GetComponent<SkiPhysicsController>();}
        void Update()
        {
            if(skier.Input.marker && skier.Grounded){HasMarker=true;Position=skier.Body.position;Rotation=skier.Body.rotation;}
            if(skier.Input.returnMarker&&HasMarker)skier.ResetTo(Position,Rotation);
        }
    }
}
