> 状态：FROZEN v0.1 · 根据已确认原对话整理，原有第一人称及示例为规格说明。CR-01～CR-05 的澄清优先，见 [变更记录](CHANGE_REQUESTS.md)。
> 来源：source/S1.md；可选、SPIKE、未来能力保留原有性质，不能因冻结而变成必做。

# 01 — Product Spec v0.1

## 1. 产品定义

**Working Name**

> **Bilingual Input**

当前仓库工作名：

> `BilingualInput`

品牌名称暂不冻结，未来可以单独命名。

### 产品是什么

Bilingual Input 是一个以 **Bilingual Native Input（原生双语输入）** 为核心的 Windows 输入产品。

它不是：

> 中文输入法 + 翻译按钮

也不是：

> AI 翻译悬浮窗

也不是：

> 把一句中文输入完成以后再翻译成英文。

核心体验是：

> **用户继续按照自己熟悉的方式输入，系统理解当前实际表达语言，并在输入过程中提供 Primary Candidate、Shadow Candidate 以及必要的跨语言补全。**

最终目标是让：

> **输入、跨语言表达和语言学习发生在同一个输入动作中。**

### 验收项 P-01

我建议冻结以上产品定义。

已确认，FROZEN v0.1。

---

# 2. 目标用户定义

这里采用我们刚刚修正后的版本。

第一阶段不是简单定义：

> ❌ 中文用户  
> ❌ 英语不好的人  
> ❌ 英语学习者

而是：

> **Primary Language 为中文，并存在真实中文 + English 双语输入需求的用户。**

“Primary Language = 中文”表示：

> 中文是用户**更熟悉、更有表达把握的语言**。

它**不代表用户英语水平低**。

因此第一阶段可以覆盖：

初级英语用户、中级英语用户、高级英语用户，以及能够大部分用英语表达、但偶尔存在词汇/表达缺口的用户。

例如高级一些的用户可能输入：

```text
I think this 方案 is better because...
```

或者：

```text
We probably need to 优化 this part.
```

系统必须理解：

> **当前用户正在用 English 表达。**

中文只是当前出现的 Language Gap。

因此正式区分三个概念：

```text
Primary Language
用户长期更熟悉的语言

Secondary Language
用户需要使用 / 学习 / 辅助表达的另一语言

Current Expression Language
当前这句话实际上正在使用的主要语言
```

例如：

```text
Primary Language          ZH
Secondary Language        EN
Current Expression        EN
```

完全正常。

### 核心判断原则

> **Current Expression Intent > Profile Bias**

用户 Profile 是辅助判断，不是强制规则。

### 验收项 P-02

**确认以上目标用户和三层语言模型。**

---

# 3. 产品解决的核心问题

第一阶段解决三个核心需求。

### A. Cross-language Expression

用户知道自己想表达什么，但第二语言的：

词汇、短语、搭配或者自然表达存在缺口。

例如：

```text
wo juede zhege fangan hai keyi jixu youhua
```

可以获得：

> I think this approach could be further improved.

也可能是：

```text
I think这个方案 still needs 优化
```

系统帮助补齐：

> I think this approach still needs improvement.

---

### B. No-switch Bilingual Input

用户不应该不断执行：

> 切中文输入法 → 打中文 → 切英文 → 打英文 → 再切回来。

产品目标是：

> **中文、English、中英混输都可以在同一个输入环境中自然发生。**

---

### C. Learning by Real Use

用户不需要退出工作：

> 打开词典 → 搜索 → 看 IPA → 听发音 → 收藏 → 返回原应用。

而是在真实输入中：

```text
Shadow Candidate
      ↓
Hover
      ↓
IPA / Meaning / Audio
      ↓
Save
      ↓
Encounter again
```

逐渐积累真实需要的词汇。

学习是：

> **输入体验的增强层**

而不是：

> 输入产品变成背单词软件。

### 验收项 P-03

确认核心需求为：

**跨语言表达 + 无切换双语输入 + 真实使用中学习。**

---

# 4. 核心语言模型

这是这一版相比此前最大的正式升级。

系统不能只有：

```text
Chinese Mode
English Mode
```

也不能只有：

```text
Chinese → English
```

而应该理解：

```text
User Profile
      ↓
Input Stream
      ↓
Language Context Resolver
      ↓
Dominant Language
      +
Language Segments
      +
Language Gaps
```

例如：

```text
zhege design hai keyi
```

可以理解成：

```text
Dominant = ZH

[ZH] zhege
[EN] design
[ZH] hai keyi
```

而：

```text
I think this 方案 needs 优化
```

则：

```text
Dominant = EN

[EN] I think this
[ZH-GAP] 方案
[EN] needs
[ZH-GAP] 优化
```

所以系统解决的并不是简单 Translation，而是：

> **理解当前表达框架 → 找出跨语言缺口 → 在尽量不改变其他内容的情况下补齐缺口。**

### 验收项 P-04

确认把：

**Dominant Language + Language Segmentation + Language Gap**

正式提升为核心产品概念。

---

# 5. Shadow Candidate 定义

这是产品最核心的 UI / interaction primitive。

正式定义：

> **Shadow Candidate 是相对于当前 Primary Candidate / Current Expression 的辅助表达层。通常提供另一语言的语义映射；存在 Language Gap 时，优先提供符合当前 Dominant Language 的补全。**（CR-02）

它不是：

> raw input 的机械翻译。

例如：

```text
youhua

1 优化    2 油画    3 有话
  ↑
──────────────────────────
EN optimize · improve · refine
```

移动：

```text
1 优化    2 油画    3 有话
          ↑
──────────────────────────
EN oil painting
```

Shadow 随当前 Primary 改变。

完整句子：

```text
我觉得这个方案还可以继续优化
────────────────────────────
EN I think this approach could be further improved.
```

Shadow 同时允许承担：

**Translation / Semantic Mapping / Language Gap Assistance / Natural Expression**

但用户不需要理解这些技术分类。

### 验收项 P-05

确认 Shadow Candidate 以上正式定义。

---

# 6. Product Constitution v0.1

我把此前原则重新整理了一次，现在建议冻结成 **11 条**：

1. **Primary First** — 熟悉语言是长期 Primary，但不覆盖当前表达意图。
2. **Current Intent First** — 当前句子的表达语言优先于用户 Profile bias。
3. **Muscle Memory First** — 尽可能保留成熟输入法已有肌肉记忆。
4. **Shift = Shadow Action** — Shift 系列操作承担 Shadow 行为。（CR-01）
5. **Minimum Intervention** — 已经正确表达的内容尽量不修改。
6. **Local First, AI Second** — 高频基础输入必须脱离 AI 也能工作。
7. **Shallow UI, Deep Intelligence** — 默认浅层，智能和学习按需展开。
8. **Privacy by Architecture** — 敏感内容从架构层阻断云端和学习。
9. **More Context, More Confidence** — 上下文越少，系统越克制。
10. **Personalize Judgment, Not Complexity** — 个性化改变判断，不增加操作负担。
11. **AI Failure Must Never Become Input Failure** — 云端失效不能破坏基本输入。

这里把原来的 `Respect User Intent` 实质升级成了第 2 + 第 5 条。

### 验收项 P-06

确认这 **11 条 Product Constitution**。

---

# 7. 产品价值优先级

为了以后 Codex 或我们自己遇到冲突时知道该牺牲什么，我建议明确：

```text
输入可靠性
   ↓
输入速度
   ↓
用户表达意图
   ↓
跨语言准确性
   ↓
自然表达质量
   ↓
学习价值
   ↓
视觉效果
```

举个非常重要的例子：

如果：

> AI Natural Expression 更漂亮

但需要让候选框卡 800ms，

选择：

> **先显示本地普通 Shadow。**

如果：

> AI 认为可以把一句话改得更漂亮

但用户原句已经正确，

选择：

> **尊重原句。**

如果：

> Hover 学习卡可以展示 12 项语言知识

但会让输入体验很重，

选择：

> **少显示。**

这套优先级以后可以避免产品越做越“AI”。

### 验收项 P-07

确认以上价值优先级。

---

# 8. Phase 0 产品边界

Phase 0 的目标不是：

> 做出可以替代微软拼音的正式 IME。

而是验证：

> **这套双语输入模型是否真的好用。**

所以 Phase 0 是：

**Windows Interactive Prototype**

但不是静态 Figma Demo。

它必须能够真实输入、真实操作快捷键、真实变化候选、真实调用词典/AI、真实收藏学习词。

也就是说你可以实际坐下来用它：

```text
type
type
type
Space
type
Shift+Enter
Tab
Esc
hover
save
```

然后判断：

> “这个东西到底舒服不舒服。”

### Phase 0 成功的定义

不是：

> UI 看起来像输入法。

而是：

> **我们连续实际使用后仍然认为 Shadow Candidate + Current Language Context + Shift interaction 值得继续做成真正 TSF IME。**

### 验收项 P-08

确认 Phase 0 的定位与成功标准。

---

# 9. 第一阶段明确不是这些产品

这是 Scope Guard。

Phase 0 **不是**：

AI 写作助手、Grammarly 替代品、DeepL 替代品、完整语言学习 App、微软拼音替代品、ChatGPT 输入框、翻译悬浮窗、通用多语言输入法。

即使未来其中一些能力会出现，也不能改变第一阶段产品核心。

第一阶段只有：

> **Bilingual Native Input Prototype**

### 验收项 P-09

确认这个 Scope Guard。

---

# 10. 用户数据原则

正式冻结：

**默认不保存用户完整聊天/输入句子。**

允许保存：

用户设置、学习收藏、Encounter Count、候选偏好统计，以及实现产品必要的非敏感本地状态。

上下文例句如果需要进入 Learning Library：

> 用户必须主动允许。

密码/Secure Input：

> Cloud OFF  
> Learning OFF  
> Logging OFF

未来 Private Mode：

> Local ON  
> Cloud OFF  
> Learning/History OFF

同时我们遵守：

> **收集数据必须有具体产品用途。**

不能以：

> “以后 AI 可能用得到”

为理由默认保存用户输入。

### 验收项 P-10

确认数据原则。

---

# 11. 产品方向最终一句话

如果以后有人问：

> “Bilingual Input 到底在做什么？”

我建议内部标准答案不是：

> AI 输入法。

也不是：

> 中英翻译输入法。

而是：

> **Bilingual Input 是一种以用户当前表达意图为中心的原生双语输入体验：用户继续用自己熟悉的方式输入，在需要时自然获得另一种语言的词汇、表达和学习辅助，而无需不断切换输入模式。**

这个定义足够宽，可以容纳未来：

`ZH ↔ EN`

甚至：

`EN ↔ JA / ZH ↔ JA`

但又不会把我们变成“什么 AI 都做”。

### 验收项 P-11

确认这个最终产品定义。

---
