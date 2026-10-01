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
Escape: pause. L: Day/Sunset. F1: telemetry. F3: 1x / 0.5x / 0.25x speed.
Hold right mouse to look around.

Gamepad: left stick steers/spins, stick vertical flips, right stick
horizontal rolls/balances, A pops on release, Y tucks, X brakes,
LT/RT grab, LB/RB modify, B retries, D-pad Up/Down sets/retries marker,
Start pauses. Menus: D-pad/left stick navigates, A selects, B goes back.
Left/Right adjusts settings; O/Y opens the live outfit preview.

Gameplay uses plain corner text: speed top left, timed-session score/time
top right, and brief trick feedback. No gameplay boxes or persistent hints.

Settings and local high score are saved in the Unity persistent data
folder for PowderFlow Studio/PowderFlow. Linux typically stores this
under ~/.config/unity3d/PowderFlow Studio/PowderFlow/.

Original Blender-generated content and procedural audio.

CONTACT / FLOW LINE PASS
Pause -> RESTART PARK starts the standing roll-in. After the first medium
jump, take the left down rail or right wide box; reconnect for the down box,
large kicker and wide exit rail. HUMAN_PLAYTEST.md contains the short feel
rating sheet, branch instructions and retry controls.
This development build still needs human feedback on skiing feel,
grab readability and visual polish. Photo/replay modes are not included.

Menus/HUD use DejaVu Sans; signs use DejaVu Sans Bold mesh outlines.
Font notices: ThirdPartyLicenses/DejaVu.txt.
