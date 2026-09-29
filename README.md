# PowderLine

Unity 6.3 C# project for a physics based freestyle skiing game. This is the project setup stage; gameplay has not been implemented.

## Requirements

- Unity Editor 6000.3.25f1 through Unity Hub
- Blender 5.2.2 or newer
- Git
- .NET SDK 8 for external C# tooling (installed locally in `~/.local/share/dotnet`)

Run `./scripts/check_toolchain.sh` to test Blender's Python API, `.blend` save, GLB export, and preview rendering, and to locate Unity. Override paths with `BLENDER_BIN` and `UNITY_BIN`. Run `./scripts/open_editor.sh` to open the Unity project. A batch-mode editor launch completed and resolved Input System, Cinemachine, and Universal Render Pipeline. Unity still emits a nonfatal .NET build-server warning during shutdown; editor import exits successfully.

This repository is ready for Unity scene, C# skiing systems, and Blender-generated asset development. There is no playable game or build yet.
