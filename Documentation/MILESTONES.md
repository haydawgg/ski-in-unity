# Milestone log

## M1 — Setup: passed

- Added `Tools/Build`, the required FBX Blender smoke test, assembly definitions, Unity packages, URP settings and setup test. Configured Linear color, HDR, Input System and persisted 100 Hz physics.
- Validation: `Tools/Build/setup.sh` exits 0; Blender PNG/blend/FBX validated; Unity compiles without C# errors. EditMode: 1/1 passed.
- Try: open this project in Unity 6000.3.25f1, inspect Assets/Settings/PowderFlowURP; run the two commands above to repeat validation.
- Issues: no gameplay yet. Unity emits licensing connection retries and a bundled dotnet shutdown message, but activation and batch execution succeed. No reference video was supplied, so use the plan's written visual reference.
