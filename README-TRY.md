# Bilingual Input v0.0.3 — Bilingual Context 试用
Windows 10 2004+ / Windows 11 x64。完整解压后运行 BilingualInput.exe；无需 API Key。

## 本次新增
- 明确英文 design 保留为英文，Shadow 可显示“设计”。英文单词用 Space 提交并留下词间空格。
- 输入 zhege + Space、一个空格、design + Space、haikeyi + Space，可得到“这个 design 还可以”。
- 在编辑区粘贴 I think this 方案 is better，保持光标在本句，点击下方英文补全或 Shift+Enter，可得到 I think this approach is better。
- 粘贴 We probably need to 优化 this part.，补全为 We probably need to optimize this part.。
- 先输入“这个方案”，再输入 youhha：在明确中文上下文中恢复为“优化”；Shift+Enter 可用 optimize。Enter 仍提交原始 youhha。
- shi/he/can/an/in/me 结合当前句判断；无明确证据时才使用偏好。判断并非每次正确，Enter 总能保留原文。

补全只作用于当前句或明确选区。无 composition 时 Shift+Enter 应用当前句补全；有 composition 时仍提交当前候选的 Shadow。编辑其他句子后旧结果不会继续应用。正常 Ctrl+Z 撤销由编辑器处理。

原有引导、Primary 高亮与 Shift+1–3 保持；system keyboard 请切英文。词表有限，不认识的中文缺口保留原文，整句自动翻译、AI、Hover、学习和系统级输入法尚未实现。

请重点体验英文是否误转、切换句子/选区是否准确、拼音与英文交替是否自然；反馈后再推进 v0.0.4。