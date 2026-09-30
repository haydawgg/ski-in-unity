#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.ConfigureSnowVisuals -logFile "$PROJECT_ROOT/Logs/snow-graphics.log"
echo "Snow interaction materials and textures configured."
