#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
"$UNITY_BIN" -batchmode -nographics -quit -projectPath "$PROJECT_ROOT" -executeMethod PowderFlow.EditorTools.ConfigureCharacterGraphics -logFile "$PROJECT_ROOT/Logs/character-graphics.log"
echo "Character visual materials configured."
