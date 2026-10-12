# Offline pinyin data provenance — v0.2.0 trial

The package keeps two third-party source files unchanged and separate from project-authored `bilingual.dict.yaml`:

| source_id | File | Pinned upstream commit | SHA-256 |
|---|---|---|---|
| `rime-luna-pinyin` | `luna_pinyin.dict.yaml` | [`rime/rime-luna-pinyin@56b934b`](https://github.com/rime/rime-luna-pinyin/tree/56b934b099dfbeab842320f13aa8b461a6ab3e42) | `75BCF6EB3FF62B129882ED89CC22B2D80A5347AA72BCFA2CCC839BAC298E7314` |
| `rime-essay` | `essay.txt` | [`rime/rime-essay@054920d`](https://github.com/rime/rime-essay/tree/054920de4f54c9e5994276a96a4fc2a35cb51aa3) | `3E8512EBAA6657E35961B1F5762060717C647E3BF471D5E3B86E783B4E934FFB` |

Both upstream repositories publish LGPL-3.0. The package includes `licenses/rime-luna-pinyin-LGPL-3.0.txt` and each upstream `AUTHORS` file. The Luna dictionary header also credits CC-CEDICT and other projects; the essay and Luna `AUTHORS` files identify refined sources. These credits are retained without claiming item-level provenance. The user approved both fixed versions and this attribution for a **trial package only** on 2026-10-12. Commercial distribution requires the separate Data & Dependency Release Review.

`bilingual.dict.yaml` imports Luna and keeps project-authored mappings at high priority. Rime uses `essay.txt` as preset vocabulary and frequency data. The native adapter converts candidate output to simplified Chinese with the Windows NLS API; this does not modify the bundled original files. No OpenCC data tables, translation definitions, IPA, audio, CC-CEDICT download or ECDICT files were added in this milestone. The bilingual Shadow still uses `data/lexical/own-v0.0.2.tsv`, so broad pinyin coverage does not imply broad English translation coverage.
