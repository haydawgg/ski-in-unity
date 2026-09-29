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

## M7 — Environment generation: passed

- Added seeded pines (3 LODs each), six rocks, five rails/five boxes with JSON paths, six smooth jumps, ten 400×150 m gameplay chunks, three ridge silhouettes, hut/lift/floodlight/fence/sign/flag/cable props. Catalog contains 47 assets, with Blender sources, FBX, manifests and selected previews.
- `Tools/Build/generate-assets.sh all` regenerates, validates and imports all assets successfully. EditMode 19/19 passed, including adjacent chunk collision height/normal continuity and imported tree LOD/path metadata.
- Try AssetPreview for the skier; generated prefabs are in Assets/Prefabs/Generated. Asset selection and full world assembly follow in M8.
- Preview meshes/normals/UVs/scale validated. Trees use three geometry LODs rather than a texture billboard; colliders are simple trunks. Rocks/jump colliders reuse their low-poly visual surfaces. No downloaded assets or textures.

## M8 — Connected mountain: passed

- Added a deterministic 1.5 km/approximately 345 m vertical mountain, flowing jump/rail/box lines, freeride powder zone, trees/rocks, ridge hut, lift cable/towers and floodlights, region labels, area respawns and boundary resets. All world meshes come from Blender prefabs.
- Continuous physics descent reached 1,408 m, traversed all five regions and launched/landed features without NaN or bail; EditMode 19/19 still pass. Scene is Assets/Scenes/Mountain.unity.
- Try Mountain, Play, carve toward center kickers or side rails, use markers for feature retries. The complete line runs toward +Z. Reaching the end returns to the start.
- Fixed FBX single-mesh orientation loss by preserving imported transforms inside neutral prefab roots; added an expected world-height check to seam tests. Tree/rock placement avoids core approaches. Subjective line variety and freeride tuning remain part of final playtesting.

## M9 — Graphics: passed with documented implementation differences

- Added original procedural URP snow/rock blend, wind-ripple normals/glitter, gradient/cloud/sun sky, ACES/bloom/SSAO, sunset/day lighting and fog, per-ski persistent fading groove meshes, bounded spray/landing/crash flakes and final valley backdrop. Captured 1920×1080 Sunset/Day and 720×1280 portrait frames.
- Graphics render test passes with 428.8 FPS in a synchronous GPU-flushed offscreen 1080p camera benchmark on Radeon RX 6600 XT. This measures rendering, not full standalone gameplay; standalone profile follows in M10. Shader compilation has no errors. Imported-ski crossing direction check also passes.
- Try Mountain; L switches Day/Sunset. Screenshots are in Documentation/Screenshots. Tracks remain around 100 seconds with 1,100 samples per ski and stop across air/reset gaps.
- Fixed ski burial, Blender bone-axis crossing/grab targets, a background gap and graphics assignments after regeneration. Snow/sky are source HLSL shaders (with editable material properties), not Shader Graph files; this keeps the automated pipeline reproducible. Lighting/geometry is stylized, and no reference video was supplied for a literal visual comparison. Human subjective visual matching is not asserted.

## M10 — Menus, verification and Linux package: passed automated gates

- Added Free Ride/150 second sessions, controller and mouse menus, saved settings/high score, outfit selection, generated audio, build/package/run tooling and final documentation. Tuned switch steering, pop buffering, hockey-stop braking, reset/camera ownership, landing pose and particle response.
- Regenerated all 48 assets after fixing FBX X-axis mirroring. Added off-center terrain and left/right rig assertions; added foot IK and snow-aligned skis to prevent the tucked pose rotating skis vertically.
- Final checks: EditMode **21/21**, PlayMode **12/12**, manifest **48/48**. Live tests cover controller launch and settings save, session expiry, 180–1080 spins/front/back flips, flat/down/kink rails, ragdoll/reset, carving, imported rig, both lighting presets and a **1,404 m** connected descent through five regions.
- Linux BuildPipeline succeeds (**178,064,392 bytes**). Visible 1080p native player: **751.8 FPS** uncapped short-run average on RX 6600 XT, maximum **17.49 m/s**, **101.9 m** measured descent, exit 0; no runtime exceptions. The separate fixed-camera benchmark measures **469.0 FPS**. Batch-mode player timing was excluded because it skipped normal rendering.
- Try: `Tools/Build/run.sh`, choose FREE RIDE. A/D carves, W tucks, S brakes, hold/release Space pops, arrows flip/roll, Q/E grabs, R retries, T/Y stores/retries a grounded marker. Or open MainMenu.unity in Unity and press Play. Build/package commands and complete controls are in README.md.
- Deliverables: `Builds/Packages/PowderFlow-Linux.tar.gz`, portable Play.sh, source/FBX assets, validation XML/JSON, gameplay/menu/lighting captures, README and FINAL_REPORT.
- Issues/limits: the default X11 launch stalled on this Wayland desktop; Play.sh selects native Wayland and has an X11 opt-out. Human feel/visual matching and hours-long play are not certified. HLSL/Generic rig, approximate grab poses, no billboard/shrub assets, some source constants, preset outfits and Linux-only packaging are documented differences. Optional replay/photo/challenges and music are omitted.
