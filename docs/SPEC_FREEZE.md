# Spec Freeze v0.1 登记
整理日期：2026-09-21。产品工作名 Bilingual Input；仓库名 BilingualInput；品牌名未冻结。

## 权威与冲突
本次用户明确要求采用已确认的五份规格和 CR-01～CR-05。五份规格正文保留既有范围，CR 的澄清优先；实施计划只能安排实现顺序，不能覆盖产品行为。历史原文中的“建议”“请确认”属于当时的讨论状态，不构成重新审批要求。未决技术事项不能自行宣布已通过。
发现无法由 CR 解决的行为矛盾：记录具体条款、复现案例、影响和建议 CR，暂停受影响的实现，继续无关工作。

| 文档 | 状态 | 应用变更 |
|---|---|---|
| 01 Product P-01～P-11 | FROZEN v0.1 | CR-01、CR-02 |
| 02 Interaction I-01～I-15 | FROZEN v0.1 | CR-01、CR-02 |
| 03 Architecture A-01～A-18 | FROZEN v0.1 | CR-03、CR-04 |
| 04 Data D-01～D-19 | FROZEN v0.1 | source boundary、voice availability 复核 |
| 05 Acceptance AC-01～AC-30 | FROZEN v0.1 | CR-04、CR-05 |

## 尚未冻结的实验参数
Shift+Number 人体工学与键盘布局；Hover delay（初始实验 300–500ms）；AI loading（初始实验约 700–1000ms）与 debounce；真实机器 P50/P95 后的性能 SLA；商业词典、IPA 来源、Cloud TTS vendor；Phase 1 TSF compatibility/security。
这些数值是实验起点，不是 SLA。替代冻结按键/行为必须走 CR；不能把可调参数当成全面重新设计许可。

## 阶段边界
Phase 0 是 WinUI 交互原型；不是全系统 IME。Phase 1 前另做 TSF spike：composition、candidate presentation、keyboard interception、highlighted state、commit/cancel；真实宿主安全检测单独验证。IPC、后台服务和正式签名不在 Phase 0。
商业 Release 前执行 Data & Dependency Release Review。内部 Alpha 可使用有明确分发权限的 PROTOTYPE_ONLY 数据，但不得把该标签当成分发许可；UNKNOWN 权限阻断打包。

## 原始依据
[S1 Product](SOURCE_PROVENANCE.md)、[S2 Interaction/CR-01](SOURCE_PROVENANCE.md)、[S3 Architecture](SOURCE_PROVENANCE.md)、[S4 Data](SOURCE_PROVENANCE.md)、[S5 Acceptance/复核](SOURCE_PROVENANCE.md)、[S6 Final Review/CR-02～05](SOURCE_PROVENANCE.md)。
原对话 ID：6aaf87f9-6cd0-83ee-b38f-d1f55d1670a0。最终用户“好的，那进行下一步吧，开始整理移交给codex”，并由本次请求明确要求采用全部 CR；本包不冒充独立技术 PoC、法律审核或 UX 验收。
