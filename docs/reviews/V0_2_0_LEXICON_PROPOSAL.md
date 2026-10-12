# v0.2.0 broad offline lexicon — approved trial source review

The user wants vocabulary beyond the current authored ~50-entry pinyin fixture. The next useful step is broad offline Primary coverage while preserving the product's current candidate, Shadow and raw-Enter rules. “Full dictionary” is not a measurable finite set; the initial target is a substantially broader, source-audited pinyin corpus with a measured common-word/phrase fixture, followed by separately reviewed bilingual data.

## Approved first import: Rime Luna Pinyin and Rime Essay

| Field | Proposal |
|---|---|
| Source | `rime/rime-luna-pinyin` at commit `56b934b099dfbeab842320f13aa8b461a6ab3e42` (checked 2026-10-12) |
| Files | Unmodified `luna_pinyin.dict.yaml` and separately approved `rime-essay/essay.txt`; own `bilingual` dictionary imports Luna; exact hashes in `data/rime/SOURCE.md` |
| Purpose | Expanded offline Chinese pinyin candidates in librime |
| Terms | Repository `LICENSE` is LGPL-3.0; include its license, attribution and unmodified source files with the prototype package. Audit any imported related data separately. |
| Distribution | Internal/public prototype proposal only. Commercial deployment requires the frozen Data & Dependency Release Review. |
| Fallback | Previous v0.1.1 package remains available if the new corpus fails real-engine tests; v0.2.0 must not ship without both pinned files. |
| Approval state | **APPROVED FOR v0.2.0 TRIAL** by the user on 2026-10-12 for these fixed files/purpose only. Commercial release remains unapproved. |

Acceptance before shipping: `youhua` retains candidate/highlight/raw behavior; common Chinese word/phrase fixture gives broad coverage; candidate ranking and pinyin composition measured; no online request or user-text logging; source ID and license packaged; no unknown file or nested dependency. Review simplified/traditional output before choosing the schema and do not silently change the frozen input preference.

## Bilingual follow-up

The [CC-CEDICT editor](https://cc-cedict.org/editor/editor.php?handler=Download) states CC BY-SA 4.0 terms and recommends the [MDBG verified release](https://www.mdbg.net/chinese/dictionary?page=cc-cedict). On 2026-10-12 MDBG listed a 2026-10-11 release of 125,262 entries. Its page prohibits automated/scripted access, so this proposal does **not** script that download. A separately approved acquisition, pinned hash, attribution/ShareAlike review and distinct source boundary are required before importing its pinyin/Chinese-English fields. The [ECDICT repository](https://github.com/skywind3000/ECDICT) publishes MIT text, but field provenance must be checked before using translation, phonetic, frequency or other fields; no ECDICT payload is proposed for this first import.

The file-level audit found upstream credits to CC-CEDICT and other projects in the Luna header, and the separate `rime/rime-essay` preset vocabulary required for common words and ranking. On 2026-10-12 the user explicitly approved retaining the Luna file unchanged with full credits for trial use and separately approved `rime/rime-essay` `essay.txt` at commit `054920de4f54c9e5994276a96a4fc2a35cb51aa3`, also LGPL-3.0, for trial use. Exact SHA-256 values, package paths and upstream author notices are recorded in `data/rime/SOURCE.md`; hashes are checked before build and after package creation. CC-CEDICT and ECDICT remain separate decisions. Commercial distribution remains unapproved.
