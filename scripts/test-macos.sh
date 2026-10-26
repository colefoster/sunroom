#!/bin/bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"
bundle="$root/build/SunroomReleaseChecks.app"
mkdir -p "$bundle/Contents/MacOS"
cat > "$bundle/Contents/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0"><dict><key>CFBundleIdentifier</key><string>local.sunroom.release-checks</string><key>CFBundleExecutable</key><string>SunroomReleaseChecks</string><key>LSUIElement</key><true/></dict></plist>
PLIST
xcrun swiftc -D SUNROOM_TESTING -parse-as-library -swift-version 5 \
    -target "$(uname -m)-apple-macosx13.0" -sdk "$(xcrun --sdk macosx --show-sdk-path)" \
    -framework AppKit -framework SwiftUI Sunglasses/SunglassesApp.swift scripts/test-macos.swift \
    -o "$bundle/Contents/MacOS/SunroomReleaseChecks"
"$bundle/Contents/MacOS/SunroomReleaseChecks"
defaults delete local.sunroom.release-checks >/dev/null 2>&1 || true
