# Supplied skiing reference analysis

Source: `ski_reference.mp4`, copied from the user's Downloads path on 2026-09-30. ffprobe: 720×1280 portrait, 30 fps, 64.898367 seconds. This is an edited montage, not one continuous physical run. Measurements below are screen estimates, not recovered game parameters. Reviewed all 65 one-second samples across the entire clip and six-fps sequences at 0–4, 24–27, 31–34, 38–41 and 57–62 s. Contact sheets and individual frames are in Analysis/. No original game assets, designs, logos or animations are reused.

## Timeline covering the full clip

| Time | Motion/visual evidence |
| --- | --- |
| 0–4 | Straight approach, quick lip extension into an off-axis inverted trick. Knees move toward torso, skis separate/cross, free arm extends for balance. Camera remains upright behind travel. |
| 4–8 | Powder impact/tumble, then gliding with bent knees. Strong bursts at contact, persistent paired grooves; montage title overlay. |
| 8–12 | Compact aerial rotation against sky, poles independent of the ski pair, deep compression on a steep landing wall. |
| 12–17 | Rail vicinity/sideways ski posture, recovery into upright glide, takeoff and ski crossing. Camera shows terrain ahead. |
| 17–22 | Straight powder travel, close crossed-ski grab, large lip takeoff and compact airborne pose. Camera height drops relative to the rising skier. |
| 22–27 | Off-axis aerial sequence: folded knees and hand near ski, skis sweep around body, then feet extend underneath before touchdown, crouch/compression and glide recovery. |
| 27–34 | Sideways compact aerials, crouch/pop across successive park lips, compact twist then arms open and legs extend for contact. |
| 34–41 | Long aerial/grab/cork sequences; skier size varies with altitude. Deep landing compression followed by carve, inward hip shift and outside-arm counterbalance. |
| 41–47 | Rail/park travel and consecutive lips. Carve transitions stay continuous; skis remain separately readable in air. |
| 47–52 | Off-axis trick and landing on a wall; slow-motion-like camera travel, short posture extension before contact. |
| 52–57 | Aerial tuck/grab, landing recovery and straight skiing. Body remains active even outside named tricks. |
| 57–62 | Approach → extension around 57.8 s → sideways rail contact around 58.1 s → balanced slide with arms out → rotate off around 60.1 s → compact aerial → feet below body around 61.3 s → landing around 61.6 s. |
| 62–64.9 | Powder bail and sliding with large snow bursts; horizon still readable. |

## Body mechanics and actionable targets

Neutral: feet roughly hip-width, knees flexed, hip slightly behind boots with torso pitched forward. Arms are held away from the chest for balance; poles trail. Ground skier occupies roughly 18–25% of portrait height in representative approach frames (screen estimate including skis/poles). The original project is desktop landscape; retain its framing instead of adopting the clip's aspect ratio.

Carving (38–41 s): hips and knees shift into the turn; torso counterbalances rather than rotating rigidly with the whole root. Inside knee compresses more than outside knee. Input direction alone does not convey the actual load: lateral acceleration and grip should influence the pose.

Crouch/takeoff (0–1, 57.5–58.1 s): a brief compression precedes rapid extension. Knees begin folding again once airborne. Launch height still comes from velocity and lip geometry, not an animation curve.

Aerial/cork (1–3, 23–25, 31–33 s): knee/hip flexion compacts the silhouette. Skis rotate with the physical body while independently twisting/separating. Arms draw inward during fast rotation and open again to spot contact. Poles lag the hands. Cork is an off-axis physical rotation plus compact pose, not a separate canned trajectory.

Grab (31–34, 35–37 s): torso/hips bring the ski toward the wrist; moving only the hand would not create this silhouette. Smooth entry/release matters. Preserve existing reachable authored leg arrangements; blend whole-body and arm IK weight. Make nose/mid/tail attachment points explicit on both ski bones.

Landing (25.25–26.25, 61.1–61.9 s): extend legs and arms before first contact, compress rapidly on contact, then recover more slowly. Larger impact has visibly deeper flexion. Slightly sideways landings permit torso/arm correction before settling; hard inverted impacts bail.

Rails (57–62 s): sideways skis straddle the tube, knees remain flexed, arms wide. Rail entry approaches continuously without snapping from the capture envelope. Slide speed persists and exit rotation carries into air. Retain the existing center-path system and improve its approach offset blend.

Camera: follows travel/trajectory, upright horizon during flips, low rear viewpoint. Looks toward upcoming slope and follows high jumps with lag. Existing custom camera already follows these principles; new posing must remain legible at its current distances.

Environment/VFX: close dark rocky ridges with snow streaks, varied evergreen clusters, broad smooth park transitions, small black tubes with supports; warm pink-lit snow and violet shadows, sky/sun often in frame. Fine edge spray changes to strong bursts at impacts. Existing production terrain/park/scenery/VFX already implement the same broad ingredients, though distant ridges remain more geometric and the layout is more open.

## Comparison method

DevelopmentCaptures/Baseline contains the incoming native rail/pop/landing and descent plus actual injected action captures. DevelopmentCaptures/Current uses the same scenario timing after changes. BeforeCameraComparison.jpg retains the intermediate comparison with incoming camera tuning; the final comparison also includes the documented closer framing. Deterministic rotation tests supplement captures because the montage fixture is not a full uninterrupted trick line. Compare crouch/pop, both carve signs, compact fast rotation, grab reach, precontact extension, impact/recovery, rail capture offset, and camera roll. Pixel/art equivalence to the reference is not claimed.
