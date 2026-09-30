# VP5 — Snow interaction and motion effects

All seven steps are complete. Final results, reproduction commands and remaining issues are recorded in `REPORT.md`.

1. Capture matched Day/Sunset action views with the existing effects: straight descent, carving, braking, switch, powder, landing, rail, crash and reset. Preserve the VP4 archive and physics settings.
2. Replace hard track strips with UV-profiled grooves: soft cavity, light raised edges, deeper/wider powder marks, time fade and terrain-aligned placement. Break connections after flight, rail entry and retry; clear expired meshes and dispose runtime meshes/materials.
3. Accumulate fractional particle emission so the 144 FPS cap does not suppress spray. Use fixed reusable flake, powder-puff and rail-shaving pools with a combined configured particle cap. Direct snow using actual travel/slip and edge direction, including switch.
4. Generate soft procedural particle textures and a depth-softened, fog-aware particle shader. Separate fine carve spray, low translucent brake clouds, contact-centered landing bursts/rings, rail frost and crash flakes. Stop ground emission during flight/bail and clear live particles on retry.
5. Put effect sizes/rates/fade/gap limits in GraphicsConfig. Keep force, air, rail, terrain and camera tuning unchanged.
6. Inspect both lighting presets and a short action sequence. Check ski separation, connection breaks, expiry, frame-rate-independent emission, grounded/air/rail transitions, burst positioning and the combined cap with focused tests.
7. Build the reviewed version, inspect one capped native run and launch the extracted package. Update evidence/report, package checksum and milestone/plan status, then commit VP5. No routine performance benchmark or regional launch matrix.
