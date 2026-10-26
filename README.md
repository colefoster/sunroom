# Sunroom

A free macOS menu-bar utility that puts a movable tint box over a bright part of your screen.

Dark mode works until someone shares a bright window in Zoom or Teams. Sunroom lets you dim that area locally while the rest of your desktop stays as it is. Position the box by hand, then lock it so clicks pass through to the app underneath.

[Download v0.1.0](https://sunroom.colefoster.ca/#setup) · [Try the web demo](https://sunroom.colefoster.ca/)

## Requirements

- **macOS 13 Ventura or later.** The same app includes Apple Silicon and Intel binaries.
- **macOS only.** The Windows experiment in `windows/` is deferred and is not a supported release.
- No account, screen-recording permission, or Accessibility permission is required.

The release is compiled for macOS 13+, with local runtime checks performed on macOS 26.4.1 on Apple Silicon. Older macOS releases and Intel hardware have not been runtime-tested.

## Install

1. Download `Sunroom-0.1.0.dmg` from the [website](https://sunroom.colefoster.ca/#setup).
2. Open the disk image and drag **Sunroom** into **Applications**.
3. Eject the disk image and launch Sunroom from Applications.
4. Look for the **sun icon in the menu bar**. There is no Dock icon.

**First-release signing:** v0.1.0 is ad-hoc signed, not Developer ID signed or notarized. macOS may block its first launch because Apple has not verified the developer. If you trust the download, attempt to open it, then go to **System Settings → Privacy & Security → Open Anyway** and confirm. A managed Mac may prevent this. Do not disable Gatekeeper. [Apple’s explanation](https://support.apple.com/en-us/102445).

A ZIP is also available. Release assets include `SHA256SUMS.txt`; compare a download with:

```sh
shasum -a 256 Sunroom-0.1.0.dmg
```

## Use

- **Unlock · Move & Resize:** position the box using its title bar and resize its edges.
- **Lock · Click Through:** remove the editing frame and let mouse input reach the app underneath.
- **Show/hide:** hide or restore the box independently of locking. Closing the editing window hides the tint.
- Choose any tint color, change opacity from **5–80%**, or use **Amber**, **Blue**, or **Gray**.
- Quit from the menu-bar panel to stop Sunroom.

Sunroom remembers its color, opacity, visibility, position, size, and stacking preference locally. It starts locked. If a monitor disappears, it brings an off-screen box back to an available display.

## What it does—and its limits

The box stays where you put it. It does not follow windows, identify a screen share, or convert an app to dark mode. It visually tints everything within its rectangle, including images and video. Locking makes it click-through; unlocking makes it editable.

The overlay joins Spaces and uses a high window level when **Always on top** is enabled. Turning that off uses a lower floating level; it does not turn the tint into an ordinary background window. Secure system prompts and some full-screen situations may cover it.

## Privacy

The native app makes no network requests, captures no screen content, and contains no analytics or account system. Preferences live in macOS UserDefaults under the existing bundle ID, `ca.colefoster.Sunglasses`. The web demo changes only its own page and remembers its demo controls in browser storage.

## Build from source

Install Apple’s Command Line Tools (`xcode-select --install`), then:

```sh
git clone https://github.com/colefoster/Sunglasses.git
cd Sunglasses
./scripts/build-macos.sh
```

Artifacts are written to `build/release/`: a universal `Sunroom.app`, versioned DMG and ZIP, and SHA-256 checksums. The version comes from `Sunglasses/Info.plist`. Builds use system AppKit and SwiftUI; there are no third-party native dependencies.

The script ad-hoc signs by default. Release maintainers with a Developer ID identity can set `SUNROOM_SIGN_IDENTITY`. Set `SUNROOM_NOTARY_PROFILE` to an existing `notarytool` keychain profile to submit and staple the DMG. Credentials are not stored in this repository.

With full Xcode installed, you can also open `Sunglasses.xcodeproj`, select the **Sunglasses** target and **My Mac**, and run it. The source project and bundle ID retain the earlier name to preserve existing preferences; the built app is **Sunroom**.

Run the native release checks on a logged-in Mac:

```sh
./scripts/test-macos.sh
```

These checks use a separate preferences domain and a temporary overlay. They verify native window behavior programmatically; they are not a complete OS/hardware compatibility matrix.

## Repository

The public website and downloads are hosted on **Cloudflare Pages**. See [deployment instructions](docs/CLOUDFLARE_PAGES.md). The source repository's visibility is managed separately.

- `Sunglasses/` — native macOS app and bundle metadata.
- `scripts/` — universal build, packaging, and native checks.
- `site/dist/` — buildless website and interactive demo.
- `windows/` — deferred Windows prototype; no supported Windows download.
- `comparison/`, `site-opus/` — archived design alternatives and evaluation evidence.

The repository keeps the original **Sunglasses** URL so existing links keep working.

## License

App and website code: [MIT](LICENSE). Bundled website fonts retain their SIL Open Font Licenses: [DM Sans](site/dist/assets/licenses/dm-sans-OFL.txt), [Outfit](site/dist/assets/licenses/outfit-OFL.txt), and [Archivo in the archived alternative](site-opus/dist/fonts/OFL.txt).
