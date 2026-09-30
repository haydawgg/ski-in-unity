# Reference video improvement pass

Completed 2026-09-30 in the existing PowderFlow Unity project. The current Linux development build was rebuilt, run and packaged. The reference guided several measured implementation/review iterations. **36 EditMode and 24 PlayMode checks pass.** Three native 1080p runs averaged 143.87–144.00 FPS on the available RX 6600 XT / Ryzen 5 9600X.

This pass starts from the completed preceding gameplay/animation pass, not a clean checkout of `ec91d35`. Its actual incoming configuration, sources and captures are preserved in [baseline report](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/REFERENCE_PASS_BASELINE.md) and `DevelopmentCaptures/BeforeReferencePass/`. Earlier `Baseline/`, `Current/` and `GAMEPLAY_QUALITY_REPORT.md` describe the preceding pass. The working Rigidbody, ski probes, force/carving model, angular momentum, graded landings, grab solver, rails, procedural rig, custom camera and Blender importer remain the architecture.

## 1. Reference observations

The original 64.898367-second, 30 FPS, 720×1280 portrait clip was reviewed across its entire duration, including 65 regular samples and consecutive takeoff, carving, grab/release, landing and rail frames at 6/12 FPS. The original Downloads file is unchanged; the project copy is byte-identical. [Detailed analysis](/home/haydend/Documents/ChatGPT/ski-in-unity/Reference/REFERENCE_GAMEPLAY_ANALYSIS.md) distinguishes observations from estimates.

The defining qualities are sustained momentum, committed edge turns, short lip extension, compact knees-up rotation, ski-contact grabs with a free balancing arm, early landing opening followed by impact compression, sideways rail balance/spin-off, and a stable upright camera following travel. Snow spray increases under load. Warm sunlight, cooler shadows and craggy snow-covered mountain layers give scale. Exact source speed, force, FOV, camera meters and hidden assistance cannot be uniquely recovered from an edited montage; these were not invented as measured targets. Original assets and Foley remain in use.

## 2. Original-game differences

The incoming controller already supported physical lips, continuous rotations, working grabs and smooth rail capture. The largest gaps were broad/delayed turn response, slow rotation input, a short released-input spotting window, a small skier in the follow view, less compact free-air posing, light loaded-turn spray, regular snow stripes, and sparse ridge snow coverage. The 12-second park review additionally exposed repeated low-energy one-probe contacts applying full Sketchy speed penalties. [Prioritized gap analysis](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/REFERENCE_GAP_ANALYSIS.md) records the original category-by-category comparison.

## 3. Gameplay changes

- Sidecut radius **14 → 12 m**, edge response **5 → 7**, edge grip **10 → 11.5**, maximum grip **18 → 20 m/s²**. Turns still result from the existing sidecut/grip forces and curved velocity path. Gravity, forward friction, drag, braking deceleration and ramp profiles stay at their incoming values.
- Yaw/pitch/roll torque **60/48/38 → 78/62/48**. Angular momentum, inertia, tuck and drag remain continuous. Measured input response rises from **0.830 → 1.122 rad/s after .25 s** and **3.316 → 4.352 rad/s after 1 s**. Free-air release retains inertia rather than stopping instantly.
- Landing preparation **.38 → .46 s**; released-input spotting **.28 → .42 s**, damping **3 → 6**. An identical aligned released 5 rad/s near-contact replay slows to **3.866**, compared with **4.790 rad/s** before. The existing 42° assistance envelope and held-input/inverted protections remain. Released ground preload now qualifies for spotting.
- Perfect/Clean impact limits **9/18 → 10.5/21 m/s**; Sketchy speed retention **.67 → .76**. The 34 m/s impact, 65° upright, 60° alignment and 13 rad/s angular bail limits remain. A new **2.5 m/s** mild-contact threshold blends retention toward full momentum only for aligned, upright non-bail contacts; a brief one-probe terrain recapture can grade Clean. Hard one-foot landings and bad alignment still fail or scrub normally. Reset clears stale last-landing state.

The same small/large production lips launch at **18.83/21.98 m/s**, with **1.67/3.27 s** airtime and **5.26/18.51 m** terrain clearance. The large lip now lands Clean at **19.93 m/s** in the focused trajectory test. Jump arcs still come from incoming velocity, slope/ramp collision, gravity and pop.

The matched six-second center park checkpoint retains **13.35 → 19.84 m/s**. The added uninterrupted twelve-second review starts at z=290 m and 22 m/s, clears the existing medium lips at z=310/440, lands at z≈376/506, reaches z=525.26, and ends at 16.11 m/s without a bail. Existing line geometry was retained; improved contact retention carries the rider into the second feature. This is a straight jump line plus separately reviewed rail approaches, not a claim of a continuous jump–rail–box combo.

## 4. Animation changes

Motion state remains the physics input to procedural body pose and final IK. Neutral bend **.26 → .30**, free-air bend **.34 → .38**, angular tuck **.30 → .46**, rotation response scale **7 → 6 rad/s**. Fast air adds up to **18° knees-up shaping and 10° torso bend**. Landing prediction reduces the tuck and visually aligns skis toward the predicted plane without driving the Rigidbody rotation.

The first .14 m stance drop was visually too crouched and was revised to **.10 m**, compared with the incoming .08 m. The settled neutral knee angles remain around 102°. Strong turns produce different inside/outside knee shapes, hip commitment and torso counterlean from actual load. Pop still rapidly extends before air tuck; the focused review measures bend .979 at crouch, .443 at extension, .731 during fast rotation, and .458 at impact recovering to .300.

Grabs retain their real ski targets and two-bone arm/leg solving. The opposite arm gains **18° spread and 20° elbow participation**, and release blends over **.13 s** instead of .09 s. All nine type / three yaw samples retain wrist error rounded to **0.000 m**; the dedicated both-hand blend checks remain under 3 cm. Ground, air and rail equipment/contact checks pass. Impact recovery slows from **2.8 → 2.2**, with existing sketchy arm/hip recovery retained.

The already improved Blender skier, five renderers, twenty-one-bone hierarchy and ten imported temporal foundations were inspected and retained. Runtime remains procedural; Animator playback and root motion do not drive skiing or trick rotations.

## 5. Exact camera tuning

| Parameter | Before | After |
| --- | --- | --- |
| Base distance | 5.8 m | 4.6 m |
| Ground / air height | 1.8 / 1.5 m | 1.6 / 1.25 m |
| Follow damping | .14 s | .11 s |
| Default normal / speed FOV | 73 / 82° | 67 / 76° |
| Speed / air pullback | 1.7 / .8 m | .9 / .45 m |
| Rail pullback / height | .5 / 1.8 m | .35 / 1.6 m |
| Look ahead / look height | 1.8 / .35 m | retained |
| Heading response / speed reference | 3 / 35 m/s | retained |

Trajectory heading, world-up horizon, portrait adaptation, landing anticipation and obstruction protection remain. Fresh settings use 67°; existing saved FOV choices are respected. Applying settings now retains the configured **9° speed widening** relative to the selected base FOV, including repeated changes. Camera regression captures at 720p and 1080p retain a **0° horizon roll** through cruise, carve, rail, air and bail states. In the matched straight state, measured camera horizontal offset changes from 8.75 to 6.77 m; its rise from 4.28 to 3.60 m includes slope/follow lag and is not the nominal height setting.

## 6. Blender work

Only **Ridge0, Ridge1, Ridge2, Ridge3 and Ridge4** were regenerated in this follow-up pass using the existing Blender CLI/Python export/import pipeline. Each editable source is `ArtSource/Blender/RidgeN.blend`, each FBX is in `Assets/Art/Generated/EnvironmentDistant/`, and Ridge2 has a regenerated preview.

The grid changes from 120×32 to 144×48 (**7,680 → 13,824 triangles per ridge**), adding branching relief/gullies/shelves while preserving seeded peak profiles, layer positions, buried skirts and collider-free/shadow-free rendering. Open-surface normals now explicitly face upward; Ridge0 previously had all normals reversed and no snow material coverage. New area-weighted snow coverage is **43.15%, 44.99%, 45.19%, 41.26%, 40.51%**, compared with 0% / roughly 7% before. All five sources pass finite-coordinate, normal, coverage and <20,000-triangle checks.

The catalog still has **63 assets**, and **58 non-ridge manifest entries are byte-for-byte unchanged as JSON records**. The skier/equipment, six working jumps, ten rail/box paths, terrain chunks, four evergreen variants with three LODs, rocks and props were retained. The prior pass's separate skier/ramp work remains documented in its report.

## 7. Environment, snow, lighting and audio

Snow ripple strength **.05 → .022**, scale **3 → 1.8**, reduced regular ripple color contribution and stronger irregular phase/strength variation make wind detail less like a repeated washboard. Snow still has broad variation, slope rock, distance fading, sparkle and cool shadows.

Turn spray now uses normalized **actual lateral acceleration** in addition to speed/edge/slip/brake. Added load strength is .65 and fan speed 2.1; base rate **50 → 60**, landing burst **80 → 95**, contact accents **14 → 18**, brake cloud rate **22 → 28**, flake size **.07 → .075 m**, puff size **.42 → .50 m**, opacity **.25 → .30**. The combined 1,000-particle pool cap and airborne/reset suppression remain. Tracks retain separate contact paths, stop in air and fade over 100 seconds; width **.11 → .095 m**, groove alpha **.40 → .32**.

Sunset light changes to RGB **1/.84/.76**, cooler ambient **.37/.43/.58**, a less pink horizon **.76/.60/.64**, fog **.51/.56/.69** at .0007 density. The low 13° sun, long shadows, Day preset, 150 m shadow distance, instancing and tree LODs remain. The five rebuilt layers provide more snow/rock definition and atmospheric scale. Loaded carving/braking now modulates original snow Foley, while grind volume/pitch use actual rail tangent velocity; wind/pop/landing clips and volume preferences remain.

## 8. Tests and runtime results

The final **36/36 EditMode** and **24/24 selected PlayMode** checks have zero failures/skips. XML receipts and the exact selected filter are under `AfterReferencePass/`. This includes force/ground/brake checks, landing failure boundaries, production lips, released preload, full rotation acceptance, rail capture/exit/contact, nine grab types, both-hand blend/release, equipment attachment, camera states, all five regions, snow expiry/reset/caps, input/settings/save/outfit/session flows, full mountain traversal and matched reference captures. The explicit grab tuning utility is not included in the acceptance count.

The full mountain test reaches **1,408 m across all five regions**. Native development runs exit 0 with no detected runtime exception or shader-error marker: Park **141.12 m**, Big Air **128.17 m**, Day Freeride **72.36 m** over their twelve-second sample windows. Native rail review captures successfully, pops at **10.00 m/s**, lands, and preserves both ski axis dot products at 1.000. The Park timing route differs from the seeded 22 m/s matched park fixture; their distances/speeds are not interchangeable.

Final clips/states cover straight, both carve signs, hard skid, small/large production jumps, 360, 720, flip, cork, grab, sketchy landing, rail entry/slide/pop/landing, park run and Day/Sunset mountain views. Full rotations use reproducible initial angular momentum, with input responsiveness checked separately; the rail pop rider uses input feedback to aim for switch. Captures are rendered at 1280×720, with twelve-FPS encoded sequences and a six-FPS twelve-second park-line sequence. Named scenarios relocate explicitly; each individual sequence is continuous. Captures were visually inspected alongside reference frames rather than treating test passes as proof of feel.

## 9. Observed performance

Native Mono development player, **1920×1080, quality 2, render scale 1, vSync off, 144 FPS cap**, AMD RX 6600 XT / Ryzen 5 9600X. Each route samples twelve seconds after a two-second warmup. Screenshot readback, Blender, Unity Editor and video encoding were absent from the profiling windows.

| Route | Samples | Average FPS | Mean ms | p95 ms | p99 ms | GC bytes/frame |
| --- | --- | --- | --- | --- | --- | --- |
| SunsetPark | 1728 | 144.00 | 6.945 | 7.078 | 7.173 | 1970 |
| SunsetBigAir | 1727 | 143.87 | 6.951 | 7.068 | 7.169 | 2008 |
| DayFreeride | 1728 | 143.99 | 6.945 | 7.072 | 7.167 | 1713 |

The matched before Park profile was **143.99 FPS**, mean 6.945 ms, p95 7.076 ms and 1963 allocation bytes/frame. After Park is effectively unchanged at this cap; no significant route slowdown was observed despite the ridge/spray changes. These samples exceed the requested practical 60 FPS target on this hardware/configuration. They are not an uncapped headroom measurement or a low-end hardware guarantee. Main-thread recorder time includes frame-cap waiting; GC values are managed allocation, not GC pause duration. JSON receipts retain exact values.

## 10. Before versus after

[Camera/carve comparison](/home/haydend/Documents/ChatGPT/ski-in-unity/DevelopmentCaptures/ReferenceComparison/CameraCarve.jpg), [air/grab/landing comparison](/home/haydend/Documents/ChatGPT/ski-in-unity/DevelopmentCaptures/ReferenceComparison/AirGrabLanding.jpg), [rail/environment comparison](/home/haydend/Documents/ChatGPT/ski-in-unity/DevelopmentCaptures/ReferenceComparison/RailEnvironment.jpg) and [all measurements](/home/haydend/Documents/ChatGPT/ski-in-unity/DevelopmentCaptures/ReferenceComparison/measurements.md) accompany the matched states and clips.

| Matched state | Head-to-boot screen height before / after | Speed m/s before / after |
| --- | --- | --- |
| Straight | 8.5% / 11.7% | 16.62 / 16.62 |
| CarveLeft | 8.3% / 11.6% | 16.19 / 16.02 |
| CarveRight | 8.2% / 11.3% | 16.16 / 15.97 |
| SmallJumpAir | 7.6% / 11.0% | 18.67 / 18.67 |
| LargeJumpLanding | 7.5% / 10.6% | 14.12 / 19.38 |
| Spin720 | 7.8% / 9.8% | 21.08 / 21.08 |
| Grab | 4.3% / 6.2% | 17.12 / 17.12 |
| RailSlide | 10.0% / 13.0% | 11.78 / 11.77 |
| RailExitLanding | 9.6% / 13.5% | 12.50 / 12.56 |
| ParkRun | 7.9% / 11.6% | 13.35 / 19.84 |

The straight skier screen span grows about **37%**. Both loaded turns tighten from estimated radii around **24.6–24.8 → 20.1 m** (speed² / sampled lateral load), with similar 16 m/s speed. Turn particle snapshots rise from **29/31 → 50/52**, and small/large landing bursts are stronger. Crouch/pop phases remain distinct, fast-air compactness increases, grabs retain actual contact, and preparation starts earlier. The large matched landing changes from Sketchy at 14.12 to Clean at 19.38 m/s; spin/flip/cork review contacts also become Clean. Rail framing improves while slide momentum and switch-pop landing remain valid. The snow surface has fewer regular stripes, grooves are subtler, and ridge snow/rock structure is more readable.

Screen height measures projected skeleton head-to-boot span, not the entire equipment silhouette. The portrait reference's estimated 18–25% skier/equipment span is a different metric/aspect ratio. Snapshot particle counts and bend values are state samples, not universal averages. Baseline last-landing labels could persist across scenario relocation; airborne labels do not represent new contacts. Temporal sequences and focused motion metrics support the phase comparisons.

Iterations and receipts remain in `ReferenceComparison/ControlIteration`, `PoseCameraIteration`, `EnvironmentIteration`, `FlowIteration` and `FlowCorrection`. Visual review rejected excessive standing squat; the longer line review then exposed and corrected mild-contact speed loss before final validation.

## 11. Remaining gaps

- The reference has more body looseness and outfit deformation at close range. Current procedural shoulders/hips and compact poses improve gameplay readability, but are not equivalent to a hand-authored animation set.
- The landscape follow view remains wider than many edited portrait shots. Large tricks and grabs are more readable, while exact shot framing is deliberately not reproduced.
- Ridge snow faces remain visibly faceted, particularly near the lower/freeride vista; finer organic rock/snow transitions and more intermediate spurs would improve them further. The existing trees/terrain remain stylized.
- Small terrain contact oscillations still occur in the twelve-second line trace. Their extreme speed penalty is corrected, but underlying contact/suspension smoothing deserves a later focused pass. Larger one-foot/badly aligned attempts still scrub or bail.
- This pass validates a continuous two-jump line and separate rail flows. A longer authored jump–rail–bank–box line and human controller feedback remain useful next refinements.
- The short native windows and automated traversal do not establish hours-long stability or all-hardware performance. Edited reference sound cannot determine an exact Foley mix.

## 12. Exact regeneration, test, build and run commands

Run from the existing project:

```bash
cd /home/haydend/Documents/ChatGPT/ski-in-unity

# Existing pipeline: Blender source/FBX generation, manifest validation, Unity import.
Tools/Build/generate-assets.sh backdrop
blender --background --python-exit-code 1 --python Tools/Blender/validate_backdrop.py -- --require-quality --output DevelopmentCaptures/AfterReferencePass/backdrop-validation.json
python3 Tools/Blender/validate_assets.py

# Final regression selection and matched frame capture.
Tools/Build/test.sh EditMode
POWDERFLOW_REVIEW_OUTPUT=DevelopmentCaptures/AfterReferencePass POWDERFLOW_REFERENCE_OUTPUT=DevelopmentCaptures/AfterReferencePass Tools/Build/test.sh PlayMode "$(cat DevelopmentCaptures/AfterReferencePass/test-filter.txt)"
python3 Tools/Validation/compare_reference_pass.py

# Linux Mono Development build; optional native profiling/captures; package.
Tools/Build/build-linux.sh
python3 Tools/Validation/capture_regions.py --output DevelopmentCaptures/AfterReferencePass --only SunsetPark --rail --profile
python3 Tools/Validation/capture_regions.py --output DevelopmentCaptures/AfterReferencePass --only SunsetBigAir DayFreeride --profile
Tools/Build/package.sh

# Interactive play.
Tools/Build/run.sh
# Or the self-contained packaged launcher:
/home/haydend/Documents/ChatGPT/ski-in-unity/Builds/Linux/Play.sh
```

The Blender wrapper uses `/usr/bin/blender` 5.2.2 LTS and the Unity wrapper uses `/home/haydend/Unity/Hub/Editor/6000.3.25f1/Editor/Unity`. The Blender stage is `--background --python-exit-code 1 --python Tools/Blender/build_all_assets.py -- --stage backdrop`; Unity import uses `-batchmode -nographics -quit -projectPath <project> -executeMethod PowderFlow.EditorTools.ImportGeneratedAssets`. The build invokes `PowderFlow.EditorTools.BuildGame`. Source snapshots/hashes, asset retention, final XML, metrics, profile JSON and native images are in `AfterReferencePass/`. The refreshed archive is [PowderFlow-Linux.tar.gz](/home/haydend/Documents/ChatGPT/ski-in-unity/Builds/Packages/PowderFlow-Linux.tar.gz); keep the complete Linux folder together.
