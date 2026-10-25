# Opus 5.5 blind ranking

**Final ranking: 1st A, 2nd B. Confidence: medium.** The corrected images confirm my first ranking. A's score rises slightly, from 83.75 to 84.5. B's scores are unchanged.

## Final scores (0–10; weighted total out of 100)

| Criterion (weight) | A | B |
|---|---|---|
| Product clarity & truthfulness (25%) | 9.0 | 8.0 |
| Interaction usability/discoverability (30%) | 8.5 | 8.0 |
| Visual craft & identity (25%) | 8.0 | 8.5 |
| Responsive & accessibility (15%) | **8.0** (was 7.5) | 7.5 |
| Conversion clarity/trust (5%) | 9.0 | 8.0 |
| **Weighted total** | **84.5** | **80.5** |

## What the corrected evidence changed

- **A desktop-demo (second demo, "Make a test strip"):**
  - The tint box is more clearly separate here than in the hero demo. Its title bar sits above the shared slide's top edge. Its right and bottom edges stop well inside the window, at a different size.
  - This partly answers my earlier concern that A's box looked fitted to the window. The hero demo still has that problem.
  - The 5–80% opacity strip sits under the demo and shows the range at a glance.
  - Clarity and interaction scores stay the same.
- **A desktop-install:**
  - Confirms what I had read from the full-page image: honest "no signed download yet" copy, four numbered steps, and a git clone line with a Copy button.
  - Also a spec card with the exact preset values and first-launch RGB, and the macOS 14 `onChange` warning.
  - No score change. It still deserves the 9 for conversion/trust.
- **A mobile-demo:**
  - The bright slide sits at the top, with dark call tiles and a dark code pane stacked below. Bright area is roughly a third of the demo, which meets the brief.
  - The box is shifted modestly from the slide and is readable at 375px wide.
- **A mobile-controls:**
  - Locked/Unlocked keeps its explanatory sub-labels at phone width. The live geometry readout ("84% × 35%…") and the "Always-on-top and Spaces aren't simulated here" note are both visible.
  - The tap targets look comfortable. **This is why A's responsive score goes up by 0.5.**
- **B mobile-controls:**
  - A clean stack of two toggles, a helper sentence, the opacity slider, and the Amber/Blue/Gray buttons.
  - "A little / A lot" and "Or choose your own" are small and low-contrast.
  - The "Always on top" checkbox is still there, though a web page cannot really show that behavior.
  - No change; it matches my earlier reading.

## Core reasons A ranks first

1. **Lock is easier to discover.** On desktop and mobile alike, A splits lock into two labelled choices: "Clicks pass through" and "Move & resize". It also gives a concrete instruction ("lock it and click View Options through it") and a live status line. B relies on two bare toggles plus a helper sentence. That works but explains itself less well.
2. **Its claims are more truthful and specific.** A has a "What it does / What it doesn't" list and says outright that always-on-top and Spaces aren't simulated. Its spec card matches the brief exactly. B's copy is honest, but its "Always on top" demo checkbox implies a native behavior the page can't actually show.
3. **More behavior is verified by receipt.** QA confirmed click-through in both of A's demos with a status message, lock mode surviving reload, and separate saved positions for wide and narrow layouts. B's receipt covers the main behaviors but less than that. (This is from the receipts, not visible in the images.)

## Where B still leads

- **Visual identity.** One coherent warm charcoal-and-coral palette from top to bottom. It never breaks the page's own dark-mode message.
- **The hero box is clearly separate.** B's hero tint box hangs well past the shared window into the dark panes. That is the clearest single picture of the product's core idea in either candidate.

## One weakness each

- **A:** The first demo, which visitors see first, places the tint box almost exactly over the shared window. The large bright-orange install block also clashes with a product pitched against glare.
- **B:** Much of the secondary text is small and low-contrast: hints, slider labels, "Reset demo", the footer. On mobile, the shared-window copy is cut off at the right edge.

## Limitations

- The corrected A desktop images are somewhat soft or compressed but fully legible.
- Accessibility judgments are visual, plus a markup count I took from the source in the first pass. No assistive-technology testing was done for either candidate.
