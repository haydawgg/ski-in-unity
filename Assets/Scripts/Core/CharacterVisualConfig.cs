using System;
using UnityEngine;
namespace PowderFlow
{
    [Serializable]public class OutfitPalette
    {
        public string name;public Color jacket,accent,pants,panels,skis,graphics;
        public OutfitPalette(string name,Color jacket,Color accent,Color pants,Color skis)
        {this.name=name;this.jacket=jacket;this.accent=accent;this.pants=pants;panels=Color.Lerp(pants,accent,.18f);this.skis=skis;graphics=accent;}
    }
    [CreateAssetMenu(menuName="PowderFlow/Character Visuals")]
    public class CharacterVisualConfig:ScriptableObject
    {
        [Header("Garment light response")]
        public Color ambientFill=new Color(.12f,.15f,.20f);
        public float fabricRim=.14f,equipmentRim=.08f,fabricWeave=.035f;
        [Header("Body posing")]
        public float neutralBend=.30f,airBend=.38f,railBend=.48f,tuckBend=.82f,poseResponse=14,rootDrop=.20f;
        public float stanceDrop=.10f;
        [Header("Physics motion response")]
        public float motionResponse=9, carveHipShift=.065f, carveCounterLean=12, carveKneeBend=12, carveArmBalance=20;
        public float rotationTuck=.46f, rotationPoseSpeed=6, airArmFold=24, landingArmSpread=18;
        public float airKneeLift=18,airTorsoBend=10,grabCounterArm=18;
        public float popDuration=.18f, popExtension=.85f, landingLegBend=.24f, landingRecovery=2.2f;
        public float sketchyDuration=.65f, sketchyHipShift=.045f, sketchyArmBalance=24;
        public float grabBlendResponse=11, grabTorsoTwist=12, grabHipShift=.025f, grabReleaseTime=.13f;
        public float grabReachAdjustment=.28f;
        public float thighBend=55,shinBend=95,footBend=40,spineBend=25,armNeutral=-12,armBend=30,elbowBend=-35,armSpread=12,headCounterBend=.55f;
        [Header("Grab and equipment posing")]
        public float grabThigh=-145,grabShin=105,grabFoot=-30,grabSpine=35,crossYaw=30,poleSway=18,poleFrequency=3.5f;
        public Vector3 crossBodyGrabAngles=new Vector3(-150,80,-30),noseGrabAngles=new Vector3(-130,100,60);
        public Vector3 poleTrail=new Vector3(.6f,-.5f,-.8f);
        public OutfitPalette[] outfits={
            new OutfitPalette("Evergreen",new Color(.17f,.50f,.34f),new Color(.77f,.86f,.79f),new Color(.12f,.18f,.25f),new Color(.045f,.56f,.62f)),
            new OutfitPalette("Violet",new Color(.49f,.24f,.68f),new Color(.95f,.72f,.35f),new Color(.16f,.12f,.24f),new Color(.88f,.49f,.14f)),
            new OutfitPalette("Slate",new Color(.08f,.10f,.14f),new Color(.68f,.84f,.87f),new Color(.23f,.28f,.35f),new Color(.14f,.22f,.30f)),
            new OutfitPalette("Glacier",new Color(.58f,.64f,.69f),new Color(.18f,.57f,.67f),new Color(.13f,.21f,.30f),new Color(.10f,.50f,.60f)),
            new OutfitPalette("Alpine",new Color(.43f,.47f,.22f),new Color(.87f,.70f,.33f),new Color(.20f,.24f,.18f),new Color(.24f,.32f,.16f)),
            new OutfitPalette("Signal",new Color(.95f,.34f,.10f),new Color(.91f,.94f,.93f),new Color(.08f,.14f,.24f),new Color(.035f,.38f,.49f))};
        static CharacterVisualConfig fallback;
        public static CharacterVisualConfig Default{get{if(!fallback){fallback=CreateInstance<CharacterVisualConfig>();fallback.hideFlags=HideFlags.HideAndDontSave;}return fallback;}}
    }
}
