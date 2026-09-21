# Acceptance Traceability Matrix

D=Deterministic；S=Semantic；M=Human/manual。下表是测试计划，**尚未执行**；不预填 PASS。最终结果仅 PASS/FAIL/NOT IN PHASE 0，后者仅适用于真正不在 Phase 0 范围的事项（如 AC-29），不能用来隐藏延后/失败项。

| AC | 能力 | 里程碑 | 层级 | 关键证据 |
|---|---|---|---|---|
| AC-01 | 构建/启动，无 key | 0.0.1 → alpha | D+M | clean checkout，Core/Host/tests 构建并启动 |
| AC-02 | 真实五步 onboarding | 0.0.2 → alpha | D+M | Space/Shift+Enter 真提交、设置持久、可重进 |
| AC-03 | 持续 Input Session | 0.0.1 起，alpha 完整 | M | 10–20 分钟输入/候选/Shadow/Tab/Esc |
| AC-04 | 真实 Primary | 0.0.1 | D+M | 真实 librime，合理候选，所有选择路径 |
| AC-05 | Enter raw | 0.0.1 | D | asdfg 与 youhua 原样提交 |
| AC-06 | Shadow highlight | 0.0.2 | D+S | 优化→油画时切语义且仅一个 context |
| AC-07 | Shadow selection | 0.0.2 | D+M | ≤3、Shift+Enter；Shift+Number spike 独立报告 |
| AC-08 | Phrase | 0.0.2 | D+S | 短语测试集，非机械逐词拼接 |
| AC-09 | ZH mixed | 0.0.3 | D+S | zhege design hai keyi 保留 design |
| AC-10 | EN gap | 0.0.3 | D+S | 两组冻结英文 gap，ZH profile 不强转中文 |
| AC-11 | English/ambiguity | 0.0.3 | D | design 与六个歧义 token 的可观察 resolver |
| AC-12 | Basic typo | 0.0.3 | D+S | youhha 恢复；Shadow 消费纠正 Primary |
| AC-13 | Sentence | 0.0.6 | D+S | 一个主要句子；保义/语法/自然/无虚构意图 |
| AC-14 | Local First | 0.0.1/2/5 → alpha | D+M | 断网候选/本地 Shadow/选择/Enter/Learning UI |
| AC-15 | Stale AI | 0.0.6 | D | rev10 A 晚于 rev11；跨 session 与取消补测 |
| AC-16 | AI failure | 0.0.6 | D+M | timeout/HTTP/invalid/unavailable；可输入且不泄露技术错误 |
| AC-17 | Quick Peek | 0.0.4 | D+M | delay/off/mouse leave，无自动播放 |
| AC-18 | Pronunciation | 0.0.4 | M+D | 真实 word/phrase 音频及无 voice 降级 |
| AC-19 | IPA provenance | 0.0.4 | D | word/locale/IPA/sourceId 可追踪 |
| AC-20 | Deep Dive | 0.0.4/5/6 → alpha | D+M | 三种状态 Tab/Esc；保 composition；模块开关 |
| AC-21 | Collection | 0.0.5 | D+M | 所需字段持久、Library 查看、不默认存整句 |
| AC-22 | Encounter | 0.0.5 | D+M | 自然再次成为 Shadow 增加计数；不等于掌握 |
| AC-23 | Secure/Private | 0.0.1 基础，0.0.5/6 完整 | D | 四个零；Private 本地可用；模拟检测/真实阻断 |
| AC-24 | Privacy logs | 各阶段，alpha 总扫 | D | 唯一测试字符串不在正常日志；存储/cache 补测 |
| AC-25 | Performance | 各阶段，alpha 测量 | D+M | 本地不等网；UI 不阻塞；五种 latency P50/P95 |
| AC-26 | Visual | 0.0.1/4 → alpha | M | compact/Light-Dark/Primary 权重/约五候选/句子约两行 |
| AC-27 | Deployment | 各阶段 artifact，alpha clean machine | M | Build→Artifact→另一台机器→核心场景 |
| AC-28 | Dependencies | 各阶段，alpha 包级 | D+审核 | 清单对应实际包；识别 PROTOTYPE_ONLY，未知权利阻断 |
| AC-29 | Out of scope | 全程 | 范围审核 | 冻结排除列表保持 NOT IN PHASE 0 |
| AC-30 | Final Gate | alpha | D+S+M | Engineering + UX + Product，各自真实决定 |

## Semantic rubric

每案记录 source expression、dominant/target language、必须保留含义、禁止新增含义/实体、容许表达、实际结果、评审结论及理由。固定自有 fixture/Mock 可精确断言；真实 LLM 不以示例句唯一匹配。错误语言、丢失必要含义、虚构实体/意图判失败；语法自然度由评审记录，未冻结数字评分阈值。

## 证据状态

执行期间记录：未执行/执行中/已完成，另设最终判定字段。Milestone 子集完成不自动让整项 AC PASS；例如 AC-20 的真实 Natural Expression 到 v0.0.6 才齐全。最终 AC-30 的人类 UX/Product 判定不可由 agent 代签。

