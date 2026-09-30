# PowderFlow

An original, playable freestyle skiing game made with Unity URP and Blender. Gravity drives the skier down a connected 1.5 km mountain; carve, pop, rotate, grab, grind, land switch, and retry lines with session markers.

## Visual polish

The detailed seven-phase plan is in `Documentation/VISUAL_POLISH_PLAN.md`. VP1 repairs persistent post-processing, native-player fog, soft shadows and snow assignments. VP2 adds a tailored 10,846-triangle skier, continuous garments, equipment details, coordinated outfits, readable materials and improved grab/pose handling. Vegetation, props, motion effects and UI have dedicated later phases.

Current captures and validation are in `Documentation/VisualPolish/Phase2/REPORT.md`; VP1 evidence remains in `Documentation/VisualPolish/Phase1/`. `Documentation/FINAL_REPORT.md` and `Documentation/Validation/` preserve the earlier M10 delivery results. The latest archive remains `Builds/Packages/PowderFlow-Linux.tar.gz`.

## Play the Linux build

From this project directory:

```bash
Tools/Build/run.sh
```

Choose **FREE RIDE** for untimed skiing or **SCORE SESSION** for a 150 second run. The executable is `Builds/Linux/PowderFlow.x86_64`; keep it beside `UnityPlayer.so` and `PowderFlow_Data`. A complete archive is at `Builds/Packages/PowderFlow-Linux.tar.gz`.

To use the archive separately:

```bash
tar -xzf Builds/Packages/PowderFlow-Linux.tar.gz -C /your/destination
/your/destination/Linux/Play.sh
```

This is a Linux x86_64 development build. Unity and Blender are needed to edit/regenerate the game, not to play it.

The launcher selects Unity's native Wayland backend when the desktop uses Wayland. This fixed a startup stall in the default X11 backend on this workstation. To opt into the default backend, use `POWDERFLOW_X11=1 Tools/Build/run.sh`. Unity documents the flag in its [player command-line reference](https://docs.unity.com/en-us/engine/6000.7/manual/unity-editor/command-line-arguments/player).

## Installed development tools

| Tool | Verified version / location |
|---|---|
| Unity | 6000.3.25f1, `/home/haydend/Unity/Hub/Editor/6000.3.25f1/Editor/Unity` |
| Blender | 5.2.2, `/usr/bin/blender` |
| Git | Installed; all ten milestones have commits |
| Python | 3.14; audio and manifest validation use the standard library |
| .NET | SDK 8.0.425 in `~/.local/share/dotnet`; optional external tooling |
| ffmpeg / ffprobe | Installed for reference-video inspection |

Unity supplies its own C# compiler. Blender supplies `bpy`; do not install `bpy` into system Python. Unity resolves URP 17.3, Input System 1.14.2, Cinemachine 3.1.5, Animation Rigging 1.4.1, and Test Framework from `Packages/manifest.json` / `packages-lock.json`. The camera and IK use custom runtime code.

Open the project:

```bash
cd /home/haydend/Documents/ChatGPT/ski-in-unity
./scripts/open_editor.sh
```

Open `Assets/Scenes/MainMenu.unity` and press Play. `Mountain.unity` starts directly on the mountain. `PhysicsTest.unity` contains slope, kicker and rail fixtures. In `AssetPreview.unity`, press Play, use **1–6** to review the outfits and **Left/Right** to rotate the generated skier.

## Reproduce setup, assets, tests and build

Close the interactive Unity editor before running batch commands against this project. Defaults are in `Tools/Build/toolchain.env`; exported variables override them:

```bash
export UNITY_BIN=/home/haydend/Unity/Hub/Editor/6000.3.25f1/Editor/Unity
export BLENDER_BIN=/usr/bin/blender

Tools/Build/setup.sh
python Tools/Build/generate_audio.py
Tools/Build/generate-assets.sh all
Tools/Build/polish-graphics.sh
Tools/Build/polish-character.sh
Tools/Build/test.sh EditMode
Tools/Build/test.sh PlayMode
Tools/Build/build-linux.sh
Tools/Build/package.sh
Tools/Build/run.sh
```

`generate-assets.sh character` and `generate-assets.sh environment` regenerate individual stages. Asset generation saves `.blend` sources, exports FBX and rail-path JSON, validates the manifest, and runs the Unity importer. No manual per-asset import work is required.

EditMode runs without graphics. PlayMode and the standalone build require a graphics session. Results and diagnostics are in ignored `Logs/`. An optional second argument filters tests, for example `Tools/Build/test.sh PlayMode PowderFlow.Tests.GraphicsPlayTests`.

`polish-graphics.sh` reapplies the VP1 palette and render foundation. In Unity use **PowderFlow → Apply Visual Polish Foundation**. GraphicsConfig and the snow/sky materials contain editable parameters; normal asset regeneration reapplies graphics and preserves these assignments.

`polish-character.sh` applies the character material response from `Assets/Settings/CharacterVisualConfig.asset`. The same config contains pose values, grab angles, pole motion and six outfit palettes. Character regeneration reapplies these materials automatically. **PowderFlow → Configure Character Visuals** is the matching editor command.

Unity occasionally reports a Bee backend closed-pipe failure before compilation. If the log contains that specific transient failure, rerun the command once. Actual C# errors must be fixed. The licensing retry and bundled .NET shutdown messages seen on this workstation did not prevent successful tests or builds.

To repeat the standalone 1080p smoke profile and capture:

```bash
Tools/Build/run.sh --smoke-test -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -logFile "$PWD/Logs/standalone.log"
Tools/Build/run.sh --smoke-test --visual-day -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -logFile "$PWD/Logs/standalone-day.log"
Tools/Build/run.sh --menu-capture -logFile "$PWD/Logs/menu.log"
```

The smoke test runs an automatic tuck descent for 14 seconds, writes `smoke-report.json` and `smoke-gameplay.png` beside the executable, and exits. Its JSON includes average FPS and p95/p99 frame times. `--visual-day` selects Day for that launch. The menu capture exits after saving `smoke-menu.png`. Diagnostic launches run in the background so focus changes do not suspend them; normal play retains its focus behavior. These flags are optional.

## Controls

| Action | Keyboard / mouse | Gamepad |
|---|---|---|
| Carve on snow / spin in air | A / D | Left stick horizontal |
| Flip in air | Up / Down arrows | Stick vertical |
| Roll / rail balance | Left / Right arrows | Right stick horizontal |
| Crouch, then pop | Hold Space, release | Hold A / South, release |
| Tuck, reduce air inertia | W | Y / North |
| Brake / hockey stop | S | X / West |
| Left / right grabs | Q / E | LT / RT |
| Grab / style modifiers | Left Shift / Left Ctrl | LB / RB |
| Quick retry | R | B / East |
| Set grounded session marker | T | D-pad Up |
| Retry marker | Y | D-pad Down |
| Pause / menu | Escape | Start |
| Menu navigation / select | Arrows / Enter, or mouse | D-pad / A |
| Look around | Hold right mouse and move | — |
| Day / Sunset | L, or settings | Settings |
| Telemetry / editor gizmos | F1 / F2 | — |

Use Q for Safety and E for Mute. Shift changes them to Tail/Nose; Ctrl to Stale/Method; Shift+Ctrl to Japan/Blunt. Q+E crosses the skis. Gamepad triggers and bumpers select the same poses. Ctrl/RB also adds off-axis rotation/preload.

For repeated attempts, stop or ride safely before a feature, press **T**, ski the line, then press **Y** to return immediately. **R** returns to the most recent safe position. Bails recover automatically after roughly two seconds; R retries immediately. Pause has restart buttons for the top and park.

Settings include volumes, sensitivities, FOV, quality, Day/Sunset, six outfits, fullscreen, landscape/portrait resolutions, speed units and inverted mouse camera. Navigate every settings row with D-pad/arrows, adjust with Left/Right, and choose **SAVE & BACK**. Saves and the local high score live under `~/.config/unity3d/PowderFlow Studio/PowderFlow/powderflow.json` on this workstation.

## Architecture

- **Physics:** Rigidbody at 100 Hz; two contacts per ski; spring/damper support; slope gravity; low forward drag and edge-dependent lateral grip. Air rotation maintains angular momentum with torque, inertia changes and limited near-landing assistance. Landing evaluation selects perfect/clean/sketchy/bail.
- **Tricks and rails:** cumulative quaternion rotation tracking, grab hand IK, composed names, repeat penalties, combos, rail projection/capture/balance and momentum-preserving pop exits. Bails use a jointed ragdoll.
- **Camera:** travel-following spring camera with speed FOV, obstruction casts, portrait framing, landing shake and an upright horizon during rotations.
- **World:** ten Blender terrain chunks, approximately 345 m drop, designed jump/rail lines, ridge/easy area, park, big air, freeride powder and lower run. A seeded generator places vegetation and props away from central approaches.
- **Art:** `Tools/Blender` creates all 48 asset entries. The skier uses five skinned renderers with continuous joint weights and separate equipment bones. `ArtSource/Blender` stores editable source files; `Assets/Art/Generated` stores FBX, previews and metadata. `GeneratedAssetImporter` builds prefabs, colliders, LODs, materials and `AssetCatalog` automatically.
- **Rendering/audio:** original URP snow/rock and sky shaders, ACES/bloom/SSAO, two lighting presets, fading per-ski grooves, capped snow particles, and generated WAV loops/impacts. No downloaded content or music.
- **Tuning:** `Assets/Settings` holds physics, trick, camera, world, graphics and game-system ScriptableObjects. Runtime code lives in `Assets/Scripts` by subsystem; automated checks are in `Assets/Tests`.

## Scope and limitations

The core playable loop, build and automation are implemented. This delivery remains a stylized development game that needs human feedback on skiing feel, grab readability, camera framing and line variety.

- Snow and sky are editable HLSL shaders rather than Shader Graph assets. The Blender rig uses Generic import to retain ski/pole bones; it has a humanoid-compatible hierarchy, not Humanoid retargeting.
- Grab poses are procedural approximations. Trees have three geometry LODs; no billboard LOD or shrubs. Some low-poly world meshes share visual/collision geometry.
- Most gameplay tuning is in ScriptableObjects; some layout, pose, VFX and UI constants remain in source.
- Audio is generated Foley without a music track. Music volume is retained for future music. Outfit customization uses six coordinated color presets.
- Physics debug gizmos require the Unity Scene view. Optional photo mode, replay buffer and challenges are not included.
- Only Linux is packaged. Windows/macOS need their Unity build modules and target-specific verification.
- No reference video was supplied. Automated checks and inspected captures cannot certify subjective fun, literal reference matching, or hours-long stability. Performance measurements describe this workstation and the measured scenes.

See `Documentation/FINAL_REPORT.md`, `MILESTONES.md`, `DECISIONS.md` and `MASTER_PLAN.md` for evidence and implementation details.
