# Decisions

- 2026-09-29: Adopt PowderFlow from the new master plan; preserve the existing project path and installed Unity 6000.3.25f1.
- Use URP and the Input System. Keep generated geometry deterministic and use Blender FBX exports. Temporary geometry is limited to the early physics milestones.
- Linux is the development and first shipping target because this workstation runs CachyOS. Other build targets require their Unity Hub modules.
- Each milestone is gated by batch compilation and automated checks, with simulation runs and captured frames for gameplay/visual inspection. Subjective feel cannot be certified by a headless test; document that limit explicitly.
- Snow and sky use original URP HLSL source shaders instead of generated Shader Graph JSON; material parameters and GraphicsConfig remain editable. This differs from the plan's authoring format while implementing its visual effects.
- Separate render benchmarks from gameplay FPS. Unity test coroutine iterations run faster than rendered frames in batch mode; camera rendering plus GPU readback is used for M9 and standalone unscaled frame timing for M10.
- Correct Blender export coordinates for Unity's imported X reflection; verify left/right rig placement and off-center analytic terrain height after every regeneration.
- Select Unity's native Wayland backend in the portable launcher when a Wayland session is detected. Default X11 stalled before scene initialization on this workstation; native Wayland completed visible gameplay and menu captures. Keep an explicit X11 opt-out for other desktops.
- Deliver a Linux development build with original procedural Foley and coordinated outfit presets. Document remaining authoring-format/content differences and human playtest requirements in FINAL_REPORT rather than claiming automated checks certify subjective feel.
