#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
exec "$PROJECT_ROOT/Builds/Linux/Play.sh" "$@"
