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
