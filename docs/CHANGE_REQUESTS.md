# 变更记录
以下均纳入本次用户指定的冻结基线；不扩大 Phase 0 范围。

| ID | 生效定义 | 影响 | 验证 |
|---|---|---|---|
| CR-01 | Shift = Shadow Action；首发中文体验可表现为 Shift → English | P-06/I-02 | 英文 gap 场景不得按 Profile 固定翻译目标 |
| CR-02 | Shadow 为 contextual assistance；通常跨语言映射，gap 时优先补当前 dominant language | P-05/I-04/A-06 | AC-06、09、10、13 |
| CR-03 | Primary Candidate ≠ Current Expression；分别建模 | A-03/A-05/A-06 | 高亮切换与整句 gap 分别测试 |
| CR-04 | Phase 0 安全阻断真实，宿主安全检测模拟；Phase 1 另验证 | A-10/AC-23 | 四个零计数；不声称系统密码框全覆盖 |
| CR-05 | Deterministic / Semantic / Human 三层测试 | A-17/05 | 状态精确、语义评审、真人体验独立记录 |

后续 CR 从 CR-06 起编号。每条记录原条款、问题证据、建议行为、受影响测试/数据/部署、批准者与决定、目标版本。批准前冻结行为继续有效；自动化 agent 无权自批产品变更。

## CR-06 — Tab 再按关闭 Details（用户批准，目标 v0.1.1）

- 原条款：02 Interaction I-13 与 AC-20 为 Tab 打开、Esc 关闭。
- 试用证据：用户在 v0.1.0-alpha 试用后明确提出“Tab 键展开 details，也可以 close detail”，并要求继续开发；Esc 仍保留。
- 生效行为：编辑区 Tab 展开 Details；Details 已展开时，无修饰键 Tab 关闭并将焦点还给编辑区。Esc 仍关闭。Shift/Ctrl/Alt+Tab 不改变原有含义；输入会话、候选高亮、原文和 Shadow 不被提交或清空。上下文变化时仍按现有规则关闭旧详情。
- 影响：仅 Host 键盘处理与提示文案；不引入数据或外部依赖，不改变隐私/AI 请求策略。
- 验证：连续 Tab→Tab、Tab→Esc、焦点位于详情按钮时 Tab 关闭；修饰键不触发关闭；输入组合、候选及旧异步结果回归。
- 批准者与决定：用户于 2026-10-12 直接批准上述交互方向，并要求进入下一开发阶段。按其明确指令实施。
