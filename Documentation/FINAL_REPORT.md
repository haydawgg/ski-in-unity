# PowderFlow delivery report

**Historical M10 report.** Visual polish now has a separate plan in `VISUAL_POLISH_PLAN.md` and current results in `VisualPolish/Phase3/REPORT.md`. VP1 repaired the saved render configuration; VP2 improves the character, equipment, outfits and posing; VP3 replaces repeated vegetation/ridges and adds shrub/rock clusters and regional composition. Current runs are capped at 144 FPS and routine performance benchmarks have been removed at the user's request. The M9/M10 rendering descriptions below recorded the intended configuration; those specific effects were not fully active in the delivered M10 player. The original package is preserved locally as `Builds/Packages/PowderFlow-M10-Linux.tar.gz`; the usual `PowderFlow-Linux.tar.gz` path contains the latest build and uses the VP3 checksum.

## Delivered

A playable Unity 6000.3.25f1 / URP freestyle skiing game and Linux x86_64 development build. The ten milestone commits record setup through packaging; `MILESTONES.md` records each gate. The entire art pipeline runs headlessly in Blender 5.2.2 and Unity batch mode.

### Gameplay and systems

- Gravity, four ski support contacts, anisotropic grip, smooth speed-dependent carving, skid/brake, tuck drag, powder slowdown and switch skiing.
- Crouch-release pop with buffering/coyote time, torque-driven spins/flips/off-axis rotation, inertia changes, landing assistance and four landing tiers.
- Eight procedural grab selections, crossed skis, hand/foot IK, landing compression, pole lag, a 16-body momentum ragdoll, automatic recovery, quick reset and session markers.
- Accumulated rotations, composed trick names, landing/hold-time/repeat scoring, combos, rail capture/balance/slides/pop exits and JSON paths.
- Travel-follow camera, stable horizon through flips, speed FOV, obstruction handling, portrait framing and landing shake.
- Free Ride and 150 second Score Session, controller/mouse/keyboard menus, saved settings, local high score, six outfit presets and dynamic procedural Foley.
- Sunset and Day, custom snow/rock and sky shading, ACES/bloom/SSAO, fog/shadows, independent fading ski grooves and bounded spray/landing/crash particles.

### Generated content

The manifest validates **48 asset entries**: one 8,148-triangle rigged skier including separate skis, bindings and poles; four pine variants with three LODs; six rocks; five rails; five boxes; six jump forms; ten gameplay mountain chunks; three distant ridges; seven props; and a valley floor. Source files are in `ArtSource/Blender`; runtime exports, previews and metadata are in `Assets/Art/Generated`.

The mountain is approximately 1.5 km long, 400 m wide and 345 m in vertical drop. Areas include ridge/easy run, terrain park, big air, freeride/powder and lower run. Center kickers and side rail/box approaches are designed separately from seeded tree/rock placement. Lift towers/cables, hut, flags, fences, signs and floodlights populate the run. End/boundary zones reset the skier.

## Validation

The delivered XML files and standalone JSON are copied into `Documentation/Validation/`. Captures are in `Documentation/Screenshots/`.

| Gate | Result |
|---|---|
| Blender manifest | 48 entries passed geometry/metadata validation |
| EditMode | 21 passed, 0 failed |
| PlayMode | 12 passed, 0 failed |
| Connected mountain simulation | Reached 1,404 m, traversed five areas without NaN |
| Ground dynamics | 20° slope terminal speed: 25.61 m/s upright, 37.48 m/s tucked |
| Air dynamics | Momentum conservation and tuck spin-up; 180/360/540/720/1080, forward/backward flips land in simulation |
| Rail simulation | Flat/down/kink capture and exits preserve momentum |
| Imported rig/terrain | Ground tuck alignment, independent crossing skis, left/right bone placement, off-center heights and seams pass |
| Menus/save | Gamepad launches Free Ride and saves portrait settings; session expiry preserves high score; atomic save roundtrip passes |
| Rendering | Sunset/Day 1920×1080 and portrait 720×1280 captures; 469.0 FPS GPU-flushed camera benchmark |
| Linux build | Unity BuildPipeline succeeded; 178,064,392 bytes reported |
| Visible standalone gameplay | 751.8 FPS uncapped average at 1080p; 101.9 m descent; maximum 17.49 m/s; exit 0 |
| Main menu | Captured through the packaged launcher with native Wayland; exit 0 |

The standalone profile is described in `Validation/standalone.json`. It disables VSync, warms up for two seconds, then measures 12 seconds of an automatic tuck descent at 1920×1080. This is full player frame timing. The separate camera benchmark renders a fixed view with a synchronous pixel readback; it does not measure the complete game loop.

The test machine is a Ryzen 9600X, Radeon RX 6600 XT, approximately 14 GB RAM, running CachyOS Linux. Results are specific to this hardware and measured scenarios. Automated checks cover meaningful movement and state changes, but cannot establish subjective fun or hours-long stability.

### Fixes made during final verification

- Corrected the Blender-to-FBX X-axis convention, which mirrored character left/right and off-center mountain heights. Regenerated all assets and added placement/height assertions.
- Added ground foot IK and explicit ski alignment to keep tucked skis parallel to the surface. Air posing still permits grab/crossing motion.
- Fixed camera ownership and reset subscriptions, test-world cleanup, switch steering and session completion state. Added input buffering and a stronger sideways brake.
- Kept gameplay and render FPS measurements separate and ran the actual packaged executable.
- Resolved a default X11 startup stall on this Wayland desktop by selecting Unity's native Wayland backend in the launcher. The launcher allows opting back into X11 with `POWDERFLOW_X11=1`. Unity documents the switch in its [player command-line reference](https://docs.unity.com/en-us/engine/6000.7/manual/unity-editor/command-line-arguments/player). A batch-mode run was excluded from performance evidence because it skipped normal frame rendering.

## Try it

From `/home/haydend/Documents/ChatGPT/ski-in-unity`:

```bash
Tools/Build/run.sh
```

Select **FREE RIDE**. A/D carves, W tucks, S brakes. Hold and release Space to pop. A/D spins in air, Up/Down flips, Left/Right rolls, Q/E grabs. Shift/Ctrl change grabs; Q+E crosses skis. R retries immediately. T sets a grounded marker and Y returns to it. Escape pauses; L switches lighting. F1 displays telemetry.

Gamepad: sticks steer/rotate, A pops on release, Y tucks, X brakes, triggers grab, bumpers modify, B retries, D-pad sets/returns to the marker, Start pauses. D-pad and A navigate menus. Use Left/Right to adjust settings rows. Complete controls and grab mappings are in the root README.

Editor: `./scripts/open_editor.sh`, open `Assets/Scenes/MainMenu.unity`, press Play. `Mountain.unity` starts immediately; `PhysicsTest.unity` is the isolated tuning area.

## Reproduction and package

```bash
Tools/Build/setup.sh
python Tools/Build/generate_audio.py
Tools/Build/generate-assets.sh all
Tools/Build/test.sh EditMode
Tools/Build/test.sh PlayMode
Tools/Build/build-linux.sh
Tools/Build/package.sh
```

Paths can be overridden with `UNITY_BIN` and `BLENDER_BIN`. Batch commands require the interactive editor to be closed. Logs and build outputs are ignored by Git; source, generated assets, settings, scenes and validation evidence are tracked.

- Executable: `Builds/Linux/PowderFlow.x86_64`
- Portable launcher: `Builds/Linux/Play.sh`
- Complete distributable: `Builds/Packages/PowderFlow-Linux.tar.gz` (approximately 64 MB); checksum in `Validation/package.sha256`. The extracted archive was launched independently and exited 0 after its menu capture.
- Saved settings/high score: `~/.config/unity3d/PowderFlow Studio/PowderFlow/powderflow.json`

Keep the executable, data folder and Unity libraries together. The archive includes player controls and run instructions.

## Remaining limitations and plan differences

- This is a stylized development game. Human playtesting remains necessary for feel, visual polish, grab readability and line variety. No reference video was provided, so literal reference matching is unverified.
- Snow/sky use original HLSL rather than Shader Graph. The rig uses Generic import with a humanoid-compatible hierarchy and extra ski/pole bones; Humanoid retargeting is not configured.
- Grabs are approximate procedural poses. Trees use three geometry LODs without billboards; shrubs and dedicated collision meshes for every world asset are not included.
- Most tuning lives in ScriptableObjects; some layout, pose, VFX and UI constants remain in source. Outfit selection uses coordinated presets rather than independently editing every garment.
- Generated sound covers snow/skid, wind, rail, takeoff, landing and crash without music. The music setting is reserved. F2 debug gizmos display in the editor Scene view.
- Optional challenges, photo mode and replay buffer are omitted. Only Linux is built and verified; other targets require their Unity modules and validation.
- A rare Unity Bee closed-pipe backend failure was resolved by a fresh launch. Nonfatal licensing retries and bundled .NET shutdown warnings remain in editor logs.

These limits are explicit; passing the automated gates is not a claim that every subjective or authoring-format requirement has been certified.
