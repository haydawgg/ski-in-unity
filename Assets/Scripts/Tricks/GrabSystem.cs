using UnityEngine;
namespace PowderFlow
{
    public enum GrabType { None,Safety,Mute,Japan,Tail,Nose,Stale,Blunt,Method,CrissCross }
    public class GrabSystem : MonoBehaviour
    {
        public GrabType Current { get; private set; }
        public bool LeftHand { get; private set; }
        SkiPhysicsController skier;TrickTracker tracker;
        void Start(){skier=GetComponent<SkiPhysicsController>();tracker=GetComponent<TrickTracker>();}
        void Update()
        {
            var i=skier.Input;Current=GrabType.None;if(skier.Grounded||skier.Bailed)return;
            LeftHand=i.grabLeft;
            if(i.grabLeft&&i.grabRight)Current=i.modifierRight?GrabType.Method:GrabType.CrissCross;
            else if(i.grabLeft||i.grabRight)
            {
                if(i.modifierLeft&&i.modifierRight)Current=i.grabLeft?GrabType.Japan:GrabType.Blunt;
                else if(i.modifierLeft)Current=i.grabLeft?GrabType.Tail:GrabType.Nose;
                else if(i.modifierRight)Current=i.grabLeft?GrabType.Stale:GrabType.Method;
                else Current=i.grabLeft?GrabType.Safety:GrabType.Mute;
            }
            if(Current!=GrabType.None && tracker.tracking){tracker.current.grab=Current.ToString();tracker.current.grabTime+=Time.deltaTime;}
        }
        public Vector3 Target(Transform leftSki,Transform rightSki)
        {
            var ski=Current==GrabType.Mute||Current==GrabType.Japan ? (LeftHand?rightSki:leftSki) : LeftHand?leftSki:rightSki;
            float along=Current==GrabType.Tail||Current==GrabType.Blunt?-.7f:Current==GrabType.Nose?.72f:0;
            return ski.TransformPoint(new Vector3(0,.06f,along));
        }
    }
}
