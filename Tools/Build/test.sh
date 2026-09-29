#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
platform="${1:-EditMode}"
graphics_args=(); if [[ "$platform" == "EditMode" ]]; then graphics_args=(-nographics); fi
filter_args=(); if [[ -n "${2:-}" ]]; then filter_args=(-testFilter "$2"); fi
"$UNITY_BIN" -batchmode "${graphics_args[@]}" -projectPath "$PROJECT_ROOT" -runTests -testPlatform "$platform" "${filter_args[@]}" -testResults "$PROJECT_ROOT/Logs/$platform-results.xml" -logFile "$PROJECT_ROOT/Logs/$platform-tests.log"
python "$PROJECT_ROOT/Tools/Validation/check_tests.py" "$PROJECT_ROOT/Logs/$platform-results.xml"
