# Sunroom

Sunroom is the website identity for the existing Sunglasses macOS menu-bar utility. It is currently free to use. The source and native menu still call it Sunglasses. No packaged download is verified, so the page gives Xcode/source-build instructions rather than a download or purchase flow.

Platform scope: macOS only. The experimental Windows port is deferred; do not advertise Windows support or downloads.

The motivation is specific: the owner uses dark mode throughout their Mac, but another person’s bright Zoom/Teams screen share cannot be switched to dark mode locally. The utility puts a manually adjusted translucent rectangle over that selected area while the rest of the desktop stays unchanged.

Native facts from SunglassesApp.swift: RGB tint picker; 5–80% opacity; Amber/Blue/Gray presets; separate visibility and move/click-through behavior; always-on-top setting; all Spaces; geometry and settings stored locally. Initial tint is RGB .15/.10/0 at 30%; presets are Amber .2/.12/0 at30%, Blue 0/.05/.2 at25%, Gray .05/.05/.05 at35%.

The combined webpage uses one working demo. Show/hide preserves geometry and lock mode. Unlocked exposes a draggable title bar and resize corner; locked lets clicks pass through. A real checkbox underneath demonstrates this and confirms successful click-through. Color, opacity, presets, geometry and mode are remembered in browser storage. Demo changes do not affect the actual display.

Keep the tone practical and modest: explain the utility, offer the demo, and show how to run it. No sales CTAs, fabricated testimonials, pricing funnel, health claims or promises that it will remain free forever. Do not imply automatic window tracking or dark-mode conversion.
