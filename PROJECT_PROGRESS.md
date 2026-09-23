# Project Progress
更新时间：2026-09-23。

## 当前状态
v0.0.1 已交付首轮试用：CI c05b7a5、包 10635147909；详见 docs/releases/v0.0.1.md。用户本轮明确授权下一版本，开始 v0.0.2 Shadow。未把上一版未执行的第二台干净机器、持续 UX 验收标成 PASS。

## 不变量与范围
Spec Freeze v0.1 + CR-01～05 为权威。Enter=raw；Space=高亮 Primary；Shift=Shadow action。Core C++20 与 WinUI Host 分离，本地优先，不记录原始输入。
本轮只做本地词级/短语 Shadow、来源追踪、Shift 选择、五步 onboarding/偏好保存/重新体验。禁止推进 Bilingual Context、Hover、Learning、AI、TSF。

## 执行计划
- [ ] T1：本地 LexicalStore / Shadow + 自动测试与真实引擎回归。
- [ ] T2：Shift 按键、五步真实 onboarding、偏好重启保存与故障降级。
- [ ] T3：CI、包校验、真实 UI 冒烟、独立复核、试用交付。

Ruling：新分支 codex/shadow-v0.0.2 基于 0558d6b，保留 v0.0.1 原目录/试用包；本地无 MSVC，C++ 由现有 GitHub Windows CI 验证。短路径 staging 用于本机 WinUI 构建。
Ruling：本轮仅使用项目自有 curated TSV，不新增外部库、第三方词典、服务或费用。来源 OWN/PROTOTYPE_ONLY、逐条 source/entry id；缺失/损坏时保留 Primary。
Ruling：Shift 选择目标取自当前 Shadow 项的语言，不由 profile 强制；完整语言上下文识别仍在 v0.0.3。英文 profile 可保存，但如实提示智能识别尚未实现。

## 恢复规则
每次开工读本文件、冻结规格、CODEX_IMPLEMENTATION_PLAN.md；有效工作后更新。后续顺序 v0.0.3 Context → v0.0.4 Hover → v0.0.5 Learning → v0.0.6 AI → v0.1.0-alpha。当前原始聊天快照留在旧本地目录，不上传。仓库私有化上次受工具额度阻断，未确认私有；本轮按公开内容边界部署。
