# Ski contact quality and continuous freestyle line

Completed in the existing PowderFlow project, from Git baseline `0011ff6`. Unity 6000.3.25f1 / Blender 5.2.2 LTS. The Rigidbody controller, four spherecast samples, support spring/damper, carving, air momentum, trick scoring, procedural pose/IK, rail paths, camera, mountain, shaders and Blender importer remain the architecture.

## 1. Measured source of oscillation

Two causes were isolated from 100 Hz traces, before changing physical tuning:

- **Sweep normals at mesh triangles and open lips:** a mathematically constant mesh slope retained all four hits, but the sphere sweep's separation normal and derived height varied. This produced 10.009 mm contact-height standard deviation and 1.250 m/s² support-acceleration standard deviation in the isolated deterministic fixture. Raw front/rear edge normals could point downhill near an uphill jump lip. This was a normal/height signal problem, not constant-slope hit/miss noise.
- **Stale support frame on landing:** after a valid landing projected velocity onto the snow, the damper still used the lip's old support normal. The baseline park's first landing at 4.22 s had contact-normal z=+0.224, support-normal z=−0.178 and a false −7.883 m/s normal approach velocity. It applied **158.932 m/s² upward support acceleration**. Repeated contact-reach crossings followed. The matched twelve-second park trace had **34** ground/air transitions.

The fixture has separate Flat, constant Slope, convex/concave Rollers, Ripple, shallow triangle Seam and legitimate OneSki profiles. It is isolated at x=4000 m to avoid other editor fixtures; production traces near the central route separately establish the practical improvement. The interactive PhysicsTest contact route and rendered lab captures use x=270 m. All original baseline code/config bytes are retained under `Before/SourceSnapshot/`.

## 2. Exact contact changes

`SkiContactSystem` still makes **two spherecasts per ski**. For each existing valid sweep hit, it queries the **same collider** for the supporting face. At an open edge, it tries the existing hit point and a 3 cm inward/outward offset; the face must remain within 18 cm of that hit and face upward. This corrects the separation normal and derives height from the physical surface plane. A face query cannot create or extend a spherecast hit. Reach, sample locations and takeoff locking are unchanged.

Each ski exposes raw hit, sample count, raw normal/height, refined normal/height and sample confidence. Confidence is diagnostic; it does not manufacture support or force both skis to contact. The controller initializes `SupportNormal` from the new snow contact on a ground/air transition, then retains the established normal smoothing during ground travel. Support-acceleration and grounded-transition diagnostics and an opt-in 100 Hz CSV recorder were added. F1 shows the raw/refined values and acceleration / equivalent force; F2 retains editor contact gizmos.

**No time hysteresis, heavy height filter, artificial hold contact, new suspension model or recapture increase was required.** Missing physical support releases immediately. This avoids latency at genuine lips. Contact confidence does not scale the established spring force.

## 3. Before/after stability

Same controls, routes and 100 Hz step; first second excluded from variance calculations. Production metrics use grounded samples. Height SD is contact-distance variation, and height step is mean absolute adjacent-sample change.

| Route | Height SD before → after (mm) | Height step before → after (mm) | Support acceleration SD before → after (m/s²) |
| --- | ---: | ---: | ---: |
| Constant isolated slope | 10.009 → 0.004 | 2.279 → 0.002 | 1.250 → 0.0005 |
| Legitimate one-ski slope | 10.031 → 0.004 | 3.048 → 0.002 | 1.483 → 0.0005 |
| Shallow triangle seam | 25.303 → 20.236 | 5.947 → 2.647 | 3.747 → 3.395 |
| Convex / concave rollers | 20.913 → 22.902 | 2.640 → 0.531 | 2.119 → 1.722 |
| Production straight skiing | 3.845 → 2.661 | 0.646 → 0.212 | 0.540 → 0.266 |
| Production small undulations | 7.653 → 8.396 | 0.830 → 0.494 | 0.763 → 0.647 |

The isolated smooth slope's normal-velocity SD fell from **0.066821 → 0.000025 m/s**. Four probes remained valid throughout. The one-ski route retained exactly one supported ski for **all 1,000 ticks**, with no ground transitions. Rollers and seam also retained continuous grounding. Total roller/undulation variance can rise slightly while the skier follows real curvature; their abrupt sample-to-sample height changes and support-force variation decreased. These were not flattened with temporal filtering.

Production transition counts, including initial ground acquisition:

| Matched production route | Before | After | Expected behavior |
| --- | ---: | ---: | --- |
| Small jump / five seconds | 13 | 3 | Initial contact, takeoff, landing |
| Large jump / seven seconds | 13 | 3 | Initial contact, takeoff, landing |
| Existing two-jump park / twelve seconds | 34 | 5 | Initial contact plus two launches/landings |

The existing park checkpoint ends at **21.49 m/s**, compared with **16.09 m/s** before. Seventeen matched route summaries, CSVs and the [contact comparison plot](../DevelopmentCaptures/ContactLinePass/Comparison/ContactStability.png) are in `ContactLinePass/Comparison/`. Variance across jump/ramp samples combines intentional profile changes and suspension travel; it is not a pure jitter measurement. Raw height is unavailable in clear air, so the comparison plot masks contact height there.

## 4. Configuration and spring review

`SkiPhysicsConfig.refineContactSurface = true` is the only new physics setting. It is exposed for diagnosis. **supportSpring=180, supportDamping=25, normalResponse=9, contactReach=1.05** remain unchanged.

For acceleration support, natural frequency is √180=13.416 rad/s and damping ratio is 25/(2√180)=**0.932**, near critical damping. An 80 kg equivalent is 14,400 N/m and 2,000 N·s/m, but mass does not change the acceleration model. Measured frame/normal defects justified correcting contact information rather than changing these values.

Sidecut **12**, steer response **7**, edge grip **11.5**, maximum grip **20**, yaw/pitch/roll torque **78/62/48**, landing preparation **.46 s**, spotting **.42 s / 6**, and terrain recapture threshold **2.5 m/s** are retained. Camera and character visual tuning are unchanged. WorldConfig's park restart moves **z=280 → 250** to provide a proper standing-start roll-in; authored placement and branch data live in `WorldConfig.freestyleLine`.

## 5. Jump regressions

Original production collision profiles and all 63 pre-existing FBXs/manifest records are byte-identical to the baseline. The additional park large kicker instantiates the existing valid `JumpLarge` at unit scale.

| Original production lip | Launch speed | Airtime | Maximum terrain clearance | Landing |
| --- | ---: | ---: | ---: | --- |
| Small, z=105 | 18.87 m/s | 1.70 s | 5.39 m | Perfect / 20.62 m/s |
| Large, z=900 | 22.16 m/s | 3.33 s | 18.94 m | Clean / 20.82 m/s |

The prior pass measured 18.83/21.98 m/s and 1.67/3.27 s respectively. Small numerical differences come from corrected surface information; lips were not regenerated. Immediate pop, clean physical lip release, preserved takeoff lock, no false grounding while clearly airborne, full spins/flips, rail capture/pop and both production lips pass.

## 6. Landing regressions

Focused checks cover changing from a takeoff plane to a landing plane without a second launch, one landing per pop, stable two-ski support, true one-ski travel, mild recapture with preserved momentum, and immediate support loss on removal of the real surface. Production small/large contacts now settle without repeated re-grounding. Existing inverted/alignment/impact bail limits remain in force.

The existing two-jump test counted `TookOff` only when the **previous motion snapshot's** airtime was below .1 s. Clean landings exposed that stale-snapshot assumption. The test now counts the actual event directly, verifying two launches and two landings. No gameplay event was fabricated to satisfy it.

## 7. Authored freestyle line

All features are within the existing Terrain Park. The final automated review continues through the last rail's snow landing and **.35 s continuous grounded recovery**, ending before z=800.

| Stage | Asset / placement (x, z), metres |
| --- | --- |
| Standing roll-in | (0, 250) |
| Medium kicker | Existing JumpMedium (0, 310) |
| Branch left | LineRailEntry (−12, 417) → relocated Rail_down (−12, 425) |
| Branch right | LineBoxEntry (+12, 417) → relocated Box_wide (+12, 425) |
| Left side hit | Existing JumpSmall model (−12, 468); right branch bypasses it |
| Reconnection / down box | LineBoxEntry (−12, 527) → relocated Box_down (−12, 535) |
| Large kicker | Existing JumpLarge model (−18, 615) |
| Wide exit rail | LineRailEntry (−18, 727) → relocated Rail_wide (−18, 735) |
| Natural snow exit | Starts at (−18, 790), after the final pop/landing |

The previous kink/approach were shifted to x=−26 at z=440/417 to clear the new entry. Other central jumps, the original small/large lips, flat/rainbow rails, remaining boxes, mountain/scenery and regions remain available. The last wide rail was moved uphill during measured iteration so its landing and recovery fit inside the park.

A small serialized definition stores asset IDs, positions, yaw, scale, approach-speed guidance, exit direction, replacement/reuse flags, branch designation and two routes. There is no new world framework. Markers reuse existing Blender-generated feature markers.

## 8. Measured speeds and spacing

The diagnostic rider injects **controls only** after the explicit start: no inter-feature relocation, velocity edits, forces, catch/rescue or bail suppression. The main review starts from rest; the right-branch review starts at 22 m/s to represent downhill entry. Human entry speeds can vary.

Main-line approach speeds: **13.55 m/s** at the medium ramp, **18.37** at the rail bank, **20.27** at the side hit, **22.15** at the box bank and **23.98** at the large ramp. Authored `approachSpeed` values are guidance, not a speed clamp.

| Main transition | Launch/pop (m/s) | Airtime (s) | Landing z (m) | Landing (m/s) | To next bank/feature |
| --- | ---: | ---: | ---: | ---: | --- |
| Medium kicker | 12.74 | 1.91 | 349.51 | 12.76 | 67.5 m / 4.61 s |
| Down-rail pop | 18.43 | 0.91 | 452.36 | 19.44 | 15.6 m / 0.82 s |
| Side hit | 20.35 | 1.79 | 514.73 | 21.73 | 12.3 m / 0.58 s |
| Down-box pop | 22.08 | 0.70 | 559.31 | 22.76 | 55.7 m / 2.47 s |
| Large kicker | 21.99 | 3.30 | 705.42 | 20.67 | 21.6 m / 1.07 s |
| Wide-rail pop | 21.01 | 2.09 | 789.00 | 20.72 | Snow exit / .35 s stable support |

Distances are longitudinal snow distances, not flight path lengths. The side-hit-to-box gap is shortest: about **12.3 m / .58 s to the bank**, **19.2 m / .90 s to capture**, in the same lane with roughly 5 cm landing offset. The alternate branch bypasses this side hit and offers a longer recovery. The large landing gives approximately **1.41 s before wide-rail capture** and requires only about 2.5 m of lateral correction. Human feedback should check whether these short recoveries feel rushed.

The final editor runs take **34.80 s from rest** and **26.47 s on the faster right branch**, ending grounded at z≈797.7/797.5 after three/two jumps and three rail/box captures. Both complete runs have zero bail/reset. The final camera review samples the whole route, including approach, large flight, rail entry, landing and high-speed carve. Player viewport remains near the center (x≈.49–.52, y≈.43–.55), with a level horizon. Visual sheets confirm open sightlines and visible landing terrain; no camera rewrite or retuning was needed.

## 9. Blender assets

Only **LineRailEntry** and **LineBoxEntry** are new: 8 m long, 4 m riding width, 1.2 / .8 m raised height, smooth packed-snow approach and an exit tangent to the existing grade. Each has 384 total exported triangles including its explicit collider; both use the existing snow material and static batching. These small features do not need additional LOD levels.

Editable `.blend` sources are in `ArtSource/Blender/`; FBXs/previews are in `Assets/Art/Generated/Jumps/`; stable generated prefabs and catalog entries use the existing importer. `build_all_assets.py` supports `line` and includes these assets in `park`/`all` regeneration. No production feature is a Unity primitive. `asset-retention.json` verifies all **63** old FBXs and manifest records unchanged; manifest validation passes **65** assets.

## 10. Human playtest

[HUMAN_PLAYTEST.md](HUMAN_PLAYTEST.md) contains the compact three-run sheet, eleven 1–5 ratings, all eight requested free-text questions, controls, branch instructions and one-value-at-a-time tuning guidance for the five existing configs. Ratings remain blank until a real player supplies them. Restart Park now begins at the standing roll-in; a downhill entry from the top gives more first-jump airtime.

## 11. Automated results and evidence

- Baseline: **36/36 EditMode**, **23/23 selected PlayMode**, **8/8 instrumented deterministic routes**, production telemetry capture and native park/rail run.
- Final physics/config: **48/48 EditMode**.
- Relevant broad suite: **27/27 PlayMode**, including full mountain traversal through all five regions, production jumps, full rotations, rails/grabs, camera, settings/save/outfits, snow caps/reset and contact/line review.
- Final layout/exit verification: **2/2 PlayMode** complete branch checks, requiring snow recovery and completion below z=800.

Receipts are in `Before/` and `After/`; final layout receipt is `FinalLine-results.xml`. The brief earlier line check's stale-event assertion was corrected and rerun. One Unity batch startup stalled during asset refresh and was restarted; that incomplete run is not a pass receipt.

`Before/Telemetry` and `After/Telemetry` contain fixed-step CSVs for straight/gentle/hard carving, rough ground, undulations, production jumps, old park/rail, flat/slope/seam/rollers/ripple and one/two contact. `After/Sequences` also contains the rendered contact lab and both uninterrupted authored runs. Encoded videos and comparison sheets are compact artifacts; raw PNG sequences remain ignored/local.

## 12. Native performance

Linux Mono Development player, 1920×1080, quality 2, vSync off, 144 FPS cap, RX 6600 XT / Ryzen 5 9600X. Sampling begins after a two-second warmup. Screenshot readback, editor, Blender and encoding were absent from profiling windows.

Same existing native park route: **143.98 → 143.98 FPS**, p95 **7.12 → 7.14 ms**, mean allocation **1,978 → 1,958 bytes/frame**. Native flat-rail capture/pop/landing succeeds; both ski axes retain dot≈1.000 and pop speed is **9.98 m/s**.

| Final native run | Time to stable snow exit | Jumps / landings / rail captures | Average FPS | p95 frame (ms) |
| --- | ---: | ---: | ---: | ---: |
| NativeAuthoredLine | 34.79 s | 3 / 6 / 3 | 143.93 | 7.15 |
| NativeAlternateLine | 26.48 s | 2 / 5 / 3 | 143.93 | 7.16 |


These capped measurements establish no material regression on this workstation, not an uncapped GPU benchmark or a long human session.

## 13. Remaining subjective weaknesses

Actual fun, controller comfort, rotation stopping and the shortest side-hit recovery still need human ratings. The diagnostic rider performs neutral jumps and predictable pops; it does not establish advanced-trick success for every entry speed. The right branch provides a simpler side-hit bypass. Existing procedural shoulder/hip looseness and wide landscape framing remain as in the reference pass; no speculative pose tuning was added. Small natural suspension travel on rollers/ripples remains, while smooth support and post-landing continuity are materially quieter.

## 14. Exact test / review commands

From `/home/haydend/Documents/ChatGPT/ski-in-unity`:

```bash
POWDERFLOW_CONTACT_OUTPUT=DevelopmentCaptures/ContactLinePass/After Tools/Build/test.sh EditMode
POWDERFLOW_CONTACT_OUTPUT=DevelopmentCaptures/ContactLinePass/After POWDERFLOW_REVIEW_OUTPUT=DevelopmentCaptures/ContactLinePass/After POWDERFLOW_REFERENCE_OUTPUT=DevelopmentCaptures/ContactLinePass/After Tools/Build/test.sh PlayMode "$(cat DevelopmentCaptures/ContactLinePass/After/test-filter.txt)"
POWDERFLOW_CONTACT_OUTPUT=DevelopmentCaptures/ContactLinePass/After Tools/Build/test.sh PlayMode 'PowderFlow.Tests.ContactLineReviewTests.MainAuthoredLineUsesRealPhysics;PowderFlow.Tests.ContactLineReviewTests.AlternateBoxBranchReconnects'
python3 Tools/Validation/review_contact_line_pass.py
python3 Tools/Blender/validate_assets.py
```

The line review deletes/replaces only its own raw sequence folder; archive an iteration first if needed. Human feedback remains separate from the automated review driver.

## 15. Exact Blender regeneration

Convenience command, through the existing pipeline:

```bash
Tools/Build/generate-assets.sh line
```

Equivalent individual commands used in this pass:

```bash
blender --background --python-exit-code 1 --python Tools/Blender/build_all_assets.py -- --stage line
python3 Tools/Blender/validate_assets.py
/home/haydend/Unity/Hub/Editor/6000.3.25f1/Editor/Unity -batchmode -nographics -quit -projectPath /home/haydend/Documents/ChatGPT/ski-in-unity -executeMethod PowderFlow.EditorTools.ImportGeneratedAssets -logFile /home/haydend/Documents/ChatGPT/ski-in-unity/Logs/import-line-assets.log
```

## 16. Exact build / package / run commands

```bash
Tools/Build/build-linux.sh
python3 Tools/Validation/capture_regions.py --output DevelopmentCaptures/ContactLinePass/After --only SunsetPark --rail --profile
python3 Tools/Validation/capture_contact_line.py
Tools/Build/package.sh
Tools/Build/run.sh
```

The standalone complete-line commands used by the native wrapper are:

```bash
Tools/Build/run.sh --smoke-test --smoke-line --profile -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -logFile "$PWD/Logs/NativeAuthoredLine.log"
Tools/Build/run.sh --smoke-test --smoke-line --line-branch --smoke-speed=22 --profile -screen-width 1920 -screen-height 1080 -screen-fullscreen 0 -logFile "$PWD/Logs/NativeAlternateLine.log"
```

The native wrapper applies a 60-second timeout, validates exit/launches/landings/branch capture, and copies JSON/CSV/PNG receipts to `After/`. The playable folder is `Builds/Linux/`; the self-contained archive is `Builds/Packages/PowderFlow-Linux.tar.gz`. Keep the executable, UnityPlayer.so and data folder together. Human launch opens normal player controls and the menu. The final archive was also extracted into an isolated temporary directory and launched successfully: **79.53 m descent, exit 0**, with the human playtest sheet included. `After/package-run.json` records its size/SHA-256 and launch result.
