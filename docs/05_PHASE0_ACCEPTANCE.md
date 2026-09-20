> 状态：FROZEN v0.1 · 根据已确认原对话整理，原有第一人称及示例为规格说明。CR-01～CR-05 的澄清优先，见 [变更记录](CHANGE_REQUESTS.md)。
> 来源：source/S5.md；可选、SPIKE、未来能力保留原有性质，不能因冻结而变成必做。

# 05 — Phase 0 Acceptance Criteria v0.1

这一轮的原则非常简单：

> **不是“代码写出来了”就 PASS，而是用户能完成核心体验才 PASS。**

每项最终只有 `PASS / FAIL / NOT IN PHASE 0`。

## AC-01｜Clean Build & Launch

在一台符合 Phase 0 支持范围的 Windows 测试环境中，开发者能够从干净 checkout 按 README 完成构建。

Prototype 可以正常启动：

```text
BilingualInput.exe
        ↓
Onboarding / Main Prototype
```

不能要求：

> 手工复制神秘 DLL  
> 修改源码里的绝对路径  
> 安装未记录依赖  
> 把 API Key 写进代码

没有 Cloud API Key：

> **程序仍然必须启动并可以完成 Local Fast Path。**

Phase 0 CI 至少构建：

```text
BilingualCore
Prototype Host
Core Tests
```

**AC-01：FROZEN**

---

# AC-02｜首次启动 / Onboarding

第一次启动出现已冻结的五步 onboarding：

```text
Welcome
   ↓
Primary Language
   ↓
Input Preference
   ↓
Goals
   ↓
First Success
```

中文 Primary 首次成功必须真的可操作：

```text
youhua
  ↓ Space
优化
```

然后：

```text
youhua
  ↓ Shift+Enter
optimize
```

不是播放动画假装输入。

完成后设置持久保存；第二次启动不强制重复 onboarding。

必须提供重新进入 onboarding/demo 的入口。

**AC-02：FROZEN**

---

# AC-03｜真实 Input Session

Prototype 中必须存在一个真正可持续输入的 demo/editor region。

用户可以连续：

```text
type
Space
type
Arrow
Shift+Enter
Tab
Esc
```

而不是每测试一个功能都点：

> “Demo Word”

或：

> “Generate Example”。

我们需要能够坐下来连续使用 10～20 分钟，观察真实体验。

**AC-03：FROZEN**

---

# AC-04｜Primary Pinyin

输入：

```text
youhua
```

必须得到真实 Primary Candidates，并至少能够出现合理候选，例如：

```text
优化
油画
有话
...
```

候选具体排序不要求与 Microsoft Pinyin 完全一致。

用户可以：

Space 提交、数字选择、方向键改变高亮、鼠标选择。

这一项验证的是：

> **我们已经拥有真实 Primary Candidate Engine，而不是 hardcoded screenshot。**

**AC-04：FROZEN**

---

# AC-05｜Enter = Raw Input

这是硬验收：

```text
asdfg + Enter
→ asdfg
```

以及：

```text
youhua + Enter
→ youhua
```

不能因为系统认为 `youhua` 是拼音就覆盖用户 Enter 意图。

**AC-05：FROZEN**

---

# AC-06｜Shadow 跟随 Highlight

核心验收案例：

```text
youhua
```

高亮：

```text
优化
```

Shadow 应属于“优化”的语义：

```text
optimize / improve / refine
```

移动到：

```text
油画
```

Shadow 必须切换：

```text
oil painting
```

不得继续显示：

```text
optimize
```

也不得一次性显示所有 Primary 的所有翻译。

这是 Phase 0 **最高优先级 PASS 条件之一**。

**AC-06：FROZEN**

---

# AC-07｜Shadow Selection

Word Shadow：

> ≤ 3 个有意义候选。

`Shift+Enter`：

> 提交当前第一/推荐 Shadow。

如果 Prototype 保留 `Shift+1–5`：

> 能选择对应 Shadow。

但因为我们已经把 Shift+Number 标为 SPIKE：

**Shift+Enter 是必须 PASS。**

**Shift+Number 的最终键位可以在 UX Spike 后修改，不因此判整个 Phase 0 失败。**

**AC-07：FROZEN**

---

# AC-08｜Phrase

至少覆盖一组真实 Phrase：

```text
youhua fangan
```

Primary 可以产生类似：

> 优化方案

Shadow：

> optimization plan

不得机械地：

```text
optimize + plan
```

就宣称完成 Phrase intelligence。

至少需要一个 context-sensitive phrase mapping 测试集。

**AC-08：FROZEN**

---

# AC-09｜Chinese-dominant Mixed Input

必须通过：

```text
zhege design hai keyi
```

系统能够判断：

```text
Dominant = ZH
```

并保留明确输入的：

> `design`

而不是因为中文 Profile 强制抹掉 English token。

合理 Primary 类似：

> 这个 design 还可以

完整英文 Shadow 可以进一步：

> This design is pretty good.

具体措辞可以不同，只要语义正确且没有过度改写。

**AC-09：FROZEN**

---

# AC-10｜English-dominant Language Gap

这是另一个最高优先级案例。

Primary Language：

> Chinese

输入：

```text
I think this 方案 is better
```

系统必须识别：

```text
Dominant Language = EN
```

并识别：

```text
方案 = ZH language gap
```

能够得到自然补全，例如：

> I think this approach is better.

再至少测试：

```text
We probably need to 优化 this part.
```

→

> We probably need to optimize this part.

**如果系统因为用户 Primary=Chinese 而把这两句话强行转回中文，AC-10 FAIL。**

**AC-10：FROZEN**

---

# AC-11｜Pure English & Ambiguity

输入明确 English：

```text
design
```

不得无理由强行按拼音处理。

同时至少建立 ambiguity test set：

```text
shi
he
can
an
in
me
```

Phase 0 **不要求每次都判断正确**。

验收要求是：

> 它们必须经过 Language Context Resolver，而不是代码里粗暴规定“Latin characters = English”或“Chinese Primary = Pinyin”。

测试应能够观察：

```text
decision
confidence
reason/debug signals
```

而不是黑箱。

**AC-11：FROZEN**

---

# AC-12｜Basic Typo Recovery

至少验证类似：

```text
youhha
```

在明显 Chinese Pinyin context 下可以被 Primary pipeline 合理纠正/恢复。

Shadow 必须基于：

> **纠正后的 Primary**

而不是自己独立猜测 raw typo。

不要求 Phase 0 达到商业 IME 的 typo quality。

**AC-12：FROZEN**

---

# AC-13｜Sentence Shadow

至少准备一组 sentence test：

```text
wo juede zhege fangan hai keyi jixu youhua
```

Primary：

> 我觉得这个方案还可以继续优化

Shadow 应提供 **一个**主要自然表达，例如：

> I think this approach could be further improved.

具体英文不做 exact-string assertion。

测试判断：

> Meaning preserved  
> Grammatically valid  
> Natural enough  
> No invented intent

Sentence 默认不能一次展示三四种 AI 写法。

**AC-13：FROZEN**

---

# AC-14｜Local First

关闭网络。

重新测试：

```text
youhua
```

必须仍然拥有：

Primary Candidate、本地可提供的 Shadow、候选选择、Enter Raw Input、基本 Learning UI。

不能出现：

> “No Internet — Input unavailable.”

云端 Natural Expression 可以不可用。

**AC-14：FROZEN**

---

# AC-15｜Stale AI Result

自动化测试模拟：

```text
Revision 10
→ AI request A

用户继续输入

Revision 11
→ AI request B

A 晚回来
```

结果：

> A **不得更新 Revision 11 的 UI**。

这是必须有自动测试的 concurrency requirement。

**AC-15：FROZEN**

---

# AC-16｜AI Failure

模拟：

Timeout、HTTP error、invalid response、provider unavailable。

结果：

```text
Primary          PASS
Local Shadow     PASS
Typing           PASS
```

Candidate UI 不出现：

> HTTP 500  
> JSON parse failed  
> OpenAI/Azure xxx error

诊断进入 privacy-safe development log。

**AC-16：FROZEN**

---

# AC-17｜Hover Quick Peek

Hover Shadow word：

短暂 delay 后出现：

```text
refine
IPA
POS
Primary-language meaning
🔊
Save
```

必须：

> hover 不自动播放声音。

点击 🔊 才播放。

关闭 Quick Peek setting：

> 不再弹出。

Delay 允许 UX Spike 后调整。

**AC-17：FROZEN**

---

# AC-18｜Pronunciation

Phase 0 至少一个本地 Windows Speech Provider。

点击：

```text
refine 🔊
```

如果存在合适 voice：

> 能实际听到正确目标语言的 TTS。

如果系统没有合适 voice：

> UI 优雅降级，不崩溃，并允许未来 Cloud Provider fallback。

Phrase 至少能播放：

> further improve

架构不能只支持预录单词 MP3。

Windows SpeechSynthesizer 本身就是针对已安装 speech synthesis voice 的接口，因此这里把“检测可用 voice”作为验收条件，而不是假定环境必然具备某个 voice。

**AC-18：FROZEN**

---

# AC-19｜IPA Provenance

Hover 显示的 IPA 必须能够追踪：

```text
word
locale
IPA
sourceId
```

不能：

> 临时问 LLM → 显示 → 不知道来源。

Phase 0 数据可以是 approved prototype source。

但 Source Manifest 必须能回答：

> 这个 `/.../` 是哪里来的？

**AC-19：FROZEN**

---

# AC-20｜Tab Deep Dive

在 word / phrase / sentence 状态：

`Tab`

可以展开 Deep Dive。

至少能够展示适用的：

```text
Natural Expression
Phrase Breakdown
Vocabulary
```

`Esc`

关闭并回到正常输入。

不能破坏当前 composition。

用户设置关闭某模块后：

> Deep Dive 尊重设置。

**AC-20：FROZEN**

---

# AC-21｜Learning Collection

Hover / Deep Dive 可以 Save 一个 lexical item。

至少保存：

```text
lemma
meaning/source mapping
IPA
POS
pronunciation reference
savedAt
encounterCount
```

Learning Library 可以重新查看。

默认：

> **不要求保存原始完整聊天句子。**

**AC-21：FROZEN**

---

# AC-22｜Encounter

收藏一个词，例如：

> `refine`

之后它自然再次成为 Shadow 时：

```text
encounterCount += 1
```

UI 可以有一个克制的：

> ★

作为“已收藏/再次遇到”的提示。

Encounter 只表示：

> **再次遇见。**

不得显示：

> “你已经掌握这个单词。”

Phase 0 不做 AI mastery scoring。

**AC-22：FROZEN**

---

# AC-23｜Secure / Private

Phase 0 因为不是正式 TSF，真实 password-field detection 可以模拟。

但是 Privacy architecture 必须真实。

Secure Mode：

```text
cloudCallCount       = 0
learningWriteCount   = 0
contentLogCount      = 0
persistentTextCache  = 0
```

Private Mode：

```text
Local input          = YES
Cloud                = NO
Learning/history     = NO
```

这些应该有自动化测试，而不仅仅是 UI 上显示一个锁。

**AC-23：FROZEN**

---

# AC-24｜Privacy Logging Test

测试输入一个唯一字符串，例如：

```text
PRIVACY_TEST_UNIQUE_7F3...
```

完成 session 后扫描正常应用日志。

期望：

> **找不到原文。**

允许看到：

```text
resolver confidence=.91
candidate_count=5
provider_latency=...
```

不允许：

```text
input="PRIVACY_TEST..."
```

**AC-24：FROZEN**

---

# AC-25｜Performance Budget

这里我不建议现在冻结一个拍脑袋的：

> “所有候选必须 16ms”。

不同电脑差异太大。

Phase 0 先冻结**行为预算**：

> Local Primary 不等待 Network。  
> Local Shadow 不等待 LLM。  
> UI thread 不执行网络请求。  
> 用户连续输入时不因为旧 AI 请求阻塞。  
> Hover/Tab 不阻塞 composition。

同时建立 telemetry：

```text
Primary latency
Local Shadow latency
Resolver latency
AI latency
UI render latency
```

等第一次可运行版本出来，我们在真实 Windows 机器上测 P50/P95，再冻结具体 ms SLA。

这是一个我刻意**不建议过早量化**的地方。

**AC-25：行为 FROZEN；具体 SLA = SPIKE。**

---

# AC-26｜视觉验收

Phase 0 不是工程师默认控件拼起来就算完成。

Candidate 默认必须达到：

> Windows 11-compatible spirit  
> restrained  
> compact  
> professional  
> Light/Dark  
> Primary 明显强于 Shadow  
> 不用 flashy AI gradient  
> 不持续 animation

默认 Candidate Window：

> 大致容纳 5 个常规中文候选。

Sentence Shadow：

> 最多自然扩展约两行。

Hover / Tab：

> 视觉属于同一个系统。

但 Pixel-perfect 设计系统留到后续。

**AC-26：FROZEN**

---

# AC-27｜部署测试

这一项是根据你刚才“冻结不能影响后续部署”的要求，我专门加进去的。

Phase 0 每个 Alpha milestone 应能产生：

> **另一台 Windows 测试机可运行的 artifact。**

不能只能在 Codex/开发机上运行。

WinUI 3 官方支持 packaged 和 unpackaged 等不同部署路径；unpackaged 也可以采用 self-contained 方式携带 Windows App SDK runtime，只是体积更大。

因此 Phase 0 暂时不冻结最终商店发行形式。

验收：

> Build → Artifact → Clean test machine → Launch → Core scenario works.

**AC-27：FROZEN。**

---

# AC-28｜Dependency Audit

自动生成/维护：

```text
THIRD_PARTY_SOURCES.md
```

每个 dependency/data source 至少知道：

```text
name
version
source
purpose
license
releaseStatus
```

如果：

```text
releaseStatus = PROTOTYPE_ONLY
```

Release pipeline 必须能识别。

不得出现：

> “这个 180MB dictionary.db 不知道谁加进来的。”

**AC-28：FROZEN**

---

# AC-29｜明确不属于 Phase 0

这些**没做不算失败**：

```text
Real TSF IME
System-wide typing

Microsoft-Pinyin-level vocabulary
Advanced fuzzy pinyin
Advanced typo correction

Account system
Cloud sync
Multi-device sync

Languages beyond ZH/EN

Payment / Subscription

Pronunciation scoring
Speaking assessment

AI mastery judgment

Word / Excel export
Formal CSV export

Many writing tones
Formal / Friendly / Business modes

Theme marketplace
Layout editor

Microsoft Store release
Production signing

Production commercial dictionary
Production cloud TTS vendor
```

这样 Codex 不会“顺手”扩 Scope。

**AC-29：FROZEN**

---

# AC-30｜Phase 0 最终 Gate

Phase 0 不以：

> 30 个 AC 都有代码

作为唯一成功标准。

最后我们三种判断：

```text
ENGINEERING GATE
核心测试是否通过？

        +

UX GATE
连续真实使用是否顺手？

        +

PRODUCT GATE
Shadow Candidate 是否真的
比“输入 → 翻译软件 → 粘贴”
更自然、更值得存在？
```

Engineering Gate 可以自动化。

UX/Product Gate：

> **必须由我们实际使用后决定。**

如果技术上全 PASS，但实际感觉：

> Shift+Enter 很别扭；

我们改。

如果：

> Hover 太烦；

我们改。

如果：

> 三个 Shadow 太多；

我们改。

**Frozen Spec 不意味着禁止学习。**

它意味着：

> **Codex 不能擅自改产品。**

而我们根据 Prototype 的真实证据，可以正式提出：

```text
CR-02
CR-03
...
```

再修改 v0.2。

**AC-30：FROZEN**

---


## CR-04 / CR-05 验收解释

AC-23 仅模拟 secure-field detection，实际阻断必须真实；系统宿主检测留待 Phase 1。测试分三层：Tier 1 deterministic（候选状态、分段 fixture、按键、Privacy Gate、调用计数、过期结果、数据库写入）；Tier 2 semantic（目标语言、保留必要含义、无禁止含义、无虚构实体、合理语法）；Tier 3 human UX（自然度、选择数量、速度、Shift/Hover 的真实体验）。Mock/固定词典可以 exact assertion，真实 AI 的正确表达不能限成单一句式。
