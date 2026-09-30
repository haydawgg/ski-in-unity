using UnityEngine;
namespace PowderFlow
{
    public class OutfitPreview:MonoBehaviour
    {
        GameObject host;Camera camera;RenderTexture image;Transform model;OutfitSystem outfit;float yaw;int shown=-1;
        public RenderTexture Image=>image;
        public void Initialize(AssetCatalog catalog)
        {
            host=new GameObject("Outfit preview studio");host.transform.SetParent(transform);host.transform.position=new Vector3(10000,10000,10000);
            model=Instantiate(catalog.Find("Skier"),host.transform).transform;var pose=host.AddComponent<SkierPose>();pose.config=catalog.characterVisuals;pose.Bind(model);outfit=host.AddComponent<OutfitSystem>();
            foreach(var t in host.GetComponentsInChildren<Transform>())t.gameObject.layer=30;
            camera=new GameObject("Outfit portrait camera").AddComponent<Camera>();camera.transform.SetParent(host.transform,false);camera.enabled=false;camera.cullingMask=1<<30;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=PresentationConfig.Active.card;camera.fieldOfView=PresentationConfig.Active.previewFov;camera.nearClipPlane=.05f;camera.farClipPlane=20;
            image=new RenderTexture(512,640,24){name="Outfit preview"};image.Create();camera.targetTexture=image;yaw=PresentationConfig.Active.previewYaw;
            foreach(var follow in GetComponentsInChildren<SkiCameraController>())follow.GetComponent<Camera>().cullingMask&=~(1<<30);
        }
        public void Rotate(float delta){yaw+=delta;}
        void LateUpdate()
        {
            if(!host||GetComponent<GameFlow>().Page!="outfit")return;
            if(shown!=SaveStore.Current.outfit){shown=SaveStore.Current.outfit;outfit.Apply(shown);}
            model.localRotation=Quaternion.Euler(0,yaw,0);Bounds bounds=default;bool first=true;
            foreach(var renderer in model.GetComponentsInChildren<Renderer>()){if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}
            var center=bounds.center;float distance=Mathf.Max(bounds.size.y,bounds.size.x)*.65f/Mathf.Tan(camera.fieldOfView*Mathf.Deg2Rad*.5f);
            camera.transform.position=center+new Vector3(0,.12f,distance);camera.transform.LookAt(center);camera.Render();
        }
        void OnDestroy()
        {
            if(host){foreach(var r in host.GetComponentsInChildren<Renderer>())foreach(var material in r.sharedMaterials)if(material&&material.name.EndsWith(" (Instance)"))Destroy(material);Destroy(host);}
            if(camera)Destroy(camera.gameObject);if(image){image.Release();Destroy(image);}
        }
    }
}
