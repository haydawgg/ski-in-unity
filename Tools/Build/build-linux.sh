#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.BuildGame -logFile "$PROJECT_ROOT/Logs/build.log"
cp "$PROJECT_ROOT/Tools/Build/player-launcher.sh" "$PROJECT_ROOT/Builds/Linux/Play.sh"
cp "$PROJECT_ROOT/Tools/Build/player-readme.txt" "$PROJECT_ROOT/Builds/Linux/README.txt"
chmod +x "$PROJECT_ROOT/Builds/Linux/Play.sh"
echo "Built Builds/Linux/PowderFlow.x86_64"
