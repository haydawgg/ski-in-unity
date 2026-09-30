# VP7 — Final consistency and packaged review

Completed 2026-09-30. All seven visual phases are complete. Current validation covers **1280×720 and 1920×1080 desktop landscape**, following the user's desktop-only scope. Runs retain the **144 FPS cap**; no machine FPS or frame-time benchmarks were collected.

## Repairs

### Rail and airborne ski alignment

Rail capture sets the body kinematic and the skier ungrounded. The old pose therefore used airborne knee/ankle rotations while grinding: skis pitched into the rail, and tucking could make them nearly upright. The final pose samples the existing rail path and actual riding collider, places bindings 3 cm above its surface, and aligns the boots/skis with the rider's heading projected onto the rail plane. A narrower forward stance keeps the skis over a tube; sideways slides retain their wider stance. Switch and sideways yaw remain visible. Held grab/style inputs cannot lift or cross the skis during capture; airborne grabs resume after pop.

Two-bone IK now accepts the rider's bend direction, so knees and grab elbows follow a rotating skier rather than the world root. Ordinary ungrabbed air poses keep their skis along the body heading with a small toe lift while the knees compress. Grabs retain their separate authored poses and independent ski bones.

- **12 actual captured slides:** flat/down/kink/rainbow/wide rails; flat/down/kink boxes; switch, sideways, tuck and held grab/style inputs.
- Final ski-heading dot products **1.000**, contact offset **0.030 m**, boot/binding origin spacing **0.095 m**. Pop, Safety grab and grounded transition checks pass.
- **Six plain air poses:** glide/tuck at yaw 0°, 90° and 180°; the 90° pose also has 22° roll. Heading alignment improved from **0.891 / 0.417** to **0.999 / 0.993** for glide/tuck. Binding spacing stays **0.085 m**.
- **Nine grabs at three orientations:** 27 wrist-target checks, maximum error **0.112 m**, with independent crossed skis preserved. These are stylized pose/reach checks; they do not certify every animation combination.

Matched baselines and final views are in `Before/`, `Rails/`, `BeforeAir/` and `Air/`. `RailsReview.jpg`, `AirReview.jpg` and `GrabsReview.jpg` were inspected directly. Native rail capture, pop and landing also pass: pop speed **9.98 m/s**, both ski heading values **1.000**, successful grounded landing. The displayed speed remains correct during capture.

![Matched visual repairs](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase7/BeforeAfter.jpg)

### Decorative jump banks

The ten regional captures exposed a large raised decorative bank obscuring the skier beside JumpLarge. The bank formerly spread its raised shoulder beyond the existing collision wall. The generator now brings that shoulder down to the grade within **0.20 m** of the wall and keeps the broad outer toe low. The revised Day and Sunset views show the skier beside the jump. All six jump Blender sources and FBXs were regenerated with the focused `jumps` stage.

The **12 original continuous-surface/side collision fingerprints** still match the VP4 baseline. Decorative banks remain noncolliding. The jump top, physics contacts and riding geometry are unchanged. `BeforeBanks/` preserves the obscured views; `Regions/` contains the final captures.

## Action, scenery and presentation review

Two **12-second** Day/Sunset sequences use actual injected carving/braking, crouch/preload/pop, spin/Safety, rail capture/pop, landing, crash, reset and powder travel. Each sequence records one takeoff, two successful landings, one crash, 19 rail samples and 26 grab samples. Maximum tracked air yaw is **172.5° Day / 169.8° Sunset**. The captured spin is not a completed full rotation; the separate existing full-spin landing regression passes.

The clips contain 288 samples each at **24 samples per simulated second**, 1280×720. This is a capture cadence, not measured hardware FPS. The fixture deliberately relocates the skier between reviewed features. `ActionsReview.jpg`, the 18 action stills and half-second samples from both encoded clips were inspected for skier visibility, ski orientation, camera continuity within each action, snow/contact transitions and retry cleanup. Effects remain within the configured particle cap; reset samples have no live particles.

![Sunset action clip](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase7/SunsetMotion.mp4)

Ten final 1920×1080 captures cover **Easy, Park, Big Air, Freeride and Lower Run in both presets**, with **Evergreen, Violet, Signal and Alpine** outfits. The skier stays visible, the horizon stays upright, powder/groomed assignments are correct, and the six custom shaders pass compilation checks. The unchanged scenery LOD, rig, terrain seams, post-processing resources, soft shadows and runtime fog pass focused asset checks.

The final native UI review contains **18 actual captures**, nine pages/states at each desktop size: title, settings, controls, outfit, results, Day title/outfit, riding HUD and pause. Both contact sheets and the full HUD/settings views were inspected. Real IMGUI pointer adjustment and Save & back pass. The HUD's **720 Safety** is synthetic layout feedback; the separate native rail run earns its actual Flat Slide feedback.

Gameplay retains the user's minimal UI: plain speed text top left, timed-session score/time top right, and 1.8-second trick feedback. Free Ride shows only speed between tricks. Camera tuning and the presentation theme are unchanged.

## Validation and delivery

| Check | Final evidence |
|---|---|
| Focused EditMode | **10/10 pass** after jump regeneration: rig, terrain/LOD, explicit collision/fingerprints, post/fog/shadow resources, desktop layout/fonts and atomic save |
| Initial focused PlayMode | **7/7 pass**: four final-review checks plus existing rail, full-spin landing and gamepad desktop settings save |
| Affected final pose/art checks | **5/5 pass** after bank/air repairs: rail, ungrabbed air, grabs, both action sequences and ten region views |
| Distinct PlayMode checks | **8 passing checks** across the two runs; overlapping checks are not counted twice |
| Protected files | **27/27** match the saved snapshot and VP6 commit `a3b7980`: ten terrain FBXs, ten rail/box paths and seven tuning assets |
| Native UI | **18 captures**, exact 720p/1080p dimensions, pointer adjustment/save, cap 144 and exit 0 |
| Native gameplay | One High-quality 1080p Sunset Park launch, actual rail capture/pop/landing then **92.28 m** descent, cap **144**, VSync 0 and exit 0 |
| Final build | Successful Linux build, **192,796,536 bytes** reported by Unity |
| Portable archive | **71,930,639 bytes**, font notice included, smoke outputs excluded |
| Independent package launch | Fresh extraction to `Builds/FinalPackageValidation`, its own `Play.sh`, fresh 1920×1080 menu PNG and exit 0 |

Native UI/gameplay/package logs contain no runtime exceptions or shader errors. The previous archive is preserved as `Builds/Packages/PowderFlow-VP6-Linux.tar.gz` with its recorded VP6 checksum. The final archive is `Builds/Packages/PowderFlow-Linux.tar.gz`, SHA-256 **f22db69956304e0a70469b8571a9d9c8a63adae1a7773f841afa0c213ae2286b**, also recorded in `package.sha256`.

No gravity, carving, air/rail forces, capture, balance, scoring, camera/world/character/snow tuning or terrain/path edits were made. The rail/air repair changes visual transforms and IK only. Asset catalog order follows the regenerated manifest; existing asset references are retained. Serializer whitespace and preview object-ID noise were removed from the final diff.

One editor startup stalled before tests began and was restarted; its stalled output was not counted as a passing run. During fixture preparation, grab inputs were cleared before the Safety pop check, and the action fixture gained actual crouch/preload input to produce meaningful rotation. Final evidence comes from the passing runs listed above.

## Reproduction

Close the interactive editor before batch work:

```bash
Tools/Build/generate-assets.sh jumps
Tools/Build/test.sh EditMode 'PowderFlow.Tests.CharacterTests;PowderFlow.Tests.EnvironmentAssetTests;PowderFlow.Tests.ParkAssetTests;PowderFlow.Tests.GraphicsFoundationTests;PowderFlow.Tests.SaveTests;PowderFlow.Tests.PresentationTests'
Tools/Build/test.sh PlayMode 'PowderFlow.Tests.FinalPolishTests;PowderFlow.Tests.RailPlayTests;PowderFlow.Tests.AirPlayTests;PowderFlow.Tests.GameFlowTests.GamepadSettingsCanSaveDesktopResolution'
Tools/Build/build-linux.sh
python Tools/Validation/capture_presentation.py
python Tools/Validation/capture_regions.py --output Documentation/VisualPolish/Phase7 --only SunsetPark --rail
```

Encode each `Logs/VP7-<preset>-frames/%04d.png` sequence with ffmpeg at 24 samples/s. Before `Tools/Build/package.sh`, remove the seven known `smoke-*` outputs from `Builds/Linux`. Extract the resulting archive into a fresh directory and launch its own `Linux/Play.sh --menu-capture` for the independent launch check. Normal play uses `Tools/Build/run.sh`.

Baseline flags `POWDERFLOW_FINAL_BASELINE=1` and `POWDERFLOW_FINAL_AIR_BASELINE=1` are optional comparison tooling; run them against an appropriate preserved source revision rather than overwriting the saved original baselines with current poses. XML results, metrics, native JSON reports, package validation and protected-file checksums remain in this folder; raw frames and logs stay in ignored `Logs/`.

## Remaining limits

The visual plan is complete. Human play feedback is still needed for skiing feel, grab readability and line variety. Short representative captures/checks cannot certify every trick, obstacle or hours-long stability. Grabs remain procedural approximations; outfits are six presets, lift chairs are static, and snow effects are visual overlays. Photo/replay modes and challenges remain outside this delivery. Only Linux is packaged; Windows/macOS require their build modules and target-specific verification. No reference video was supplied, so literal reference matching is not claimed.
