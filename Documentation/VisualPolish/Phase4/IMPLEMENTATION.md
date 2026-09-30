# VP4 implementation and review plan

The existing VP4 scope is park features and mountain props. Keep the 144 FPS limit and avoid routine benchmarks.

1. Preserve the mountain FBXs and all rail/box path JSON. Record the imported triangles for all six jump riding surfaces and sides before regeneration.
2. Author park content in `Tools/Blender/create_park.py`, reusable from full environment generation and a focused `park` stage. Use a slate/teal palette, cream mesh lettering and orange accents. Consolidate visual hardware per asset.
3. Separate `Collision_` meshes from decorative geometry in the importer. Keep rail tube/path alignment, box riding surfaces and jump collision geometry. Signs and lodge retain simple solid collision; chairs, cables, flags, gates and markers add none.
4. Connect supports to actual rail/box profiles, close box shells and shape visual snow banks outside the jump riding width. Detail lodge, sheaves, ladder, light housing and fence.
5. Build continuous lift spans between the actual tower positions. Add chairs beneath the cables. Place five region boards, graded feature markers, start gate and outer boundary markers outside designed entries.
6. Run relevant asset/collision checks, the park review and connected mountain descent. Inspect both presets for rail, box, jump, lodge, lift, sign and gate readability. Repair concrete defects, with targeted repeats only as needed.
7. Build once after the reviewed art is final. Inspect one capped native gameplay run and launch an extracted archive. Record evidence and remaining issues, update docs and commit VP4. VP5 follows.
