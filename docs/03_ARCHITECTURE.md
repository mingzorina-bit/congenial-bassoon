> 状态：FROZEN v0.1 · 根据已确认原对话整理，原有第一人称及示例为规格说明。CR-01～CR-05 的澄清优先，见 [变更记录](CHANGE_REQUESTS.md)。
> 来源：source/S3.md；可选、SPIKE、未来能力保留原有性质，不能因冻结而变成必做。

# 03 — Architecture v0.1

## A-01｜总体架构：Core 与 Host 分离

正式采用：

```text
BilingualInput
│
├── Presentation / Host
│      WinUI 3 + C# / XAML
│
├── BilingualCore
│      Native C++20
│
├── Data
│      SQLite + lexical data
│
├── Providers
│      Dictionary / Translation / AI / Pronunciation
│
└── Infrastructure
       Privacy / Cache / Logging / Networking
```

最重要的边界：

> **UI 不是产品核心。**

Phase 0 的 WinUI Prototype Host 将来可以被 TSF Host 替换，而 `BilingualCore` 尽可能继续使用。

**A-01：FROZEN。**

---

## A-02｜Presentation：WinUI 3 + C# / XAML

Phase 0 使用：

> **WinUI 3 + Windows App SDK + C# / XAML**

负责：

Onboarding、Prototype 输入区域、Candidate Window、Shadow、Hover Quick Peek、Tab Deep Dive、Settings、Learning Library，以及 Light/Dark UI。

它不负责：

拼音逻辑、Shadow 排序、语言判断、AI prompt 决策、词典业务规则。

也就是说 UI 收到的应该接近：

```text
CandidateViewModel
{
    rawInput
    primaryCandidates
    highlightedPrimary
    shadowCandidates
    expressionState
    loadingState
}
```

而不是 UI 自己计算：

> “`youhua` 大概应该是 `优化`。”

这样以后 TSF 不需要重新实现业务逻辑。

**A-02：FROZEN。**

---

# A-03｜BilingualCore：C++20

真正长期保留的核心使用 Native C++20。

建议模块：

```text
BilingualCore
│
├── InputSession
├── RimeAdapter
├── LanguageContextResolver
├── LanguageSegmenter
├── GapResolver
├── CandidateEngine
├── ShadowEngine
├── ShadowRanker
├── PersonalizationEngine
└── PrivacyPolicy
```

其中 `InputSession` 是中心。

不要让各模块互相随意调用。

大致：

```text
Keystroke
   ↓
InputSession
   ↓
LanguageContextResolver
   ↓
CandidateEngine
   ↓
ShadowEngine
   ↓
ShadowRanker
   ↓
Presentation Model
```

**A-03：FROZEN。**

---

# A-04｜librime 的边界

librime 只承担：

> **成熟拼音输入基础设施。**

包括：

拼音解析、中文候选生成、候选选择/高亮、composition state，以及它本身擅长的输入引擎能力。

但 librime **不拥有**：

Shadow、Language Gap、Natural Expression、学习系统、AI、用户 Profile、跨语言排序。

架构：

```text
               RimeAdapter
                   │
              ┌────▼────┐
Raw Pinyin →  │ librime │
              └────┬────┘
                   ↓
          Primary Candidates
                   ↓
             BilingualCore
                   ↓
                Shadow
```

另外明确：

> **允许依赖许可证审核通过的 librime；禁止为了省时间直接复制/移植 GPL Windows frontend（例如 Weasel）的源码进入我们的闭源代码。**

第三方依赖最终统一进 Source Manifest。

**A-04：FROZEN。**

---

# A-05｜Language Context Pipeline

这是新架构最重要的模块之一。

不要做成：

```text
if ChineseProfile:
    Chinese
else:
    English
```

而是：

```text
Input
 ↓
Language Context Resolver
 ↓
┌──────────────────────────┐
│ Dominant Language        │
│ Segment Languages        │
│ Language Gaps            │
│ Confidence               │
└──────────────────────────┘
```

例如：

```text
I think this 方案 needs 优化
```

输出内部结构可以接近：

```text
dominantLanguage = EN
confidence = 0.96

segments:
[EN]     "I think this "
[ZH-GAP] "方案"
[EN]     " needs "
[ZH-GAP] "优化"
```

注意 **Phase 0 不需要训练一个神经网络分类器**。

第一版可以组合：

词典命中、Latin/Chinese script、拼音合法性、English lexical probability、前后文、Primary Language prior、当前 session history。

后面数据证明值得再升级。

> **先可解释、可调试，再智能化。**

**A-05：FROZEN。**

---

# A-06｜Shadow Pipeline

Shadow 不直接等于 Translator API。

正式结构：

```text
Highlighted Primary
       │
Language Context
       │
       ▼
 Shadow Resolver
       │
 ┌─────┼───────────┐
 ↓     ↓           ↓
Local  Translation Expression
Lexical Provider   Provider
 ↓     ↓           ↓
 └─────┼───────────┘
       ↓
  Shadow Ranker
       ↓
Presentation
```

不同输入走不同成本路径。

**Word**

```text
优化
→ Local Lexical
→ optimize / improve / refine
```

正常情况下不调用 LLM。

**Phrase**

```text
优化方案
→ Local lexical/composition
→ optional translation provider
```

**Sentence**

```text
我觉得这个方案……
→ Local fallback immediately
→ Expression Provider asynchronously
```

**English + Chinese Gap**

```text
I think this 方案 is better
→ Gap Resolver
→ lexical/contextual replacement
→ expression provider only when necessary
```

所以：

> **AI 使用量由语言问题决定，而不是“每敲一个键调用一次 AI”。**

**A-06：FROZEN。**

---

# A-07｜Provider Architecture

这里正式定义四个主要 Provider abstraction：

```text
ILexicalProvider
ITranslationProvider
IExpressionProvider
IPronunciationProvider
```

其中第四轮还会进一步拆：

```text
Pronunciation
├── IPA / phonetic data
└── Audio / TTS
```

Provider 的核心原则：

> **No provider owns product logic.**

例如换掉 Azure、Google、某个词典或某个 LLM：

Candidate UI 不应该需要重写。

未来允许：

```text
IExpressionProvider
├── ProviderA
├── ProviderB
├── LocalModelProvider
└── MockExpressionProvider
```

Mock 非常重要，因为：

> **UI/自动化测试不能依赖真实 AI API 才能运行。**

**A-07：FROZEN。**

---

# A-08｜异步与性能模型

输入线程不能等待云端。

严格采用：

```text
Keystroke
   ↓
LOCAL FAST PATH
   ↓
Primary immediately
   ↓
Local Shadow if available
   ↓
UI
```

同时：

```text
Input snapshot
     ↓
Debounce
     ↓
Async Provider Request
     ↓
Result validation
     ↓
Is session still current?
   YES ↓
Update Shadow
    NO ↓
Discard
```

这个 **Discard stale result** 非常重要。

例如用户快速输入：

```text
you
youh
youhu
youhua
```

AI 对 `youh` 的结果晚回来时：

> **绝对不能覆盖现在 `youhua` 的 Shadow。**

因此每个请求至少绑定：

```text
sessionId
revision
requestId
```

结果只有仍对应当前 revision 才允许更新 UI。

Phase 0 具体 debounce 数值属于 SPIKE。

**A-08：FROZEN；具体延迟参数 SPIKE。**

---

# A-09｜SQLite 与数据分区

SQLite 可以使用，但不要所有东西塞一张数据库。

逻辑至少分成：

```text
User Preferences
Learning Items
Encounter Statistics
Candidate Preferences
Provider Cache
```

尤其：

**Learning Data ≠ Raw Input History**

我们不建立：

```text
input_history
-----------------------------------
"今天老板跟我说..."
"I told Sarah that..."
"我的信用卡..."
```

这种默认完整输入日志。

Candidate preference 应尽可能保存：

```text
semantic candidate id
selection count
context category
```

而不是整句原文。

Provider Cache 也必须服从 Privacy Policy，不能因为叫“缓存”就绕过“不保存完整输入”的原则。

**A-09：FROZEN。**

---

# A-10｜Privacy Gate 必须位于 Provider 之前

我建议这一项做成硬架构约束。

错误设计：

```text
Input
 ↓
AI Provider
 ↓
发现是密码
 ↓
Oops
```

正确：

```text
Input
 ↓
Privacy Gate
 ↓
┌─────────────────────┐
│ Normal              │ → providers allowed
│ Private             │ → local only
│ Secure              │ → strict local/minimal
└─────────────────────┘
```

任何 Cloud Provider 必须经过 Privacy Gate。

即：

> **Provider 本身没有权利决定能不能上传文本。**

Secure：

```text
Cloud = BLOCK
Learning = BLOCK
Logging content = BLOCK
Persistent cache = BLOCK
```

Private：

```text
Local = ALLOW
Cloud = BLOCK
Learning/history = BLOCK
```

Normal：

按用户设置执行。

**A-10：FROZEN。**

---

# A-11｜Logging 也必须 Privacy-aware

开发阶段我们很容易为了调试写：

```text
LOG("input = {}", userSentence);
```

正式项目从第一天就禁止这种习惯。

默认日志允许：

```text
session started
provider latency = 312ms
candidate count = 5
resolver confidence = 0.91
provider timeout
```

不允许默认：

```text
user text = "I told my manager..."
```

开发 Debug 如果未来真的需要内容诊断：

> 必须是明确开启的开发模式，并与正式 Release 分离。

这样 Privacy 不是发布前才补。

**A-11：FROZEN。**

---

# A-12｜AI / Translation 网络层

我建议所有云端 Provider 都不要自己随便创建网络逻辑。

统一：

```text
Provider
   ↓
Provider Gateway
   ↓
Privacy Gate
   ↓
HTTP Client
```

统一处理：

timeout、cancellation、retry policy、rate limit、request revision、telemetry、privacy mode。

尤其**输入场景不要激进 Retry**。

一句话已经过期，再 retry 两次回来已经没有意义。

所以：

> stale request → cancel/discard

通常比：

> retry until success

更重要。

**A-12：FROZEN。**

---

# A-13｜发音架构现在先留正确接口

第四轮再选来源，但 Architecture 先固定：

```text
PronunciationService
│
├── PhoneticDataProvider
│      └── IPA / locale
│
└── AudioProvider
       ├── Recorded Audio
       ├── Cloud TTS
       └── Local TTS
```

不要设计成：

```text
DictionaryEntry.audioMp3Url
```

因为那会把“词典”和“发音”永久绑死。

单词：

```text
refine 🔊
```

短语：

```text
further improve 🔊
```

句子未来也可以：

```text
I think this approach... 🔊
```

使用同一 Audio abstraction。

而：

> IPA 和 Audio 可以来自不同来源。

**A-13：FROZEN。**

---

# A-14｜Phase 0 不需要真正 IPC

这里我想避免过度设计。

Phase 0：

```text
WinUI Prototype
       │
   in-process
       │
BilingualCore
```

先不要为了未来 TSF 就立即构建：

Named Pipes / gRPC / local service / broker process……

否则我们还没验证产品就开始造基础设施。

但是接口边界要保证未来能够变成：

```text
TSF Host
   │
Local Core
   │
 IPC
   │
Bilingual Service
   │
Cloud Providers
```

所以：

> **Interface ready, IPC deferred.**

**A-14：FROZEN。**

---

# A-15｜Phase 1 TSF 边界

未来真正系统 IME：

```text
Application
    │
Windows TSF
    │
Bilingual TSF Host
    │
BilingualCore
    │
Local Fast Path
```

TSF Host 只处理 Windows 输入系统职责，例如：

composition、key events、text insertion、candidate presentation integration、application context。

它不应该变成：

> 第二套 BilingualCore。

未来 Cloud：

```text
TSF
 ↓
Core
 ↓
IPC Client
 ↓
Bilingual Background Service
 ↓
Privacy Gate
 ↓
Providers
```

所以 Phase 0 写任何功能时，都要问：

> “这个代码属于 Core，还是只是 Prototype Host？”

如果属于 Core，就不能偷偷写死 WinUI dependency。

**A-15：FROZEN。**

---

# A-16｜崩溃与降级原则

正式 IME 最大的风险之一不是“翻译不够自然”，而是：

> **把用户正在工作的应用搞卡或搞崩。**

所以从 Prototype 就建立降级层：

```text
AI unavailable
→ Local Shadow

Translation unavailable
→ Local lexical

Dictionary unavailable
→ Primary still works

Learning DB unavailable
→ Input still works

Pronunciation unavailable
→ Input still works

Shadow Engine exception
→ Primary still works
```

最底线：

```text
Bilingual enhancement failure
≠
Typing failure
```

未来 TSF 阶段这会升级成更严格的 crash isolation。

**A-16：FROZEN。**

---

# A-17｜Testing Architecture

Codex 不应该靠我们人工每次敲 `youhua` 验证。

Core 从第一天提供 deterministic tests。

例如：

```text
INPUT:
youhua

EXPECTED PRIMARY:
contains 优化
```

以及：

```text
PRIMARY:
优化

EXPECTED SHADOW:
optimize / improve / refine
```

语言 Context：

```text
INPUT:
I think this 方案 is better

EXPECTED:
dominant = EN
gap contains 方案
```

Privacy：

```text
MODE:
Secure

EXPECTED:
cloud provider call count = 0
learning write count = 0
content log count = 0
```

Provider 用 Mock：

```text
MockExpressionProvider
→ fixed deterministic result
```

这样 Codex 每个 milestone 都必须跑测试。

**A-17：FROZEN。**

---

# A-18｜工程依赖原则

第三方依赖进入项目必须记录：

```text
Name
Version
Purpose
Source
License
Modification
Redistribution status
Commercial-use status
```

不允许 Codex 为了方便：

> “我发现 GitHub 有个库，直接装了。”

尤其禁止未经审核引入：

GPL frontend code、来源不明词库、来源不明 MP3、抓取词典网站数据、来源不明 IPA 数据。

开发 Prompt 会明确：

> **New external dependencies require explicit approval.**

这正好保护下一轮 Dictionary / Pronunciation Strategy。

**A-18：FROZEN。**

---

## Architecture v0.1 最终图

最终可以压缩成：

```text
                         ┌────────────────────┐
                         │   WinUI 3 / C#     │
                         │ Prototype / UI     │
                         └─────────┬──────────┘
                                   │
                         Presentation Model
                                   │
                  ┌────────────────▼───────────────┐
                  │      BilingualCore / C++20     │
                  │                                │
                  │ InputSession                   │
                  │ LanguageContextResolver        │
                  │ GapResolver                    │
                  │ CandidateEngine                │
                  │ ShadowEngine / Ranker          │
                  │ Personalization                │
                  └───────┬────────┬────────┬──────┘
                          │        │        │
                     librime   Lexical    SQLite
                          │     Layer       │
                          │        │        │
                          │    Privacy Gate │
                          │        │        │
                          │   Provider Gateway
                          │        │
                   ┌──────┼────────┼─────────┐
                   ↓      ↓        ↓         ↓
                Lexical Translate Expression Audio
                Provider Provider  Provider  Provider
```

未来：

```text
WinUI Prototype Host ─┐
                      ├── BilingualCore
Windows TSF Host ─────┘
                           │
                      Local Fast Path
                           │
                      Future IPC
                           │
                  Background Service
                           │
                       Cloud AI
```

---


## CR-03 / CR-04 已并入的解释

Primary Candidate 是 composition 中当前高亮候选；Current Expression 是当前正在构造的完整表达。两者不是同一个字段。Shadow Resolver 可消费前者，或后者加 Language Gaps。CompositionState 至少表达 Raw Input、Candidates、Highlighted、Expression、Segments、Gaps。

Phase 0 Secure enforcement = REAL，secure-field detection = SIMULATED。Phase 1 必须进行真实 TSF Secure Context Detection spike，覆盖 Windows 原生密码框、浏览器、Chromium/Electron、Office、常见消息应用；无法可靠判断时在技术适用处 fail closed。Phase 0 通过不能替代该安全验证。
