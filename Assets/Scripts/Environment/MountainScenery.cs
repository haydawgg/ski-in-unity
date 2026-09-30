using System.Collections.Generic;
using UnityEngine;
namespace PowderFlow
{
    /// <summary>Seeded visual composition; riding surfaces and feature paths remain in MountainWorld.</summary>
    public class MountainScenery:MonoBehaviour
    {
        public Transform Trees {get;private set;}public Transform Accents {get;private set;}public Transform Ranges {get;private set;}
        MountainWorld world;System.Random random;readonly List<Vector2> treePositions=new List<Vector2>();
        float Range(float min,float max)=>Mathf.Lerp(min,max,(float)random.NextDouble());
        Transform Group(string name){var obj=new GameObject(name);obj.transform.SetParent(transform,false);return obj.transform;}
        GameObject Place(string name,float x,float z,float scale,Transform parent,float burial=0)
        {
            var obj=world.Place(name,world.OnSnow(x,z,burial),Quaternion.Euler(0,Range(0,360),0));obj.transform.SetParent(parent,true);obj.transform.localScale=Vector3.one*scale;return obj;
        }
        public static bool IsProtected(WorldConfig config,float x,float z,float radius=0)
        {
            float clearance=config.treeExclusion;
            foreach(var region in config.sceneryRegions)if(z>=region.from&&z<region.to){clearance=Mathf.Max(clearance,region.clearance);break;}
            if(Mathf.Abs(x)<clearance+radius)return true;
            if(z>950-radius&&z<1280+radius&&x>55-radius&&x<100+radius)return true;
            // Hip/quarter-pipe approaches, lift spans and fence retain clean space.
            if(x>-65-radius&&x<-25+radius&&z>665-radius&&z<740+radius)return true;
            if(x>25-radius&&x<65+radius&&z>735-radius&&z<805+radius)return true;
            if(Mathf.Abs(x-config.liftX)<5+radius||Mathf.Abs(x-40)<3+radius)return true;
            return false;
        }
        public void Build(MountainWorld source)
        {
            world=source;random=new System.Random(world.worldConfig.seed);Trees=Group("Tree clusters");Accents=Group("Rock and shrub clusters");Ranges=Group("Layered distant ranges");
            LODGroup.crossFadeAnimationDuration=.3f;
            var settings=world.worldConfig;float weight=0;foreach(var region in settings.sceneryRegions)weight+=region.treeWeight;
            int requested=0;
            for(int r=0;r<settings.sceneryRegions.Length;r++)
            {
                var region=settings.sceneryRegions[r];int count=r==settings.sceneryRegions.Length-1?settings.treeCount-requested:Mathf.RoundToInt(settings.treeCount*region.treeWeight/weight);requested+=count;
                int placed=0,clusterRemaining=0;float cx=0,cz=0;
                for(int attempt=0;attempt<count*60&&placed<count;attempt++)
                {
                    if(clusterRemaining--<=0){cx=Range(region.clearance+10,settings.sceneryEdge-12)*(random.Next(2)==0?-1:1);cz=Range(region.from,region.to);clusterRemaining=random.Next(5,11);}
                    float x=cx+Range(-region.clusterSpread,region.clusterSpread),z=cz+Range(-region.clusterSpread,region.clusterSpread);
                    if(Mathf.Abs(x)>settings.sceneryEdge||z<region.from||z>=region.to||IsProtected(settings,x,z,5))continue;
                    var position=new Vector2(x,z);bool crowded=false;foreach(var existing in treePositions)if((position-existing).sqrMagnitude<settings.treeSpacing*settings.treeSpacing){crowded=true;break;}if(crowded)continue;
                    var tree=Place("Pine"+random.Next(4),x,z,Range(region.minScale,region.maxScale),Trees);tree.name=region.name+" "+tree.name;treePositions.Add(position);placed++;
                }
                if(placed!=count)Debug.LogWarning($"Scenery {region.name}: placed {placed}/{count} trees; clearance took priority");
            }
            int rocks=0,shrubs=0;
            for(int attempt=0;attempt<settings.rockCount*40&&(rocks<settings.rockCount||shrubs<settings.shrubCount);attempt++)
            {
                float cx=Range(50,settings.sceneryEdge-8)*(random.Next(2)==0?-1:1),cz=Range(50,1440);
                if(IsProtected(settings,cx,cz,8))continue;
                int cluster=random.Next(3,6);
                for(int i=0;i<cluster&&rocks<settings.rockCount;i++)
                {
                    float x=cx+Range(-5,5),z=cz+Range(-5,5);if(IsProtected(settings,x,z,3))continue;
                    Place("Rock"+random.Next(6),x,z,Range(.55f,1.15f),Accents,-.25f);rocks++;
                }
                for(int i=0;i<random.Next(5,10)&&shrubs<settings.shrubCount;i++)
                {
                    float x=cx+Range(-9,9),z=cz+Range(-9,9);if(Mathf.Abs(x)>settings.sceneryEdge||IsProtected(settings,x,z,1.5f))continue;
                    Place("Shrub"+random.Next(2),x,z,Range(.65f,1.4f),Accents,-.10f);shrubs++;
                }
            }
            foreach(var layer in settings.ridgeLayers)
            {
                var obj=world.Place("Ridge"+layer.variant,layer.position,Quaternion.Euler(0,layer.yaw,0));obj.transform.SetParent(Ranges,true);obj.transform.localScale=layer.scale;
            }
        }
    }
}
