# Current project audit — gameplay, animation and Blender pass

Audited 2026-09-30 before gameplay/asset changes. Baseline source: `ec91d35` (clean checkout). Unity 6000.3.25f1, URP 17.3.0, Blender 5.2.2 LTS. No applicable AGENTS.md found. This report describes the incoming implementation; final changes belong in GAMEPLAY_QUALITY_REPORT.md.

## Existing architecture

Four small serialized scenes instantiate runtime worlds: MainMenu, Mountain, PhysicsTest and AssetPreview. Scripts are split into Core, Player, SkiPhysics, Tricks, Rails, Camera, Environment, Snow, Audio, UI, Debug and Editor assemblies/folders. 63 generated production assets/prefabs and Blender sources are catalogued by AssetCatalog. Configuration is already centralized in SkiPhysicsConfig, TrickConfig, CameraConfig, CharacterVisualConfig, GraphicsConfig, WorldConfig, GameSystemConfig and PresentationConfig.

MountainWorld assembles ten contiguous skiable mesh tiles, kickers, five rail and box variants, varied evergreen/rock/shrub LODs, distant ridges, lift structures and signage. It reuses PhysicsTestWorld.SpawnPlayer as its factory. PhysicsTest has slopes, small/large kickers and rail fixtures: use it as the existing animation lab. Production world geometry comes from Blender; Unity primitives are confined to test fixtures and an unused fallback skier. No scene or folder reorganization is needed.

## Player architecture

There is no saved gameplay player prefab. The runtime factory creates one capsule (height 1.25 m, radius .25 m, center y .15) and one 80 kg Rigidbody, layer 8, continuous dynamic collision, interpolated, zero engine damping. Fixed simulation is 100 Hz. A generated Skier prefab is the visual child. Physics contacts use body-relative probes, independent of visual bones. Components include SkierInput, SkiContactSystem, SkiPhysicsController, AirControlSystem, SkierPose, OutfitSystem, TrickTracker, GrabSystem, ComboSystem, SessionMarkerSystem, BailSystem, RailSystem, HUD/debug/audio/snow.

The 21-bone generic rig has Hips/Spine/Chest/Neck/Head, upper/lower arms and hands, thighs/shins/feet and independent Ski/Pole bones. Five skinned renderers combine continuous clothing and independent equipment. Existing bone names and bind hierarchy are coherent and must stay compatible. The rig is not a mapped Humanoid Avatar.

## Current skiing physics

Gravity drives downhill velocity; support spring/damping follows the averaged normal of four spherecasts (two per ski). Grounding accepts either ski. Along-ski Coulomb friction is low; lateral acceleration opposes slip with edge-dependent grip and a force cap. Powder and ice alter friction/grip. Sidecut radius widens with speed, and MoveRotation changes ski heading while lateral forces turn velocity. Brake turns skis toward a hockey stop, reduces grip and applies speed scrub. Crouch changes center of mass; tuck reduces quadratic air drag on the ground. Pop is buffered/coyote-timed and applies an upward velocity impulse with speed scaling. Ground-contact locking prevents immediately re-grounding after takeoff.

AirControlSystem integrates three-axis angular momentum, input torque, slight drag and a maximum speed; tuck/grab reduces modeled inertia. Input preloading is optional. Near-ground assist casts vertically and only corrects attempts already within 42 degrees. There is no trajectory prediction or separate input-release spotting damping. Do not replace this working movement model.

## Current animation architecture

The importer disables any generated Animator and imports no clips. Blender stores 17 one-frame source poses, but FBX export has bake_anim=False. Runtime SkierPose is the sole visual authority: input flags select bend; compression is a scalar from impact; root rolls with Edge; spine/legs/arms receive Euler offsets; a two-bone solver puts boots/skis on snow or sampled rail surfaces and hands on procedural grab targets. Pole trailing is procedural. Runtime downhill motion has no root motion.

There is no dedicated physics-to-animation snapshot. Pose reads the controller directly. Both legs receive the same bend before contact IK. Actual lateral acceleration and angular speed do not shape the pose. Crouch is visible, but pop has no distinct rapid extension phase; air arms are largely identical between slow and fast rotations. Landing compression exists, but preparation/recovery and sketchy balance cues are weak. Grabs jump immediately to extreme leg poses and solve the hand at full weight rather than blending toward the target.

## Current trick architecture

TrickTracker accumulates quaternion deltas projected onto world yaw and prior body pitch/roll; takeoff records speed/switch stance. Landing terminates the record. GrabSystem selects nine grab/style types from trigger/modifier combinations and records held time. Targets are mathematically located along independent skis. ComboSystem quantizes rotations with tolerance, scores flips/spins/grabs/rails/switch, decays repeated tricks and cancels bails. LandingSystem grades alignment, upright angle, impact speed, angular speed and dual contact; forward/switch are valid. Successful contacts project velocity onto the slope with tiered retention. Bails spawn an inherited-momentum joint ragdoll and quickly reset.

Rails use preserved sampled center paths and JSON metadata. A proximity/height/upright/along-speed envelope captures the body kinematically, retains tangent speed, applies gravity/friction and balance, permits yaw, and pops with tangent velocity plus lift. Capture currently projects position fully to the centerline on the following tick: an off-center capture can visibly jump up to its .85 m envelope. Rail yaw velocity also survives Clear. These are targeted fix candidates, not reasons to replace paths or rail physics.

## Current camera

Custom SkiCameraController, although Cinemachine 3.1.5 is installed. Heading follows planar velocity with damping, independent of trick orientation. It keeps world-up, adds speed/air/rail/bail framing and FOV, casts for obstruction, anticipates terrain height and gives small landing shake. Desktop minimal HUD and 144 FPS cap are established. Preserve this camera and its tuning; validate readability with new poses before considering any change.

## Current Blender pipeline

Tools/Build/toolchain.env supports BLENDER_BIN/UNITY_BIN overrides. generate-assets.sh runs build_all_assets.py, manifest validation and synchronous Unity import. Stages are character/environment/scenery/park/jumps/all. Existing common.pipeline builds materials/geometry, validates finite unit-scale meshes, saves ArtSource/Blender/*.blend, exports FBX in Unity meters, renders previews and updates Assets/Art/Generated/asset_manifest.json. GeneratedAssetImporter preserves stable prefab paths, maps materials to the six custom shaders, imports Generic character bones, makes explicit collision proxies and LODGroups, and wires catalog/rail paths. Blender already produces skier, skis/bindings/poles, trees/rocks/ridges, continuous jump collision, bevelled rails/boxes and props. Do not duplicate this pipeline.

Clothing lofts blend across elbow/knee but shoulder vertices are wholly arm-weighted and upper pants wholly thigh-weighted: extreme poses need deformation inspection around shoulder/hip seams. Foundation poses contain no temporal pop/landing sequence. Improve these in the existing generator, retaining the skeleton, equipment geometry and asset GUIDs.

## Other audited systems

Input System 1.14.2 is used directly via Keyboard.current/Gamepad.current; there are no InputAction assets. Keep the established controls. Animation Rigging 1.4.1 is installed but unused because custom two-bone IK is already functional. URP settings/post profile, custom AlpineSnow/AlpineSky/AlpineProp/SkierSurface/SkiGrooves/SnowParticle shaders, capped pooled snow VFX, persistent paired tracks, terrain collision seams, varied LOD scenery and day/sunset lighting have extensive prior verification. SaveStore uses atomic JSON replacement with isolated test saves; GameFlow manages settings, outfits, pause and timed/free sessions. Keep all of these.

EditorTools automates setup/import/materials/build/terrain diagnostics. BuildGame creates a Linux Mono Development build. Existing EditMode/PlayMode suites cover physics, ballistic jumps, rotation/landing, grabs/rig, rails, art/collision, UI/settings/save and world flow. Older validation output is retained in Documentation/VisualPolish. Console audit found CS0108 for OutfitPreview.camera hiding Component.camera, plus normal headless/audio/platform diagnostics; no known current compile error. Fresh baseline/build logs must be checked, not assumed from historical reports.

## Working systems to preserve

Rigidbody trajectory and anisotropic skiing; contact probes; speed-dependent sidecut; buffered pop/preload; angular momentum and switch landings; nine grabs and two-bone IK; existing rail metadata/paths; trick scoring/combo flow; ragdoll/retry; custom upright camera; central configs; Blender geometry/collision/import pipeline; terrain, scenery, shaders, effects, audio, minimal desktop UI and atomic settings save.

## Weak systems and material technical debt

1. Motion reads sparse direct flags rather than force/trajectory state, limiting body responsiveness and debugging.
2. Pop, air tuck, landing preparation and sketchy recovery lack distinct temporal phases. Grab transition snaps; poles use unrelated oscillation.
3. Ground assist uses present vertical distance only, so fast downhill travel can miss impending landing; no intentional rotation stopping near impact.
4. Rail capture lacks a positional blend; stale rail yaw and reset compression/trick state can leak between retries.
5. Blender source actions are static and not exported; shoulder/hip weighting needs extreme-pose validation.
6. Historical action fixture covers only about 170 degrees of spin, so full-rotation checks and matched motion captures are needed together.

These affect gameplay/motion quality directly. No architecture replacement is justified.

## Recommended modifications

Extend SkiPhysicsController with a read-only motion snapshot and predicted landing state; keep force model and probes. Reuse CharacterVisualConfig for force-driven pelvis/spine/counterbalance, pop extension, rotation tuck/arm motion, IK blend and impact recovery. Add optional input-release damping only close to a valid predicted landing, preserving free-air inertia and failed trick bails. Smooth existing rail capture offset without changing paths/momentum. Reset transient state on retry. Improve Blender shoulder/hip weights and author temporal pop/land source actions; preserve bone hierarchy/equipment. Add focused deterministic regressions and captures; reuse PhysicsTest rather than add a duplicate lab. Preserve camera tuning unless captures expose a problem.

## Baseline evidence

Existing development build ran before gameplay changes: SunsetPark native rail capture/pop/landing and 92.28 m descent, 144 FPS cap, successful exit. Outputs: DevelopmentCaptures/Baseline/. Baseline scenario/pose and rotation tests are being recorded before implementation; final baseline results will be appended below. Supplied reference copied to Reference/ski_reference.mp4, analyzed across all 64.898 s in Reference/Analysis and REFERENCE_ANALYSIS.md.

Fresh baseline validation: **30/30 EditMode** checks and **5/5 PlayMode** checks pass. PlayMode includes the seven physical 180/360/540/720/1080/frontflip/backflip landing cases, ordinary full-spin landing, flat/down/kink rails, 27 grab reach samples and two 12-second Day/Sunset action captures. Native rail and descent also passed. Incoming grab wrist error maximum .112 m. Images/clips/XML copied into DevelopmentCaptures/Baseline before edits; original VP7 reports/captures restored. Headless logs contain standard graphics/audio/debugger startup diagnostics; no gameplay exception or shader error was found.

Follow-up audit findings during physical feature review (before the corresponding repairs): production open jump strips had downward-facing riding surfaces. Downward contact casts at z=107–115 hit Mountain00 beneath JumpSmall, slowing a skier against its underside. This was an incoming asset defect missed by geometry-only fingerprints. The existing Blender jump generator was extended to explicitly orient open riding strips upward; vertex profiles and all twelve original collision fingerprints remain preserved. Added downward-cast and actual small/large lip regressions verify the functional surface.

Matched action captures also showed compact tricks shrinking with combined speed/air pullback. A small distance/height/pullback tuning pass in the existing CameraConfig is justified; the custom follow/heading/damping/obstruction system is preserved. Final desktop framing and obstruction checks accompany the revised capture review.
