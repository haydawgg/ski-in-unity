using UnityEngine;
namespace PowderFlow
{
    public enum SurfaceType { Groomed, Powder, Ice, Rail, Rock }
    public class SnowSurface : MonoBehaviour { public SurfaceType type; public float depth=1; }
}
