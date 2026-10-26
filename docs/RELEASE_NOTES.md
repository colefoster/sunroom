# Sunroom 0.1.0 — macOS preview

A free menu-bar utility for putting a movable tint box over a bright screen share.

## Download

- **Sunroom-0.1.0.dmg** — drag Sunroom to Applications.
- **Sunroom-0.1.0.zip** — alternative app bundle archive.
- **SHA256SUMS.txt** — SHA-256 hashes for both downloads.

Requires **macOS 13 or later**. The same app includes Apple Silicon and Intel binaries. This is a macOS-only release; the Windows prototype is deferred.

## First launch

**This preview is ad-hoc signed, not Developer ID signed or notarized.** macOS may block its first launch. If you trust the download, try opening it once, then choose **System Settings → Privacy & Security → Open Anyway** and confirm. Managed Macs may prevent this. Do not disable Gatekeeper. [Apple’s instructions](https://support.apple.com/en-us/102445).

Look for the **sun icon in your menu bar**, not the Dock. Unlock to move/resize the tint, then lock to click through. Show/hide is independent. Color, strength, and geometry are saved locally.

## Included

- Custom tint colors, 5–80% opacity, Amber/Blue/Gray presets.
- Separate visibility and edit/click-through modes.
- Saved geometry and settings, all-Spaces behavior, two stacking levels.
- Safe editing-window close and off-screen recovery when displays change.
- A universal build script, native release checks, and local-only preferences with no analytics or account.

## Verification and limits

17 native release checks passed on macOS 26.4.1 on Apple Silicon. Both architecture slices declare macOS 13.0 minimum. The ad-hoc signature and disk image verify, and the app launches from the mounted DMG. Intel hardware and older macOS runtimes have not been tested directly.

The overlay is positioned by hand. It does not track windows or convert their content to dark mode. Some full-screen or secure system surfaces may cover it.
