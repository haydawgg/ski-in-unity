#!/usr/bin/env bash
set -euo pipefail
game_root="$(cd "$(dirname "$0")" && pwd)"
window_args=()
if [[ "${XDG_SESSION_TYPE:-}" == "wayland" && -n "${WAYLAND_DISPLAY:-}" && "${POWDERFLOW_X11:-0}" != "1" ]]; then
    window_args=(-force-wayland)
fi
exec "$game_root/PowderFlow.x86_64" "${window_args[@]}" "$@"
