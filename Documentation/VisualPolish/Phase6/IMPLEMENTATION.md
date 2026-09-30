# VP6 — Camera, HUD and menu presentation

Completed 2026-09-30. All seven steps below are complete; results and remaining limits are in `REPORT.md`.

1. Preserve VP5 package, tuning/terrain hashes and matched camera/menu baselines. Review current layouts and the Park obstruction.
2. Follow travel with a stable horizon and slope-aware framing. Add cruise/carve/rail/air/bail framing parameters, portrait pullback, landing anticipation and restrained landing shake. Resolve obstructions after smoothing and reset safely.
3. Add a reusable presentation config/theme: slate surfaces, cream type, teal selection and warm accents. Scale the UI into the safe area for 1280×720, 1920×1080 and 1080×1920.
4. Replace menu buttons and group settings. Preserve keyboard/gamepad/mouse activation and saves; add a live outfit preview and clear back/focus controls. Compose a quieter menu mountain view away from the gate.
5. Follow the user’s correction: remove every gameplay panel and persistent hint. Put small speed text in the top left; timed-session score/time and brief trick feedback in the top right. Use a subtle text shadow, tier color and 1.8-second fade, keeping the center/bottom entirely clear.
6. Inspect matched camera states in landscape/portrait and all menu pages at supported sizes. Check actual gamepad/keyboard actions, saves, layout bounds, camera horizon/framing and obstruction clearance. Keep physics/trick/rail behavior unchanged.
7. Build final art, run one capped native gameplay review, capture the native UI sizes/pages and launch the extracted package. Update evidence/report/plan and commit. No routine performance benchmark.
