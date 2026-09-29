#!/usr/bin/env bash
set -euo pipefail
PROJECT_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
source "$PROJECT_ROOT/Tools/Build/toolchain.env"
mkdir -p "$PROJECT_ROOT/Logs"
