# Project Progress — v0.0.3
更新时间：2026-09-29。
用户已试用 v0.0.2 并明确批准下一版。v0.0.2 未完成独立复核与第二机验收继续保留；不冒充 PASS。
当前：v0.0.3 Bilingual Context 开发中。规格和范围见 docs/plans/v0.0.3-context.md，权威为冻结五份规格与 CR-01～05。
- Context RED：CI 36579679922，9 tests/7 failures；旧 Core/Shadow PASS。
- Session RED：CI 36580074086，6 tests/4 failures。
- ExpressionRange RED 7/4 → GREEN 7/0，本地 .NET 实测。
- Context 多 gap 测试初版用了不自然的 needs optimize，改为 I use this approach to optimize this part；保留双空格/标点断言。
- 当前整合 native/WinUI/真实 Rime，等待最终 CI 与包验证。
只做当前上下文、gap、歧义、typo；不做 Hover/Learning/AI/TSF，不引入新外部依赖。自有词形规则有意有限，未知词保留。当前分析仅在内存，不写输入日志。
仓库 mingzorina-bit/congenial-bassoon，分支 codex/context-v0.0.3。原始聊天快照不上传。完成后停止等待 v0.0.3 试用反馈。
最终复核：两项重要问题进入一次修复。UnknownLatinNeverSentToPinyin 在 CI 36580980955 明确 FAIL；已改 Unknown 原文保留，拼音词形集合只来自批准的自有 Rime 字典。候选快照测试 10/2 RED→10/0 GREEN，光标/文本事件统一重新渲染候选并在点击时核对原始串与候选文本。次要延后：补充平面汉字不参与中文证据，原文仍保留。
