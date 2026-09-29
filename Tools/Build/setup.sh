#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$BLENDER_BIN" --background --python-exit-code 1 --python "$PROJECT_ROOT/Tools/Blender/test_blender.py" > "$PROJECT_ROOT/Logs/blender-setup.log" 2>&1
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.ConfigureProject -logFile "$PROJECT_ROOT/Logs/setup.log"
if rg -n 'error CS|Scripts have compiler errors|Aborting batchmode' "$PROJECT_ROOT/Logs/setup.log"; then exit 1; fi
echo "Blender FBX smoke test and Unity setup passed. Logs in Logs/."
