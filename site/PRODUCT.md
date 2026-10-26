# Sunroom

Sunroom is a free macOS menu-bar utility. The source project and bundle ID retain the original Sunglasses name to preserve existing preferences; the app and menu use Sunroom. Version 0.1.0 has a universal Apple Silicon/Intel DMG and ZIP for macOS 13+. The initial release is ad-hoc signed and not notarized; explain the first-launch steps honestly. Offer the download, checksums, release notes, and source build instructions.

Platform scope: macOS only. The experimental Windows port is deferred; do not advertise Windows support or downloads.

Canonical hosting: public Cloudflare Pages at https://sunroom.colefoster.ca/ (Pages origin: https://sunroom-593.pages.dev/). The GitHub repository remains private, so serve downloads and release notes directly from the site and omit inaccessible GitHub/source-build links. The earlier private ChatGPT Sites copy is separate.

The motivation is specific: the owner uses dark mode throughout their Mac, but another person’s bright Zoom/Teams screen share cannot be switched to dark mode locally. The utility puts a manually adjusted translucent rectangle over that selected area while the rest of the desktop stays unchanged.

Native facts from SunglassesApp.swift: RGB tint picker; 5–80% opacity; Amber/Blue/Gray presets; separate visibility and move/click-through behavior; always-on-top setting; all Spaces; geometry and settings stored locally. Initial tint is RGB .15/.10/0 at 30%; presets are Amber .2/.12/0 at30%, Blue 0/.05/.2 at25%, Gray .05/.05/.05 at35%.

The combined webpage uses one working demo. Show/hide preserves geometry and lock mode. Unlocked exposes a draggable title bar and resize corner; locked lets clicks pass through. A real checkbox underneath demonstrates this and confirms successful click-through. Color, opacity, presets, geometry and mode are remembered in browser storage. Demo changes do not affect the actual display.

Keep the tone practical and modest: explain the utility, offer the demo, and show how to run it. No sales CTAs, fabricated testimonials, pricing funnel, health claims or promises that it will remain free forever. Do not imply automatic window tracking or dark-mode conversion.
