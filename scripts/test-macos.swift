import AppKit
import SwiftUI

@main
struct ReleaseChecks {
    static func main() {
        let app = NSApplication.shared
        app.setActivationPolicy(.accessory)
        let delegate = AppDelegate()
        delegate.overlayEnabled = false
        delegate.overlayX = 100
        delegate.overlayY = 100
        delegate.overlayW = 600
        delegate.overlayH = 400
        delegate.setupOverlayWindow()
        let window = delegate.overlayWindow!
        var checks = 0
        func check(_ condition: Bool, _ description: String) {
            guard condition else { fputs("FAIL: \(description)\n", stderr); exit(1) }
            checks += 1
            print("PASS: \(description)")
        }
        check(!window.isVisible, "Hidden startup is respected")
        check(window.ignoresMouseEvents, "Startup is locked and click-through")
        check(!window.isReleasedWhenClosed, "Closing cannot release the reusable overlay")
        delegate.overlayEnabled = true
        delegate.applySettings()
        check(window.isVisible, "Show tint reveals overlay")
        delegate.enterMoveMode()
        check(!window.ignoresMouseEvents && window.styleMask.contains(.titled), "Unlock exposes a movable titled window")
        let moved = NSRect(x: 180, y: 160, width: 640, height: 420)
        window.setFrame(moved, display: true)
        delegate.windowDidMove(Notification(name: NSWindow.didMoveNotification))
        delegate.windowDidResize(Notification(name: NSWindow.didResizeNotification))
        check(delegate.overlayX == 180 && delegate.overlayW == 640, "Position and size are saved")
        delegate.tintOpacity = 0.5
        delegate.applySettings()
        check(window.contentView?.layer?.borderWidth == 2, "Changing tint preserves the unlocked frame")
        check(abs((window.contentView?.layer?.backgroundColor?.alpha ?? 0) - 0.5) < 0.01, "Tint strength reaches the overlay")
        window.performClose(nil)
        check(!window.isVisible && !delegate.overlayEnabled, "Window close hides instead of destroying the overlay")
        check(delegate.isInMoveMode, "Hiding preserves unlocked mode")
        delegate.overlayEnabled = true
        delegate.applySettings()
        check(window.isVisible && window.frame == moved, "Showing restores geometry")
        delegate.exitMoveMode()
        check(window.ignoresMouseEvents && !window.styleMask.contains(.titled), "Lock restores click-through and removes editing title bar")
        check(window.contentView?.layer?.borderWidth == 0, "Locked tint has no editing border")
        delegate.pinToTop = false
        delegate.applySettings()
        check(window.level == .floating, "Always-on-top setting uses the lower floating level")
        delegate.pinToTop = true
        delegate.applySettings()
        check(window.level == .screenSaver, "Always-on-top setting restores the higher overlay level")
        window.setFrameOrigin(NSPoint(x: -100000, y: -100000))
        delegate.restoreVisibleFrame()
        check(NSScreen.screens.contains { $0.visibleFrame.intersects(window.frame) }, "Disconnected or off-screen geometry is recovered")
        check(window.collectionBehavior.contains(.canJoinAllSpaces), "Overlay joins all Spaces")
        window.orderOut(nil)
        NotificationCenter.default.removeObserver(delegate)
        print("\(checks) macOS release checks passed")
    }
}
