# Phase 0 alpha — manual acceptance record

Record date, tester, Windows version, machine model, CPU/RAM, package SHA-256, app version, and whether a clean machine was used. Use synthetic test text only. Each result is PASS, FAIL, or PENDING with an evidence link; a prior milestone's passing test does not automatically pass this integrated trial.

## Engineering and deployment

| AC | Check | Result |
|---|---|---|
| 01, 27 | Clean checkout CI build; extract package on a second clean Windows 10 2004+ or Windows 11 x64 machine; launch without build tools or key | PENDING |
| 02–05 | First-run onboarding, persistent choices and re-entry; 10–20 minutes of continuous edit, real Rime candidates and Enter raw | PENDING |
| 06–12 | Highlight-linked Shadow, keyboard and mouse selection, phrase, both mixed-language directions, ambiguity and typo recovery | PENDING |
| 13–16 | Sentence Shadow, local-first, stale cancellation and API failure; record real-provider semantics separately from Mock | PENDING — real API credit unavailable |
| 17–20 | Hover quick peek, installed voice / missing-voice message, phonetic provenance, Tab detail | PENDING |
| 21–24 | Explicit Save, encounter count, Private/Secure gates, inspect diagnostic output for input text | PENDING |
| 25–26 | Record P50/P95 and sample count for Primary, local Shadow, resolver, real AI and UI render; inspect ordinary candidate width, long items, ~two-line sentence, Light/Dark, no sustained animation | PENDING |
| 28 | Audit actual package manifest, data paths and accompanying notices; exclude unknown rights | PENDING |
| 29 | Microsoft-Pinyin-scale lexicon, system-wide TSF, store/signing and commercial dictionary are out of scope | NOT IN PHASE 0 |
| 30 | Engineering, UX and Product gates independently decided | PENDING |

## Timing record

Use monotonic timestamps on the same machine. At least 30 synthetic interactions per local path; record P50/P95 in milliseconds, sample count, OS and hardware. Real AI samples must be distinguished from Mock and API failures. Do not invent an SLA or mark unmeasured paths PASS.

| Path | Count | P50 ms | P95 ms | Machine / method |
|---|---:|---:|---:|---|
| Primary | — | — | — | PENDING |
| Local Shadow | — | — | — | PENDING |
| Resolver | — | — | — | PENDING |
| Real AI | — | — | — | PENDING — API credit |
| UI render | — | — | — | PENDING |

## Human trial

For 10–20 minutes type continuously with synthetic Chinese, English, phrases, language gaps and complete sentences. Note Shift+Enter, Tab, Hover and selection friction, latency, candidate usefulness, and whether Shadow is preferable to leaving the editor for translation. Record the exact version and explicit reviewer decisions in `docs/reviews/phase0-gates.md`. Keep any personal text out of evidence files.
