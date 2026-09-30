using UnityEngine;
namespace PowderFlow
{
    public class OutfitSystem : MonoBehaviour
    {
        public int preset;
        public void Apply(int selected)
        {
            var pose=GetComponent<SkierPose>();if(!pose||!pose.root)return;
            var palettes=pose.Visuals.outfits;preset=((selected%palettes.Length)+palettes.Length)%palettes.Length;var p=palettes[preset];
            foreach(var r in pose.root.GetComponentsInChildren<Renderer>())foreach(var m in r.materials)
            {
                string name=m.name.Replace(" (Instance)","");
                if(name=="jacket")m.color=p.jacket;else if(name=="jacket_accent"||name=="helmet_accent")m.color=p.accent;
                else if(name=="pants")m.color=p.pants;else if(name=="pants_panel")m.color=p.panels;
                else if(name=="ski")m.color=p.skis;else if(name=="ski_graphic")m.color=p.graphics;
            }
        }
    }
}
