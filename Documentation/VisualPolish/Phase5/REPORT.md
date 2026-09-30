# VP5 — Snow interaction and motion effects

Completed 2026-09-29. Separate ski grooves now sit on the snow, and reusable flake, powder and rail pools give the action softer contact effects. All runtime sessions retain the **144 FPS cap**. This phase used focused behavior and visual checks without machine FPS measurements or performance benchmarks.

## What changed

- Grooves have a soft cavity and pale snow lip, wider/deeper powder marks, shader age fade and shadow/fog response. New samples project onto actual snow geometry: support contacts average ski tip/tail probes and can otherwise put the visual strip inside uneven terrain. A gap found in the powder capture was repaired this way.
- Trail connections break during flight, rail riding, bail and retry. Existing tracks remain and fade; retry creates no connecting teleport streak. Expired lists now clear their rendered meshes, including when fewer than two samples remain. Runtime meshes, world objects and the cloned groove material are disposed with the run.
- Fractional emission accumulates across render steps. The old per-frame rounding suppressed steady spray: the baseline Day carve had zero live particles and powder had one. The final matched shots have 27 carve and 46 powder particles. The emission test also checks equal simulated duration at different logical step sizes and limits catch-up bursts.
- Three reusable world-space particle pools separate fine carve spray, translucent low powder/brake puffs and rail frost. Travel, slip, edge, speed and snow type determine the emission direction/rate; switch trails follow actual travel. Rail emission follows the captured path and uses position change for its speed, since the rail body can be kinematic.
- Landing flakes and a low radial puff accent originate at actual ski contacts. Crash flakes stay bounded; ground spray stops during air/bail. Retry clears all live particles and fractional emission debt. Both baseline reset views retained 217 particles; both final reset views contain zero.
- Procedural soft flake and three-lobed puff textures use a new URP shader with depth-softened intersections, camera fade, lighting, shadows and fog. The textures, material assignments, rates, sizes, opacity and trail limits are reproducible through GraphicsConfig and the snow configuration command.

The default combined maximum remains **1,000 particles**: 550 flakes, 350 powder puffs and 100 rail frost. The controller enforces both the individual pool limits and the combined cap. Trails retain a maximum of **1,100 samples per ski** and fade over **100 seconds**; at high speed the sample cap can shorten their visible history.

## Inspected evidence

`Before/` preserves 20 VP4 action captures. Final captures repeat straight descent, carving, braking, switch, powder, flight, landing, rail, crash and retry in **Day and Sunset at 1920×1080**. The review follows actual physics/input/contact transitions, including real rail capture and two actual landings. Camera offset, FOV, spawn points and timing are recorded in `SnowPolishTests.cs`; action state/resource counts are in `action-review.txt`.

Before:

![VP4 carve tracks](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase5/Before/DayCarve.png)

After:

![VP5 soft carve grooves](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase5/DayCarve.png)

![Powder grooves after terrain projection](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase5/DayPowder.png)

![Contact-centered sunset landing accent](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase5/SunsetLanding.png)

`DayReview.png` and `SunsetReview.png` collect the ten final states. `DayMotion.mp4` and `SunsetMotion.mp4` contain two eight-second sequences: descent, carve, brake, takeoff, landing, crash and retry. They have **10 image samples per simulated second at 1280×720**; this is a capture setting, not a machine frame-rate result. Each sequence contains an actual takeoff and clean landing. Inspected frames retain readable skis/landing terrain without opaque snow clouds; fine spray and rail frost are intentionally small at gameplay distance.

## Validation

| Check | Result |
|---|---|
| Targeted EditMode | 2/2 pass: fractional emission/catch-up bound, saved shader/material/soft texture settings |
| Targeted PlayMode | 3/3 pass: action states, lifecycle/resource limits and Day/Sunset motion sequences |
| Final repeat | Same three PlayMode checks pass after correcting the powder groove gap |
| Ski separation | Both grooves present; measured separation 0.3–0.65 m |
| Connection breaks | Retry mesh triangles do not bridge distant contact points |
| Expiry/disposal | Empty track lists clear old mesh geometry; both runtime meshes destroyed with the world |
| Particle limits | Repeated landing/crash events respect a reduced 120-particle combined test cap |
| Flight/retry | No new ground emission in flight; retry clears live particles to zero |
| Protected files | 25/25 hashes match VP4: ten terrain FBXs, ten path JSON files and five tuning assets |
| Linux build | BuildPipeline succeeds; 191,286,457 bytes reported |
| Native gameplay | Sunset Easy, 1920×1080, High, cap 144, VSync 0; 101.95 m travel; exit 0 |
| Extracted package | Its own Play.sh saves a fresh 1920×1080 menu capture; exit 0 |

Native gameplay and extracted-package logs contain no runtime exceptions or shader errors. The native run uses the final reviewed build. The 144 FPS cap is a runtime policy, not a claim about hardware performance. XML, `native-runs.json`, `package-validation.json` and `summary.json` record the results.

Ski-force, air, trick, rail, camera, character and world tuning are unchanged. All protected file hashes were also checked against commit `0a0f2bf`, not only against the working-tree snapshot. A new full mountain descent or regional launch matrix was unnecessary for these visual-only changes.

## Build and reproduction

Play with `Tools/Build/run.sh`. The latest archive is `Builds/Packages/PowderFlow-Linux.tar.gz`: **71,178,305 bytes**, SHA-256 **87909a3d3563dc7403ad9ca3f6ecaa6158159ade7bcf2da41eedb93e561df1f9**. The previous VP4 archive is preserved locally as `PowderFlow-VP4-Linux.tar.gz`. The package includes `ThirdPartyLicenses/DejaVu.txt` and excludes smoke diagnostic output.

Close the interactive Unity editor before batch commands:

```bash
Tools/Build/polish-snow.sh
Tools/Build/test.sh EditMode PowderFlow.Tests.SnowVisualTests
Tools/Build/test.sh PlayMode PowderFlow.Tests.SnowPolishTests
Tools/Build/build-linux.sh
python Tools/Validation/capture_regions.py --output Documentation/VisualPolish/Phase5 --only SunsetEasy
Tools/Build/package.sh
```

The editor command is **PowderFlow → Configure Snow Interaction Visuals**. Full graphics configuration also reapplies these materials/textures. Clear the three `smoke-*` files from `Builds/Linux` before packaging after a diagnostic launch. The reviewed archive was independently extracted to `Builds/SnowPackageValidation` and launched there.

## Limits and next phase

Grooves are visual overlays, not physical snow deformation. Puffs are soft billboards, not volumetric simulation. The short action reviews and resource stress check do not certify hours-long play or subjective skiing feel.

The real rail capture exposes an existing vertical ski pose in both baseline and final images; this remains assigned to **VP7 pose consistency review**. The previously recorded Park camera/jump overlap and the busy gate behind the menu belong to **VP6 camera/menu presentation**.

Next: **VP6 — camera, HUD and menu presentation**.
