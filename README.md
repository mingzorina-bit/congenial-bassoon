# BilingualInput — Local Shadow v0.0.2

Windows 原生输入原型，采用 WinUI 3/C# 与 C++20 Core、真实 librime。当前推进第二个里程碑，等待实际试用反馈；完整 Phase 0 尚未完成。

## 试用
按 [体验指南](README-TRY.md) 下载成功 Actions 构建中的 BilingualInput-v0.0.2-win-x64，完整解压后运行 BilingualInput.exe。无需 API Key。有限自有词表用于验证候选交互；已加入本地 Shadow 与首次引导；上下文智能、学习、AI 与系统级输入尚未实现。

## 规格与推进
1. [冻结登记](docs/SPEC_FREEZE.md)、[CR-01～05](docs/CHANGE_REQUESTS.md)。
2. [Product](docs/01_PRODUCT_SPEC.md)、[Interaction](docs/02_INTERACTION_SPEC.md)、[Architecture](docs/03_ARCHITECTURE.md)、[Data](docs/04_DATA_SOURCE_STRATEGY.md)、[Acceptance](docs/05_PHASE0_ACCEPTANCE.md)。
3. [实施计划](CODEX_IMPLEMENTATION_PLAN.md)、[验收矩阵](docs/ACCEPTANCE_MATRIX.md)、[依赖来源](THIRD_PARTY_SOURCES.md)。
4. [本阶段执行证据](PROJECT_PROGRESS.md)。

## 从干净 checkout 构建
环境：Windows x64，Visual Studio 2022 C++ desktop workload 与 Windows SDK 10.0.19041+，CMake、Git、7-Zip、PowerShell 7、.NET SDK 8.0.425。建议短路径如 C:/src/BilingualInput，避免 XAML 工具的路径长度限制。首次构建需要联网下载已登记依赖；运行原型无需联网。

在仓库根目录的 PowerShell 依次执行：

```powershell
./scripts/Prepare-Rime.ps1
cmake -S . -B build -A x64 -DBI_WITH_RIME=ON -DRIME_ROOT="$PWD/.vendor/rime"
cmake --build build --config Release
ctest --test-dir build -C Release --output-on-failure
dotnet run --project tests/HostRoutingTests/HostRoutingTests.csproj -c Release
./scripts/Package.ps1
```

成品位于 artifacts/BilingualInput-v0.0.2-win-x64.zip。GitHub Actions 使用同一流程。包内附来源、许可和文件校验清单。不要只拷贝 exe；原型不安装为系统输入法。

用户已批准本阶段依赖。下一里程碑须先根据本阶段试用结果验收，新增依赖或冻结行为修改仍按实施计划处理。

