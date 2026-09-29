# Source Manifest 与依赖审批
以下策略表保留冻结来源边界；本轮实际引入项见文末 v0.0.1 清单。
架构技术方向已批准不等于任意版本、schema、数据包或 SDK 自动批准。新增外部依赖须明确批准（A-18）；已获明确批准的同一版本/用途不重复申请。版本、用途、分发范围或数据来源变化需补审。

| source_id | 类型 | 名称/方向 | 当前许可状态 |
|---|---|---|---|
| platform-winui | CODE | WinUI 3 / Windows App SDK / C# | 架构已冻结；实施时登记具体版本和分发条件 |
| engine-rime | CODE | librime / C++20 | 方向已冻结；具体版本与再分发审核 |
| rime-data | DATA | Rime schema/dictionary | 独立审核；未选择 |
| storage-sqlite | CODE | SQLite | 方向已冻结；登记具体版本 |
| lexical-curated | DATA | 自有 curated prototype lexicon | 逐条记录出处/权利，禁止照抄未授权释义 |
| lexical-cedict | DATA | reviewed CC-CEDICT subset | Phase 0 方向批准；source boundary/attribution；商业复审 |
| lexical-ecdict | DATA | ECDICT | 候选，字段 provenance 待审，非正式商业依赖 |
| phonetic | DATA | IPA source | 仅审核过的来源；不得以 LLM 为 canonical |
| audio-windows | SERVICE | WindowsSpeechProvider | Phase 0 基础；voice 可用性需运行时检测 |
| expression | SERVICE | 一个真实 LLM Provider | 供应商未选择；另有 Mock |
| audio-cloud | SERVICE | Cloud TTS / Azure 第一 PoC 候选 | 可选；最终 vendor 未冻结 |
| examples | DATA | 自有/许可/generated examples | 独立来源；生成例句显式标识 generated |

## 每个实际引入项必填
source_id、name、type（CODE/DATA/AUDIO/MODEL/SERVICE）、version/snapshot date、official source、purpose、license、attribution requirement、commercial use、modification right、redistribution right、cache right、ship-with-app right、derived-data restriction、review status、releaseStatus、approval reference、实际包路径/校验摘要。
权利字段记录条款依据，不以“GitHub 可下载”替代。代码、schema、词库、音频及服务分开建项。

## 操作流程
开发者先提出具体版本/来源/用途/替代方案、授权依据、分发/缓存影响，得到明确批准后才引入。可先完成无依赖接口/自有测试 fixture，但 fixture 不替代真实引擎验收。禁止 GPL frontend 代码、抓取网页词典、来源不明 MP3/IPA/词库。
打包必须枚举实际内容并关联 source_id。内部原型包核对其原型分发权限；商业包阻断 UNKNOWN 和 PROTOTYPE_ONLY。审计证据及 attribution 与 artifact 一起保存。

## v0.0.1 实际构建清单（用户已批准）

| source_id | 固定版本/出处 | 用途/许可 | 分发状态 |
|---|---|---|---|
| engine-rime | rime/librime 1.13.1 / 1c23358157934bd6e6d6981f0c0164f05393b497 | Primary / BSD-3-Clause；无插件源码构建，日志关闭 | 原型可分发，附 LICENSE |
| leveldb | google/leveldb 99b3c03b3284f5886f9ef9a4ef703d57373e61be | Rime 必需静态依赖 / BSD-3-Clause | 附 notice；用户学习关闭 |
| marisa | rime/marisa-trie 0d4e8ab58eec355facf8f65ff11ef811b330e373 | Rime 字典索引 / 选择 BSD 许可分支 | 附 COPYING |
| yaml | jbeder/yaml-cpp f7320141120f720aecc4c32be25586e7da9eb978 | Rime schema / MIT | 附 LICENSE |
| opencc-code | BYVoid/OpenCC e5d6c5f1b78e28a5797e7ad3ede3513314e544b7 | Rime 必需代码 / Apache-2.0 | 附 LICENSE；不带 OpenCC 数据库 |
| boost | Boost 1.84.0 官方源码 headers；SHA256 A5800F405508F5DF8114558CA9855D2640A2DE8F0445F051FA1C7C3383045724 | Rime headers / Boost Software License 1.0 | 附 LICENSE；构建环境项 |
| rime-deps | official release rime-deps-1c23358-Windows-msvc-x64.7z | 上述静态库和 headers；SHA256 3EDE059E6C1F4CDD5843CED3205F76666B706E5F55CCF8E56E2D04791A376FF6 | 不整体分发，不复制 share 数据/工具 |
| rime-data-own | data/rime/*.yaml v0.0.1 | 项目自有有限词表/schema | OWN / PROTOTYPE_ONLY；非大词库 |
| windowsappsdk | NuGet Microsoft.WindowsAppSDK 1.8.260804001 | WinUI 与应用运行时 | 官方 redistributable 随应用，不单独销售；包级许可一并收集 |
| dotnet | SDK 8.0.425；运行时版本记录在 publish deps | C# self-contained Host / .NET MIT 与 third-party notices | 官方发布 runtime notices 随包 |
| msvc-runtime | GitHub Windows 2022 runner VC143 x64 CRT | app-local Microsoft redistributable runtime | 仅可再分发列表中的 CRT DLL，不含开发工具 |

所有 third-party 库原样使用。除 Rime 配置禁用日志/插件外无上游代码修改。来源清单、NuGet 解析依赖和文件 SHA256 inventory 随 artifact 保存；最终商业/签名发行仍须独立 Release Review。


## v0.0.2 新增自有数据
仅新增 data/lexical/own-v0.0.2.tsv（OWN / PROTOTYPE_ONLY）及自有短语拼音映射。每行 sourceId/entryId 可追溯，来源与语义复核见 data/lexical/README.md。不引入新外部依赖、CC-CEDICT/ECDICT、IPA、音频或云服务；复用 v0.0.1 固定工具链与许可。
