#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.ConfigureVisualPolish -logFile "$PROJECT_ROOT/Logs/visual-polish.log"
echo "Visual polish foundation configured."
