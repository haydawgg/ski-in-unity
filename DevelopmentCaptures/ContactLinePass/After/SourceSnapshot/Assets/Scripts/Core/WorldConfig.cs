using System;
using UnityEngine;
namespace PowderFlow
{
    [Serializable]public class FreestyleFeature
    {
        public string label,assetId,replaceExisting;public Vector3 position,scale=Vector3.one,exitDirection=Vector3.forward;
        public float yaw,approachSpeed;public bool reuseExisting,alternateBranch;
    }
    [Serializable]public class FreestyleLineDefinition
    {
        public string id="FLOW / CONTACT LINE";
        public Vector3 start=new Vector3(0,0,250),exit=new Vector3(-18,0,790);
        public FreestyleFeature[] features={
            new FreestyleFeature{label="Medium kicker",assetId="JumpMedium",position=new Vector3(0,.02f,310),approachSpeed=22,reuseExisting=true},
            new FreestyleFeature{label="Left rail entry",assetId="LineRailEntry",position=new Vector3(-12,.02f,417),approachSpeed=20},
            new FreestyleFeature{label="Left down rail",assetId="Rail_down",replaceExisting="Rail_down",position=new Vector3(-12,0,425),approachSpeed=20},
            new FreestyleFeature{label="Right box entry",assetId="LineBoxEntry",position=new Vector3(12,.02f,417),approachSpeed=20,alternateBranch=true},
            new FreestyleFeature{label="Right wide box",assetId="Box_wide",replaceExisting="Box_wide",position=new Vector3(12,0,425),approachSpeed=20,alternateBranch=true},
            new FreestyleFeature{label="Side hit",assetId="JumpSmall",position=new Vector3(-12,.02f,468),approachSpeed=20},
            new FreestyleFeature{label="Main box entry",assetId="LineBoxEntry",position=new Vector3(-12,.02f,527),approachSpeed=21},
            new FreestyleFeature{label="Down box",assetId="Box_down",replaceExisting="Box_down",position=new Vector3(-12,0,535),approachSpeed=21},
            new FreestyleFeature{label="Large kicker",assetId="JumpLarge",position=new Vector3(-18,.02f,615),approachSpeed=24},
            new FreestyleFeature{label="Exit rail entry",assetId="LineRailEntry",position=new Vector3(-18,.02f,727),approachSpeed=23},
            new FreestyleFeature{label="Wide exit rail",assetId="Rail_wide",replaceExisting="Rail_wide",position=new Vector3(-18,0,735),approachSpeed=23},
            // Clear the new entry from the previous kink approach, retaining it beside the line.
            new FreestyleFeature{label="Existing kink",assetId="Rail_kink",replaceExisting="Rail_kink",position=new Vector3(-26,0,440),approachSpeed=12,alternateBranch=true},
            new FreestyleFeature{label="Existing kink approach",assetId="JumpSmall",position=new Vector3(-26,.02f,417),approachSpeed=12,alternateBranch=true}};
        public Vector3[] mainRoute={new Vector3(0,0,330),new Vector3(-12,0,417),new Vector3(-12,0,449),new Vector3(-12,0,486),new Vector3(-12,0,527),new Vector3(-12,0,558),new Vector3(-18,0,606),new Vector3(-18,0,650),new Vector3(-18,0,727),new Vector3(-18,0,790)};
        public Vector3[] alternateRoute={new Vector3(0,0,330),new Vector3(12,0,417),new Vector3(12,0,444),new Vector3(-12,0,520),new Vector3(-12,0,527),new Vector3(-12,0,558),new Vector3(-18,0,606),new Vector3(-18,0,650),new Vector3(-18,0,727),new Vector3(-18,0,790)};
    }
    [Serializable]public class RidgeLayer
    {
        public int variant;public Vector3 position,scale;public float yaw;
        public RidgeLayer(int variant,Vector3 position,float yaw,Vector3 scale){this.variant=variant;this.position=position;this.yaw=yaw;this.scale=scale;}
    }
    [Serializable]public class SceneryRegion
    {
        public string name;public float from,to,treeWeight,clearance,minScale,maxScale,clusterSpread;
        public SceneryRegion(string name,float from,float to,float weight,float clearance,float minScale,float maxScale,float spread)
        {this.name=name;this.from=from;this.to=to;treeWeight=weight;this.clearance=clearance;this.minScale=minScale;this.maxScale=maxScale;clusterSpread=spread;}
    }
    [CreateAssetMenu(menuName="PowderFlow/World")]
    public class WorldConfig:ScriptableObject
    {
        public int seed=42,treeCount=320,rockCount=70,shrubCount=160;
        public float treeSpacing=5.5f,sceneryEdge=174;
        public SceneryRegion[] sceneryRegions={
            new SceneryRegion("Easy",40,240,.55f,54,.75f,1.05f,18),
            new SceneryRegion("Park",240,800,1.7f,32,.9f,1.3f,13),
            new SceneryRegion("BigAir",800,1030,.35f,62,.75f,1.05f,24),
            new SceneryRegion("Freeride",1030,1320,1.4f,34,.95f,1.4f,17),
            new SceneryRegion("Lower",1320,1450,.7f,45,.65f,1,22)};
        public RidgeLayer[] ridgeLayers={
            new RidgeLayer(0,new Vector3(-780,55,650),90,Vector3.one),
            new RidgeLayer(1,new Vector3(780,25,700),-90,Vector3.one),
            new RidgeLayer(2,new Vector3(0,-35,1570),0,Vector3.one),
            new RidgeLayer(3,new Vector3(-350,-45,2070),7,new Vector3(1.3f,.9f,1.15f)),
            new RidgeLayer(4,new Vector3(400,-55,2520),-8,new Vector3(1.6f,1.25f,1.2f))};
        public float length=1500,width=400,treeExclusion=28,boundary=190,endReset=1480;
        public int liftTowerCount=14;
        public float liftX=-85,liftStartZ=30,liftSpacing=100,regionSignX=-29,boundaryMarkerX=116;
        public Vector3 start=new Vector3(0,0,12),park=new Vector3(0,0,250),bigJump=new Vector3(0,0,820),freeride=new Vector3(75,0,970);
        public float[] jumpLocations={105,310,440,590,900};
        public FreestyleLineDefinition freestyleLine=new FreestyleLineDefinition();
    }
}
