import AppKit

let destination = URL(fileURLWithPath: CommandLine.arguments[1])
try FileManager.default.createDirectory(at: destination, withIntermediateDirectories: true)
for size in [16, 32, 128, 256, 512] {
    for scale in [1, 2] {
        let pixels = size * scale
        let image = NSImage(size: NSSize(width: pixels, height: pixels))
        image.lockFocus()
        let side = CGFloat(pixels)
        NSColor(calibratedRed: 0.10, green: 0.115, blue: 0.105, alpha: 1).setFill()
        NSBezierPath(roundedRect: NSRect(x: side * 0.04, y: side * 0.04, width: side * 0.92, height: side * 0.92), xRadius: side * 0.21, yRadius: side * 0.21).fill()
        NSColor(calibratedRed: 0.95, green: 0.74, blue: 0.55, alpha: 1).set()
        NSBezierPath(ovalIn: NSRect(x: side * 0.35, y: side * 0.35, width: side * 0.30, height: side * 0.30)).fill()
        for ray in 0..<8 {
            let angle = CGFloat(ray) * .pi / 4
            let path = NSBezierPath()
            path.lineWidth = max(1, side * 0.045)
            path.lineCapStyle = .round
            path.move(to: NSPoint(x: side * (0.5 + 0.23 * cos(angle)), y: side * (0.5 + 0.23 * sin(angle))))
            path.line(to: NSPoint(x: side * (0.5 + 0.32 * cos(angle)), y: side * (0.5 + 0.32 * sin(angle))))
            path.stroke()
        }
        image.unlockFocus()
        let bitmap = NSBitmapImageRep(data: image.tiffRepresentation!)!
        let suffix = scale == 2 ? "@2x" : ""
        let file = destination.appendingPathComponent("icon_\(size)x\(size)\(suffix).png")
        try bitmap.representation(using: .png, properties: [:])!.write(to: file)
    }
}
