# CMUdict limited prototype subset

- sourceId: `cmudict-7479086`; DATA, PROTOTYPE_ONLY; user approval 2026-09-29.
- Upstream: https://github.com/cmusphinx/cmudict/tree/74790861f652b15e4ac49015a90074ad62a27690
- cmudict.dict SHA256: `81917843C7F44CE2B094AC63873C2C7A4CF802040792C455BA3CA406891C3D22`
- LICENSE SHA256: `BD4CE8E44170A5F9F481310CA85C51DE3C4F851A65E679B40E603B143BD3542A`
- License: BSD-style 2-clause Carnegie Mellon terms, preserved at `licenses/CMUdict-LICENSE.txt` in source/package. Copyright (C) 1993-2015 Carnegie Mellon University.
- Redistribution/modification/commercial use: permitted subject to notices and disclaimer; no additional grant for unrelated sources. Final commercial data review remains required.
- Derived work: extract first dictionary pronunciation for 25 prototype word forms using `scripts/Extract-Phonetics.ps1`; preserve the ARPAbet tokens unmodified. The text subset is the editable source distributed with the app.
- Rendering: deterministic broad phonemic ARPAbet→IPA in `PhoneticDataProvider`; AH0→ə, ER0→ɚ. Stress is placed before a conservative common onset. This is not a separately reviewed dictionary transcription or an LLM output. Source variants are not silently corrected: CMUdict refine uses AH0, rendered /rəˈfaɪn/, unlike the interaction spec's illustrative /rɪˈfaɪn/.
- Locale: en-US only. en-GB returns missing, never an en-US fallback. Phrase IPA is not constructed by concatenating words. Windows TTS may pronounce entire phrases independently of IPA availability.
- Runtime/cache/shipping: local read-only TSV shipped in its own directory; no input/audio cache or network download. No underlying data is merged into the self-authored bilingual catalog.
- Quality limitation: 25 selected word forms, first variant only, broad conversion, no claim of complete IPA coverage. Prototype user trial approval does not constitute a linguistic or commercial legal audit.
