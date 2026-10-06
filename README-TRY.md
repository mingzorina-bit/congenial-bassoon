# Bilingual Input v0.0.6 — AI Expression 试用

Windows 10 2004+ / Windows 11 x64。完整解压后运行 `BilingualInput.exe`。没有 API Key 时，本地候选、Shadow、详情、发音和收藏仍可试用。

[下载 GitHub 正式试用包](https://github.com/mingzorina-bit/congenial-bassoon/actions/runs/37484316733/artifacts/11423735176)。

## 完整句子的云端辅助

1. 若要体验真实 AI，请在本机配置 OpenAI API 密钥为当前用户的 `OPENAI_API_KEY` 环境变量，然后重新启动程序。不要把密钥发到聊天、提交到 GitHub 或写进程序目录。API 独立计费。
2. 在“详情设置 / Settings”明确开启“云端辅助”。默认关闭；Private Mode 或模拟安全字段开启时自动阻断云端发送。
3. 在编辑区粘贴 `我觉得这个方案还可以继续优化。`。完整句子稍作停顿后会显示一个主要英文 Shadow；Shift+Enter 可以应用。点击 Tab 可按需查看 Natural Expression，Esc 关闭详情。
4. 试用 `youhua`、`I think this 方案 is better`、收藏词库和离线状态，确认本地输入不等待云端。重复修改句子，看旧 AI 结果不会覆盖新内容。

发送给服务的只有当前完整句子、语言方向和操作类型；不会发送其他段落、历史、收藏词库或仓库文件。请求设置 `store:false`，应用不持久缓存输入句子。官方 API 仍可能按其数据政策保留滥用监测日志；细节见仓库内 `docs/reviews/V0_0_6_PROVIDER_APPROVAL.md`。本版用 .NET 自带 HTTP 客户端，无新增 NuGet 包。

本试用仍是 WinUI 原型编辑区，不是全系统输入法。有限自有词表、美式 CMUdict、系统已安装声线的边界延续 v0.0.5。英式音标未覆盖；系统没有目标声线时会提示。用户提出的 Tab 关闭、中文 Shadow 拼音、未提交混输识别、`youh` 提前预测及不完整英文提示留待完整初代之后讨论。
