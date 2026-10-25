# Candidate A — behavior evidence

Captured with requested browser viewports 1440×900 and 390×844; screenshots show actual browser capture dimensions. Default page, desktop demo, mobile views and installation presentation are included. Source files accompany the images. No visual scores are assigned here.

Verified in the browser:
- Dark default; bright document/slide content occupies roughly 28–29% of the desktop scenes, excluding the call’s dark controls. Each initial overlay is offset and differently sized from its underlying window.
- Pointer dragging the primary box title bar changed x/y by the requested delta. Arrow keys moved and resized the box.
- Lock removed the handles and made the overlay click-through. View Options in the primary demo and Request control in the second demo both worked under the locked overlay, with an explicit click-through status confirmation.
- Show/hide retained box x/y/width/height. The lock mode could be changed while hidden, and showing it restored the independent mode and geometry.
- Presets, custom color, 5–80% opacity and the clickable opacity strip changed the visual state. Custom color, opacity, geometry and lock mode survived reload.
- Two demo instances have separate controls and saved state. Wide/narrow layouts store separate placements. Mobile controls and click-through were checked.
- Install CTA navigated to inline source/Xcode instructions, explicitly stating no signed download is supplied.
- No horizontal overflow at the requested mobile width. No browser console errors observed in the functional checks. JavaScript syntax check passed.
- Always-on-top and Spaces are explicitly not simulated by this page; no demo control for those is offered.

Limits: physical touch hardware, assistive technology, actual native-app installation, OS-level window stacking and Spaces were not exercised. The build section’s macOS14 onChange compatibility note has not been independently compiled/verified. These are webpage demonstrations, not the native app itself.

Capture update: corrected A’s desktop-demo, desktop-install and mobile-demo frames after confirming settled section positions. Added mobile-controls.jpg for both candidates. Page source and behavior evidence are unchanged.
