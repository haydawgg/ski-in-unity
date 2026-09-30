# VP4 — Park features and mountain props

Completed 2026-09-29. Park hardware, snow banks and mountain props now share a slate/teal palette with cream lettering and orange entry accents. Runs retain the **144 FPS cap**; this phase used targeted collision, placement and visual checks without performance benchmarks.

**Historical VP4 package record:** this phase's archive is preserved locally as `Builds/Packages/PowderFlow-VP4-Linux.tar.gz`. The usual `PowderFlow-Linux.tar.gz` path now contains VP5; its current checksum is recorded in `../Phase5/REPORT.md`.

## What changed

- All five rails now have closed tube ends, supports joined to their actual profiles, saddles, braces, footplates and anchor bolts. Entry sleeves and restrained wear distinguish the riding tube from its supports.
- All five boxes have closed end/underside panels, beveled edge bars, lower trim and footplates. Orange bands sit on the entry face rather than inside the shell.
- All six jump assets have flared visual snow banks and a packed approach apron. Their existing riding-top and vertical-side collision triangles are preserved.
- Ridge House has a gabled snow roof, closed gable walls, fascia, framed windows, mullions, snowy sills, door/handle, chimney and original nameplate. Lift towers have sheaves, braces and ladders; floodlights have housings, warm lenses, hoods and guards. Fence crossrails follow the nominal slope.
- The lift has **14 towers, 26 connected cable spans and 52 static chairs**. Cables follow the actual tower elevations with a small sag. The chairs provide scale and are scenery rather than operating transport.
- Five original region boards, three grades of feature-entry panel, outer boundary poles, flags and a start gate add wayfinding. The gate sits ahead of the spawn. Region signs use green circles, blue squares or orange diamonds, a descending chevron and cream mesh lettering.
- The Alpine Prop shader retains readable shaded pigment and letters while supporting fog, shadows and screen-space ambient occlusion. Original sign text uses installed DejaVu Sans Bold outlines; the font notice is included in the portable package.

The focused `park` generator updates **34 assets**; the complete manifest has **63**. Decorative hardware is consolidated into one visual mesh per asset. WorldConfig exposes lift count/spacing/offset, region-sign offset and outer-marker placement. Regeneration reapplies the materials and explicit-collision rules.

## Riding surfaces and collision

The ten mountain FBXs and ten rail/box path JSON files match their saved hashes. For all six jumps, imported top and side triangle fingerprints match the pre-VP4 baseline at **0.1 mm coordinate precision**. Rail tube radii and paths are unchanged; box tops retain their original geometry.

The importer hides `Collision_` renderers and creates colliders only on those meshes when present. Rails use tube collision; boxes use their original riding top/side strips; jumps use their original top and sides. Lodge, towers, floodlights, fences and region boards have simplified solid collision. Decorative banks, hardware, chairs, cables, flags, gate and feature/boundary markers add none. The visual banks therefore do not extend the playable jump width.

Ski-force, air, rail and feature-location tuning are unchanged. The descent and sampled approach checks cover the designed lines; they do not certify every possible off-line route or subjective skiing feel.

## Inspected visual evidence

`Before/` preserves VP3 park/vista views and its capped gameplay image. The review fixture captures **18 views at 1920×1080**: park detail, vista, rail, box, jump, lodge, lift, sign and gate in Day and Sunset. Camera positions and FOV are recorded in `ParkPolishTests.cs`; the matched park/vista views retain the earlier fixture conditions.

Before:

![VP3 Day park](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase4/Before/DayParkDetail.png)

After:

![VP4 Day park](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase4/DayParkDetail.png)

![Connected rail supports](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase4/DayRail.png)

![Region sign and direction chevron](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase4/DaySign.png)

![Detailed lodge at sunset](/home/haydend/Documents/ChatGPT/ski-in-unity/Documentation/VisualPolish/Phase4/SunsetHut.png)

[Box entry](DayBox.png), [jump banks](DayJump.png), [lift spans/chairs](SunsetLift.png), [start gate](SunsetGate.png) and [native Park gameplay](SunsetParkPlayer.png) are saved beside this report. Final inspection repaired an open lodge gable, underexposed lettering/wood, a one-sided chevron and hidden box entry bands. `BeforeMaterialFix/` retains two intermediate views for provenance.

## Validation

| Check | Result |
|---|---|
| Asset manifest | 63 entries valid |
| Targeted EditMode | 3/3 pass: terrain/rig/scenery constraints, explicit collision, jump preservation and rail/box contact alignment |
| Targeted PlayMode | 2/2 pass: connected descent and park placement/review |
| Shader asset check | 1/1 pass after adding the prop shader |
| Final park review | 1/1 pass; final Day/Sunset captures, lettering assignments and shader compilation |
| Connected descent | Reached 1,382 m through all five regions |
| Protected files | 20/20 hashes identical |
| Jump collision fingerprints | 12/12 top/side records match |
| Lift/markers | Span endpoints within 1 cm of tower sheave tops; markers noncolliding; sampled central approaches clear |
| Native gameplay | Sunset Park, 1920×1080, High, cap 144, VSync 0; 92.33 m travel; exit 0 |
| Linux build | BuildPipeline succeeds; 191,206,045 bytes reported |
| Extracted package | Its own Play.sh saves a fresh 1920×1080 menu capture; exit 0 |

The native gameplay and extracted-package logs contain no runtime exceptions or shader errors. Native gameplay was checked before the final visual gable panels were added; the final import/capture/build/package checks include that correction. The cap is a runtime setting, not a measurement of machine FPS. XML, `native-runs.json`, `package-validation.json` and `summary.json` retain the evidence.

## Build and reproduction

Play with `Tools/Build/run.sh`. The latest distributable is `Builds/Packages/PowderFlow-Linux.tar.gz`: **71,128,613 bytes**. Its SHA-256 is **80ed6eac2ff0910c9c0ae98c7e6f4e4c82fbfdccd34010dd400cef170a98bda8**, also recorded in `package.sha256` and `summary.json`. The earlier VP3 archive is preserved locally as `PowderFlow-VP3-Linux.tar.gz`. The package includes `ThirdPartyLicenses/DejaVu.txt` and excludes smoke diagnostic output.

For park edits, close the interactive Unity editor, run `Tools/Build/generate-assets.sh park`, then use relevant targeted checks. `Tools/Build/polish-environment.sh` reapplies the material response. The review test is `Tools/Build/test.sh PlayMode PowderFlow.Tests.ParkPolishTests`; build/package commands are `Tools/Build/build-linux.sh` and `Tools/Build/package.sh`.

One selected native visual check was run with `python Tools/Validation/capture_regions.py --output Documentation/VisualPolish/Phase4 --only SunsetPark`. It validates launch, travel, resolution and the cap without collecting frame times. No regional launch matrix or performance profile was used.

## Remaining work

The existing camera/jump overlap still appears as a strip at the top of the native Park image and remains assigned to **VP6 camera obstruction review**. Lift chairs and flags are static. Snow banks are visual dressing around the preserved collision boundary. Dynamic action sequences remain part of VP7's consistency review.

Next: **VP5 — snow interaction and motion effects**: per-ski grooves, carve/brake spray, landing bursts, rail shavings and clean transitions during flight/reset.
