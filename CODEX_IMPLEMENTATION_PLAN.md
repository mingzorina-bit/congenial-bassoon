# BilingualInput Codex Implementation Plan

> **For agentic workers:** 使用 superpowers:executing-plans 按任务逐项执行；仅在用户另行选择时使用 subagent-driven-development。步骤用复选框跟踪。

**Goal:** 以 Spec Freeze v0.1 为唯一产品基线，分七个可验收里程碑验证原生双语输入。
**Architecture:** WinUI 3/C# Host 与 C++20 Core 分离，librime 仅负责 Primary。本地优先，Provider 可替换，所有云端经过 Privacy Gate，Phase 0 in-process。
**Tech Stack:** Windows App SDK、WinUI 3、C#/XAML、C++20、librime、SQLite；具体版本不是本包新增的冻结决定。
**Spec:** [冻结登记](docs/SPEC_FREEZE.md)及其中五份规格；[CR](docs/CHANGE_REQUESTS.md)。

## Global Constraints
- Enter=Raw Input；Space=当前 Primary；Shift=Shadow Action。
- Current Expression Intent > Profile Bias；Primary Candidate ≠ Current Expression。
- Word Shadow≤3；Phrase minimum useful choice；Sentence 默认一个。
- 默认不保存完整输入；Secure/Private 约束不能被 cache/log 绕过。
- Local Primary 不等 Network；Local Shadow 不等 LLM；增强失败不能导致输入失败。
- 新外部依赖 explicit approval；技术方向批准不替代具体来源/版本审批。
- 本文的文件路径、任务分配与测试编号是工程安排，不是新增产品行为。不得把示例输出固定为真实 AI 唯一合法句式。

## 工程布局（待 v0.0.1 创建）
```text
src/BilingualCore/        会话、候选、语言上下文、Shadow、隐私规则
src/PrototypeHost/        WinUI/C# UI 与 native interop
src/Lexical/              source-aware local store/import
src/Pronunciation/        phonetic 与 audio adapters
src/Learning/            收藏与 encounter
src/Storage/             SQLite 分区
src/Providers/           lexical/translation/expression/audio contracts
src/Infrastructure/      gateway/cache/diagnostics
tests/CoreTests/         deterministic state/keyboard
tests/fixtures/          自有或批准来源 fixture
tests/semantic/          semantic rubric 与真实结果
tests/manual/            UX/部署/发音操作证据
scripts/                 可复现构建/测试/打包/来源审计入口
docs/releases/           每阶段完成证据
```
native/managed interop 具体 binding、文件后缀和测试 runner 在 v0.0.1 依据批准的工具链确定并记录；不在这里冒充已经存在的 API 或可运行命令。C++ Core 不得包含 WinUI 业务依赖。

## Review Focus
1. A 请求晚到、跨 session 或模式已变化：v0.0.6 必须拒绝结果，不能只取消而不校验。
2. 候选/词典/DB/voice 缺失：对应 v0.0.1/2/4/5 仍能输入。
3. 中英文短词歧义和明确英文 gap：v0.0.3 尊重当前表达而不是 profile。
4. Hover/Tab/设置切换和持续 composition 并发：v0.0.4 不丢输入。
5. 原型数据混入 release、句子混入日志/cache：各阶段审计，v0.1.0-alpha 包级复核。

## 每个任务的执行与证据循环
- [ ] 先读对应条款及前置里程碑证据，确认当前任务范围。
- [ ] 为下列具体测试案例写 deterministic 测试或人工步骤；自动测试先观察预期失败。
- [ ] 只实现让本任务案例通过的最小功能，保持 Core/Host/Provider 边界。
- [ ] 运行本任务和受影响的前序回归；记录实际命令、环境、结果、截图/日志摘要（不得包含真实用户输入）。
- [ ] 审查 diff、依赖来源、产品行为；按仓库授权提交小改动，填写完成记录。
本包是 milestone 移交计划；不提前写全部产品代码或固定跨层函数签名。每阶段开工时仅细化当前阶段的实现测试代码。

## 统一推进规则与完成定义
顺序固定：v0.0.1 → v0.0.2 → v0.0.3 → v0.0.4 → v0.0.5 → v0.0.6 → v0.1.0-alpha。
前置：上阶段 scope 内测试通过、证据提交、产品方验收当前阶段；不得自动越过里程碑大规模实现。
每阶段 DoD：本阶段全部任务完成；绑定 AC 的本阶段子集有证据；前序已通过项无回归；无未批准依赖；无冻结行为变更；可复现构建和运行说明更新；交付可运行 artifact 及风险记录。Alpha 必须有另一台干净 Windows 测试机运行证据。完整 AC 只在全部子项完成时 PASS。
依赖审批：先提交具体清单和依据，未批则阻断该依赖的引入，继续不依赖它的工作；不得用 mock 声称真实 provider/engine 已完成。无行为影响的内部实现选择可自行决定；冻结行为改变必须 CR 批准。

## v0.0.1 — Candidate Foundation

**允许 scope：** 真实 WinUI editor、C++20 Core/InputSession、librime adapter、Primary 列表/高亮、Space/数字/方向/鼠标、Enter raw、最小设置与隐私基础、构建与 artifact。

**禁止项：** Shadow、完整 onboarding、Hover、Learning、云端 provider、TSF、IPC；不能拿 hardcoded candidates 代替 librime。

**依赖审批：** 核定 WinUI/Windows App SDK、.NET/C++ 工具链、librime 与独立 schema/data 的具体版本；不得直接搬用 Weasel。

**绑定验收：** AC-01、04、05、24；AC-03/14/25/26/27/28 的基础子集。

### v0.0.1-T1 候选核心与可复现构建

- **创建/修改：** src/BilingualCore/InputSession.*; src/BilingualCore/RimeAdapter.*; tests/CoreTests/CandidateTests.cpp; CMakeLists.txt
- **接口边界：** 审核后的 Rime composition → PrimaryCandidates/highlight/raw。会话拥有状态，UI 不能自行猜词。
- **验收测试：** 真实引擎输入 youhua 得合理候选（含优化；油画/有话作为代表集核验，排序不锁死）；两个不同输入连续更新；空候选仍可 Enter raw。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.0.1-T2 键盘与可持续编辑

- **创建/修改：** src/PrototypeHost/EditorPage.xaml; src/PrototypeHost/EditorPage.xaml.cs; src/PrototypeHost/Interop/*; tests/CoreTests/KeyboardTests.cpp
- **接口边界：** Host 传递按键，Core 返回 presentation 与 commit；C#↔C++ 绑定形式在本任务工程记录中确定，不新增未批准框架。
- **验收测试：** youhua+Enter=youhua；asdfg+Enter=asdfg；Space 提交高亮；数字选择；方向/鼠标改变同一高亮；连续输入不需 Demo 按钮。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.0.1-T3 隐私基础、CI 与可分发运行

- **创建/修改：** src/BilingualCore/PrivacyPolicy.*; src/Infrastructure/Diagnostics/*; tests/CoreTests/PrivacyTests.cpp; .github/workflows/build.yml; README.md
- **接口边界：** 只记录计数/耗时等无内容事件；CI 构建 Core、Host、Core Tests。
- **验收测试：** 无 key 启动、断网 Primary 工作；唯一测试文本不出现在日志；干净 checkout 按 README 构建；记录第二台 Windows 上启动及 Primary 操作。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

**阶段 DoD：** 统一 DoD 全部满足，并且上列 3 个任务的测试有证据。提交 v0.0.1 完成记录，停在本阶段验收边界；未到本阶段的 AC 保持未执行，不能标为 NOT IN PHASE 0。

## v0.0.2 — Shadow

**允许 scope：** Lexical Store/来源隔离、一个高亮一个 Shadow、word/phrase、Shift 提交、本地降级、真实五步 onboarding。

**禁止项：** 真实云端 AI、整套语言分类学习模型、Hover/Deep Dive/Learning 的提前实现；不得强行凑满 3 个词。

**依赖审批：** curated 与 reviewed 词库逐项审核；CC-CEDICT 边界保留；ECDICT 候选不能直接转正式依赖。

**绑定验收：** AC-02、06、07、08；AC-14/16/25 的本地部分。

### v0.0.2-T1 本地 Shadow 与 provenance

- **创建/修改：** src/Lexical/*; src/BilingualCore/ShadowEngine.*; src/BilingualCore/ShadowRanker.*; tests/CoreTests/ShadowTests.cpp
- **接口边界：** 输入高亮候选+语言上下文，返回带 sourceId 的有序 Shadow。排序为 context correctness → preference → frequency。
- **验收测试：** 优化→optimize/improve/refine，油画→oil painting；切换后不残留旧义；word≤3；phrase 优化方案采用短语语义映射，不能拼 optimize+plan；来源缺失时 Primary 可用。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.0.2-T2 Shadow 按键与 onboarding

- **创建/修改：** src/PrototypeHost/Onboarding/*; src/PrototypeHost/Candidates/*; tests/CoreTests/ShadowSelectionTests.cpp
- **接口边界：** Shift+Enter 提交推荐 Shadow；Shift+1–5 概念保留并实测，不把 Shift 固定成英语。
- **验收测试：** 真实完成 Welcome/Primary Language/Input Preference/Goals/First Success；Space→优化，Shift+Enter→optimize；重启不重复引导，可重进 demo；记录 Shift+Number 布局/冲突证据。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

**阶段 DoD：** 统一 DoD 全部满足，并且上列 2 个任务的测试有证据。提交 v0.0.2 完成记录，停在本阶段验收边界；未到本阶段的 AC 保持未执行，不能标为 NOT IN PHASE 0。

## v0.0.3 — Bilingual Context

**允许 scope：** Dominant language、segments/gaps/confidence、Primary Candidate 与 Current Expression 分离、双向混输、明确英文、歧义与基础 typo。

**禁止项：** Profile 强制中文；无理由抹去合法 English；训练重型分类器；整句 AI 润色提前进入。

**依赖审批：** 优先复用已批准词典与本地规则；新分词/识别库必须另批。

**绑定验收：** AC-09、10、11、12；复测 AC-04～08。

### v0.0.3-T1 上下文与 gap 本地路径

- **创建/修改：** src/BilingualCore/LanguageContextResolver.*; src/BilingualCore/LanguageSegmenter.*; src/BilingualCore/GapResolver.*; tests/CoreTests/LanguageContextTests.cpp
- **接口边界：** session snapshot → dominant/segments/gaps/confidence/reason；Shadow Intent Resolver 分辨跨语言映射和 gap completion。
- **验收测试：** zhege design hai keyi：ZH 且保留 design；ZH profile 下 I think this 方案 is better：EN/gap 方案并补英文；We probably need to 优化 this part. 保持英文框架与其他含义。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.0.3-T2 歧义与 typo

- **创建/修改：** tests/CoreTests/AmbiguityTests.cpp; tests/fixtures/language-context.json; src/BilingualCore/RimeAdapter.*
- **接口边界：** 上下文优先于 profile prior；typo 恢复进入 Primary 后才生成 Shadow。
- **验收测试：** design 不强行拼音；shi/he/can/an/in/me 在不同上下文可观察 decision/confidence/reason；不要求全对但必须经过 resolver；youhha 在明显拼音上下文恢复，Shadow 使用纠正后的候选。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

**阶段 DoD：** 统一 DoD 全部满足，并且上列 2 个任务的测试有证据。提交 v0.0.3 完成记录，停在本阶段验收边界；未到本阶段的 AC 保持未执行，不能标为 NOT IN PHASE 0。

## v0.0.4 — Hover / Deep Dive

**允许 scope：** Quick Peek、Tab/Esc、模块设置、来源可追踪 IPA、本地 Windows TTS、US/UK、缺失 voice 降级。Natural Expression 区域仅消费已有结果/明确标注的测试 Mock，真实能力在 v0.0.6。

**禁止项：** Hover 自动播放；候选行铺音频/收藏图标；LLM 生成权威 IPA；任意 MP3；自由拖拽；把 Mock 当真实 AI 验收。

**依赖审批：** IPA source 单独审核；本地 Windows Speech 先行，Cloud TTS 可选不作为门槛；任何音频 cache 遵循 provider terms。

**绑定验收：** AC-17、18、19；AC-20 交互/设置部分；AC-26 卡片部分。

### v0.0.4-T1 Quick Peek 与 pronunciation

- **创建/修改：** src/PrototypeHost/QuickPeek/*; src/Pronunciation/WindowsSpeechProvider.*; src/Pronunciation/PhoneticDataProvider.*; tests/PronunciationTests/*
- **接口边界：** IPA 和 audio 独立，locale 同步影响展示/voice；默认 American；点击才播放。
- **验收测试：** Hover 延迟出现、移开关闭、设置关闭不再弹；默认 word/IPA/POS/meaning/speaker/save；refine 与 further improve 真播放；无合适 voice 不崩溃；IPA 可反查 word/locale/sourceId；UK 切换不只更换 audio。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.0.4-T2 Deep Dive 状态保留

- **创建/修改：** src/PrototypeHost/DeepDive/*; tests/CoreTests/ExpansionStateTests.cpp; tests/manual/deep-dive.md
- **接口边界：** Tab 展开，Esc 关闭回到原 composition；顺序 Sentence→Phrase→Vocabulary；模块尊重开关。
- **验收测试：** word/phrase/sentence 均可开关；重复 Tab/Esc、Hover 期间继续输入不丢 composition；Examples 默认关闭，Natural Expression 不强行生成第二句。Save 在 v0.0.5 接通，当前不能报告 AC-21 通过。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

**阶段 DoD：** 统一 DoD 全部满足，并且上列 2 个任务的测试有证据。提交 v0.0.4 完成记录，停在本阶段验收边界；未到本阶段的 AC 保持未执行，不能标为 NOT IN PHASE 0。

## v0.0.5 — Learning

**允许 scope：** SQLite 分区、Save/Library、Encounter、偏好与用户词典隔离、Private/Secure 全面写入控制、数据故障降级。

**禁止项：** 默认保存完整句子；AI mastery scoring、间隔重复学习大系统、同步、账号、导出；用户偏好压过语义正确性。

**依赖审批：** SQLite 版本与绑定经审批；不得为简单存储额外引入未经批准 ORM；用户与第三方数据分开。

**绑定验收：** AC-21、22、23、24；AC-14、20 的学习部分。

### v0.0.5-T1 收藏与再遇见

- **创建/修改：** src/Learning/*; src/Storage/*; tests/LearningTests/*; src/PrototypeHost/LearningLibrary/*
- **接口边界：** 保存 lemma、meaning/source mapping、IPA、POS、pronunciation reference、savedAt、encounterCount；可从 Hover/Deep Dive Save。
- **验收测试：** 保存 refine，重启 Library 可读；之后自然再次成为 Shadow 则计数增加；普通 UI 重绘不得制造虚假再次遇见；星标克制、不宣称掌握；不写回第三方词库。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.0.5-T2 隐私与故障

- **创建/修改：** tests/PrivacyTests/*; tests/StorageTests/*; src/Infrastructure/PrivacyGate.*
- **接口边界：** 所有写入/cache/log 服从同一 privacy policy；上下文例句只有用户主动允许才存。
- **验收测试：** Secure 下 cloud/learning/content-log/persistent-text-cache 全零；Private 本地可输入但无 cloud/learning/history；DB 不可用输入仍工作；扫描日志/DB/cache 不含唯一原文；模式切换后不接纳旧任务内容。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

**阶段 DoD：** 统一 DoD 全部满足，并且上列 2 个任务的测试有证据。提交 v0.0.5 完成记录，停在本阶段验收边界；未到本阶段的 AC 保持未执行，不能标为 NOT IN PHASE 0。

## v0.0.6 — AI Expression

**允许 scope：** 一个真实 LLM provider + Mock；异步 Gateway、最小上下文、debounce/cancel/revision、sentence Shadow/Natural Expression、延迟与错误降级。

**禁止项：** 逐按键无节制调用；UI 直接请求网络；固定 provider 产品逻辑；自动扩写意图、多 tone、永久缓存文本、泄露 key；把真实 AI exact-string 化。

**依赖审批：** 真实 provider 供应商、SDK/HTTP 库、费用/隐私与缓存条件明确审批；Azure/其他名称不是已选供应商。

**绑定验收：** AC-13、15、16；AC-20 全部适用内容；AC-23/24/25 网络路径。

### v0.0.6-T1 Gateway 与异步不变量

- **创建/修改：** src/Providers/*; src/Infrastructure/ProviderGateway.*; tests/ProviderTests/*
- **接口边界：** 请求携带 sessionId/revision/requestId；发送前过 Privacy Gate，接收前再次核验有效状态；Core 不依赖品牌。
- **验收测试：** 模拟 A(rev10) 晚于 B(rev11) 返回，A 不覆盖；跨 session、已提交/取消、切 Secure 后返回均不能污染当前 UI；超时/HTTP error/invalid response/unavailable 保留 Primary/Local Shadow；无需真实 key 的 Mock 自动测试。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.0.6-T2 真实语义与按需增强

- **创建/修改：** tests/semantic/cases.md; tests/manual/ai-expression.md; src/PrototypeHost/LoadingState.*
- **接口边界：** 句子默认一个主要 Shadow；Tab 才按需 Natural Expression；局部 gap 最小改写。
- **验收测试：** 中文完整句按意义/语法/自然度/无虚构意图评审；不锁单一句式；词级正常走本地且 provider call count=0；快响应不闪 loading，慢响应弱提示；断网可输入；请求只含最少必要上下文。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

**阶段 DoD：** 统一 DoD 全部满足，并且上列 2 个任务的测试有证据。提交 v0.0.6 完成记录，停在本阶段验收边界；未到本阶段的 AC 保持未执行，不能标为 NOT IN PHASE 0。

## v0.1.0-alpha — Integrated Phase 0

**允许 scope：** 回归、真实 10–20 分钟 UX、Light/Dark、性能测量、干净机器部署、来源审计、三项最终 Gate。

**禁止项：** TSF/全系统输入、IPC、正式商店/签名/商业词典等 AC-29 项；不得以 Alpha 名义默认商业授权。

**依赖审批：** 对 artifact 实际内容复审，UNKNOWN 权限排除；PROTOTYPE_ONLY 仅在获准的内部原型用途；商业分发另过 Release Gate。

**绑定验收：** AC-01～28 全覆盖；AC-29=NOT IN PHASE 0；AC-30 最终决策。

### v0.1.0-alpha-T1 集成与发布候选证据

- **创建/修改：** tests/manual/phase0-acceptance.md; docs/releases/v0.1.0-alpha.md; scripts/package.*; scripts/audit-sources.*
- **接口边界：** 构建→artifact→干净测试机→核心场景；记录硬件/OS/runtime/版本/hash；无 key 和断网路径单独测试。
- **验收测试：** P50/P95 实测 Primary/Local Shadow/Resolver/AI/UI render latency，不自造 SLA；候选约五个常规中文宽、长项少显示、sentence 约两行、Light/Dark、无持续动画；来源清单与包内文件对应。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

### v0.1.0-alpha-T2 用户体验与产品决策

- **创建/修改：** docs/reviews/phase0-gates.md; docs/CHANGE_REQUESTS.md
- **接口边界：** Engineering、UX、Product 三项独立决策。
- **验收测试：** 10–20 分钟真实输入覆盖全链；记录 Shift/Hover/选择量/延迟；真人评估是否比输入后转翻译软件自然；不满意记 FAIL/CR，不用代码覆盖率代替批准。
- [ ] 按统一循环先建立上述测试与预期结果。
- [ ] 实现并运行测试，保存证据。
- [ ] 检查 scope、来源与回归，完成任务审核。

**阶段 DoD：** 统一 DoD 全部满足，并且上列 2 个任务的测试有证据。Engineering、UX、Product Gate 分别签署，任何一个失败均不宣称 Phase 0 完成。

## 完成记录模板
版本/commit/artifact hash；允许 scope 与实际改动；测试名称→对应 AC→命令/步骤→预期→实际→证据；依赖批准引用；已知限制；性能测量环境；手工 UX 评审人；阶段通过/未通过决定；下一阶段是否获准。
计划初建时测试尚未执行。当前 v0.0.1 实际进度、测试证据和未完成验收以 docs/releases/v0.0.1.md 为准；后续里程碑未执行，不能推定 AC 通过。


