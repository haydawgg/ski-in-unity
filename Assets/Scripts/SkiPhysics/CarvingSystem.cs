using UnityEngine;
namespace PowderFlow
{
    public static class CarvingSystem
    {
        public static float Radius(float speed,float edge,SkiPhysicsConfig c) => c.sidecutRadius*(1+speed*speed/c.speedRadiusScale)/Mathf.Max(.05f,Mathf.Abs(edge));
        public static float Friction(SurfaceType surface,SkiPhysicsConfig c) => surface==SurfaceType.Powder?c.powderFriction:surface==SurfaceType.Ice?c.iceFriction:c.snowFriction;
        public static Vector3 Drag(Vector3 velocity,bool tuck,SkiPhysicsConfig c)
            => -.5f*c.airDensity*(tuck?c.tuckDragArea:c.uprightDragArea)*velocity.magnitude*velocity/c.mass;
    }
}
