# Post-alpha development sequence

The user's 2026-10-12 trial feedback authorizes CR-06 (Tab closes an open Details card) and requests broader vocabulary plus input in other applications. These are separate milestones. The v0.1.0-alpha Engineering/UX/Product gates remain pending where recorded; this roadmap does not retroactively pass them or change the frozen Phase 0 scope.

## v0.1.1 — Tab interaction trial (current)

- Allowed: CR-06 keyboard handling, on-screen hint, regression instructions, versioned trial package.
- Excluded: new dictionaries, TSF registration, global keyboard hooks, additional AI behavior.
- Acceptance: with active composition, Tab opens and the next plain Tab closes; Esc closes; focus returns to editor; composition/highlight/editor text/Shadow unchanged. Details-button focus also permits plain Tab close. Shift/Ctrl/Alt+Tab keep their existing meaning. Existing Core, Host, real Rime and packaging CI pass.
- Done when CI passes and a downloadable, audited Windows package is available for the user's trial. The user decides whether CR-06 feels right before further keyboard changes.

## v0.2.0 — reviewed vocabulary expansion (next data milestone)

- Goal: replace the current roughly 50-entry authored pinyin fixture with a useful broad offline pinyin vocabulary and expand bilingual lookups without mixing source boundaries.
- First gate: approve a **concrete** source/version/file set and redistribution terms under A-18. The existing librime approval does not approve a Rime dictionary. CC-CEDICT is CC BY-SA 4.0 with attribution and ShareAlike obligations; its verified release is recommended by its editor. ECDICT's repository publishes MIT terms, but field-level provenance still needs review. Rime Luna Pinyin publishes LGPL-3.0 terms and requires a separate packaging review. Do not import any of these by default.
- Implementation after approval: reproducible download/pin/hash, source-preserving import and normalization, separate datasets with source IDs, attribution in package, no unreviewed IPA/audio/definitions, graceful fallback when a source is absent. Measure candidate coverage, ordering and lookup latency using a fixed common-word/phrase fixture; retain Enter raw, local-first and existing Shadow behavior.
- Excluded: a promise of Microsoft-Pinyin-equivalent ranking or unrestricted commercial redistribution. A commercially distributed lexicon requires the frozen release review.

## Phase 1-A — TSF feasibility spike (after a trial-ready lexicon)

- Goal: prove actual input in at least two external Windows text editors through a minimal Text Services Framework (TSF) service, reusing the current Core session. Microsoft's TSF architecture describes a COM in-process text service mediated by the TSF manager; custom IME guidance specifies TSF and host-compatibility constraints.
- Probe: composition, candidate display, keyboard interception, highlighted state, commit/cancel, focus transitions and user-controlled enable/disable. Test ordinary editors and an unsupported/secure field separately; never claim password-field detection from the Phase 0 simulation.
- Gate: review registration/unregistration, signing and installer permissions before installing a text service on the user's machine. No global keyboard hook or background injector is a substitute for TSF. The spike is not a releasable system-wide IME until host compatibility, privacy, uninstall and deployment tests pass.

## Source references for the next approval review

- [CC-CEDICT download and CC BY-SA 4.0 statement](https://cc-cedict.org/editor/editor.php?handler=Download)
- [ECDICT repository and published license](https://github.com/skywind3000/ECDICT)
- [Rime Luna Pinyin repository and license](https://github.com/rime/rime-luna-pinyin)
- [Microsoft TSF architecture](https://learn.microsoft.com/en-us/windows/win32/tsf/architecture)
- [Microsoft custom IME requirements](https://learn.microsoft.com/en-us/windows/apps/design/input/input-method-editor-requirements)
