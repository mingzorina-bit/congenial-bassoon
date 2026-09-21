# v0.0.1 依赖提案

状态：用户已明确批准依赖并要求继续开发（2026-09-21）。官方预编译 rime.dll 经审查含 GPL 插件，不予分发；改用同版源码禁用所有插件和日志构建。官方 deps 包仅用于链接已审查的必要库，不复制其工具或数据进入应用。

| 项目 | 具体方案 | 用途与边界 |
|---|---|---|
| .NET SDK | 8.0.425 | C# WinUI Host 的构建工具；运行包 self-contained |
| Windows App SDK | Microsoft.WindowsAppSDK 1.8.260804001 | 已冻结 WinUI 3；采用官方稳定 1.8 维护版本 |
| librime | 1.13.1，commit 1c23358157934bd6e6d6981f0c0164f05393b497 | 真实 Primary engine；BSD-3-Clause；使用官方 Windows msvc x64 artifact，须保留其传递依赖 notices |
| Rime schema/dictionary | 项目自有、有限 prototype schema/词表 | 真实 Rime 编译/查询；不复制第三方大词库，不承诺商业输入法词库覆盖；来源标注 OWN/PROTOTYPE_ONLY |
| GitHub Actions | Windows 2022 runner + checkout/setup-dotnet/upload-artifact 官方 actions（固定 commit） | 构建 Core/测试/WinUI，生成 win-x64 zip；不引入产品功能依赖 |
| C++ toolchain | runner 自带 MSVC/CMake | C++20 Core；本机无该工具链，优先云端构建，避免修改系统开发环境 |

暂不引入 SQLite、双语词典、IPA、TTS、AI SDK；它们属于后续里程碑。

下载/集成前记录固定 URL 与 SHA256；打包前枚举实际 DLL、运行时及 notice。传递依赖许可不清楚时阻断分发，不以顶层 librime BSD 代替审核。具体 Windows SDK/CsWinRT NuGet 版本在 restore 后写入锁定清单并检查，不开放任意外部库授权。

官方依据：
- https://github.com/rime/librime/releases/tag/1.13.1
- https://github.com/rime/librime/blob/1.13.1/LICENSE
- https://github.com/microsoft/WindowsAppSDK/releases
- https://api.nuget.org/v3-flatcontainer/microsoft.windowsappsdk/index.json
- https://builds.dotnet.microsoft.com/dotnet/release-metadata/8.0/releases.json
- https://learn.microsoft.com/en-us/windows/apps/package-and-deploy/self-contained-deploy/deploy-self-contained-apps

依赖批准的来源：冻结 A-18 要求 New external dependencies require explicit approval。本提案只覆盖 v0.0.1 的上述技术栈及其必需构建运行时；新增数据源/云服务继续单独批准。
