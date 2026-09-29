#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
platform="${1:-EditMode}"
graphics_args=(); if [[ "$platform" == "EditMode" ]]; then graphics_args=(-nographics); fi
"$UNITY_BIN" -batchmode "${graphics_args[@]}" -projectPath "$PROJECT_ROOT" -runTests -testPlatform "$platform" -testResults "$PROJECT_ROOT/Logs/$platform-results.xml" -logFile "$PROJECT_ROOT/Logs/$platform-tests.log"
python "$PROJECT_ROOT/Tools/Validation/check_tests.py" "$PROJECT_ROOT/Logs/$platform-results.xml"
