# v0.0.1 执行与验证合同

本文件细化已批准 milestone 的工程接口与测试，不新增产品行为。

## Core / Host 边界

Core 维护 raw composition、候选列表、highlight index、revision 与 privacy mode。Host 维护 editor 的光标/选择范围，接收 Core commit text 并在选区插入。

建议 native C ABI：create(sharedDataPath, sessionDataPath)、destroy(handle)、processKey(handle, key, modifiers)、highlight(handle, pageIndex)、select(handle, pageIndex)、读取 raw/candidate/highlight/commit。字符串 UTF-8，native 内存所有权必须明确；C# 在下次 native 调用前复制快照。异常不能穿越 ABI。

RimeAdapter 是唯一接触 Rime API 的模块。禁用不需要的学习/自动同步/输入历史；使用无内容诊断。候选不从 UI 生成。所有按键交互在一个串行会话内运行，engine 初始化/词表部署期间 UI 保持可响应。

## 首批 deterministic tests

| Test | 输入/动作 | 必须观察到的结果 |
|---|---|---|
| EnterPreservesPinyin | type youhua, Enter | commit=youhua，composition 清空 |
| EnterPreservesUnknown | type asdfg, Enter | commit=asdfg，不改写 |
| SpaceUsesHighlight | 候选非空，改变高亮，再 Space | commit 为高亮项而非永远第一项 |
| NumberSelectsCorresponding | 多候选，选择数字 2 | 第二个候选提交，仅一次 |
| MouseAndKeyboardAgree | 鼠标/方向分别高亮同一项 | 高亮与 Space 提交一致 |
| InvalidSelectionSafe | 空候选/越界 index | 不崩溃、不提交错误项 |
| TwoCompositionsIndependent | 第一组提交后输入第二组 | 第二组候选与 raw 无前次残留 |
| EnhancementFailureKeepsRaw | engine 异常/返回空候选，再 Enter | 原文仍可提交，UI 不暴露内部异常文本 |
| ReservedShiftNotPrimary | composition 中 Shift+Enter | 不误触发 Primary 或 raw 提交；Shadow 当前未实现 |
| SecurePolicyNoExternalEffects | secure mode | cloud/learning/content-log/persistent-text-cache 均不允许 |

上表的 mock 只能隔离真实 Session 测试；Rime integration 另测真实引擎，禁止用 mock PASS 代替。

## 真实 Rime 与部署 tests

- 经过审核的 schema/data 真实部署后：youhua 得到优化/油画/有话等合理候选；另测 xuexi、gongzuo、zhongguo，证明不止演示词硬编码。
- 连续字符输入、退格、高亮、数字/Space/Enter 提交；候选排序不要求与微软拼音相同。
- 无网络、无 key 启动；数据目录缺失给出可理解的状态，原文输入仍可继续。
- 唯一测试输入在日志与 engine session 目录中不可出现可恢复的输入历史；运行后扫描持久化内容。
- C# Host 显示同一真实 Core 状态，启动后 10 分钟持续输入；不得靠“生成示例”按钮。
- clean runner 构建并打包；另一台 Windows 上解压后启动，不要求安装开发 SDK、不手抄 DLL。

## GitHub 构建输出

输出名 BilingualInput-v0.0.1-win-x64.zip；内容为 exe、必要 native/managed runtimes、schema/data、licenses、README-TRY 与 checksum。用 Windows runner 完成 Build→Core tests→Rime integration→Host publish→来源审计→zip；任一步失败不宣称可用。

## 首版体验限度

首版仅体验真实拼音候选、导航与提交、原文 Enter。Shadow/Shift+Enter 英文效果在下一里程碑，Hover/Learning/AI 在后续里程碑。原型只在自身 editor 中输入，不会替换系统输入法。
