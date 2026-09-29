using UnityEngine;
namespace PowderFlow
{
    public class OutfitSystem : MonoBehaviour
    {
        public int preset;
        public void Apply(int selected)
        {
            preset=selected;var colors=new[]{new Color(.13f,.44f,.28f),new Color(.39f,.18f,.56f),new Color(.055f,.065f,.09f),new Color(.45f,.48f,.52f),new Color(.32f,.36f,.15f),new Color(.92f,.32f,.08f)};
            var pose=GetComponent<SkierPose>();if(!pose||!pose.root)return;
            foreach(var r in pose.root.GetComponentsInChildren<Renderer>())foreach(var m in r.materials){if(m.name.StartsWith("jacket"))m.color=colors[selected%colors.Length];if(m.name.StartsWith("ski"))m.color=selected%2==0?new Color(.08f,.6f,.62f):new Color(.88f,.55f,.22f);}
        }
    }
}
