using UnityEngine;
namespace PowderFlow
{
    public enum GrabType { None,Safety,Mute,Japan,Tail,Nose,Stale,Blunt,Method,CrissCross }
    public class GrabSystem : MonoBehaviour
    {
        public GrabType Current { get; private set; }
        public bool LeftHand { get; private set; }
        public GrabType PoseGrab { get; private set; }
        public float Blend { get; private set; }
        SkiPhysicsController skier;TrickTracker tracker;
        void Start(){skier=GetComponent<SkiPhysicsController>();tracker=GetComponent<TrickTracker>();skier.ResetPerformed+=Clear;}
        void Clear(){Current=PoseGrab=GrabType.None;Blend=0;}
        void OnDestroy(){if(skier)skier.ResetPerformed-=Clear;}
        void Update()
        {
            var i=skier.Input;Current=GrabType.None;
            bool riding=TryGetComponent<RailSystem>(out var rail)&&rail.Riding;
            var visual=GetComponent<SkierPose>()?.Visuals??CharacterVisualConfig.Default;
            bool release=skier.PredictedLanding.valid&&skier.PredictedLanding.time<visual.grabReleaseTime;
            if(skier.Grounded||skier.Bailed||riding){Blend=0;return;}
            if(i.grabLeft&&i.grabRight)Current=i.modifierRight?GrabType.Method:GrabType.CrissCross;
            else if(i.grabLeft||i.grabRight)
            {
                if(i.modifierLeft&&i.modifierRight)Current=i.grabLeft?GrabType.Japan:GrabType.Blunt;
                else if(i.modifierLeft)Current=i.grabLeft?GrabType.Tail:GrabType.Nose;
                else if(i.modifierRight)Current=i.grabLeft?GrabType.Stale:GrabType.Method;
                else Current=i.grabLeft?GrabType.Safety:GrabType.Mute;
            }
            if(release)Current=GrabType.None;
            if(Current!=GrabType.None){PoseGrab=Current;LeftHand=i.grabLeft;}
            Blend=Mathf.MoveTowards(Blend,Current!=GrabType.None?1:0,Time.deltaTime*visual.grabBlendResponse);
            if(Current!=GrabType.None && tracker&&tracker.tracking){tracker.current.grab=Current.ToString();tracker.current.grabTime+=Time.deltaTime;}
        }
        public Vector3 Target(Transform leftSki,Transform rightSki)
        {
            var type=PoseGrab;var ski=Ski(leftSki,rightSki);
            string targetName=type==GrabType.Tail||type==GrabType.Blunt?"TailGrab":type==GrabType.Nose?"NoseGrab":"MidGrab";
            var authored=ski.Find(targetName);if(authored)return authored.position;
            float along=targetName=="TailGrab"?-.7f:targetName=="NoseGrab"?.72f:0;
            return ski.position+ski.up*along+transform.up*.06f;
        }
        public Transform Ski(Transform leftSki,Transform rightSki)=>PoseGrab==GrabType.Mute||PoseGrab==GrabType.Japan?(LeftHand?rightSki:leftSki):LeftHand?leftSki:rightSki;
    }
}
