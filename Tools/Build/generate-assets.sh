#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$BLENDER_BIN" --background --python-exit-code 1 --python "$PROJECT_ROOT/Tools/Blender/build_all_assets.py" -- --stage "${1:-all}" > "$PROJECT_ROOT/Logs/assets.log" 2>&1
python "$PROJECT_ROOT/Tools/Blender/validate_assets.py"
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.ImportGeneratedAssets -logFile "$PROJECT_ROOT/Logs/import-assets.log"
