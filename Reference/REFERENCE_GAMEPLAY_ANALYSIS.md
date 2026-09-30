# Detailed skiing reference review

Reviewed 2026-09-30. Source `ski_reference.mp4` is byte-identical to the user's Downloads MP4. ffprobe reports **64.898367 s, 720×1280, 30 fps**. The clip is an edited montage with changing kits, lighting and locations; shots cannot be treated as one uninterrupted trajectory. The original video is unmodified and no source assets/audio/branding are reused.

## Sampling and limits

`Analysis/full-video-contact.jpg` covers all 65 regular one-second samples (0.5–64.5 s). Existing four contact pages retain larger samples. Six-fps sequences cover takeoff/cork (0–4 s), landing (24–27 s), grab/release (31–34 s), carve (38–41 s), and rail (57–62 s). Additional twelve-fps consecutive frames cover 0–4, 31–34, and 59.5–62 s. The earlier `REFERENCE_ANALYSIS.md` retains the full timeline.

Durations and normalized image positions can be observed directly. Physical speed, force, FOV, camera meters, actual input and hidden landing assistance cannot be recovered uniquely from this video. Ranges below are visual estimates or inferences, explicitly marked. They guide comparative tuning rather than claim the source game's internal settings.

## Camera

The camera is behind and slightly above the skier, commonly near shoulder/head height above the snow. Estimated distance is a few body heights (roughly 3–6 m if the character is human scale); the exact FOV is not recoverable, though a moderate/wide lens is consistent with the views. Skier plus equipment occupies approximately **18–25% of portrait image height** on representative ground approaches; compact or distant airborne poses are smaller, and close grabs become larger. A horizon/ridge line usually occupies the upper third to half, with the next lip/landing visible in the lower/middle field.

The camera lags behind quick body actions and follows travel/trajectory. A full spin or inversion does not roll the horizon with the body. Air framing allows more sky near ascent, then anticipates the landing wall below; touchdown has a brief disturbance without sustained camera roll. Speed pullback appears modest relative to character size. Damping is visible but cannot be separated numerically from edited shot motion. Desktop landscape should preserve this behavior and improve rider legibility without copying the aspect ratio.

## Skiing

Downhill travel accelerates and retains substantial momentum between park features. There is no reliable speed HUD/calibrated world distance in these samples, so numerical acceleration/speed is unavailable. Turns around 38–41 s bend the travel path smoothly: skis edge, hips/knees commit inward and the torso counterbalances. More spray appears under load, especially near the turn exit and landing. Some sideways ski posture and speed scrub are visible, but no isolated controlled braking trial establishes stopping distance. The broad ingredients suggest responsive, forgiving control with inertia; they do not imply physically exact ski friction.

## Jumping

At 0–1 s and approximately 57.5–58.1 s the rider compresses on approach then extends rapidly at the lip. The extension occupies a few video frames (roughly .1–.25 s estimate), after which the legs fold again. Incoming slope/lip direction drives launch; the skier continues forward through the aerial instead of pausing vertically. Separate shots include approximately 1–3 seconds of visible airborne action, with longer/incomplete aerials cut by edits. Exact launch angle depends on camera projection and cannot be measured as a world angle.

## Air tricks

Large spins and flips progress continuously; off-axis tricks combine yaw with a tilted/inverted rotation axis. At 1–3, 23–25, 31–33 and 59.8–61.5 s, skis and poles sweep through clearly different orientations while the body stays compact. Rotation is visibly substantial during one aerial, plausibly several radians per second, but editing and perspective prevent a single angular-speed target. Knees and arms change the silhouette while rotating. Late extension/arm opening often accompanies slower apparent rotation and a spotted landing; input release or assistance is an inference, not directly observable.

## Body pose

Ground posture keeps persistent knee flexion, hips slightly behind the boots and torso leaning forward. A loaded turn compresses the inside leg more than the outside; the upper body does not rigidly follow the root lean. Arms remain available for balance and head direction generally faces the route. Large tricks bring the knees much closer to the torso, with independent ski separation/crossing. The rider is loose through shoulders/hips, not only a rotating static crouch.

## Grabs

The 31–34 s sequence shows body compression during rotation, a wrist near the ski, distinct equipment movement and subsequent release. Torso and knees bring ski/hand together. The opposite arm extends or counterbalances; both arms should not look identical to a non-grab tuck. At roughly 33.0–33.5 s, hand release, arm opening and foot extension precede touchdown. Occlusion prevents exact wrist contact distances or identification of every grab type. Smooth whole-body transitions are the reproducible property.

## Landings

At 25–26 and 61.1–61.9 s, feet extend below the body before contact, skis approach the terrain plane, and knees compress on impact. Larger drops produce a deeper and longer recovery than minor hops. Some sideways/wobbly contacts recover into travel, while the closing shot is a large powder tumble. Late orientation corrections suggest forgiveness, but the amount of hidden assistance cannot be inferred. Desired behavior is a reasonable recovery envelope and gradual damping, with badly inverted/misaligned attempts still failing.

## Rails

The 57–62 s sequence includes approach, pop/extension near 57.8 s, sideways rail contact near 58.1 s, balanced slide, rotation off around 60.1 s, then compact air and landing around 61.6 s. Arms widen and knees stay flexed during slide. Forward momentum continues on exit. No obvious centerline teleport is visible; approach tolerance cannot be quantified because only successful examples are shown. Rails appear slim with supports rather than oversized solid blocks. Spin-on/spin-off combines physical rotation and a stable slide posture.

## Environment

The reference presents a broad park surrounded by relatively close, irregular dark rock ridges with snow ledges/gullies and layered distant silhouettes. Park lips, rails and landing banks form sequential opportunities. Evergreen clusters vary in height/silhouette and are denser near boundaries. Terrain remains open around feature approaches. Jump scale varies, including substantial expert lips; ski/equipment scale remains readable. Exact maps, tree positions or feature dimensions are not targets for replication.

## Graphics

Warm low-angle sun, pink horizon/clouds, cool violet/blue snow shadows and atmospheric fading separate terrain layers. Snow is tinted and textured rather than flat white; nearby surface detail fades into broad shapes at distance. Dark rocky faces alternate with white snow pockets, making the ridges more complex than uniform pyramids. Lighting differs across shots, so one frame's pink/orange cast should not become a universal grading target. Silhouette clarity and warm/cool separation matter more than saturation matching.

## Effects and audio

Paired ski grooves follow contact and disappear during flight. Carving/landing/bail spray changes with energy: a fine trailing fan on snow, larger contact bursts and powder clouds at failed impact. Loaded turns throw spray away from the engaged edge. There is no persistent spray floating under the airborne body. A few samples suggest speed/wind presentation, but edited soundtrack/foley does not provide a reliable physical-audio mix target. Keep original procedural audio and tune it from actual speed/slip/load; source music/audio must not be reused.

## Active comparison targets

Prioritize measured turn response and usable rotation stopping, then screen size and ground/air body shape. Preserve valid physical lip trajectories and existing IK contact. Strengthen load-scaled snow and impact presentation without obscuring the skier. Replace only visibly weak distant range output through the existing Blender generator, retaining skiable terrain and functional trees/rails. Compare the exact same current-game scenarios before/after, alongside reference samples, and state where evidence is staged, estimated or incomplete.
