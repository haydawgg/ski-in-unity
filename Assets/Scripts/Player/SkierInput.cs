using UnityEngine;
using UnityEngine.InputSystem;
namespace PowderFlow
{
    public class SkierInput : MonoBehaviour
    {
        public float steer, flip, roll;
        public bool tuck, brake, crouch, grabLeft, grabRight, modifierLeft, modifierRight;
        public bool pop, reset, marker, returnMarker, debugToggle, pause, lighting;
        public bool injected;
        public void BeginInjected()
        {
            injected=true;steer=flip=roll=0;
            tuck=brake=crouch=grabLeft=grabRight=modifierLeft=modifierRight=false;
            pop=reset=marker=returnMarker=debugToggle=pause=lighting=false;
        }
        void Update()
        {
            if (injected) return;
            var k=Keyboard.current; var p=Gamepad.current;
            float Held(Key key) => k!=null && k[key].isPressed ? 1 : 0;
            bool Press(Key key) => k!=null && k[key].wasPressedThisFrame;
            steer=Mathf.Clamp((Held(Key.D)-Held(Key.A))*SaveStore.Current.keyboardSensitivity+(p?.leftStick.x.ReadValue()??0)*SaveStore.Current.gamepadSensitivity,-1,1);
            flip=Mathf.Clamp((Held(Key.UpArrow)-Held(Key.DownArrow))*SaveStore.Current.keyboardSensitivity+((p?.leftStick.y.ReadValue()??0)+(p?.rightStick.y.ReadValue()??0))*SaveStore.Current.gamepadSensitivity,-1,1);
            roll=Mathf.Clamp((Held(Key.RightArrow)-Held(Key.LeftArrow))*SaveStore.Current.keyboardSensitivity+(p?.rightStick.x.ReadValue()??0)*SaveStore.Current.gamepadSensitivity,-1,1);
            tuck=Held(Key.W)>0 || (p?.buttonNorth.isPressed??false);
            brake=Held(Key.S)>0 || (p?.buttonWest.isPressed??false);
            bool previous=crouch;
            crouch=Held(Key.Space)>0 || (p?.buttonSouth.isPressed??false);
            pop=previous&&!crouch;
            grabLeft=Held(Key.Q)>0 || (p?.leftTrigger.ReadValue()??0)>.3f;
            grabRight=Held(Key.E)>0 || (p?.rightTrigger.ReadValue()??0)>.3f;
            modifierLeft=Held(Key.LeftShift)>0 || (p?.leftShoulder.isPressed??false);
            modifierRight=Held(Key.LeftCtrl)>0 || (p?.rightShoulder.isPressed??false);
            reset=Press(Key.R)||(p?.buttonEast.wasPressedThisFrame??false);
            marker=Press(Key.T)||(p?.dpad.up.wasPressedThisFrame??false);
            returnMarker=Press(Key.Y)||(p?.dpad.down.wasPressedThisFrame??false);
            debugToggle=Press(Key.F1); pause=Press(Key.Escape)||(p?.startButton.wasPressedThisFrame??false);
            lighting=Press(Key.L);
        }
    }
}
