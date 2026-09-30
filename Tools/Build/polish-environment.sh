#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.ConfigureEnvironmentGraphics -logFile "$PROJECT_ROOT/Logs/environment-graphics.log"
