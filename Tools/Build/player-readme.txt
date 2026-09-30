POWDERFLOW - Linux development build

Run ./Play.sh from this folder. Keep PowderFlow.x86_64, UnityPlayer.so
and PowderFlow_Data together. Unity/Blender are not required to play.
The launcher selects native Wayland in a Wayland desktop session.
POWDERFLOW_X11=1 ./Play.sh uses Unity's default X11 backend instead.

All runs are capped at 144 FPS.

Choose FREE RIDE or the 150 second SCORE SESSION.

A/D: carve, or spin in air. W: tuck. S: brake.
Hold Space to crouch; release to pop.
Up/Down arrows: flips. Left/Right arrows: rolls / rail balance.
Q/E: Safety/Mute. Shift: Tail/Nose. Ctrl: Stale/Method.
Shift+Ctrl: Japan/Blunt. Q+E: cross skis.
R: quick retry. T: set grounded marker. Y: retry marker.
Escape: pause. L: Day/Sunset. F1: telemetry.
Hold right mouse to look around.

Gamepad: left stick steers/spins, stick vertical flips, right stick
horizontal rolls/balances, A pops on release, Y tucks, X brakes,
LT/RT grab, LB/RB modify, B retries, D-pad Up/Down sets/retries marker,
Start pauses. Menus: D-pad navigates, A selects, Left/Right adjusts.

Settings and local high score are saved in the Unity persistent data
folder for PowderFlow Studio/PowderFlow. Linux typically stores this
under ~/.config/unity3d/PowderFlow Studio/PowderFlow/.

Original Blender-generated content and procedural audio.
This development build still needs human feedback on skiing feel,
grab readability and visual polish. Photo/replay modes are not included.

Sign lettering uses DejaVu Sans Bold mesh outlines.
Font notices: ThirdPartyLicenses/DejaVu.txt.
