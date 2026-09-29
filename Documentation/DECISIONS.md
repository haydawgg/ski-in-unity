# Decisions

- 2026-09-29: Adopt PowderFlow from the new master plan; preserve the existing project path and installed Unity 6000.3.25f1.
- Use URP and the Input System. Keep generated geometry deterministic and use Blender FBX exports. Temporary geometry is limited to the early physics milestones.
- Linux is the development and first shipping target because this workstation runs CachyOS. Other build targets require their Unity Hub modules.
- Each milestone is gated by batch compilation and automated checks, with simulation runs and captured frames for gameplay/visual inspection. Subjective feel cannot be certified by a headless test; document that limit explicitly.
