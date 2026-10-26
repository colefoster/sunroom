import SwiftUI
import AppKit

#if !SUNROOM_TESTING
@main
struct SunglassesApp: App {
    @NSApplicationDelegateAdaptor(AppDelegate.self) var appDelegate

    var body: some Scene {
        Settings { EmptyView() }
    }
}
#endif

class AppDelegate: NSObject, NSApplicationDelegate, NSWindowDelegate {
    var statusItem: NSStatusItem!
    var overlayWindow: NSWindow!
    var popover = NSPopover()

    @AppStorage("tintRed") var tintRed: Double = 0.15
    @AppStorage("tintGreen") var tintGreen: Double = 0.1
    @AppStorage("tintBlue") var tintBlue: Double = 0.0
    @AppStorage("tintOpacity") var tintOpacity: Double = 0.3
    @AppStorage("pinToTop") var pinToTop: Bool = true
    @AppStorage("overlayEnabled") var overlayEnabled: Bool = true
    @AppStorage("overlayX") var overlayX: Double = 100
    @AppStorage("overlayY") var overlayY: Double = 100
    @AppStorage("overlayW") var overlayW: Double = 800
    @AppStorage("overlayH") var overlayH: Double = 600

    func applicationDidFinishLaunching(_ notification: Notification) {
        // Hide dock icon
        NSApp.setActivationPolicy(.accessory)

        setupOverlayWindow()
        setupMenuBar()
    }

    func setupOverlayWindow() {
        let frame = NSRect(
            x: overlayX.isFinite ? overlayX : 100,
            y: overlayY.isFinite ? overlayY : 100,
            width: overlayW.isFinite && overlayW >= 160 ? overlayW : 800,
            height: overlayH.isFinite && overlayH >= 100 ? overlayH : 600
        )
        overlayWindow = NSWindow(
            contentRect: frame,
            styleMask: [.resizable, .fullSizeContentView],
            backing: .buffered,
            defer: false
        )

        overlayWindow.isOpaque = false
        overlayWindow.isReleasedWhenClosed = false
        overlayWindow.delegate = self
        overlayWindow.backgroundColor = .clear
        overlayWindow.hasShadow = false
        overlayWindow.ignoresMouseEvents = true
        overlayWindow.level = pinToTop ? .screenSaver : .floating
        overlayWindow.collectionBehavior = [.canJoinAllSpaces, .stationary]
        overlayWindow.titlebarAppearsTransparent = true
        overlayWindow.titleVisibility = .hidden
        overlayWindow.isMovableByWindowBackground = false
        restoreVisibleFrame()

        updateOverlayContent()

        if overlayEnabled {
            overlayWindow.orderFront(nil)
        }

        // Save position/size on changes
        NotificationCenter.default.addObserver(
            self, selector: #selector(windowDidResize),
            name: NSWindow.didResizeNotification, object: overlayWindow
        )
        NotificationCenter.default.addObserver(
            self, selector: #selector(windowDidMove),
            name: NSWindow.didMoveNotification, object: overlayWindow
        )
        NotificationCenter.default.addObserver(
            self, selector: #selector(restoreVisibleFrame),
            name: NSApplication.didChangeScreenParametersNotification, object: nil
        )
    }

    @objc func restoreVisibleFrame() {
        guard let screen = NSScreen.main ?? NSScreen.screens.first else { return }
        let frame = overlayWindow.frame
        let visible = NSScreen.screens.contains {
            let intersection = $0.visibleFrame.intersection(frame)
            return !intersection.isNull && intersection.width >= 80 && intersection.height >= 60
        }
        if !visible {
            let area = screen.visibleFrame.insetBy(dx: 40, dy: 40)
            overlayWindow.setFrame(NSRect(x: area.minX, y: area.minY,
                width: min(max(frame.width, 160), area.width),
                height: min(max(frame.height, 100), area.height)), display: true)
        }
    }

    func windowShouldClose(_ sender: NSWindow) -> Bool {
        overlayEnabled = false
        applySettings()
        return false
    }

    @objc func windowDidResize(_ notification: Notification) {
        overlayW = overlayWindow.frame.width
        overlayH = overlayWindow.frame.height
    }

    @objc func windowDidMove(_ notification: Notification) {
        overlayX = overlayWindow.frame.origin.x
        overlayY = overlayWindow.frame.origin.y
    }

    func updateOverlayContent() {
        let color = NSColor(
            red: tintRed, green: tintGreen, blue: tintBlue, alpha: tintOpacity
        )
        let view = NSView(frame: overlayWindow.contentView!.bounds)
        view.autoresizingMask = [.width, .height]
        view.wantsLayer = true
        view.layer?.backgroundColor = color.cgColor
        view.layer?.borderColor = NSColor.white.withAlphaComponent(0.5).cgColor
        view.layer?.borderWidth = isInMoveMode ? 2 : 0
        overlayWindow.contentView = view
    }

    func setupMenuBar() {
        statusItem = NSStatusBar.system.statusItem(withLength: NSStatusItem.squareLength)
        if let button = statusItem.button {
            button.image = NSImage(systemSymbolName: "sun.max.fill", accessibilityDescription: "Sunroom")
            button.action = #selector(togglePopover)
        }

        let contentView = SettingsView(delegate: self)
        popover.contentSize = NSSize(width: 280, height: 340)
        popover.behavior = .transient
        popover.contentViewController = NSHostingController(rootView: contentView)
    }

    @objc func togglePopover() {
        if popover.isShown {
            popover.performClose(nil)
        } else if let button = statusItem.button {
            // Refresh the content view
            popover.contentViewController = NSHostingController(
                rootView: SettingsView(delegate: self)
            )
            popover.show(relativeTo: button.bounds, of: button, preferredEdge: .minY)
        }
    }

    func applySettings() {
        updateOverlayContent()
        overlayWindow.level = pinToTop ? .screenSaver : .floating

        if overlayEnabled {
            overlayWindow.orderFront(nil)
        } else {
            overlayWindow.orderOut(nil)
        }
    }

    func enterMoveMode() {
        popover.performClose(nil)
        overlayWindow.ignoresMouseEvents = false
        overlayWindow.level = .screenSaver
        overlayWindow.styleMask.insert(.titled)
        overlayWindow.styleMask.insert(.closable)
        overlayWindow.titlebarAppearsTransparent = true
        overlayWindow.title = "Sunroom — lock from the menu bar when done"
        overlayWindow.makeKeyAndOrderFront(nil)

        // Brief visual cue: show border
        overlayWindow.contentView?.layer?.borderColor = NSColor.white.withAlphaComponent(0.5).cgColor
        overlayWindow.contentView?.layer?.borderWidth = 2
    }

    func exitMoveMode() {
        popover.performClose(nil)
        overlayWindow.ignoresMouseEvents = true
        overlayWindow.styleMask.remove(.titled)
        overlayWindow.styleMask.remove(.closable)
        overlayWindow.contentView?.layer?.borderWidth = 0
        applySettings()
    }

    var isInMoveMode: Bool {
        !overlayWindow.ignoresMouseEvents
    }
}

struct SettingsView: View {
    let delegate: AppDelegate

    @AppStorage("tintRed") var tintRed: Double = 0.15
    @AppStorage("tintGreen") var tintGreen: Double = 0.1
    @AppStorage("tintBlue") var tintBlue: Double = 0.0
    @AppStorage("tintOpacity") var tintOpacity: Double = 0.3
    @AppStorage("pinToTop") var pinToTop: Bool = true
    @AppStorage("overlayEnabled") var overlayEnabled: Bool = true

    var tintColor: Color {
        Color(red: tintRed, green: tintGreen, blue: tintBlue)
    }

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            HStack {
                Text("Sunroom")
                    .font(.headline)
                Spacer()
                Toggle("", isOn: $overlayEnabled)
                    .toggleStyle(.switch)
                    .onChange(of: overlayEnabled) { _ in delegate.applySettings() }
            }

            Divider()

            VStack(alignment: .leading, spacing: 8) {
                Text("Tint Color").font(.subheadline).foregroundColor(.secondary)

                ColorPicker("Color", selection: Binding(
                    get: { tintColor },
                    set: { newColor in
                        if let components = NSColor(newColor).usingColorSpace(.deviceRGB) {
                            tintRed = components.redComponent
                            tintGreen = components.greenComponent
                            tintBlue = components.blueComponent
                        }
                        delegate.applySettings()
                    }
                ))

                HStack {
                    Text("Opacity")
                    Slider(value: $tintOpacity, in: 0.05...0.8)
                        .onChange(of: tintOpacity) { _ in delegate.applySettings() }
                    Text("\(Int(tintOpacity * 100))%")
                        .frame(width: 36, alignment: .trailing)
                        .monospacedDigit()
                }
            }

            Divider()

            Toggle("Always on top", isOn: $pinToTop)
                .onChange(of: pinToTop) { _ in delegate.applySettings() }

            HStack {
                Text("Presets")
                    .font(.subheadline).foregroundColor(.secondary)
                Spacer()
                Button("Amber") {
                    tintRed = 0.2; tintGreen = 0.12; tintBlue = 0.0; tintOpacity = 0.3
                    delegate.applySettings()
                }.buttonStyle(.bordered)
                Button("Blue") {
                    tintRed = 0.0; tintGreen = 0.05; tintBlue = 0.2; tintOpacity = 0.25
                    delegate.applySettings()
                }.buttonStyle(.bordered)
                Button("Gray") {
                    tintRed = 0.05; tintGreen = 0.05; tintBlue = 0.05; tintOpacity = 0.35
                    delegate.applySettings()
                }.buttonStyle(.bordered)
            }

            Divider()

            if delegate.isInMoveMode {
                Button("Lock · Click Through") {
                    delegate.exitMoveMode()
                }
                .buttonStyle(.borderedProminent)
                .frame(maxWidth: .infinity)
            } else {
                Button("Unlock · Move & Resize") {
                    delegate.enterMoveMode()
                }
                .buttonStyle(.bordered)
                .frame(maxWidth: .infinity)
            }

            Button("Quit") {
                NSApp.terminate(nil)
            }
            .buttonStyle(.bordered)
            .frame(maxWidth: .infinity)
        }
        .padding()
        .frame(width: 280)
    }
}
