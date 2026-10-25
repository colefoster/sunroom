# Candidate B — behavior evidence

Captured with requested browser viewports 1440×900 and 390×844; screenshots show actual browser capture dimensions. Default page, desktop demo, and mobile views are included. Source files accompany the images. No visual scores are assigned here.

Verified in the browser:
- Dark default; bright shared content occupies roughly 30–33% of each scene. The initial overlay is offset and differently sized from the underlying window.
- Pointer dragging the primary box title bar changed x/y by the requested delta. Arrow keys moved and resized the box.
- Lock changed the overlay to pointer-events:none. The real checkbox underneath could then be clicked.
- Show/hide retained box x/y/width/height. Lock could be toggled while hidden, then showing the box restored the independent mode and geometry.
- Presets, custom color and 5–80% opacity controls changed the visual state. Custom color, opacity and geometry survived reload.
- Separate demo instances have separate state and controls. Hero show/lock and keyboard movement were also checked.
- Install CTA opened source/Xcode instructions, explicitly stating no packaged download is supplied.
- No horizontal overflow at the requested mobile width. No browser console errors observed in the functional checks. JavaScript syntax check passed.

Limits: physical touch hardware, assistive technology, actual native-app installation, OS-level window stacking and Spaces were not exercised. These are webpage demonstrations, not the native app itself.

Capture update: corrected A’s desktop-demo, desktop-install and mobile-demo frames after confirming settled section positions. Added mobile-controls.jpg for both candidates. Page source and behavior evidence are unchanged.
