#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
blender_bin="${BLENDER_BIN:-$(command -v blender || true)}"
if [[ -z "$blender_bin" || ! -x "$blender_bin" ]]; then
  echo "Missing Blender. Set BLENDER_BIN to its executable." >&2
  exit 1
fi
echo "Blender: $blender_bin"
echo "Git: $(git --version)"
if command -v dotnet >/dev/null; then echo "C# SDK: $(dotnet --version)"; fi
"$blender_bin" --background --python "$root/tools/blender/test_setup.py"
unity_bin="${UNITY_BIN:-}"
if [[ -z "$unity_bin" ]]; then
  for candidate in "$HOME/Unity/Hub/Editor/6000.3.25f1/Editor/Unity" "$HOME/Unity/Hub/Editor/6000.3.25f1/Unity"; do
    if [[ -x "$candidate" ]]; then unity_bin="$candidate"; break; fi
  done
fi
if [[ -z "$unity_bin" || ! -x "$unity_bin" ]]; then
  echo "Unity Editor 6000.3.25f1 is missing. Set UNITY_BIN if installed elsewhere." >&2
  exit 2
fi
echo "Unity: $unity_bin"
