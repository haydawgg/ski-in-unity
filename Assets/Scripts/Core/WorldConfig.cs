using System;
using UnityEngine;
namespace PowderFlow
{
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
        public Vector3 start=new Vector3(0,0,12),park=new Vector3(0,0,280),bigJump=new Vector3(0,0,820),freeride=new Vector3(75,0,970);
        public float[] jumpLocations={105,310,440,590,900};
    }
}
