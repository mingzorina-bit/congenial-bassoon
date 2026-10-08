# Project Progress — v0.0.6 AI Expression

2026-10-07: 用户要求继续 v0.1.0-alpha，账户 API 无余额。开始 alpha-T1：打包版本改为 v0.1.0-alpha，新增 package 文件哈希、清单与已审核 data 路径复核脚本，CI 在上传前运行；新增完整 AC 人工验收记录与三 Gate 待决文档。未加入新词典、未改冻结输入行为。PowerShell 脚本语法解析通过；以九文件 fixture 验证清单 PASS、额外数据被拒，修正了 Windows 路径分隔符问题。当前环境缺少 dotnet/cmake，C# 与 native 回归以及 WinUI package 尚不能在本机重跑。25 个待提交文件已做密钥样式扫描，三个命中均为文档占位符或测试用虚构值，`.env` 未跟踪；仓库当前为 public。GitHub 写入被自动审批拦下，Windows CI 未运行；真实 API 语义、性能和干净机器验收待测。本条只记录 alpha 候选工作进度，不宣布里程碑通过。

2026-10-07: 按用户要求补齐项目级 `OPENAI_API_KEY` 配置。仓库根目录新增空的 `.env.example` 与本机 `.env` 模板，`.gitignore` 忽略 `.env*` 但保留示例；WinUI Host 启动时读取项目根目录 `.env`，已有进程环境变量优先。缺失或空 Key 保持本地输入并显示不含敏感信息的提示。Provider 测试新增项目文件加载、优先级、缺失/空值及无 Key 错误检查；Host 17、Onboarding 7、Details 22、Learning 13、Expression 10、Provider 19，共 88 项本地 C# 回归通过，短路径下 WinUI Release 构建 0 警告/0 错误。真实 Key 未写入源码、日志或提交；真实 API 语义仍待用户本机填写 Key 后验证。

2026-10-07: 用户的 API Key 与网络诊断正常，但账户返回 `credit_balance_exhausted`，用户批准暂以显式 AI 演示模式继续。新增可持久化的 Demo AI 开关与可替换 provider 路由；演示模式只调用有限固定 Mock、不联网、不收费，界面和详情明确标注 Mock，且不要求同时开启 Cloud assistance。Private/Secure 隐私门保持生效；真实 provider 路径不变。Host 17、Onboarding 7、Details 22、Learning 13、Expression 10、Provider 22，共 91 项本地 C# 回归通过，短路径下 WinUI Release 构建 0 警告/0 错误；真实语义/延迟/费用验收继续保持待执行。

本机演示验证：新发布的 `BilingualInput-demo-local` 启动正常；用户粘贴登记的完整中文句后，确认出现英文 Shadow 及“AI 演示结果（Mock，不联网）”标注。该证据只确认本地演示交互链路，不替代真实 API 语义验收。

2026-10-06: 用户要求继续下一里程碑，明确批准 OpenAI API `gpt-5.4-mini`、.NET 自带 HTTP、当期费用及所述隐私/缓存条件；用户将在本机自行配置 API Key，本机目前未检测到。按照冻结计划仅推进 AI Expression：默认关闭的 Cloud assistance、完整句子的异步 Shadow、Tab 按需 Natural Expression、Mock/真实 provider、最小上下文、过期结果拦截与降级。无新增 NuGet 或原始聊天上传。17 项 Provider 测试通过；既有 Host 69 项本地回归通过。独立复核指出无标点完整句子、EN→ZH 语言标记、详情误取消 Shadow 与并发门控问题，均已修正并加入对应回归；随后补测隐私切换后已排队的 UI 回调。WinUI 本地发布和无 Key 窗口启动、设置提示、本地中文编辑已验证。真实 API 语义与真人试用尚待完成；不能提前将 AC-13 语义标为 PASS。

正式交付：最新 GitHub Actions run 37484316733 成功；artifact 11423735176 外层 SHA-256 `01216B9B810249F02562388756AF030B365961FCE65AD8690FF188E9C0A2919E` 与 GitHub 一致，内部 ZIP SHA-256 `3DFCDE78C42D671C6EC91A34D7C4A9AEBF231FDF67C931FE6FA991326FCA8C84`，567 个文件逐项校验且无额外文件。详见 docs/releases/v0.0.6.md。真实 API 语义仍需用户本机设置 Key 后验证；本轮停在 v0.0.6 试用边界。

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
