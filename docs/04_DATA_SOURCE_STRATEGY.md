> 状态：FROZEN v0.1 · 根据已确认原对话整理，原有第一人称及示例为规格说明。CR-01～CR-05 的澄清优先，见 [变更记录](CHANGE_REQUESTS.md)。
> 来源：source/S4.md；可选、SPIKE、未来能力保留原有性质，不能因冻结而变成必做。

# 04 — Data Source Strategy v0.1

这一份要解决六类完全不同的数据：

```text
Primary/Pinyin
Bilingual Lexicon
IPA / Pronunciation Data
Audio
Examples
AI / Contextual Expression
```

它们不能被笼统叫成“词典”。

---

## D-01｜建立 Source Manifest

所有进入产品的第三方数据必须登记：

```text
source_id
name
version / snapshot date
official source
purpose
license
attribution requirement
commercial use
modification right
redistribution right
cache right
ship-with-app right
derived-data restriction
review status
```

任何字段不清楚：

> **不得因为 Phase 0 图方便而默认进入未来 Release。**

尤其禁止 Codex：

抓网页词典、下载来源不明 MP3、从 GitHub 随便找 CSV、让 LLM 批量生成 IPA 后当权威数据、复制其他输入法词库但不检查其数据许可证。

**D-01：FROZEN。**

---

# D-02｜把代码许可证与数据许可证分开

这是非常重要的工程规定。

例如：

```text
librime code license
≠
Rime schema license
≠
Rime dictionary data license
≠
third-party vocabulary data license
```

同样：

```text
dictionary app source code license
≠
dictionary content license
```

所以以后 Source Manifest 至少区分：

> `CODE / DATA / AUDIO / MODEL / SERVICE`

不能看到 GitHub 仓库写 MIT/BSD，就推断里面几百 MB 词典数据也都是 MIT/BSD。

**D-02：FROZEN。**

---

# D-03｜Pinyin / Primary 来源

Phase 0：

> **librime engine + 单独审核过的 schema/dictionary。**

我们此前已经冻结 librime 本身作为 Primary Engine 的技术方向。

但这次再增加一道门：

> librime GO ≠ 所有 Rime 词库自动 GO。

未来任何：

专业词库、网络流行词库、用户词库、第三方 schema

都独立登记来源。

Phase 0 不追求“微软拼音级百万词库”，只需要足够真实地测试：

候选排序、高亮切换、Shadow 联动、短语和句子行为。

**D-03：FROZEN。**

---

# D-04｜本地双语词典不能只有一个来源

我建议我们的内部数据层叫：

> **Lexical Store**

而不是：

> ECDICT Database。

数据最终统一成自己的 schema：

```text
LexicalEntry
{
    id
    lemma
    sourceLanguage
    targetLanguage

    senses[]
    translations[]
    pos[]

    pronunciations[]
    frequency

    sourceId
    sourceEntryId
    licenseClass
}
```

也就是说：

```text
CC-CEDICT ──┐
            │
ECDICT ─────┼──→ Import / Normalize ──→ Lexical Store
            │
Licensed ───┤
Dictionary  │
            │
Own Data ───┘
```

**数据来源仍然可追踪，不能 normalize 后把 provenance 丢掉。**

**D-04：FROZEN。**

---

# D-05｜CC-CEDICT：可用于实验，但设 License Boundary

CC-CEDICT 官方目前明确采用 **CC BY-SA 4.0**，并明确允许商业和非商业使用，但要求 attribution；如果对数据进行改进/添加，相关变化涉及 ShareAlike。

所以我的结论不是：

> ❌ 不能用

也不是：

> ✓ 随便打包进闭源产品

而是：

> **PHASE 0 APPROVED / COMMERCIAL RELEASE REQUIRES LICENSE REVIEW**

特别需要避免把：

```text
CC-CEDICT
+
我们的 proprietary lexical improvements
+
用户学习出来的词典数据
```

毫无边界地混成一个数据库，然后才考虑 ShareAlike 的影响。

更安全的架构是：

```text
BaseCCCEDICTStore
       ↓ read
Lexical Resolver
       ↑
ProprietarySupplementStore
```

逻辑合并，数据来源保持分离。

**D-05：FROZEN。**

---

# D-06｜ECDICT：进入候选，但暂不批准商业 Release

ECDICT 很吸引我们，因为它定位就是 English→Chinese dictionary database，而且仓库包含 CSV/SQLite 类数据，对离线 Shadow、词性、音标、频率等需求很接近。当前主仓库确实公开提供数据及 LICENSE。

但我不建议现在写：

> “ECDICT = 商业版正式词典。”

原因不是它一定不能用，而是：

> **聚合型词典数据库必须继续核查每类字段的数据 provenance。**

我们真正需要回答：

```text
translation 从哪里来？
phonetic 从哪里来？
frequency 从哪里来？
examples 从哪里来？
各字段是否具有相同授权？
```

所以：

> **ECDICT = PHASE 0 CANDIDATE / COMMERCIAL SPIKE**

而不是 Release Frozen Dependency。

**D-06：FROZEN。**

---

# D-07｜Phase 0 本地词典方案

综合开发效率与未来风险，我建议 Phase 0：

```text
LocalLexicalStore
│
├── Curated Prototype Lexicon
│
├── reviewed CC-CEDICT subset/import
│
└── reviewed ECDICT subset/import
```

注意这里的目标不是：

> 把两个数据库混起来变成“我们的词典”。

而是建立：

> **import pipeline + source provenance + normalization + ranking。**

我们甚至应该故意选一个规模有限但覆盖测试案例的集合。

例如覆盖：

```text
优化
方案
处理
设计
改进
完成
问题
计划
工作
学习
...
```

再增加足够真实的高频词用于连续体验。

等 UX 成立，再解决商业级 lexical corpus。

这能避免花几个月先造词典，结果后来发现 Shadow UX 本身需要大改。

**D-07：FROZEN。**

---

# D-08｜IPA 必须是独立数据

正式规定：

> **LLM 不是 canonical IPA source。**

LLM 可以解释：

> `/θ/` 怎么发。

但不能：

```text
word
↓
Ask LLM "give IPA"
↓
存数据库
↓
当标准发音
```

IPA 数据模型：

```text
PronunciationEntry
{
    lexicalEntryId
    locale
    notation
    value
    sourceId
}
```

例如：

```text
word = "schedule"

locale = en-US
IPA = ...

locale = en-GB
IPA = ...
```

这样以后不同 pronunciation source 可以共存。

**D-08：FROZEN。**

---

# D-09｜美式 / 英式不能只换 Audio

这是一个容易被忽略的问题。

用户设置：

> English pronunciation = American / British

以后影响的应该同时包括：

```text
IPA
Audio Voice
Pronunciation examples
```

而不只是换 TTS 声音。

默认 Phase 0：

> **American English**

Settings：

> American English  
> British English

UI 默认不同时展示两套。

保持 Hover：

```text
refine
/rɪˈfaɪn/       🔊
v. 改进；完善
```

而不是：

```text
US 🇺🇸 ...
UK 🇬🇧 ...
US 🔊 UK 🔊
```

**D-09：FROZEN。**

---

# D-10｜Audio 采用三级 fallback

这里是这次验证后我最建议正式确定的方案：

```text
          Pronunciation Audio
                  │
       ┌──────────┴──────────┐
       ↓                     ↓
Recorded Lexical Audio      TTS
(if licensed)                │
                      ┌──────┴──────┐
                      ↓             ↓
                  Windows        Cloud
                  Local TTS       TTS
```

优先级不一定永远固定；Service 根据用途选择。

### Word

未来如果我们取得高质量授权真人录音：

> Recorded audio 优先。

### Phrase / Sentence

通常：

> TTS。

因为真人词典不可能录下任意：

> `I think this approach could be further improved.`

**D-10：FROZEN。**

---

# D-11｜Windows 本地 TTS 正式进入 Phase 0

这是我认为我们可以比之前更积极使用的能力。

Windows 的 `Windows.Media.SpeechSynthesis.SpeechSynthesizer` 可以使用系统已安装的 Microsoft-signed voices，并能枚举 `AllVoices`、选择具体 voice、根据语言/地区生成语音；同时支持普通文本和 SSML 合成。

这意味着 Phase 0 完全可以：

```text
IPronunciationAudioProvider
       │
WindowsSpeechProvider
```

做到：

> 🔊 点击即可发音

而不要求用户先配置 Azure Key。

优势非常适合我们的 Local First：

```text
No API key
No per-click network request
Low latency
Works offline if suitable voice installed
No sentence sent to cloud
```

但我们不能假设每台 Windows 都安装了我们想要的 locale voice，因此必须检测 `AllVoices`，没有合适 voice 时再 fallback。微软文档也明确说明每个 voice 对应特定语言/地区，并可查询安装 voice。

因此：

> **Windows Local TTS = Phase 0 default/fallback foundation。**

这是我这一轮建议正式新增的技术决定。

**D-11：FROZEN。**

---

# D-12｜Cloud TTS：Azure 第一 PoC，但不绑定

Azure Speech TTS 可以处理普通文本/SSML并提供神经语音，计费按成功处理请求的字符计算。

所以 Cloud Provider：

```text
ICloudAudioProvider
        │
 AzureSpeechProvider
```

可以成为我们的第一个高质量 Cloud PoC。

但是：

> **不是 Bilingual Input 的永久依赖。**

未来可以：

```text
Windows Local
Azure Speech
Google TTS
Licensed Recorded Audio
Other provider
```

全部通过同一个 Pronunciation Service。

Phase 0 第一阶段甚至可以先把 Windows Local TTS 做好，再加入 Azure quality comparison。

这样我们还能真正做一次盲听：

> Local Windows voice 是否已经足够完成 Hover 学习需求？

如果答案是“够”，我们就没有必要让每一次 `🔊` 都产生网络调用。

**D-12：FROZEN；最终 Cloud Vendor = SPIKE。**

---

# D-13｜Audio Cache 要非常谨慎

之前我们说：

> TTS 生成一次以后缓存。

方向没错，但现在我要加一个条件：

> **能否持久缓存由 Provider Terms 决定。**

所以 Audio Cache API 不能默认：

```text
download MP3
save forever
```

而是：

```text
AudioCachePolicy
{
    memoryAllowed
    diskAllowed
    maxTTL
    redistributionAllowed
}
```

例如 Windows 本地生成音频和商业云服务生成音频，权利条件可能完全不同。

Provider 自己声明：

```text
CachePolicy
```

Infrastructure 执行。

**D-13：FROZEN。**

---

# D-14｜例句来源独立管理

这又是一个版权雷区。

词典有：

> `refine = improve`

并不意味着我们自动拥有它网站上的：

> 20 条例句。

因此 Examples 单独定义：

```text
ExampleProvider
```

Phase 0：

> **不需要大型商业例句库。**

我们可以采用：

经过许可的数据、非常有限的自有测试例句，以及 AI 按需生成的学习例句。

但是 AI 生成例句必须标识：

```text
source = generated
```

不能伪装成“权威词典例句”。

Deep Dive 里的 Examples 也不是 Phase 0 核心验收项。

**D-14：FROZEN。**

---

# D-15｜Definition 和 Translation 分开

例如：

```text
refine
```

有两个不同问题：

> 它是什么意思？

和：

> 当前中文 `优化` 应该对应哪个 English word？

所以数据模型不要只有：

```text
translation: "优化"
```

应该有：

```text
Sense
Translation
Definition
POS
Usage
```

Shadow Ranking 主要消费：

> Translation + Sense + Context。

Learning Quick Peek 可以消费：

> Meaning + POS + IPA。

这样以后换更好的 Dictionary Provider，不需要推翻 Shadow。

**D-15：FROZEN。**

---

# D-16｜Natural Expression 来源

这一层明确：

> **不是词典负责。**

例如：

```text
我觉得这个方案还可以继续优化
```

→

> I think this approach could be further improved.

属于：

```text
IExpressionProvider
```

Phase 0：

> 一个真实 LLM Provider + 一个 Mock Provider。

但 Core 不知道供应商是谁。

而且请求必须携带最少必要 context：

```text
source expression
dominant language
target language
requested operation
minimal relevant context
```

不是默认把用户前面整个聊天窗口上传。

**D-16：FROZEN。**

---

# D-17｜用户词典与第三方词典严格分离

未来用户可能：

> 收藏 `approach`

或者频繁选择：

> `方案 → approach`

这些属于：

```text
UserLexicalMemory
```

不能写回：

```text
CCCEDICT.db
```

正确结构：

```text
ThirdPartyLexicalStore
          │
          ↓
     Lexical Resolver
          ↑
          │
UserLexicalMemory
```

这样：

用户数据、第三方 ShareAlike 数据、我们 proprietary ranking data

不会混成一锅。

**D-17：FROZEN。**

---

# D-18｜最终 Source Strategy

所以 Phase 0 我建议正式采用：

| 能力 | Phase 0 | 商业版方向 |
|---|---|---|
| Pinyin Engine | librime | librime / own enhancements |
| Pinyin Lexicon | audited Rime data | audited/licensed |
| ZH→EN | CC-CEDICT + curated | reviewed/licensed hybrid |
| EN→ZH | ECDICT candidate + curated | commercial/provenance-reviewed source |
| IPA | reviewed lexical data | licensed/high-confidence phonetic source |
| Word Audio | Windows TTS initially | recorded + TTS hybrid |
| Phrase Audio | Windows TTS | local/cloud TTS |
| Sentence Audio | Windows TTS / optional cloud | TTS |
| Examples | minimal/generated | licensed + generated |
| Natural Expression | LLM Provider | multi-provider architecture |
| AI fallback | Mock/local result | provider-independent |

注意这里的：

> ECDICT candidate

意味着**第四轮批准其作为 Prototype research source，不等于批准最终商业发行。**

而 CC-CEDICT 的 BY-SA 条件必须保持数据边界和 attribution；官方目前明确说明其数据允许商业使用，但附带 attribution/share-alike 条件。

---

# D-19｜Release Gate

我建议商业 Alpha 前增加一个不能跳过的：

> **Data & Dependency Release Review**

必须回答：

```text
Can distribute?
Can use commercially?
Must attribute?
Must share modifications?
Can cache?
Can derive?
Can combine?
Can redistribute audio?
Can ship offline?
```

任何：

> `UNKNOWN`

都不能进入正式安装包。

Phase 0 可以有：

```text
PROTOTYPE_ONLY
```

标签。

Codex 也必须知道：

> `PROTOTYPE_ONLY` 数据不得被误认为 production dependency。

**D-19：FROZEN。**

---


## 冻结复核补充

CC BY-SA 数据保持独立 source boundary；允许商业使用不等于可随意混入 proprietary corpus。WindowsSpeechProvider 冻结的是能力与降级路线，不承诺目标机器已安装指定英语 voice。商业词典/IPA 最终授权及 Cloud TTS vendor 保留 Release Gate。
