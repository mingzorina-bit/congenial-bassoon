# v0.2.0 broad offline lexicon — dependency proposal (not yet approved)

The user wants vocabulary beyond the current authored ~50-entry pinyin fixture. The next useful step is broad offline Primary coverage while preserving the product's current candidate, Shadow and raw-Enter rules. “Full dictionary” is not a measurable finite set; the initial target is a substantially broader, source-audited pinyin corpus with a measured common-word/phrase fixture, followed by separately reviewed bilingual data.

## Proposed first import: Rime Luna Pinyin

| Field | Proposal |
|---|---|
| Source | `rime/rime-luna-pinyin` at commit `56b934b099dfbeab842320f13aa8b461a6ab3e42` (checked 2026-10-12) |
| Files | `luna_pinyin.dict.yaml` plus only the schema/configuration files required to use it; exact file inventory and hashes recorded before import |
| Purpose | Expanded offline Chinese pinyin candidates in librime |
| Terms | Repository `LICENSE` is LGPL-3.0; include its license, attribution and unmodified source files with the prototype package. Audit any imported related data separately. |
| Distribution | Internal/public prototype proposal only. Commercial deployment requires the frozen Data & Dependency Release Review. |
| Fallback | Existing authored schema/lexicon remains selectable until the new corpus passes real-engine tests. |
| Approval state | **PENDING** — no source file has been added to build, package or repository. |

Acceptance before shipping: `youhua` retains candidate/highlight/raw behavior; common Chinese word/phrase fixture gives broad coverage; candidate ranking and pinyin composition measured; no online request or user-text logging; source ID and license packaged; no unknown file or nested dependency. Review simplified/traditional output before choosing the schema and do not silently change the frozen input preference.

## Bilingual follow-up

The [CC-CEDICT editor](https://cc-cedict.org/editor/editor.php?handler=Download) states CC BY-SA 4.0 terms and recommends the [MDBG verified release](https://www.mdbg.net/chinese/dictionary?page=cc-cedict). On 2026-10-12 MDBG listed a 2026-10-11 release of 125,262 entries. Its page prohibits automated/scripted access, so this proposal does **not** script that download. A separately approved acquisition, pinned hash, attribution/ShareAlike review and distinct source boundary are required before importing its pinyin/Chinese-English fields. The [ECDICT repository](https://github.com/skywind3000/ECDICT) publishes MIT text, but field provenance must be checked before using translation, phonetic, frequency or other fields; no ECDICT payload is proposed for this first import.

Approval request for the next milestone: authorize only the fixed Rime Luna Pinyin source/version/file purpose above for a trial package after its file-level audit. CC-CEDICT and ECDICT remain separate decisions. If LGPL obligations are not acceptable, retain the authored data while selecting a different reviewed source; do not silently substitute one.
