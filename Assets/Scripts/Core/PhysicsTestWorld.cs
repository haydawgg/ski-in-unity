using UnityEngine;
namespace PowderFlow
{
    public class PhysicsTestWorld : MonoBehaviour
    {
        public SkiPhysicsConfig physicsConfig; public CameraConfig cameraConfig; public TrickConfig trickConfig;
        public SkiPhysicsController player;public AssetCatalog catalog;
        public static Material Material(Color color)
        {
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;return m;
        }
        public static GameObject Solid(string name, Vector3 pos, Vector3 scale, Quaternion rot, Color color)
        {
            var obj=GameObject.CreatePrimitive(PrimitiveType.Cube);obj.name=name;obj.transform.SetPositionAndRotation(pos,rot);obj.transform.localScale=scale;
            obj.GetComponent<Renderer>().sharedMaterial=Material(color);return obj;
        }
        void Awake()
        {
            for(int i=0;i<5;i++)
            {
                float angle=new[]{0,5,15,30,45}[i];var rotation=Quaternion.Euler(angle,0,0);
                var slope=Solid($"{angle} degree slope",new Vector3(i*50,-15,100),new Vector3(44,1,220),rotation,new Color(.78f,.88f,.94f));
                slope.AddComponent<SnowSurface>();
            }
            var powder=Solid("Powder zone",new Vector3(65,-2,0),new Vector3(15,1,30),Quaternion.Euler(5,0,0),new Color(.9f,.83f,.98f));
            powder.AddComponent<SnowSurface>().type=SurfaceType.Powder;
            TestFeatureBuilder.Kicker(new Vector3(100,3,35),10,12,4);
            TestFeatureBuilder.Kicker(new Vector3(100,-11,90),14,18,7);
            for(int j=0;j<3;j++)
            {
                var rail=new GameObject("Temporary rail "+j).AddComponent<RailPath>();rail.transform.position=new Vector3(93+j*7,1,50+j*12);rail.railId=j==0?"Flat":j==1?"Down":"Kink";
                rail.points=j==2?new[]{Vector3.zero,new Vector3(0,-1,5),new Vector3(0,-3,10)}:new[]{Vector3.zero,new Vector3(0,j==0?0:-3,10)};
                for(int k=1;k<rail.points.Length;k++){var a=rail.transform.TransformPoint(rail.points[k-1]);var b=rail.transform.TransformPoint(rail.points[k]);var beam=Solid("Rail tube",(a+b)*.5f,new Vector3(.13f,.13f,Vector3.Distance(a,b)),Quaternion.LookRotation(b-a),Color.black);beam.transform.SetParent(rail.transform,true);}
            }
            SpawnPlayer(new Vector3(100,13,0),Quaternion.Euler(15,0,0));
            var sun=new GameObject("Sun").AddComponent<Light>();sun.type=LightType.Directional;sun.intensity=1.6f;sun.transform.rotation=Quaternion.Euler(25,-35,0);sun.shadows=LightShadows.Soft;
            RenderSettings.ambientLight=new Color(.42f,.49f,.63f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.6f,.7f,.83f);RenderSettings.fogDensity=.0008f;
        }
        public void SpawnPlayer(Vector3 position,Quaternion rotation)
        {
            var obj=new GameObject("Skier");obj.transform.SetPositionAndRotation(position,rotation);
            var capsule=obj.AddComponent<CapsuleCollider>();capsule.height=1.25f;capsule.radius=.25f;capsule.center=new Vector3(0,.15f,0);
            capsule.material=new PhysicsMaterial("Frictionless body"){dynamicFriction=0,staticFriction=0,bounciness=0};
            obj.AddComponent<Rigidbody>(); obj.AddComponent<SkierInput>();obj.AddComponent<SkiContactSystem>();
            player=obj.AddComponent<SkiPhysicsController>();player.Initialize(physicsConfig);player.trickConfig=trickConfig;
            var pose=obj.AddComponent<SkierPose>();pose.Bind(catalog&&catalog.Find("Skier") ? Instantiate(catalog.Find("Skier"),obj.transform).transform : TemporarySkierBuilder.Build(obj.transform));obj.AddComponent<OutfitSystem>();
            obj.AddComponent<TrickTracker>();obj.AddComponent<GrabSystem>();obj.AddComponent<ComboSystem>();obj.AddComponent<SessionMarkerSystem>();obj.AddComponent<BailSystem>();obj.AddComponent<RailSystem>();
            obj.AddComponent<RideHUD>().skier=player;
            var camera=new GameObject("Follow camera").AddComponent<Camera>();camera.tag="MainCamera";camera.farClipPlane=3000;camera.transform.position=position-new Vector3(0,-3,6);camera.gameObject.AddComponent<AudioListener>();
            var follow=camera.gameObject.AddComponent<SkiCameraController>();follow.skier=player;follow.config=cameraConfig;
            obj.AddComponent<SkiDebugOverlay>().skier=player;
        }
    }
}
