# Sunroom versus Safelight — blind design comparison

**Split vote. Both judges reported medium confidence.**

| Blind judge | Sunroom, current | Safelight, Opus alternative | First place |
|---|---:|---:|---|
| Astra | 87.2 / 100 | 85.9 / 100 | Sunroom |
| Opus 5.5 | 80.5 / 100 | 84.5 / 100 | Safelight |

Both judges favored Sunroom’s visual coherence and conspicuously independent tint box. Both favored Safelight’s explicit Locked/Unlocked explanations and honest source-build CTA. Astra gave Sunroom the overall edge for mobile composition; Opus gave Safelight the overall edge for control discoverability and product explanation.

## Versions

- **Sunroom:** the current Astra-led design, refined with the user during the session. Baseline source commit `6e80a1f`; current code remains in `site/dist/`. Preview: http://localhost:5186/.
- **Safelight:** an independent Claude Opus 5.5 implementation in `site-opus/dist/`, committed as `728ab07`. Preview: http://localhost:5188/.

The current page was not replaced. Both are buildless HTML/CSS/JS. Safelight’s local Archivo font license was added during integration; its rendered design and interaction code were not changed after authoring.

## Method

1. Opus received a clean isolated checkout containing the native Swift source, a requirement brief and optional font files. It was instructed not to inspect the existing implementation, screenshots or history. The author model was verified from session metadata as `claude-opus-5-5`.
2. The current design was frozen separately. Anonymous labels were assigned **A = Safelight**, **B = Sunroom**, and model/author identifiers were checked absent from the supplied page sources and evidence brief.
3. Fresh independent judges were dispatched: Astra (`gpt-6-astra`, no parent conversation) and a separate Opus 5.5 session. Neither received author identities or the other judge’s findings. Astra examined A first; Opus examined B first.
4. Both received the same requirements, five weighted criteria, desktop/mobile screenshots, frozen source and browser QA receipts. Requested viewports were 1440×900 and 390×844; actual capture sizes vary slightly with each page’s scrollbar styling. One good demo was sufficient; quantity and page length were not rewarded.
5. Three A section screenshots were discovered to have landed mid-scroll. They were corrected after confirming settled section positions, and mobile control-panel images were added for both versions. Source, rubric and behavior receipts were unchanged. Both judges then confirmed their final rankings while still blind. Astra’s scores stayed unchanged; Opus raised A’s responsive score from 7.5 to 8.0. Initial scores are retained for the audit trail.
6. Identities were decoded only after the final rankings. A one-vote-each result is reported directly; no average of independently calibrated subjective scores is presented as an objective winner.

## Rubric

Product clarity/truthfulness 25%; interaction usability/discoverability 30%; visual craft/identity 25%; responsive layout/accessibility 15%; conversion clarity/trust 5%. Each criterion scored 0–10, converted to a weighted total out of 100.

## Reports and evidence

- [Astra’s final report](reviews/astra-final.md)
- [Opus 5.5’s final report](reviews/opus-final.md)
- [Initial scores before capture correction](reviews/first-pass-scores.json)
- [Final structured scores](scores.json)
- [Common blind brief](blind-evidence/REVIEW.md)
- [Candidate A behavior evidence](blind-evidence/A/QA.md)
- [Candidate B behavior evidence](blind-evidence/B/QA.md)
- Frozen source and all 16 final screenshots are in `blind-evidence/A/` and `blind-evidence/B/`.
- [Evidence hashes](evidence-sha256.json)

## Limits

These are two subjective model judgments, not a user study. Visual evidence was inspected directly; functional claims rely on the parent’s browser checks. Physical touch hardware, screen readers, native installation, OS-level window stacking and Spaces were not verified. Some full-page images were reduced and some A captures were soft; the judges stated those limits. Safelight’s native macOS14 compatibility note was not independently compiled during this comparison.
