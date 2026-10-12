# v0.2.1 human trial and deferred input improvements

On 2026-10-12 the user tested the v0.2.1 Windows trial in the actual editor and confirmed `beijing`, `shanghai`, `diannao`, and `beijingdiqu` now work. Screenshots also show prefix predictions such as `beij` and `beijingdiq`, the authored English Shadow, and the bounded detail rows. This closes the user-observed v0.2.0 interaction regression for these four examples; it is not a claim that the whole Phase 0 acceptance matrix or broad bilingual data gate has passed.

The following are new trial findings, not v0.2.x blockers or approved frozen-behavior changes:

1. **Internal abbreviations / imperfect pinyin.** `beijingdiq` predicts 北京地区, but `beijdiqu` and `beijingdq` do not. Explore syllable-aware abbreviation and bounded fuzzy matching only after measuring false positives against English words and existing exact/prefix results. Never silently auto-commit a guessed phrase. Test the three exact inputs above plus English passthrough.
2. **More candidates and paging.** The current candidate UI displays at most five results, consistent with the Phase 0 compact-card target. A future change should page the available Rime candidates without changing Tab's frozen Deep Dive action or Space/Enter semantics. Test page navigation, selection indices, Shadow tracking, and return to page one on a new composition.
3. **Broader Details coverage.** The four authored extension rows are present, but other Rime Chinese candidates have no bilingual explanation; English IPA is also absent for some entries. Expand only from separately reviewed source data with per-source IDs, attribution and release gates. Do not synthesize definitions from the pinyin dictionary or label missing content as complete.

These three items require their own scoped milestone and acceptance decision. The already agreed next stage is Phase 1-A TSF feasibility, which asks whether the existing Core can work in external editors; it does not promise these improvements in the TSF spike.

