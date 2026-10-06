# Bilingual Input — v0.0.6 AI Expression

Windows WinUI 3/C# 原型与 C++20 Core、真实 librime。当前仅推进到第六个试用里程碑，完整 Phase 0 和系统级 TSF 输入法尚未完成。

请按 [试用指南](README-TRY.md) 下载并完整解压 [GitHub Actions 正式试用包](https://github.com/mingzorina-bit/congenial-bassoon/actions/runs/37484316733/artifacts/11423735176)，运行 `BilingualInput.exe`。不配置 API Key 也能使用本地输入。真实句子 AI 需要本机配置 OpenAI API Key，并在设置中明确开启云端辅助；详见 [已批准的供应商及数据条件](docs/reviews/V0_0_6_PROVIDER_APPROVAL.md)。

冻结依据：[规格登记](docs/SPEC_FREEZE.md)、[CR-01～05](docs/CHANGE_REQUESTS.md)、[实施计划](CODEX_IMPLEMENTATION_PLAN.md)、[验收矩阵](docs/ACCEPTANCE_MATRIX.md)。本轮范围见 [执行简报](docs/plans/v0.0.6-ai-expression.md) 与 [正式试用证据](docs/releases/v0.0.6.md)。

从干净 checkout 构建需要 Windows x64、Visual Studio 2022 C++ desktop workload、Windows SDK 10.0.19041+、CMake、Git、7-Zip、PowerShell 7 和 .NET SDK 8.0.425。使用短路径（例如 `C:/src/BilingualInput`）避免 XAML 工具路径过长。首次构建须下载登记依赖。

```powershell
./scripts/Prepare-Rime.ps1
cmake -S . -B build -A x64 -DBI_WITH_RIME=ON -DRIME_ROOT="$PWD/.vendor/rime"
cmake --build build --config Release
ctest --test-dir build -C Release --output-on-failure
dotnet run --project tests/ProviderTests/ProviderTests.csproj -c Release
./scripts/Package.ps1
```

成品位于 `artifacts/BilingualInput-v0.0.6-win-x64.zip`，附来源、许可和逐文件校验清单。不要只拷贝 exe。每个里程碑试用后再推进下一版；新增依赖或冻结行为修改按计划审批。
