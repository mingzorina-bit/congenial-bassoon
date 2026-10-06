# Project Progress — v0.0.6 AI Expression

2026-10-06: 用户要求继续下一里程碑，明确批准 OpenAI API `gpt-5.4-mini`、.NET 自带 HTTP、当期费用及所述隐私/缓存条件；用户将在本机自行配置 API Key，本机目前未检测到。按照冻结计划仅推进 AI Expression：默认关闭的 Cloud assistance、完整句子的异步 Shadow、Tab 按需 Natural Expression、Mock/真实 provider、最小上下文、过期结果拦截与降级。无新增 NuGet 或原始聊天上传。17 项 Provider 测试通过；既有 Host 69 项本地回归通过。独立复核指出无标点完整句子、EN→ZH 语言标记、详情误取消 Shadow 与并发门控问题，均已修正并加入对应回归；随后补测隐私切换后已排队的 UI 回调。WinUI 本地发布和无 Key 窗口启动、设置提示、本地中文编辑已验证。正式 GitHub CI、artifact 校验、真实 API 语义与真人试用尚待完成；不能提前将 AC-13 语义标为 PASS。

## Earlier progress

# Project Progress — v0.0.5
2026-09-30: 用户确认按原定顺序继续 Learning，批准 Microsoft.Data.Sqlite 8.0.31 与锁定传递依赖。最近提出的 Tab 关闭、中文 Shadow 拼音、混合未提交文本识别、youh 提前候选和不完整英文双语提示仅列为未来改进，未改变冻结行为。v0.0.5 完成 Save/Library、再遇见去重、Private/Secure 与失败降级。独立复核发现并修复 Secure/Private 切换问题；窄窗词库按钮与实时计数已修正。69 项本地 C# 测试、WinUI 本地 publish 和最终 GitHub CI 36603684797 均 PASS。正式包已校验 567 项文件并启动；详细证据见 docs/releases/v0.0.5.md。此次实现没有 AI/TSF，停在本版等待用户试用反馈。
2026-09-29: 用户接受 v0.0.3 有限中英试用，明确要求按计划开发下一版。两处未来改进留到 v0.1.0 后讨论，内容未给出。
本轮仅 Hover / Deep Dive / Windows 发音 / 音标 / 设置，完成后停止试用。GitHub 沿用 mingzorina-bit/congenial-bassoon，codex/peek-v0.0.4 堆叠 v0.0.3。
新增 CMUdict 有限美式数据提案及本轮英式音标缺失提示已获用户批准。无新 NuGet；不做 Learning/AI/TSF。
本地 Host 17 + Onboarding 7 + Expression 10 + Details 18 = 52 tests PASS。WinUI 本地 publish PASS（包漏洞查询因网络提示未完成，待 CI）。原始聊天不上传。
当前等待完整 CI、独立 review、实际窗口验证；不标本轮交付完成。

最终实现 62ee4666864b477896d9696f91355b0a75143944 的完整 CI 36591040997 与 36591033413 均成功。独立复核两项 Important 已修复并 RED→GREEN；本地最终 56/56 tests PASS。Hover 实际界面/单词短语播放状态/word-phrase-sentence Tab-Esc/模块开关/UK missing voice 已验证；详细证据见 docs/releases/v0.0.4.md。正式 artifact 11043534654 正在下载校验。

## 交付
v0.0.4 正式 GitHub 包完成外层/内层 SHA256 和 560 项文件校验，已启动供用户试用。ZIP SHA256 19C75F0FE756B2D3A7C83D7845A2B9E898933861ADCFE716A33EF5CBEC227B2C。当前里程碑达到带已披露覆盖限制的试用状态；完整 Phase 0 尚未完成。停在本版，等待用户试用确认，不推进 v0.0.5。
