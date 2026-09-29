# Project Progress — v0.0.5
2026-09-30: 用户确认按原定顺序继续 Learning，批准 Microsoft.Data.Sqlite 8.0.31 与锁定传递依赖。最近提出的 Tab 关闭、中文 Shadow 拼音、混合未提交文本识别、youh 提前候选和不完整英文双语提示仅列为未来改进，未改变冻结行为。v0.0.5 本地实现进行中：Save/Library、再遇见去重、Private/Secure 与失败降级。Learning 12 项本地测试 PASS，WinUI 本地 publish PASS；尚待完整 CI、实际窗口和 GitHub 试用包校验。此次实现没有 AI/TSF。
2026-09-29: 用户接受 v0.0.3 有限中英试用，明确要求按计划开发下一版。两处未来改进留到 v0.1.0 后讨论，内容未给出。
本轮仅 Hover / Deep Dive / Windows 发音 / 音标 / 设置，完成后停止试用。GitHub 沿用 mingzorina-bit/congenial-bassoon，codex/peek-v0.0.4 堆叠 v0.0.3。
新增 CMUdict 有限美式数据提案及本轮英式音标缺失提示已获用户批准。无新 NuGet；不做 Learning/AI/TSF。
本地 Host 17 + Onboarding 7 + Expression 10 + Details 18 = 52 tests PASS。WinUI 本地 publish PASS（包漏洞查询因网络提示未完成，待 CI）。原始聊天不上传。
当前等待完整 CI、独立 review、实际窗口验证；不标本轮交付完成。

最终实现 62ee4666864b477896d9696f91355b0a75143944 的完整 CI 36591040997 与 36591033413 均成功。独立复核两项 Important 已修复并 RED→GREEN；本地最终 56/56 tests PASS。Hover 实际界面/单词短语播放状态/word-phrase-sentence Tab-Esc/模块开关/UK missing voice 已验证；详细证据见 docs/releases/v0.0.4.md。正式 artifact 11043534654 正在下载校验。

## 交付
v0.0.4 正式 GitHub 包完成外层/内层 SHA256 和 560 项文件校验，已启动供用户试用。ZIP SHA256 19C75F0FE756B2D3A7C83D7845A2B9E898933861ADCFE716A33EF5CBEC227B2C。当前里程碑达到带已披露覆盖限制的试用状态；完整 Phase 0 尚未完成。停在本版，等待用户试用确认，不推进 v0.0.5。
