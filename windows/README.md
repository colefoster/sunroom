# Sunroom for Windows

A small native Windows tray utility, based on the macOS Sunglasses app in this repository.
Place a translucent rectangle over a bright shared screen. Lock it to let clicks pass through to the app below.

## Use

- Open **Sunroom** from the Start menu. Click the sun icon in the notification area to open settings; Windows may put it under the tray's **^** menu.
- **Unlock · move & resize**: drag the box to move it; drag an edge or corner to resize it.
- **Lock tint** on the box, or **Lock · click through** in settings: remove the editing frame and pass mouse input through.
- **Show tint** hides or restores the box independently of its lock state.
- Choose Amber, Blue, Gray, or any custom color. Strength ranges from 5–80%.
- **Bring box to this screen** recovers an off-screen box and unlocks it for positioning.
- Closing settings leaves Sunroom running in the tray. Use **Quit Sunroom** to exit.

Settings and geometry are saved in `%LOCALAPPDATA%\Sunroom\settings.json`. The app does not start at login automatically.
It tints an area you position by hand; it does not detect windows or modify a screen share.
Windows' secure desktop (for example, a UAC prompt) and exclusive full-screen surfaces are outside the overlay's scope.

## Build and install

Requires Windows 10/11 and .NET SDK 8 or newer to build. The published app includes its runtime; end users do not need .NET installed. The build resolves the current .NET 8 runtime patch from Microsoft release metadata.

```powershell
.\windows\build.ps1
.\windows\install.ps1
```

The build produces `windows\artifacts\win-x64\Sunroom.exe`. Install copies it to `%LOCALAPPDATA%\Sunroom` and creates a Start menu shortcut. Neither step needs administrator rights.
Use `build.ps1 -Runtime win-arm64` to build for ARM64. Installation is per user. Quit the existing app before updating.
To uninstall, quit Sunroom, delete its folder and remove `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Sunroom.lnk`.

## Desktop integration checks

Run in an unlocked, interactive Windows desktop; the checks create their own temporary bright window and use actual mouse input. Avoid using the mouse while they run (roughly 15 seconds).

```powershell
dotnet run --project .\windows\Sunroom.Tests -c Release -- "$env:TEMP\Sunroom-tests"
Get-Content "$env:TEMP\Sunroom-tests\result.json"
```

Checks include layered-window blending, click-through, focus, native drag/resize, lock button, hide/show independence, presets, strength bounds, topmost switching, monitor positioning, saved state, and damaged settings recovery.
Screenshots contain only the temporary test window. The test restores the original cursor position and attempts to restore foreground focus. It may temporarily minimize a foreground app that captures the mouse, then restore it.

To check the installed package without mouse automation, run `Sunroom.Tests\PackageSmoke.ps1` in an interactive desktop session with Sunroom initially closed. It leaves the app running with the tint hidden.

Implementation: WinForms tray/settings UI with a Win32 per-pixel-alpha layered window. The editing frame remains opaque at low tint strength. PerMonitorV2 DPI awareness is enabled. No third-party NuGet dependencies.
