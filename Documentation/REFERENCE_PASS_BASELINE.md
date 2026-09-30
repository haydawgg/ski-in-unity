# Baseline for the second reference improvement pass

Captured 2026-09-30 from the completed gameplay/animation pass. This baseline includes its existing uncommitted implementation; `ec91d35` is the earlier repository commit, **not** the current pass baseline. `DevelopmentCaptures/BeforeReferencePass/SourceSnapshot/` and `baseline-source.json` preserve the actual starting configs and affected runtime/generator sources. Earlier `Baseline/` and `Current/` evidence remains separate.

## Current architecture review

Reviewed Unity 6000.3.25f1, URP 17.3, Input System 1.14.2, Generic twenty-one-bone rig, runtime capsule/Rigidbody factory, independent ski probes, anisotropic force/grip/sidecut, buffered pop, angular momentum air control, predictive landing preparation/spotting, smooth grab IK, sampled path rails, custom upright camera, snow/grooves/audio, Blender generators/importer, settings/save, tests and build scripts. The detailed incoming audit and prior completed changes are in `CURRENT_PROJECT_AUDIT.md` and `GAMEPLAY_QUALITY_REPORT.md`.

No architecture replacement is justified. Current production assets already include generated skier/equipment, explicit ramp collision, bevelled rail/box hardware, four evergreen variants with three LODs, rocks/shrubs and five distant range layers. The unresolved visual weakness is the distant range output, not missing Blender content. Cinemachine and Animation Rigging packages remain unused; existing custom follow and IK are the working systems.

Current tuning: sidecut 14 m with speed widening, edge response 5, edge grip 10/max 18 m/s²; yaw/pitch/roll torque 60/48/38 with inertia 18; spotting .28 s and damping 3 inside the existing assistance envelope. Camera distance 5.8 m, ground/air height 1.8/1.5 m, speed/air pullback 1.7/.8 m, FOV 73–82°. Body motion already responds to force, rotation, pop and impact; grabs already reach real ski targets. Root motion is off.

## Fresh runs before gameplay changes

Fresh **34/34 EditMode checks passed**. The current native development player passed rail capture/pop/landing and the matching 12-second Park descent: **140.96 m**, maximum **22.01 m/s**, 1920×1080, quality 2, vSync off, configured cap 144 FPS, exit 0. Receipts are in `BeforeReferencePass/`.

New `ReferencePassReviewTests` records matched states/sequences for straight, both carve signs, hard skid, actual small/large lips and landings, full 360/720/flip/off-axis rotations, grab, sketchy landing, rail entry/slide/exit, a continuous park jump run, and Day/Sunset mountain views. Known initial angular momentum is used only for reproducible complete rotation capture; separate input-response measurements exercise the current controls. Each named sequence is continuous, with explicit relocation between scenarios. Fresh **13/13 PlayMode checks passed**, including the existing regression/action/camera flows and new capture/input-response fixtures; their XML and metrics accompany captures.

Runtime instrumentation and review fixture code are measurement tools, not changes to skiing behavior. Performance measurements use a native development build without screenshot readback during the sample window; CPU/GPU identity, sample count and frame-time statistics are recorded with the configuration. A frame-rate cap alone is not a performance result.

## Baseline observations and evidence limits

Prior captures and current runtime review show a stable camera and working physical lips, grabs and rails. The rider remains small in the landscape follow view and becomes less legible during compact air. The thirteen-degree ground slope plus camera lag adds to the nominal camera distance. Broad sidecut and delayed edge response limit tighter park transitions. Released rotation remains energetic until a short near-contact window. Distant ridges are large uniform facets with sparse snow detail; loaded edge spray is light compared with the reference.

The reference is portrait and edited. Exact source friction, speed, FOV and camera meters cannot be recovered. Compare normalized rider screen height, force/trajectory measurements and visible temporal phases, rather than inventing source-game settings. The written gap analysis in `REFERENCE_GAP_ANALYSIS.md` determines the implementation order.

Native optional profile before gameplay tuning: RX 6600 XT / Ryzen 5 9600X, 1920×1080 quality 2, 1,728 samples over 12 seconds after warmup, **143.99 FPS average**, mean **6.945 ms**, p95 **7.076 ms**, p99 **7.173 ms**, measured managed allocation mean **1,963 bytes/frame**. Main-thread time includes frame-cap waiting and is not pure CPU work time. No screenshot readback occurred during sampling. See `BeforeReferencePass/SunsetPark-performance.json`.
