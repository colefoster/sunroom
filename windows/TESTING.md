# Windows verification — 2026-10-25

Test host: colepc, Windows 11 build 26200, interactive console session 1.
Built with the installed .NET SDK 9.0.301. The published x64 executable includes .NET / Windows Desktop 8.0.33.

## Completed

- An interactive desktop run verified actual mouse click-through, focus preservation, native dragging and corner resizing, opaque edit controls at 5% tint, 80% maximum strength, all three presets, topmost switching, independent hide/show, the on-box lock button, closing/reopening settings, movement onto both attached displays (`DISPLAY3` and `DISPLAY2`), and geometry/color/visibility/lock restoration.
- That run exposed an invalid-settings serialization issue. Computed color and bounds properties are now excluded from settings JSON. Separate regression checks passed for malformed JSON and invalid/off-screen settings.
- The **installed standalone executable**, with its final bundled runtime, passed all 10 package checks: startup in session 1, persisted visibility, preset controls, single-instance launch and settings reopening, clean quit, restart, saved-state restoration, and retaining the tray process after closing settings.
- Final installed executable SHA-256: `8374C9A0ABCAF22BF5AFD849F8E830D9067666B002E63ADEE3BE76DE4E0F428A`.

## Input-test status

Later full-suite reruns were interrupted by active mouse/focus changes from Overwatch and Discord. Input automation stopped; an uninterrupted rerun is pending an idle-mouse window. The previously completed input assertions and final package checks are recorded separately rather than claiming the interrupted full run passed.

Machine-local raw results and screenshots: `windows/artifacts/test-results/` (ignored by Git); the PC retains them at `C:\Users\cole\Dev\sunroom-windows\test-results`.

Installed to `%LOCALAPPDATA%\Sunroom\Sunroom.exe`, with a Start menu shortcut. Left running in the tray with tint hidden and locked.
