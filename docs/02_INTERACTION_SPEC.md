> 状态：FROZEN v0.1 · 根据已确认原对话整理，原有第一人称及示例为规格说明。CR-01～CR-05 的澄清优先，见 [变更记录](CHANGE_REQUESTS.md)。
> 来源：source/S2.md；可选、SPIKE、未来能力保留原有性质，不能因冻结而变成必做。

# 02 — Interaction Spec v0.1

这一轮我建议验收 **I-01 ～ I-15**。我会特别把我们刚修正的“英文框架 + 中文缺口”纳入正式交互，而不是继续把混输理解成单向的中文夹英文。

---

## I-01｜输入状态模型

系统内部不应简单只有“中文模式 / 英文模式”。

每个 Input Session 至少维护：

```text
Raw Input
    ↓
Language Context
    ↓
Primary Candidate / Current Expression
    ↓
Shadow Candidate
    ↓
Optional Deep Intelligence
```

根据输入内容，UI 可以处于：

```text
Word
Phrase
Sentence
Mixed-language
Direct English
Secure
```

但这些是**系统内部状态**，正常情况下不要求用户手动选择。

例如用户不应该先点：

> “现在我要进入英文句子模式。”

系统自己判断。

**I-01 FROZEN**

---

# I-02｜基础键盘模型

这是整个产品最需要稳定的肌肉记忆。

| 按键 | 默认行为 |
|---|---|
| `Space` | 提交当前 Primary 第一/高亮候选 |
| `1–9` | 提交对应 Primary Candidate |
| `Enter` | 提交 Raw Input 原始字符 |
| `Shift+Enter` | 提交当前首选 Shadow |
| `Shift+1–5` | 提交对应 Shadow Candidate |
| `Tab` | 打开 Deep Dive |
| `Esc` | 关闭 Deep Dive / 取消当前展开层 |
| `↑↓←→` | 候选导航，具体方向按最终 UI 布局 |
| Mouse | 高亮、选择、Hover |

最关键的是：

```text
youhua + Enter
→ youhua
```

不能变成：

`优化`

也不能变成：

`optimize`

因为 Enter 必须保留微软拼音用户熟悉的 Raw Input escape hatch。

### 一个需要标记为 SPIKE 的地方

`Shift+1–5`

概念冻结，但具体键位需要 Phase 0 实测：

**是否顺手 / 是否存在键盘布局问题 / 是否容易与宿主快捷键冲突。**

如果测试不好用，允许 Change Request。

**I-02 FROZEN，Shift+Number = SPIKE**

---

# I-03｜Primary Candidate

Primary 是当前输入最直接的完成结果。

中文主导：

```text
youhua

1 优化   2 油画   3 有话   4 ……   5 ……
```

默认视觉目标：

> **约 5 个普通双字候选的宽度。**

短候选可以自然显示 6–7 个。

长候选：

```text
1 优化方案   2 优化方法   3 优化设计
```

宁愿减少数量，也不要把候选窗无限拉宽。

Primary 始终拥有最高视觉权重。

**I-03 FROZEN**

---

# I-04｜Shadow Candidate 的联动

Shadow **只服务当前 Primary / Current Expression**。

例如：

```text
1 优化   2 油画   3 有话
  ↑
────────────────────────
EN  optimize · improve · refine
```

移动到：

```text
1 优化   2 油画   3 有话
         ↑
────────────────────────
EN  oil painting
```

不允许：

```text
优化 → optimize
油画 → oil painting
有话 → have words
```

全部同时塞在下面。

否则用户需要重新建立 Primary ↔ Shadow 对应关系。

**一个高亮对象，一个 Shadow Context。**

**I-04 FROZEN**

---

# I-05｜Word Shadow

Word State：

```text
优化
────────────────────────
EN  optimize · improve · refine
```

规则：

> 默认最多 **3 个**高质量、有意义的第二语言候选。

不是必须三个。

例如：

```text
油画
────────────
EN oil painting
```

只需要一个。

排序：

> Context correctness  
> → User preference  
> → General frequency

不能因为用户历史经常选 `process`，就在明显应该 `handle` 的语境里把 `process` 放第一。

**I-05 FROZEN**

---

# I-06｜Phrase Shadow

Phrase State：

```text
优化方案
────────────────────────
EN optimization plan
```

只有存在真正有价值的差异时才显示第二项：

```text
EN optimization plan · optimized solution
```

不为了“AI 看起来聪明”生成：

> optimization plan  
> improvement proposal  
> refinement scheme  
> optimization approach  
> enhanced solution

这种选择轰炸。

Phrase 仍然遵守：

> **Minimum Useful Choice**

**I-06 FROZEN**

---

# I-07｜Sentence Shadow

Sentence State：

```text
我觉得这个方案还可以继续优化
────────────────────────────────
EN I think this approach could be further improved.
```

默认只出现：

> **一个主要 Shadow Sentence。**

不出现：

```text
A. I think...
B. In my opinion...
C. I believe...
```

如果用户按 `Tab`，才允许出现：

**Natural Expression**

例如：

```text
Current
I think this approach could be further improved.

Natural Expression
There's still room to improve this approach.
```

而且 Natural Expression 不是必须为了不同而不同。

如果原 Shadow 已经非常自然：

> 可以不强行产生第二句话。

**I-07 FROZEN**

---

# I-08｜混输：中文框架 + English

例如：

```text
zhege design hai keyi
```

系统识别：

```text
Dominant Language = ZH

[ZH] zhege
[EN] design
[ZH] hai keyi
```

Primary 应优先尊重原表达：

```text
这个 design 还可以
```

而不是自动大改成：

> 这个设计很好。

如果用户需要完整英文：

Shadow：

```text
EN This design is pretty good.
```

是否把 Primary 中的 `design` 自动规范成 `设计`，以后可以根据上下文/设置实验，但默认原则：

> **用户明确输入的合法 English 不要无理由消灭。**

**I-08 FROZEN**

---

# I-09｜混输：English 框架 + 中文 Language Gap

这是本轮新增正式场景。

例如用户 Primary Language = Chinese，但输入：

```text
I think this 方案 is better
```

系统：

```text
Dominant Language = EN
```

不能因为 Profile 是中文而拉回中文。

它应识别：

```text
[EN] I think this
[ZH-GAP] 方案
[EN] is better
```

辅助完成：

```text
I think this approach is better.
```

另一个例子：

```text
We probably need to 优化 this part.
```

→

```text
We probably need to optimize this part.
```

这里有一个非常重要的 UI 原则：

> **如果用户已经明显进入英文表达框架，系统的首要帮助是补齐 Language Gap，而不是再把整句翻译回中文。**

中文 Shadow/释义仍然可以存在于 Hover / Deep Dive，但不能干扰当前英文表达。

**I-09 FROZEN**

---

# I-10｜纯 English 输入

例如：

```text
design
```

如果系统高度判断这是合法 English：

Primary：

```text
design
```

辅助层可以：

```text
ZH  设计
```

而不是把 `design` 当拼音硬猜。

但是：

```text
shi
he
can
an
in
me
```

这种同时可能属于英文和拼音的输入：

进入 **Language Context Resolver**。

判断依据：

```text
上下文
Current Expression Language
Primary Language
词形合法性
前后输入
用户历史
```

Profile 只是 prior，不是命令。

**I-10 FROZEN**

---

# I-11｜拼音错误

例如：

```text
youhha
```

系统不允许：

```text
Chinese Engine 猜一次
+
English Engine 再猜一次
+
把两套结果混起来
```

而是：

```text
Raw Input
↓
Language Context
↓
判断当前主要意图 ≈ Chinese Pinyin
↓
Pinyin Correction
↓
youhua
↓
Primary
优化 / 油画 / 有话
↓
Shadow
```

第一阶段纠错可以很基础。

Phase 0 重点不是媲美微软拼音的纠错能力，而是验证：

> **纠错发生在 Primary 意图确定以后，Shadow 使用纠正后的语义。**

**I-11 FROZEN；高级纠错 DEFERRED**

---

# I-12｜Hover Quick Peek

鼠标停在 Shadow word 上约一个短暂 delay 后：

```text
┌─────────────────────────┐
│ refine                  │
│ /rɪˈfaɪn/          🔊 ☆ │
│ v. 改进；完善；精炼     │
└─────────────────────────┘
```

默认包含：

**Word / IPA / POS / Primary-language meaning / Speaker / Save**

但默认 Candidate Row：

```text
EN optimize · improve · refine
```

不显示一排：

```text
🔊 ☆ 🔊 ☆ 🔊 ☆
```

否则会严重污染输入界面。

Hover delay 初始目标：

> **300–500ms，最终 Prototype 调参。**

鼠标移开关闭。

🔊：

> **必须点击才播放。**

禁止 hover autoplay。

Settings：

> `词汇快速预览 ON/OFF`

用户可以彻底关闭。

**I-12 FROZEN；具体 delay = SPIKE**

---

# I-13｜Tab Deep Dive

`Tab` 代表：

> **我想进一步理解当前表达。**

而不是单纯“查看更多候选”。

推荐固定尺寸 Card：

```text
┌────────────────────────────────────┐
│ 当前表达                           │
│ I think this approach could...     │
│                                    │
│ Natural Expression                 │
│ There's still room to improve...   │
│ ────────────────────────────────── │
│ Phrase                             │
│ 这个方案    this approach          │
│ 继续优化    further improve        │
│ ────────────────────────────────── │
│ Vocabulary                         │
│ approach  /.../  🔊 ☆              │
│ refine    /.../  🔊 ☆              │
└────────────────────────────────────┘
```

信息顺序：

> **Sentence → Phrase → Vocabulary**

即“长 → 短”。

`Tab` 打开。

`Esc` 关闭。

用户可以设置显示：

```text
☑ Natural Expression
☑ Phrase Breakdown
☑ Vocabulary
☐ Examples
```

Vocabulary 子项：

```text
☑ IPA
☑ Pronunciation
☑ Meaning
☑ Part of Speech
```

但 Phase 0 不做自由拖拽布局。

**I-13 FROZEN**

---

# I-14｜AI Loading / Failure

用户输入：

```text
wo juede zhege fangan...
```

Local Path 应先工作。

AI 异步计算 Natural Expression。

如果 AI 很快返回：

> **直接更新。**

不闪：

> Loading → Done → AI → ✓

如果明显超过用户感知阈值，才允许非常弱地：

> 正在完善表达…

我们此前讨论的初始实验范围：

**约 700–1000ms 后才出现提示。**

但这不是最终数字，Prototype 调参。

AI 失败：

> 保留 Local Shadow。

候选框里不显示：

```text
Error 503
API Timeout
LLM failed
```

需要诊断的信息进入开发日志，而不是输入 UI。

**I-14 FROZEN；loading threshold = SPIKE**

---

# I-15｜Onboarding + Settings

最终 Onboarding：

### ① Welcome

> **欢迎使用 Bilingual Input**  
> 用你熟悉的语言，自然连接另一种表达。

自然输入 · 双语表达 · 在真实使用中学习

### ② Primary Language

> **你更熟悉哪种语言？**

中文 / English

自动推荐 UI language，但两者独立。

### ③ Input Preference

中文 Primary：

**智能识别（推荐）**

或者：

**中文拼音优先**

不是让用户回答：

> “我有多少百分比会中英混输？”

### ④ Goals

允许多选：

**更自然地使用另一种语言表达**

**在输入过程中学习另一种语言**

**快速进行中英双语输入**

第三项解释：

> 中文、英文或中英混合输入，无需频繁切换。

### ⑤ First Success

不再：

> 🎉 设置完成！

而是：

> **准备好了，试着输入一句。**

第一次：

`youhua → Space → 优化`

反馈：

> ✓ 中文输入和以前一样

第二次：

`youhua → Shift+Enter → optimize`

反馈：

> ✓ 同样的输入，也可以直接使用另一种语言表达

随后：

> 鼠标停在英文词上，可以快速了解它。  
> 按 Tab 可以查看更多表达和学习信息。

最后：

> **就是这样。**  
> 像平时一样输入，需要时使用另一种语言。

**开始使用**

`再试一会儿`

---

Settings 第一阶段只提供真正必要的东西，例如：

```text
Language
Primary Language
UI Language

Input
Smart Recognition / Primary-first
Hover Quick Peek

Learning
Natural Expression
Phrase Breakdown
Vocabulary
Examples
IPA
Pronunciation
Meaning
POS

Pronunciation
English pronunciation preference

Appearance
Light / Dark / System

Privacy
Cloud assistance
Learning
Private Mode
```

避免设置页变成几十个技术开关。

**I-15 FROZEN**

---


## CR-01 / CR-02 解释

Shift = Shadow Action，不能硬编码为 English 或 Profile.SecondaryLanguage。一个高亮对象/当前表达只对应一个 Shadow Context；英文主导且存在中文 gap 时优先补齐英文缺口，不能先整句翻回中文。
