using UnityEngine;
namespace PowderFlow
{
    [CreateAssetMenu(menuName="PowderFlow/World")]
    public class WorldConfig:ScriptableObject
    {
        public int seed=42,treeCount=240,rockCount=70;
        public float length=1500,width=400,treeExclusion=28,boundary=190,endReset=1480;
        public Vector3 start=new Vector3(0,0,12),park=new Vector3(0,0,280),bigJump=new Vector3(0,0,820),freeride=new Vector3(75,0,970);
        public float[] jumpLocations={105,310,440,590,900};
    }
}
