#!/usr/bin/env bash
source "$(dirname "$0")/common.sh"
cd "$PROJECT_ROOT"
mkdir -p Builds/Packages
cp Tools/Build/player-launcher.sh Builds/Linux/Play.sh
cp Tools/Build/player-readme.txt Builds/Linux/README.txt
chmod +x Builds/Linux/Play.sh
mkdir -p "$PROJECT_ROOT/Builds/Linux/ThirdPartyLicenses"
cp "$PROJECT_ROOT/Documentation/DEJAVU_FONT_LICENSE.txt" "$PROJECT_ROOT/Builds/Linux/ThirdPartyLicenses/DejaVu.txt"
tar -czf Builds/Packages/PowderFlow-Linux.tar.gz -C Builds Linux
echo "Package: Builds/Packages/PowderFlow-Linux.tar.gz"
