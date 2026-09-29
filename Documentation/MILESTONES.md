# Milestone log

## M1 — Setup: passed

- Added `Tools/Build`, the required FBX Blender smoke test, assembly definitions, Unity packages, URP settings and setup test. Configured Linear color, HDR, Input System and persisted 100 Hz physics.
- Validation: `Tools/Build/setup.sh` exits 0; Blender PNG/blend/FBX validated; Unity compiles without C# errors. EditMode: 1/1 passed.
- Try: open this project in Unity 6000.3.25f1, inspect Assets/Settings/PowderFlowURP; run the two commands above to repeat validation.
- Issues: no gameplay yet. Unity emits licensing connection retries and a bundled dotnet shutdown message, but activation and batch execution succeed. No reference video was supplied, so use the plan's written visual reference.

## M2 — Ground physics: automated gate passed

- Added SkiPhysicsConfig/CameraConfig, Rigidbody controller, four ski contacts, carving/friction, surfaces, gamepad/keyboard input, travel-follow camera, F1 overlay, PhysicsTest scene and simulation tests.
- EditMode 5/5 and PlayMode 1/1 pass. On a 20° slope: upright terminal speed 25.61 m/s, tuck 37.48 m/s; flat braking, powder slowdown, uphill deceleration, smooth turning and contact retention pass. Live scene simulation carves and brakes without losing support.
- Try: open Assets/Scenes/PhysicsTest.unity and press Play. A/D carve, W tuck, S brake, R reset, F1 telemetry. Start on the 15° lane; adjacent lanes cover 0/5/30/45°.
- Fixed an unintended turn at zero input (Unity Mathf.Sign(0) returns +1). Visuals are temporary. Automated motion checks are positive; human subjective skiing feel remains a playtest item rather than a claimed certification.

## M3 — Jumps and air: automated gate passed

- Added TrickConfig, torque/angular-momentum air control, inertia changes, preload, crouch-release pop, landing evaluation/assist/compression, and continuous temporary kickers.
- EditMode 9/9, PlayMode 2/2 passed. Actual continuous-kicker simulation launches at (0, 8.38, 12.07) m/s from an 18 m/s approach. Ballistic range, momentum conservation, 54% tuck spin-up, transition speed retention, switch and bail thresholds pass. A simulated medium-height 360 lands with >10 m/s forward speed.
- Try PhysicsTest: Space hold/release pops, A/D spins while airborne, arrows flip/roll, W tucks; Ctrl preloads/corks. R retries.
- Issues: the character remains temporary and landing visual compression will be connected to its procedural pose in M4/M6. The reliable spin test uses medium-kicker-equivalent airtime; complete feature lines need later world playtesting.
