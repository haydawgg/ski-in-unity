# PowderFlow human playtest — contact and flow line

Build: the ContactLinePass Linux development player. Allow 15–20 minutes. This sheet is deliberately blank until a real player completes it; automated runs are not feel ratings.

Launch from the project with `Tools/Build/run.sh`, or run `./Play.sh` inside the extracted package’s `Linux` folder. Select **FREE RIDE**. Pause → **RESTART PARK** starts the longer roll-in at z=250. Hold W / gamepad Y to build speed. A/D / left stick carve; hold and release Space / A to pop; Q/E / triggers grab; S / X brakes. T sets a snow marker; Y / D-pad Down retries it. R / B retries the latest safe position. F1 shows contact telemetry; F2 toggles editor contact gizmos; F3 cycles slow motion.

## Three short runs

1. **Contact:** ride straight, make a gentle turn each way, make a loaded turn, then brake. Repeat on the small undulations beside the main park. Observe visible ski/body chatter and any unexpected air/ground changes. Compare normal play speed first; use F3 only for diagnosis.
2. **Jump and rail:** try the first medium kicker with neutral input, then a small spin and early release. After landing, take **left down rail** or **right wide box**. Enter the packed snow bank; steer gently to center. Pop near the end if desired. Try the opposite branch on retry. Ride the large kicker without a trick before adding rotation.
3. **Complete line:** roll-in → medium kicker → left rail / right box → reconnect → optional left side hit → left down box → left large kicker → wide exit rail → snow exit. The right box branch rejoins before the down box; it bypasses the side hit. The full line is intended to take roughly 25–45 seconds depending on entry speed. Approach targets are guidance, not required exact speeds; the report records measured runs. Coming from the top provides more first-jump airtime than a standing park restart.

After the first landing, the down rail is 12 m left of center; the wide box is 12 m right. The final kicker and rail are in the left lane, 18 m from center. Orange markers identify the large kicker. Use short turns and give the skier time to recover between features.

## Ratings

Use **1 = poor / frustrating, 3 = usable, 5 = excellent**. Record device, entry route and notes before changing any configuration.

Player: ______  Date: ______  Keyboard / controller: ______  Approx. minutes: ______
Entry: standing park restart / downhill run from top  Branch: left / right

| Item | 1–5 | One useful observation |
| --- | --- | --- |
| Carving responsiveness | | |
| Speed feeling | | |
| Jump predictability | | |
| Spin responsiveness | | |
| Ability to stop rotation | | |
| Landing satisfaction | | |
| Rail capture    | | |
| Camera readability | | |
| Character motion | | |
| Line flow | | |
| Overall fun | | |

## Questions

- What feels too slow?
- What feels too sensitive?
- What feels too automatic?
- What feels unfair?
- Which landing felt best?
- Which feature was hardest to line up?
- Where did the camera make it difficult to see?
- Which part made you want to retry?

For contact trouble, note the feature, speed, L/R hit/sample counts and whether the body appeared to bounce or just the skis. For camera trouble, note the feature, approach/flight/landing phase and what was hidden. A short clip is useful but optional.

## Tuning after feedback

Change one relevant config at a time and repeat the same marker, branch and entry speed. Keep a before/after note. `SkiPhysicsConfig` controls contact refinement, support, grip, sidecut and edge response; `TrickConfig` controls rotation, spotting, landing grades and rail capture; `CameraConfig` controls framing; `CharacterVisualConfig` controls procedural pose; `WorldConfig.freestyleLine` stores authored placements, approach-speed guidance and route/branch data. Positions use mountain x/z plus y lift above snow; existing jump/rail profiles are reused at unit scale.

Current contact spring/damping and established carving/trick/camera tuning are retained. Do not change all controls together to chase one rating. Genuine jump takeoffs and failed landings should remain distinguishable from surface chatter. After a change, rerun the focused contact/line suite and the relevant production-jump/rail regressions listed in `CONTACT_LINE_PASS_REPORT.md`.
