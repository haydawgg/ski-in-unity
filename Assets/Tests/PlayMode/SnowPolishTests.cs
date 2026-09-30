using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace PowderFlow.Tests
{
    public class SnowPolishTests:PlayWorldTestBase
    {
#if UNITY_EDITOR
        bool asyncCompilation;
        [SetUp]public void SynchronousShaders(){asyncCompilation=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;}
        [TearDown]public void RestoreShaders(){ShaderUtil.allowAsyncCompilation=asyncCompilation;}
        static MountainWorld CreateWorld(GraphicsConfig graphics=null)
        {
            var root=new GameObject("Snow action review");root.SetActive(false);var world=root.AddComponent<MountainWorld>();
            world.catalog=AssetDatabase.LoadAssetAtPath<AssetCatalog>("Assets/Settings/AssetCatalog.asset");world.physicsConfig=AssetDatabase.LoadAssetAtPath<SkiPhysicsConfig>("Assets/Settings/SkiPhysicsConfig.asset");world.trickConfig=AssetDatabase.LoadAssetAtPath<TrickConfig>("Assets/Settings/TrickConfig.asset");world.cameraConfig=AssetDatabase.LoadAssetAtPath<CameraConfig>("Assets/Settings/CameraConfig.asset");world.worldConfig=AssetDatabase.LoadAssetAtPath<WorldConfig>("Assets/Settings/WorldConfig.asset");world.graphicsConfig=graphics?graphics:AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset");root.SetActive(true);return world;
        }
        static void Spawn(MountainWorld world,float x,float z,float speed=14,float lift=0,bool switched=false)
        {
            var p=world.player;p.Input.steer=0;p.Input.brake=false;p.Input.tuck=true;
            Assert.That(Physics.Raycast(world.OnSnow(x,z,40),Vector3.down,out var hit,80,~(1<<8),QueryTriggerInteraction.Ignore),Is.True);
            var forward=Vector3.ProjectOnPlane(Vector3.forward,hit.normal).normalized;
            p.ResetTo(hit.point+hit.normal*(p.config.rideHeight+lift),Quaternion.LookRotation(switched?-forward:forward,hit.normal));p.Body.linearVelocity=forward*speed;
            if(lift>0)p.EnterAirWithoutRestartingTrick();
        }
        static int Particles(SkiPhysicsController player){int count=0;foreach(var pool in player.GetComponentsInChildren<ParticleSystem>())count+=pool.particleCount;return count;}
        static void Capture(Camera camera,string path,int width=1920,int height=1080)
        {
            var old=camera.targetTexture;var previous=RenderTexture.active;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllBytes(path,image.EncodeToPNG());camera.targetTexture=old;RenderTexture.active=previous;Object.Destroy(image);Object.Destroy(rt);
        }
#endif
        [UnityTest]public IEnumerator ActionViewsAndTransitions()
        {
#if UNITY_EDITOR
            bool baseline=System.Environment.GetEnvironmentVariable("POWDERFLOW_SNOW_BASELINE")=="1";
            string output="Documentation/VisualPolish/Phase5"+(baseline?"/Before":"");Directory.CreateDirectory(output);
            var world=CreateWorld();yield return null;var p=world.player;p.Input.BeginInjected();Camera camera=null;
            foreach(var follow in world.GetComponentsInChildren<SkiCameraController>()){follow.enabled=false;camera=follow.GetComponent<Camera>();}Assert.That(camera,Is.Not.Null);camera.fieldOfView=58;
            int landings=0;p.Landed+=r=>{if(r.quality!=LandingQuality.Bail)landings++;};var stats=new List<string>();
            void Shot(string preset,string label)
            {
                camera.transform.position=p.Body.position+new Vector3(-4,4.3f,-8);camera.transform.LookAt(p.Body.position+new Vector3(0,-.25f,-1.5f));Capture(camera,output+"/"+preset+label+".png");
                stats.Add($"{preset} {label}: grounded={p.Grounded}, surface={p.Surface}, speed={p.Speed:F2}, particles={Particles(p)}, z={p.Body.position.z:F2}");
                Assert.That(Particles(p),Is.LessThanOrEqualTo(world.graphicsConfig.maxParticles));
                if(!baseline)
                {
                    var effects=p.GetComponent<SnowEffectsController>();
                    if(label=="Carve"||label=="Brake"||label=="Powder")Assert.That(effects.LiveParticles,Is.GreaterThan(0),label+" must emit at the configured cap");
                    if(label=="Rail")Assert.That(effects.RailParticlesEmitted,Is.GreaterThan(0),"Rail frost uses its own pool");
                    if(label=="Reset")Assert.That(effects.LiveParticles,Is.Zero,"Retry must clear the old burst");
                }
            }
            foreach(bool day in new[]{false,true})
            {
                world.GetComponent<AlpineLighting>().Apply(day);yield return null;string preset=day?"Day":"Sunset";
                Spawn(world,10,40);yield return new WaitForSeconds(.9f);Assert.That(p.Grounded,Is.True);Shot(preset,"Straight");
                p.Input.steer=.7f;yield return new WaitForSeconds(1.1f);Assert.That(p.Grounded,Is.True);Shot(preset,"Carve");
                p.Input.brake=true;yield return new WaitForSeconds(.24f);Shot(preset,"Brake");
                Spawn(world,-5,45,14,0,true);yield return new WaitForSeconds(1.1f);Assert.That(p.Grounded,Is.True);Shot(preset,"Switch");
                Spawn(world,75,1120,13);p.Input.steer=.6f;yield return new WaitForSeconds(.8f);Assert.That(p.Surface,Is.EqualTo(SurfaceType.Powder));Shot(preset,"Powder");
                int before=landings;int groundBefore=p.GetComponent<SnowEffectsController>().GroundParticlesEmitted;Spawn(world,10,68,15,2.8f);yield return new WaitForSeconds(.12f);Assert.That(p.Grounded,Is.False);Shot(preset,"Flight");
                if(!baseline)Assert.That(p.GetComponent<SnowEffectsController>().GroundParticlesEmitted,Is.EqualTo(groundBefore),"Flight must not emit ground spray");
                float limit=Time.time+2;while(landings==before&&Time.time<limit)yield return null;Assert.That(landings,Is.GreaterThan(before));yield return new WaitForSeconds(.06f);Shot(preset,"Landing");
                RailPath path=null;foreach(var candidate in world.GetComponentsInChildren<RailPath>())if(candidate.name=="Rail_flat"){path=candidate;break;}Assert.That(path,Is.Not.Null);
                var point=path.Evaluate(1,out var tangent);p.ResetTo(point+Vector3.up*(p.config.rideHeight+.08f),Quaternion.LookRotation(tangent));p.Body.linearVelocity=tangent*12-Vector3.up*.3f;
                yield return new WaitForSeconds(.25f);Assert.That(p.GetComponent<RailSystem>().Riding,Is.True);Shot(preset,"Rail");
                Spawn(world,10,50,12);yield return new WaitForSeconds(.25f);p.GetComponent<BailSystem>().Crash();yield return new WaitForSeconds(.10f);Shot(preset,"Crash");
                Spawn(world,-8,42,0);yield return null;Shot(preset,"Reset");
            }
            File.WriteAllLines(output+"/action-review.txt",stats);Debug.Log("SNOW ACTION REVIEW: straight/carve/brake/switch/powder/flight/landing/rail/crash/reset; both presets; "+landings+" actual landings");Object.Destroy(world.gameObject);
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator RetryExpiryAndCombinedPoolCap()
        {
#if UNITY_EDITOR
            var graphics=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GraphicsConfig>("Assets/Settings/GraphicsConfig.asset"));graphics.trackLifetime=.35f;graphics.maxParticles=120;
            var world=CreateWorld(graphics);yield return null;var p=world.player;p.Input.BeginInjected();Spawn(world,10,40);yield return new WaitForSeconds(.6f);
            var tracks=p.GetComponent<SnowTrackSystem>();var effects=p.GetComponent<SnowEffectsController>();
            Assert.That(tracks.SampleCount(0),Is.GreaterThan(5));Assert.That(tracks.SampleCount(1),Is.GreaterThan(5));
            var left=tracks.TrackMesh(0).vertices;var right=tracks.TrackMesh(1).vertices;
            Assert.That(Vector3.Distance((left[left.Length-1]+left[left.Length-2])*.5f,(right[right.Length-1]+right[right.Length-2])*.5f),Is.InRange(.3f,.65f),"Grooves must remain separate");
            Spawn(world,-10,70);yield return new WaitForSeconds(.12f);
            for(int ski=0;ski<2;ski++)
            {
                var mesh=tracks.TrackMesh(ski);var v=mesh.vertices;var t=mesh.triangles;
                for(int i=0;i<t.Length;i+=3)for(int j=0;j<3;j++)Assert.That(Vector3.Distance(v[t[i+j]],v[t[i+(j+1)%3]]),Is.LessThan(1),"Retry must not bridge distant contact points");
            }
            for(int i=0;i<40;i++)effects.SendMessage("OnLanding",new LandingResult{quality=LandingQuality.Clean,impact=12});
            effects.SendMessage("OnCrash");Assert.That(effects.LiveParticles,Is.LessThanOrEqualTo(graphics.maxParticles));Assert.That(effects.LiveParticles,Is.GreaterThan(50));
            Spawn(world,-8,42,0);Assert.That(effects.LiveParticles,Is.Zero);
            p.enabled=false;p.Body.isKinematic=true;p.Contacts.left=p.Contacts.right=default;yield return new WaitForSeconds(.5f);
            for(int ski=0;ski<2;ski++){Assert.That(tracks.SampleCount(ski),Is.Zero);Assert.That(tracks.TrackMesh(ski).vertexCount,Is.Zero,"An expired list must clear its old rendered mesh");}
            var first=tracks.TrackMesh(0);var second=tracks.TrackMesh(1);Object.Destroy(world.gameObject);yield return null;Assert.That(!first&&!second,Is.True,"Runtime track meshes must be disposed with the run");Object.Destroy(graphics);
            Debug.Log("SNOW LIFECYCLE PASS / ski separation, no retry bridges, combined cap 120, reset clear, expiry and mesh disposal");
#else
            yield break;
#endif
        }
        [UnityTest]public IEnumerator MotionSequences()
        {
#if UNITY_EDITOR
            var world=CreateWorld();yield return null;var p=world.player;p.Input.BeginInjected();Camera camera=null;
            foreach(var follow in world.GetComponentsInChildren<SkiCameraController>()){follow.enabled=false;camera=follow.GetComponent<Camera>();}camera.fieldOfView=58;
            int launches=0,landings=0;p.TookOff+=()=>launches++;p.Landed+=r=>{if(r.quality!=LandingQuality.Bail)landings++;};
            float previousCaptureDelta=Time.captureDeltaTime;
            try
            {
                foreach(bool day in new[]{false,true})
                {
                    world.GetComponent<AlpineLighting>().Apply(day);Spawn(world,10,40,14);yield return new WaitForSeconds(.15f);string preset=day?"Day":"Sunset";
                    var output="Logs/VP5-"+preset+"-frames";Directory.CreateDirectory(output);int before=landings;Time.captureDeltaTime=.1f;
                    for(int frame=0;frame<80;frame++)
                    {
                        p.Input.steer=frame>=10&&frame<26?.6f:0;p.Input.brake=frame>=25&&frame<29;p.Input.tuck=frame<29;
                        if(frame==30)p.Input.pop=true;if(frame==60)p.GetComponent<BailSystem>().Crash();if(frame==65)Spawn(world,-8,42,13);
                        yield return null;
                        camera.transform.position=p.Body.position+new Vector3(-4,4.3f,-8);camera.transform.LookAt(p.Body.position+new Vector3(0,-.25f,-1.5f));Capture(camera,output+"/"+frame.ToString("D4")+".png",1280,720);
                        Assert.That(Particles(p),Is.LessThanOrEqualTo(world.graphicsConfig.maxParticles));
                    }
                    Time.captureDeltaTime=previousCaptureDelta;Assert.That(landings,Is.GreaterThan(before),"Motion review must contain an actual landing");
                }
            }
            finally{Time.captureDeltaTime=previousCaptureDelta;}
            Debug.Log("SNOW MOTION REVIEW: two 8-second sequences, 10 samples per simulated second; "+launches+" takeoffs / "+landings+" landings; capped runtime setting retained");Object.Destroy(world.gameObject);
#else
            yield break;
#endif
        }
    }
}
