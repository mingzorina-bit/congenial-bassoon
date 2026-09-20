# 开发前复核 — 2026-09-21

## 结论

文档方案与此前确认的五份规格及 CR-01～CR-05 一致，可按 v0.0.1 Candidate Foundation 开工。此结论是需求一致性复核，不等于代码、部署或 UX 已验证。

## 逐项对应

| 冻结决定 | 开发安排 | 禁止偏移 |
|---|---|---|
| Windows 交互原型 | WinUI 3 + C#/XAML | 不改成网页输入框、Electron 或 Python GUI 来冒充验收 |
| C++20 Core / Host 分离 | 独立 native Core，经小型 C ABI/PInvoke 对接 Host | UI 不计算候选、语言判断或 Shadow 排序 |
| librime 负责 Primary | RimeAdapter 调用真实 engine 和编译后的词表 | 不把预置截图/if-youhua 逻辑当真实拼音 |
| Enter 原文，Space 高亮 | Core 负责提交意图；UI 转发按键 | Enter 不提交中文、不调用翻译 |
| Shift = Shadow Action | 本阶段保留该键，不假装已有 Shadow | 不把 Shift 写死为 English |
| Primary Candidate ≠ Current Expression | presentation 中分开 composition/raw/candidates 和 editor text | 不以用户 Profile 代替当前表达 |
| 本地优先、故障不阻断输入 | 首版无云请求、无 key，增强不可用保留 raw 提交 | 不给基本输入增加网络依赖 |
| 默认无原文日志 | 日志限阶段、计数、耗时；Rime 学习/用户词库写入默认关闭 | 不能仅关闭自身日志而遗漏底层 engine 持久化 |
| Secure enforcement real / detection simulated | 先建立 privacy policy contract，完整模式验收到对应里程碑 | 不宣称识别所有系统密码框 |
| Source Manifest | 固定版本、source/hash/notices，代码和词表分项 | BSD engine 不替全部传递依赖和数据背书 |
| milestone 小步 | v0.0.1 验证后才推进 Shadow | 不一口气生成整个产品 |

## GitHub 与体验路线

1. 将源码、五份规格、测试、Source Manifest 放入用户指定仓库；历史聊天快照默认保留本地，不必随公开源码传播。
2. GitHub Actions 在 Windows runner 构建 Native Core、跑测试、构建 WinUI Host。
3. 产出 self-contained win-x64 zip，附来源声明、校验摘要及体验指南。workflow 只在成功后上传可分发包。
4. 首轮使用 GitHub Actions artifact 交付；体验通过后再创建标记清晰的 prerelease，不能把未验证二进制称作 release。
5. 本地实际启动与键盘验收独立记录；CI 成功不等于用户体验已经通过。

GitHub Pages 不能运行 Windows 原生 WinUI 应用，本项目不以 Pages 网页代替冻结的 Windows 原型。默认内部原型先使用私有仓库，仓库归属与可见性由用户指定。

## 当前环境事实

GitHub connector 已连接 mingzorina-bit；可见三个仓库，没有 BilingualInput。当前目录是文档输出目录，尚无 Git repository。git 可用；未检测到 dotnet、cmake、MSVC 编译器；WSL 无可用发行版。云端 Windows 构建可避免在此电脑安装整套 Visual Studio。

## 尚待落实

- 指定目标 GitHub 仓库（或新建仓库的账号/可见性）。
- 按 A-18 确认具体首版依赖提案。
- 完成真实 Core/Host、测试及构建后，才能判断“可下载、可运行、可体验”。当前没有这样的已验证二进制。
