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

## M4 — Tricks and scoring: automated gate passed

- Added quaternion rotation accumulation, composed trick names, eight grab selections plus criss-cross, two-bone hand IK, procedural crouch/lean/crossing/pole motion, combo scoring, HUD, 16-body jointed momentum ragdoll, marker and immediate reset.
- EditMode 16/16; PlayMode 3/3. Tested 180–1080 quantization, accumulated 720 rotation, Cork 720 Safety/Backflip Mute names, perfect-vs-clean scoring and bail cancellation; all grab selections, >=13 ragdoll bodies and reset below one second pass live.
- Try PhysicsTest. Q Safety / E Mute; Shift+Q Tail / Shift+E Nose; Ctrl+Q Stale / Ctrl+E Method; both modifiers+Q Japan / +E Blunt; Q+E criss-cross. T stores a grounded marker; Y retries it. Arrow up/down flips, arrow left/right rolls. R resets after a failed landing.
- Issues: grabs currently use articulated temporary geometry. Final reach, cloth silhouette and humanoid bones are checked again when Blender character replaces it in M6. One Unity Bee backend launch failed transiently; a fresh launch compiled and passed.

## M5 — Rails: automated gate passed

- Added world-space sampled path projection/tangents, forgiving rail capture, directional gravity/friction, balance, yaw control, tangent/pop exits and trick/combo duration tracking; temporary flat/down/kink rails in PhysicsTest.
- EditMode 17/17 and PlayMode 4/4 pass. Live tests capture and exit all three rail types while retaining >6 m/s. Kink projection is continuous; rotation and pops carry into air.
- Try PhysicsTest: aim skis toward a rail from above; A/D rotates the slide, arrow left/right balances, Space release pops out. T/Y markers provide retries.
- Fixed reset imposing an unnecessary capture cooldown. Rail paths are ready for JSON-driven Blender import in M7; no asset metadata has been imported yet.

## M6 — Blender character: passed

- Added deterministic original 8,148-triangle skinned skier, helmet/goggles/fabric slots, 1.7 m separate twin-tip skis, bindings, poles and 17 source poses. Blender source is in ArtSource/Blender, FBX/preview/manifest in Assets/Art/Generated. Added catalog/importer, AssetPreview scene, runtime outfit colors and imported-bone pose binding.
- Manifest validation passes; EditMode 18/18, graphics-enabled PlayMode 5/5 pass. Imported rig height and required bones validated; crossed-ski motion proven in the real imported rig. Blender and Unity frames inspected (Documentation/Screenshots/M6-character.png).
- Try PhysicsTest to ski with the generated model; AssetPreview shows the model alone. `Tools/Build/generate-assets.sh character` regenerates and imports it.
- Camera now snaps to the skier on reset. Rig uses Generic import to preserve extra ski/pole bones; naming/hierarchy is humanoid-compatible, but no retargeted Humanoid animation is required for procedural posing. Full nographics PlayMode launch stalled after importing the skinned rig; tests now run with graphics enabled. Some test fixture cameras warn about duplicate AudioListeners; production has one listener.
