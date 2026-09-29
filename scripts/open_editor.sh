#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
unity_bin="${UNITY_BIN:-$HOME/Unity/Hub/Editor/6000.3.25f1/Editor/Unity}"
exec "$unity_bin" -projectPath "$root"
