# macOS release audit — v0.1.0

Reviewed 2026-10-26 before public distribution.

## Findings fixed

- **macOS 13 build failed:** zero-argument SwiftUI `onChange` handlers required macOS 14. Replaced them with the compatible value-taking form; both architecture slices now compile with a 13.0 deployment target.
- **Bundle was incomplete for distribution:** added executable, identifier, display name, version/build, minimum system version, icon, and high-resolution metadata. The app is named Sunroom; the original bundle identifier is retained to preserve preferences.
- **Editing-window close could discard the reusable overlay:** retain the window and treat closing as hide. Changing tint now preserves the editing border. Locking closes the transient settings panel so its button does not show stale state.
- **Disconnected monitors could strand the box:** recover an off-screen rectangle at startup and when display parameters change.
- **No public entry-point documentation or code license:** added README with requirements, installation, actual behavior, privacy, build/check commands, and compatibility limits; added MIT license.
- **Bundled font attribution was incomplete:** added the DM Sans and Outfit SIL licenses. Archivo's existing SIL license is retained with the archived design.
- **Internal paths in current docs:** changed the design prompt to a repository-relative path and generalized the Windows test report's machine-specific path.

## Repository and history

Gitleaks scanned all 10 pre-release commits (about 435 KB of text) with **no secrets found**. A separate history search found an earlier absolute workspace path in a design prompt, not a credential. It is corrected in the current tree; ordinary author metadata and that harmless historical path remain in Git history. No history was rewritten.

Design comparison evidence is retained as archived project history. `windows/` is explicitly deferred; no Windows asset is part of this release. Build products, Windows test outputs, and website download binaries are ignored in the GitHub source repository. Release binaries are explicit release assets, not source-tree dependencies.

## Validation

- 17 native AppKit/SwiftUI release checks passed in an isolated preferences domain on macOS 26.4.1, Apple Silicon.
- Universal Mach-O contains **arm64 and x86_64**; both load commands declare **macOS 13.0** minimum.
- Only Apple/system frameworks and libraries are linked; no developer checkout paths are dependencies.
- Ad-hoc code signature verifies strictly, including the bundled license and icon.
- DMG checksum verifies and mounts read-only; it contains Sunroom.app, an Applications shortcut, and first-launch instructions.
- Packaged app launched successfully from the mounted DMG. Native menu-bar accessibility inspection was unavailable through the automation bridge; the direct native checks cover the underlying settings/window actions.

## Distribution limits

**No Developer ID identity is installed on the build Mac.** v0.1.0 is ad-hoc signed and is not notarized. README, DMG instructions, website, and release notes disclose the first-launch Gatekeeper step. A managed Mac may block it. This is not a claim of notarized or App Store distribution.

Intel hardware and macOS 13/14/15 runtimes have not been exercised directly. The universal binary and deployment target establish build compatibility, not a full tested hardware/OS matrix.

The public repository and public Site audience are separate switches. Preparing and publishing release artifacts does not itself change either audience.
