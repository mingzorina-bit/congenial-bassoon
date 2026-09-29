# SDD ledger — plan: docs/plans/v0.0.4-peek.md
Pre-flight: Task 1 expansion/settings -> Task 2 UI consumes snapshot, locale and module order; same interfaces, no frozen behavior conflict.
Ruling: root task is not a Git checkout and native worktree creation previously failed; use isolated version source directory plus dedicated branch, preserving previous trial package. Cost if wrong: baseline history differs locally; remote is stacked on verified v0.0.3 head.
Ruling: Windows shell cannot execute bash-oriented skill helper directly; keep equivalent ledger and named tests in PowerShell. No test or review gate omitted.
Task 1: tests initially failed because DetailState/settings did not exist; implemented and 11/11 PASS.
Task 2: source-approved phonetics provider and catalog tests initially failed for missing implementation; 18/18 PASS. Whole local C# suite 52/52. UI/TTS integration and final evidence remain pending.
Ruling: en-GB IPA data missing; explicit user approval permits honest missing-data trial, but full en-GB coverage is not PASS. Cost: users cannot yet inspect British IPA.
Task 1: complete — immutable expansion/settings tests and local whole C# suite PASS (52/52).
Task 2: complete — phonetic/canonical source, voice locale policy and catalog tests PASS; actual WinUI compile PASS. Actual UI playback evidence belongs to Task 3 delivery validation.
Task 3: GitHub PR4 db2d100 pushed, complete CI pending. Local build launched successfully; computer-use returned user-input-detected/geometry unavailable on interaction, so no UI behavior PASS claimed yet.
Final: review found 2 Important (surface audio invalidation, offscreen Tab card), 0 Critical/Minor. Runtime additionally reproduced popup clipping at viewport bottom.
Final: Ruling: placement regression initially required unexplained 12px gap above hovered target; corrected to require no target overlap and viewport containment, preserving zero-gap pointer traversal. Cost if wrong: visual spacing only.
Final: fixed surface audio invalidation — ReplacementInvalidatesPendingAudio / DelayedAudioCannotPlayAfterTab RED→GREEN, suite 56/56.
Final: fixed offscreen Deep Dive — real 478x445 UI RED (below viewport) → GREEN (card automatically visible), composition preserved.
Final: fixed bottom popup clipping — PeekFitsBottomAndRightEdges RED→GREEN plus actual upper placement, clickable playback and unchanged youhua.
Final: Ruling: reviewer deferred real UI/audio judgement; actual UI verified by implementer, audible quality is for user trial; second machine not claimed. Save/AI/TSF/commercial readiness remain out of scope by frozen plan. Cost if wrong: undetected voice-quality or environment-specific defect.
Final review: 0 Critical / 2 Important fixed / 0 Minor. CI exact final implementation full green. Task 3 awaits final package validation only.
Task 3: complete — full GitHub CI 36591040997 final implementation 62ee466 PASS, 560 package file hashes PASS, official trial process started; real UI validation and all limitations preserved in docs/releases/v0.0.4.md.
