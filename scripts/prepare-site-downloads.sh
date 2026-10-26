#!/bin/bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"
version=$(python3 -c 'import plistlib; print(plistlib.load(open("Sunglasses/Info.plist", "rb"))["CFBundleShortVersionString"])')
destination="$root/site/dist/downloads"
mkdir -p "$destination"
gh release download "v$version" --repo colefoster/sunroom --dir "$destination" --clobber \
    --pattern "Sunroom-$version.dmg" --pattern "Sunroom-$version.zip" --pattern SHA256SUMS.txt
(cd "$destination" && shasum -a 256 --check SHA256SUMS.txt)
