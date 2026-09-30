#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.ConfigurePresentation -logFile "$PROJECT_ROOT/Logs/presentation-config.log"
echo "Presentation theme and camera configured."
