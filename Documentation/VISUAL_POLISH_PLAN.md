# PowderFlow visual polish plan

## Direction

Keep the original stylized alpine identity: clean silhouettes, readable terrain, cool snow, warm sunset highlights, violet shadows, and a saturated blue daytime sky. The skier, skis and next landing must remain easy to read at speed. All new geometry, textures and effects continue to be generated locally with Blender/Python or project shaders.

The existing master plan supplies the art direction. This document turns it into staged implementation work with review shots, dependencies and completion criteria.

## Baseline review — 2026-09-29

Reviewed the delivered Day, Sunset, portrait, gameplay and main-menu captures and the Unity/Blender source.

| Finding | Impact | Priority |
|---|---|---|
| Saved AlpinePost profile contains no components | Tone mapping/bloom/vignette were configured in memory but did not survive saving | Immediate |
| Renderer has no post-process resources assigned | Post-processing cannot run even with a populated Volume | Immediate |
| URP soft shadows disabled | Hard shadow edges and weak ground contact | Immediate |
| Build scenes serialize fog off; fog is enabled only at runtime | Build can strip required fog variants; distant geometry appears much darker than in editor shots | Immediate |
| Broad snow areas have little visible surface variation | Day snow clips bright; terrain shape is hard to read | High |
| Importer snow matching ignores Blender numeric material suffixes | Most generated terrain retained plain Lit materials instead of the intended snow shader | Immediate |
| Skier torso/limbs read as separate primitive shapes | Clothing, posture and character silhouette need an art pass | High |
| Pines form repeated cone bands; ridge silhouettes repeat | Environment looks procedural and lacks natural scale cues | High |
| Jump sides, lift details and signage are sparse | Features lack finish and mountain identity | Medium |
| Tracks/spray and air posing need dedicated review shots | Existing wide screenshots do not prove motion quality | Medium |
| Menus use the default Unity button skin | Functional but visually disconnected from the game | Medium |

Before images are preserved in `Documentation/VisualPolish/Before/`. New review images and validation will be stored by phase. Earlier uncapped measurements remain historical evidence. As requested on 2026-09-29, all runs now use a 144 FPS cap and routine performance benchmarks are removed.

## Sequence

### VP1 — Rendering foundation, snow and atmosphere

**First implementation pass.**

1. Persist Volume components as Unity subassets. Configure ACES, restrained bloom/vignette and mild grading. Verify persistence in a fresh editor process.
2. Enable URP soft shadows and depth, tune shadow bias/contact and SSAO without dark halos.
3. Preserve exponential fog in player shader variants and serialize representative fog into build scenes. Match the editor and packaged build.
4. Balance Day/Sunset light, ambient and fog colors. Day snow should retain detail; sunset should have neutral/cool snow with warm highlights and violet shade.
5. Improve snow with two wind-ripple scales, restrained albedo variation, distance-faded glitter and more varied rock shading. Keep detail stable at speed and across chunk seams.
6. Improve procedural cloud shapes, sky gradient, sun disk and halo. Reduce broad featureless cloud stripes.
7. Capture matched wide views, park detail and character framing in both presets. Rebuild and inspect the visible native player.

**Primary files:** GraphicsConfig, AlpineLighting, GraphicsTools, AlpineSnow/AlpineSky shaders, graphics materials/settings, build scene render settings, graphics validation/capture tooling.

**Done when:** post components survive reload; fog/soft shadows appear in the native player; no pink/error materials or shader errors; snow has visible shape/detail without noisy sparkle; nearby obstacles retain contrast; Day/Sunset/portrait captures are inspected; the capped native launch and visual inspection pass.

### VP2 — Skier, clothing, skis and pose readability

1. Replace the oval jacket/hip silhouette with a tapered padded torso, shoulder transition, collar/hood, cuffs and a cleaner jacket hem. Keep the rig naming and left/right convention.
2. Give pants a continuous baggy shape around knees; reduce visible detached limb joints through geometry/weights and pose limits.
3. Add a readable goggle frame/strap, helmet panel accents, glove cuffs, boot buckles and binding detail. Add original ski graphics with clear tip/tail differentiation.
4. Assign distinct fabric, shell, rubber, metal and lens material responses. Add restrained rim lighting so the skier reads against snow and sky.
5. Refine neutral, tuck, carve, airborne, compression and rail poses. Review all eight grabs and crossed skis from side/front/rear; hands should reach their targets and poles should trail without intersecting the torso.
6. Improve coordinated outfit palettes and garment contrast. Preserve current preset selection and make the model preview usable for review.

**Primary files:** create_skier.py, Blender pipeline/material helpers, GeneratedAssetImporter, SkierPose, OutfitSystem, character config/materials, source blend/FBX and pose captures.

**Done when:** a coherent clothing silhouette is visible at gameplay distance; the skis are distinct in air; close views have no major gaps/intersections; tuck feet stay on snow; left/right and crossed-ski checks pass; generated character remains within a sensible 10–15k triangle budget unless a measured benefit justifies more.

### VP3 — Mountain and vegetation composition

1. Replace uniform pine cone stacks with irregular branch whorls, layered snow masses, varied silhouette and seeded asymmetry. Keep three LODs and add a cheap distant representation if profiling warrants it.
2. Add small shrubs and clustered rock/snow accents outside collision-critical approaches.
3. Shape distant ridges with several nonrepeating peak profiles, distinct foreground/midground/background layers and snow-catching ledges. Remove the flat wall-like horizon/base transition.
4. Give each mountain region recognizable composition: broad easy start, dense park edges, open big-air vista, freeride tree gaps and a calmer lower run.
5. Tune vegetation scale/spacing and LOD transitions in moving footage. Avoid tree popping or clutter on landing approaches.

**Primary files:** create_environment.py and generator helpers, WorldConfig/MountainWorld, environment material/import settings, generated source/FBX and wide captures.

**Done when:** regions are recognizable in wide shots; silhouettes vary; distant geometry layers into the sky; no new riding seams/obstructions; a top-to-bottom physics run still passes; LOD counts remain bounded and moving footage has no disruptive transitions.

### VP4 — Park features and mountain props

1. Finish rails/boxes with end caps, bevels, support joins, restrained wear and coherent trim colors. Make narrow/wide/kink shapes readable before entry.
2. Improve jump side profiles, snow berms and approach/landing transitions visually while preserving validated riding surfaces.
3. Detail the hut roof/windows, lift towers/wheels/cable spans and floodlight housings. Add chair/gate details where they improve scale and place identity.
4. Add original region signs, feature difficulty marks, boundary markers and flags. Use a restrained shared graphic language.

**Primary files:** environment generators, importer/materials, WorldConfig/world placement and park detail captures.

**Done when:** feature entry/direction is readable at speed; supports appear connected; props have correct scale; collisions and rail paths agree with visible geometry; no new obstacles interrupt designed lines.

### VP5 — Snow interaction and motion effects

1. Review separate ski grooves during carving, straight descent and switch riding. Improve groove edge/softness, powder depth, fade and placement without z-fighting.
2. Give snow spray a softer layered shape with speed/slip-dependent direction. Separate carve spray, brake cloud, landing burst, rail shavings and crash flakes.
3. Keep ground spray off during flight and avoid opaque clouds hiding landings. Pool effects and preserve particle caps.
4. Add a subtle contact/landing accent and verify effects against both lighting presets.

**Primary files:** SnowTrackSystem, SnowEffectsController, SkiGrooves shader, procedural particle textures, GraphicsConfig and action captures.

**Done when:** grooves clearly follow both skis and survive retries without teleport streaks; spray conveys motion/material; transitions are clean; no large particle spikes or camera-obscuring clouds.

### VP6 — Camera, HUD and menu presentation

1. Tune framing for cruise, low-speed carve, rail entry, big air and bails in landscape and portrait. Keep the horizon stable and show the landing zone.
2. Replace default menu styling with a consistent type scale, spacing, slate panels, warm/cool accents and clear controller focus. Use the mountain as a quieter backdrop.
3. Refine speed/score/trick labels, landing-tier colors, popup fade and safe areas. Keep riding HUD sparse.
4. Improve outfit preview/settings grouping and ensure every control remains usable with gamepad, mouse and keyboard at all supported resolutions.

**Primary files:** CameraConfig/SkiCameraController, RideHUD, GameFlow, UI style assets/config and menu/portrait captures.

**Done when:** skier/landing stay in frame during representative tricks; no menu overflow at 1280×720, 1920×1080 or 1080×1920; controller focus is obvious; overlays remain readable without covering features.

### VP7 — Final consistency and packaged review

1. Compare matched before/after images and inspect short captured action sequences: carve, brake, jump/spin/grab, rail exit, landing, bail and reset.
2. Review both presets, all regions, several outfits and landscape/portrait. Fix material, scale, LOD, shadow and fog inconsistencies.
3. Run meaningful regression gates: asset/rig/terrain checks, physics/trick/rail checks when affected, menu/save checks when affected, clean build and extracted-package launch.
4. Launch a representative native run at the 144 FPS cap and inspect gameplay/action captures. Run a performance profile only when a concrete issue needs investigation or the user asks; machine FPS is not a routine completion gate.
5. Update README/report, capture evidence, package the polished build and commit the phase.

**Done when:** the complete visual pass is coherent and inspected, relevant regressions pass, the capped native run succeeds, the extracted archive runs, and remaining limitations are stated explicitly.

## Review and implementation rules

- Work in order, complete and commit a phase before the next. Update this document with actual results and open issues.
- Use fixed camera positions, preset, FOV, resolution and exposure for comparisons. Include gameplay-distance shots as well as close-ups.
- Tune render/pose/world values through config assets or generator parameters. Regeneration must retain art assignments and post settings.
- Run targeted checks for the systems changed. A full rebuild is required before reporting player-only fog/shader behavior as fixed.
- Inspect generated images directly; tests alone cannot certify art quality. Keep before images and record capture conditions. Avoid repeated benchmarks or region launch matrices when existing evidence already covers the changed system.
- Do not change gravity, carving, air or rail tuning during a visual phase unless a visible defect proves that change is necessary; document any such change and rerun its tests.
- No reference video is present. Follow the written original art direction; do not claim literal visual matching or subjective approval.

## Status

- Baseline review: complete.
- VP1: complete. Persistent post/fog/shadows and terrain snow assignments repaired; Day/Sunset/cloud/snow refinement inspected in editor and native captures. 23 EditMode + 12 PlayMode checks passed; final render capture check repeated. Short visible 1080p native averages: Sunset 652.48 FPS / Day 737.68 FPS. Evidence and limits: `VisualPolish/Phase1/REPORT.md`.
- VP2: complete. Continuous clothing, detailed equipment, five skinned renderers, 10,846 triangles, coordinated palettes, character shader/config and improved grab/pose handling. 24 EditMode + 13 PlayMode checks passed; final character review repeated. Native short 1080p averages: Sunset 639.73 FPS / Day 675.11 FPS. Evidence: `VisualPolish/Phase2/REPORT.md`.
- VP3: complete. 25 EditMode checks and three targeted PlayMode checks pass; final scenery/graphics reviews pass, riding-terrain hashes match and the connected descent reaches 1,405 m through five regions. The 144 FPS native launch and extracted package pass. Asymmetric pines with crossfaded LODs, rock/shrub clusters, protected regional composition and layered noncolliding ranges. Evidence: `VisualPolish/Phase3/REPORT.md`.
- VP4: complete. Rails/boxes have joined supports, closed panels and entry trim; jump snow banks preserve all six riding surfaces. Detailed lodge/lift/lights, 14 towers/26 connected spans/52 static chairs, original region boards, entry grades, gate and boundary markers are reviewed in both presets. Targeted EditMode 3/3, PlayMode 2/2 and final park review 1/1 pass; the descent reaches 1,382 m through five regions. Twenty protected terrain/path files match. The capped native run and extracted package pass. Evidence: `VisualPolish/Phase4/REPORT.md`.
- VP5–VP7: planned. Next: snow interaction and motion effects.
