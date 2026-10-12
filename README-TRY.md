# Bilingual Input v0.2.0 — 离线宽词库试用

Windows 10 2004+ / Windows 11 x64。完整解压后运行 `BilingualInput.exe`。没有 API Key 时，本地候选、Shadow、详情、发音和收藏仍可试用。

[下载已验证的 v0.2.0 离线词库试用包](https://github.com/mingzorina-bit/congenial-bassoon/actions/runs/38191903076/artifacts/11721523668)。GitHub 下载的是外层 ZIP；解压后再解压其中的 `BilingualInput-v0.2.0-lexicon-trial-win-x64.zip`，运行 `BilingualInput.exe`。本版保留项目级 `.env` 配置和明确标注的 AI 演示模式；正式包运行时请用系统 `OPENAI_API_KEY` 环境变量配置真实 API。

在编辑区按 Tab 展开 Details，再按一次 Tab 可关闭；Esc 也可关闭。关闭后继续输入，原文和候选不应改变。

本版新增离线拼音词库。可连续试 `beijing`→“北京”、`shanghai`→“上海”、`diannao`→“电脑”、`nihao`→“你好”、`youhua`→“优化”，再试 `beijingdiqu`→“北京地区”；选择候选用 Space，按 Enter 提交未转换的原文。请留意候选排序和输入延迟，并把不自然的例子记下来。词库来自已固定版本的 Rime Luna Pinyin 与 Rime Essay，随试用包离线提供；不会因输入这些词而请求网络。中文候选经过系统简体转换，个别词形仍可能需要调整。

## 完整句子的云端辅助

1. 源码运行时，在仓库根目录 `.env` 中填写 `OPENAI_API_KEY=你的Key`；该文件已被 Git 忽略。直接运行已发布试用包时，请配置当前用户的 `OPENAI_API_KEY` 环境变量并重新启动程序。不要把密钥发到聊天、写进源码或提交到 GitHub。API 独立计费。
2. 在“详情设置 / Settings”明确开启“云端辅助”。默认关闭；Private Mode 或模拟安全字段开启时自动阻断云端发送。
3. 在编辑区粘贴 `我觉得这个方案还可以继续优化。`。完整句子稍作停顿后会显示一个主要英文 Shadow；Shift+Enter 可以应用。点击 Tab 可按需查看 Natural Expression，Esc 关闭详情。
4. 试用 `youhua`、`I think this 方案 is better`、收藏词库和离线状态，确认本地输入不等待云端。重复修改句子，看旧 AI 结果不会覆盖新内容。

发送给服务的只有当前完整句子、语言方向和操作类型；不会发送其他段落、历史、收藏词库或仓库文件。请求设置 `store:false`，应用不持久缓存输入句子。官方 API 仍可能按其数据政策保留滥用监测日志；细节见仓库内 `docs/reviews/V0_0_6_PROVIDER_APPROVAL.md`。本版用 .NET 自带 HTTP 客户端，无新增 NuGet 包。

## 无 API 余额时的演示模式

在“详情设置 / Settings”开启“AI 演示模式（Mock，不联网、不收费）”。演示模式不需要开启“云端辅助”，不会发送句子或使用 API Key；Private Mode 仍会阻断这条链路，以便继续验证隐私行为。当前仅支持有限固定句子：`我觉得这个方案还可以继续优化。`、`这个设计让双语输入更加自然。`、`我们需要进一步优化这个功能。`。结果区域会明确标注“AI 演示结果（Mock，不联网）”，不能把它当成真实模型语义验收。

本试用仍是 WinUI 原型编辑区，不是全系统输入法。英文 Shadow、详情释义和音标仍只覆盖已审核的有限数据，宽拼音词库不等于完整双语词典；系统声线依安装情况提供，美式 CMUdict 有限，英式音标未覆盖。中文 Shadow 拼音、未提交混输识别、`youh` 提前预测及不完整英文提示仍留待后续迭代；应用外输入需经过独立 TSF 阶段，见仓库 `docs/POST_ALPHA_ROADMAP.md`。本版词典仅获试用批准，商业发布另审。
