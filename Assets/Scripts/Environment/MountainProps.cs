using UnityEngine;
namespace PowderFlow
{
    public class MountainProps:MonoBehaviour
    {
        public Transform Lift {get;private set;}public Transform Signs {get;private set;}public Transform Markers {get;private set;}
        MountainWorld world;
        Transform Group(string name){var obj=new GameObject(name);obj.transform.SetParent(transform,false);return obj.transform;}
        GameObject Place(string name,Vector3 point,Quaternion rotation,Transform parent){var obj=world.Place(name,point,rotation);obj.transform.SetParent(parent,true);return obj;}
        public void Build(MountainWorld source)
        {
            world=source;Lift=Group("Connected chairlift");Signs=Group("Region wayfinding");Markers=Group("Feature and boundary markers");var config=world.worldConfig;
            Place("Hut",world.OnSnow(-70,30),Quaternion.Euler(0,20,0),transform);
            Place("StartGate",world.OnSnow(0,18),Quaternion.identity,Markers);
            for(int i=0;i<config.liftTowerCount;i++)
            {
                float z=config.liftStartZ+i*config.liftSpacing;Place("LiftTower",world.OnSnow(config.liftX,z),Quaternion.identity,Lift);
                if(i>2&&i<8)Place("Floodlight",world.OnSnow(-40,z),Quaternion.identity,transform);
                Place("Fence",world.OnSnow(40,z),Quaternion.identity,transform);
                if(i==config.liftTowerCount-1)continue;
                foreach(float side in new[]{-3f,3f})
                {
                    var a=world.OnSnow(config.liftX,z,14.67f)+Vector3.right*side;var b=world.OnSnow(config.liftX,z+config.liftSpacing,14.67f)+Vector3.right*side;
                    var rotation=Quaternion.LookRotation(b-a,Vector3.up);var cable=Place("LiftCable",a,rotation,Lift);cable.transform.localScale=new Vector3(1,1,(b-a).magnitude/100);
                    foreach(float t in new[]{.32f,.68f})
                    {
                        var hanger=Vector3.Lerp(a,b,t)-(rotation*Vector3.up)*(4*t*(1-t));Place("LiftChair",hanger,Quaternion.Euler(0,side<0?0:180,0),Lift);
                    }
                }
            }
            string[] regions={"Easy","Park","BigAir","Freeride","Lower"};float[] regionZ={60,260,815,1040,1335};
            for(int i=0;i<regions.Length;i++)Place("Sign"+regions[i],world.OnSnow(config.regionSignX,regionZ[i]),Quaternion.identity,Signs);
            for(int i=0;i<config.jumpLocations.Length;i++)
            {
                float z=config.jumpLocations[i],offset=i==4?14:12;string level=i==0?"FeatureEasy":i==4?"FeatureExpert":"FeatureMedium";
                foreach(float side in new[]{-1f,1f}){Place(level,world.OnSnow(offset*side,z-8),Quaternion.identity,Markers);Place("Flag",world.OnSnow(offset*side,z),Quaternion.identity,Markers);}
            }
            for(int i=0;i<5;i++)
            {
                float z=210+i*115,x=i%2==0?-17:17;Place(i<2?"FeatureEasy":"FeatureMedium",world.OnSnow(x+(x<0?-4:4),z-8),Quaternion.identity,Markers);
                float bx=i%2==0?20:-20;Place(i<2?"FeatureEasy":"FeatureMedium",world.OnSnow(bx+(bx<0?-4:4),z+37),Quaternion.identity,Markers);
            }
            for(float z=80;z<1410;z+=70)foreach(float x in new[]{-config.boundaryMarkerX,config.boundaryMarkerX})Place("BoundaryMarker",world.OnSnow(x,z),Quaternion.identity,Markers);
        }
    }
}
