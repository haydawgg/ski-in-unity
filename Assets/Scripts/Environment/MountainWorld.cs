using System;
using UnityEngine;
namespace PowderFlow
{
    public class MountainWorld : MonoBehaviour
    {
        public GraphicsConfig graphicsConfig;public AssetCatalog catalog;public SkiPhysicsConfig physicsConfig;public TrickConfig trickConfig;public CameraConfig cameraConfig;public WorldConfig worldConfig;
        public SkiPhysicsController player;public string Area {get;private set;}="RIDGE / EASY RUN";
        public static float Height(float x,float z)
        {
            float central=Mathf.Exp(-Mathf.Pow(x/48,4));float edge=Mathf.Pow(Mathf.Abs(x)/200,3)*95;
            float ripple=(Mathf.Sin(z*.022f+x*.023f)*3+Mathf.Sin(z*.07f-x*.02f)*1.5f)*(1-central*.82f);
            float freeride=Mathf.Sin(z*.046f)*4*Mathf.Clamp01((x-50)/55);return 355-.23f*z+edge+ripple+freeride;
        }
        public Vector3 OnSnow(float x,float z,float lift=0)=>new Vector3(x,Height(x,z)+lift,z);
        public GameObject Place(string name,Vector3 pos,Quaternion rotation)
        {
            var prefab=catalog.Find(name);if(!prefab)throw new Exception("Missing generated asset "+name);
            var obj=Instantiate(prefab,pos,rotation,transform);obj.name=name;return obj;
        }
        void Awake()
        {
            Place("ValleyFloor",Vector3.zero,Quaternion.identity);
            for(int i=0;i<10;i++)Place($"Mountain{i:00}",Vector3.zero,Quaternion.identity);
            for(int i=0;i<3;i++){Place("Ridge"+i,new Vector3(i==0?-650:i==1?650:0,i==2?-30:100,i==2?1800:600),Quaternion.Euler(0,i==0?90:i==1?-90:0,0));}
            for(int i=0;i<worldConfig.jumpLocations.Length;i++)
            {
                float z=worldConfig.jumpLocations[i];string model=i==0?"JumpSmall":i==3?"Tabletop":i==4?"JumpLarge":"JumpMedium";Place(model,OnSnow(0,z,.02f),Quaternion.identity);
                Place("Flag",OnSnow(-12,z),Quaternion.identity);Place("Flag",OnSnow(12,z),Quaternion.identity);
            }
            var types=new[]{"flat","down","kink","rainbow","wide"};
            for(int i=0;i<5;i++)
            {
                float z=210+i*115;Place("Rail_"+types[i],OnSnow(i%2==0?-17:17,z),Quaternion.identity);
                Place("Box_"+types[Mathf.Min(i,2)],OnSnow(i%2==0?20:-20,z+45),Quaternion.identity);
                Place("JumpSmall",OnSnow(i%2==0?-17:17,z-23),Quaternion.identity);
            }
            Place("QuarterPipe",OnSnow(-45,690),Quaternion.Euler(0,30,0));Place("Hip",OnSnow(45,760),Quaternion.Euler(0,-15,0));
            var random=new System.Random(worldConfig.seed);
            for(int i=0;i<worldConfig.treeCount;i++)
            {
                float z=40+(float)random.NextDouble()*1400,x=(float)random.NextDouble()*360-180;
                if(Mathf.Abs(x)<worldConfig.treeExclusion || (x>55&&x<100&&z>950&&z<1280))continue;
                var tree=Place("Pine"+random.Next(4),OnSnow(x,z),Quaternion.Euler(0,(float)random.NextDouble()*360,0));float scale=.8f+(float)random.NextDouble()*.5f;tree.transform.localScale=Vector3.one*scale;
            }
            for(int i=0;i<worldConfig.rockCount;i++){float z=80+(float)random.NextDouble()*1350,x=(float)random.NextDouble()*360-180;if(Mathf.Abs(x)<45)continue;Place("Rock"+random.Next(6),OnSnow(x,z,-.3f),Quaternion.Euler(0,(float)random.NextDouble()*360,0));}
            Place("Hut",OnSnow(-70,30),Quaternion.Euler(0,20,0));
            for(int i=0;i<14;i++)
            {
                float z=30+i*100;Place("LiftTower",OnSnow(-85,z),Quaternion.identity);Place("LiftCable",OnSnow(-82,z,14),Quaternion.Euler(13,0,0));
                if(i>2&&i<8)Place("Floodlight",OnSnow(-40,z),Quaternion.identity);
                Place("Fence",OnSnow(40,z),Quaternion.identity);
            }
            for(int i=0;i<5;i++)Place("Sign",OnSnow(-9,60+i*300),Quaternion.identity);
            AddPowderZone(new Vector3(90,0,1120),new Vector3(95,70,360));
            Physics.SyncTransforms();
            var spawn=OnSnow(worldConfig.start.x,worldConfig.start.z,physicsConfig.rideHeight);
            PhysicsTestWorld.FactoryOnly=true;var helper=gameObject.AddComponent<PhysicsTestWorld>();PhysicsTestWorld.FactoryOnly=false;helper.enabled=false;helper.physicsConfig=physicsConfig;helper.cameraConfig=cameraConfig;helper.trickConfig=trickConfig;helper.catalog=catalog;
            // The component is added after Awake; disable its world generation via a factory flag.
            helper.SpawnPlayer(spawn,Quaternion.Euler(13,0,0));player=helper.player;
            player.transform.SetParent(transform,true);
            var camera=Camera.main;if(camera)camera.transform.SetParent(transform,true);
            var sun=new GameObject("Mountain sun").AddComponent<Light>();sun.transform.SetParent(transform);sun.type=LightType.Directional;sun.shadows=LightShadows.Soft;sun.intensity=1.7f;sun.transform.rotation=Quaternion.Euler(13,-30,0);
            RenderSettings.ambientLight=new Color(.47f,.37f,.59f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.52f,.50f,.68f);RenderSettings.fogDensity=.0007f;
            if(graphicsConfig){gameObject.AddComponent<AlpineLighting>().config=graphicsConfig;player.gameObject.AddComponent<SnowTrackSystem>().config=graphicsConfig;player.gameObject.AddComponent<SnowEffectsController>().config=graphicsConfig;}
        }
        void AddPowderZone(Vector3 center,Vector3 size)
        {
            center.y=Height(center.x,center.z);var obj=new GameObject("Deep powder");obj.transform.SetParent(transform);obj.transform.position=center;var box=obj.AddComponent<BoxCollider>();box.size=size;box.isTrigger=true;obj.AddComponent<PowderZone>();
        }
        void Update()
        {
            if(!player)return;float z=player.Body.position.z;Area=z<240?"RIDGE / EASY RUN":z<800?"TERRAIN PARK":z<1030?"BIG AIR":z<1320?"FREERIDE / POWDER":"LOWER RUN";
            if(Mathf.Abs(player.Body.position.x)>worldConfig.boundary || z>worldConfig.endReset || z<-30)player.ResetTo(player.startPosition,player.startRotation);
        }
        public void Respawn(int area)
        {
            Vector3 p=area==1?worldConfig.park:area==2?worldConfig.bigJump:area==3?worldConfig.freeride:worldConfig.start;
            var pos=OnSnow(p.x,p.z,physicsConfig.rideHeight);player.ResetTo(pos,Quaternion.Euler(13,0,0));player.safePosition=pos;player.safeRotation=Quaternion.Euler(13,0,0);
        }
    }
}
