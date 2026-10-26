#!/bin/bash
set -euo pipefail

root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"
version=$(/usr/libexec/PlistBuddy -c 'Print CFBundleShortVersionString' Sunglasses/Info.plist)
output="$root/build/release"
app="$output/Sunroom.app"
sdk="$(xcrun --sdk macosx --show-sdk-path)"
mkdir -p "$output" "$root/build/arm64" "$root/build/x86_64"
rm -rf "$app"
mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources"
for arch in arm64 x86_64; do
    xcrun swiftc -parse-as-library -swift-version 5 -O -whole-module-optimization \
        -target "$arch-apple-macosx13.0" -sdk "$sdk" \
        -framework AppKit -framework SwiftUI \
        Sunglasses/SunglassesApp.swift -o "$root/build/$arch/Sunroom"
done
xcrun lipo -create "$root/build/arm64/Sunroom" "$root/build/x86_64/Sunroom" -output "$app/Contents/MacOS/Sunroom"
cp Sunglasses/Info.plist "$app/Contents/Info.plist"
cp LICENSE "$app/Contents/Resources/LICENSE.txt"
xcrun swift scripts/make-icon.swift "$root/build/Sunroom.iconset"
iconutil -c icns "$root/build/Sunroom.iconset" -o "$app/Contents/Resources/Sunroom.icns"
if [[ -n "${SUNROOM_SIGN_IDENTITY:-}" ]]; then
    codesign --force --options runtime --timestamp --sign "$SUNROOM_SIGN_IDENTITY" "$app"
else
    codesign --force --sign - "$app"
fi
codesign --verify --strict --verbose=2 "$app"

stage="$(mktemp -d "$root/build/dmg.XXXXXX")"
trap 'rm -rf "$stage"' EXIT
ditto "$app" "$stage/Sunroom.app"
ln -s /Applications "$stage/Applications"
cp docs/INSTALL.txt "$stage/Read me first.txt"
dmg="$output/Sunroom-$version.dmg"
rm -f "$dmg"
hdiutil create -volname "Sunroom $version" -srcfolder "$stage" -ov -format UDZO "$dmg"
if [[ -n "${SUNROOM_NOTARY_PROFILE:-}" ]]; then
    xcrun notarytool submit "$dmg" --keychain-profile "$SUNROOM_NOTARY_PROFILE" --wait
    xcrun stapler staple "$dmg"
    xcrun stapler validate "$dmg"
fi
ditto -c -k --keepParent "$app" "$output/Sunroom-$version.zip"
(cd "$output" && shasum -a 256 "Sunroom-$version.dmg" "Sunroom-$version.zip" > SHA256SUMS.txt)
printf 'Release artifacts: %s\n' "$output"
