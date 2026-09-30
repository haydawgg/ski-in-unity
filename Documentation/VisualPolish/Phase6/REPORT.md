# VP6 — Camera, HUD and menu presentation

Completed 2026-09-30. Camera framing and menus now share the mountain’s visual direction. The gameplay HUD follows the user’s correction: **plain text in the top corners, no boxes or persistent hints, and an unobstructed center/bottom**. Runtime sessions retain the **144 FPS cap**; this phase did not measure machine FPS or run a performance benchmark.

## Gameplay HUD

- Small speed text sits in the top left with a subtle shadow for contrast against snow and sky.
- Free Ride has no persistent score, multiplier or timer. Between tricks, only speed remains visible.
- Score Session adds score/multiplier and time remaining in the top right. Trick name, landing tier and earned points appear below them briefly.
- Feedback lasts **1.8 seconds**, including a **0.5-second fade**. Landing tiers have distinct colors. Feedback stays in the top right in both orientations.
- All gameplay panels, borders, centered popups and bottom hints were removed. F1 telemetry remains an explicit debug toggle.

Actual Free Ride in the final native Park run:

![Minimal Free Ride HUD](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase6/SunsetParkPlayer.png)

Timed-session layout with synthetic trick feedback:

![Plain corner text in Score Session](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase6/UI/RideHUD1920x1080.png)

`BeforeTextFix/BoxedHUD.png` preserves the intermediate boxed layout that the user rejected. Its panels and persistent hints are absent from the final build.

## Camera

The camera follows planar travel rather than body spin, holds its heading at low speed/bail and keeps an upright horizon. Grounded framing projects the follow direction onto the support slope. Cruise, low-speed carve, rail approach, big air and bail have configurable distance/height/look targets, with a restrained portrait pullback. FOV responds smoothly to speed and state; a short terrain probe anticipates the landing, and a small landing shake decays quickly.

Obstruction clearance runs **after smoothing** as well as when choosing the desired position. A reusable sphere-cast buffer finds the nearest relevant obstacle and applies padding. Player, ragdoll and outfit-preview colliders are excluded; bails expose a hips focus without changing ragdoll forces. Reset clears the camera spring, orbit and shake.

The title camera uses a fixed valley composition ahead of the busy gate. Pausing holds the riding view. The final native Park run no longer shows the camera/jump clipping recorded in VP3–VP5.

`Before/` preserves ten matched VP5 camera states, the previous menu and the Park obstruction. Final `LandscapeCameraReview.png`, `PortraitCameraReview.png` and individual state images cover the same five conditions in both orientations. `camera-review.txt` records viewport positions, FOV, grounded state and horizon roll. These are representative state checks, including an incoming rail approach, rather than proof of every trick or prolonged rail ride.

## Menus and outfit preview

Slate surfaces, cream typography, teal selection and warm highlights replace the default Unity button skin. Settings group audio, controls and display/outfit into two landscape columns or one portrait column. Keyboard arrows/Enter, D-pad or left stick/A, mouse buttons and Escape/B remain usable. Back from the title keeps the menu open; Back from completed results clears the timed mode so untimed riding can resume.

The outfit page shows the existing six coordinated palettes on the actual generated model. **O / Y** opens it from settings; Left/Right or D-pad changes the kit, and dragging turns the preview. The preview has a separate layer/camera/render texture, reuses the scene light and disposes its resources with the world. Outfit selection updates the skier and saves with settings.

Menus and HUD scale inside `Screen.safeArea`. All fourteen settings rows and footer controls fit at **1280×720**, **1920×1080** and **1080×1920**. Fonts come from the installed DejaVu Sans family; the existing notice remains in source and in the portable package.

![Grouped settings at 1280×720](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase6/UI/settings1280x720.png)

Native art review caught double multiplication of text color and an oversized portrait main panel. The text helper now sets the label color once; the portrait panel is shorter. `BeforeTextFix/` preserves those intermediate views. Final contact sheets are `UI/Review1280x720.jpg`, `UI/Review1920x1080.jpg` and `UI/Review1080x1920.jpg`.

## Validation

| Check | Result |
|---|---|
| Focused EditMode | **2/2** pass: safe-area layout/overlap/corner bounds, fonts, feedback fade and tier colors |
| Focused PlayMode | **6/6** pass: ten landscape/portrait camera states and existing/new input/save/session behavior |
| Final results Back regression | **1/1** pass: gamepad Back leaves results and resumes untimed riding without reopening results |
| Camera framing | Skier stays inside the tested viewport bounds; upright horizon and final collider-clearance assertions pass |
| Keyboard/gamepad | Free Ride launch, portrait settings save, outfit change/Back/save and session expiry/high score pass with isolated virtual input |
| Native UI | **27 actual captures**, nine pages/states at each supported size; requested PNG dimensions verified |
| Pointer buttons | Injected IMGUI down/up events drive actual volume adjustment and Save & back; both pass |
| Protected files | **26/26** hashes match the working tree and VP5 commit `6d6d4e2` |
| Native gameplay | Sunset Park, High, 1920×1080, cap **144**, VSync 0, **92.28 m** travel, exit 0 |
| Linux build/package | Successful build, **192,798,869 bytes** reported; **71,931,250-byte** archive and independently extracted launch pass |

The final UI diagnostic covers title/settings/controls/outfit/results, Day title/outfit, riding HUD and pause at all three actual native window sizes. Its **720 Safety feedback is a scoring fixture**, not a claim that the diagnostic performed that trick. The pointer check injects IMGUI events, rather than OS mouse input. The separate Park run uses real physics descent and the minimal Free Ride HUD.

Native UI, Park and extracted-package logs contain no runtime exceptions or shader errors. The extracted package’s own `Play.sh` saves a fresh 1920×1080 menu capture. The package includes `ThirdPartyLicenses/DejaVu.txt` and excludes smoke diagnostic output. The previous VP5 archive is preserved as `Builds/Packages/PowderFlow-VP5-Linux.tar.gz`; the latest is `Builds/Packages/PowderFlow-Linux.tar.gz`, SHA-256 **0104c67b67e033957cf8721f8ec145639bb0e531ac2da54654014c5de74cd3f0**, also recorded in `package.sha256`.

Ten mountain FBXs, ten rail/box path files and six non-camera tuning assets are unchanged. CameraConfig intentionally changes framing. BailSystem only adds a camera focus reference; ski-force, air, trick, rail, ragdoll force, character, world and snow settings remain unchanged. The final results-Back correction changes menu state only; it does not alter the inspected art or HUD.

## Reproduction

Close the interactive Unity editor before batch commands:

```bash
Tools/Build/polish-presentation.sh
Tools/Build/test.sh EditMode PowderFlow.Tests.PresentationTests
Tools/Build/test.sh PlayMode 'PowderFlow.Tests.PresentationPolishTests;PowderFlow.Tests.GameFlowTests'
Tools/Build/build-linux.sh
python Tools/Validation/capture_presentation.py
python Tools/Validation/capture_regions.py --output Documentation/VisualPolish/Phase6 --only SunsetPark
Tools/Build/package.sh
Tools/Build/run.sh
```

The editor command is **PowderFlow → Configure Presentation**. It assigns fonts and creates a missing theme while preserving existing camera tuning. Theme values live in `Assets/Resources/PowderFlowPresentation.asset`; framing lives in `Assets/Settings/CameraConfig.asset`. Remove the three `smoke-*` outputs from `Builds/Linux` after diagnostic launches before packaging. The reviewed archive was independently extracted to `Builds/PresentationPackageValidation` and launched there.

## Limits and next phase

Outfits remain six coordinated presets. Grabs are stylized procedural poses. Short camera/input/art checks do not certify subjective skiing feel, every possible obstacle/trick or hours-long play.

The existing vertical ski pose exposed by actual VP5 rail capture remains assigned to **VP7 — final consistency and packaged review**. Camera clipping at the reviewed Park jump and the busy title gate are resolved in VP6. VP7 will review action consistency across the completed visual pass.
