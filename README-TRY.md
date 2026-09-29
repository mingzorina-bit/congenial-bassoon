# Bilingual Input v0.0.4 — Hover / Deep Dive 试用
Windows 10 2004+ / Windows 11 x64。完整解压后运行 BilingualInput.exe；无需 API Key。

## 先体验这四步
1. 系统键盘切英文，在编辑区逐键输入 youhua，暂不提交。鼠标停在 refine 上约 400ms，可看到音标、词性、释义和播放按钮。只有点击播放才发音；移出卡片后关闭。
2. 按 Tab 展开详情，按 Esc 关闭。原来的 youhua 仍在，可继续 Space 提交“优化”或 Shift+Enter 提交 optimize。
3. 粘贴 further improve，按 Tab，在 Phrase Breakdown 中点击播放，可听整段短语。粘贴 refine 可试单词发音。未知词不会凭空生成音标或释义。
4. 点击“详情设置”，可调整悬停开关/延迟、American/British、详情模块与 IPA/发音/释义/词性。Examples 默认关闭，本版无例句数据。设置在下次启动保留。

句子测试：粘贴 I think this 方案 is better，Tab 查看已有本地结果；Esc 后 Shift+Enter 可应用当前句英文补全。详情不会提交或改写原文；没有其他结果时不强行制造第二句。

## 试用边界
- 美式音标来自经批准的 CMUdict 25 个有限词形，经过可追踪的宽式转换；英式音标本版未收录，切换后明确提示。短语音标不拼接伪造。
- 发音使用 Windows 已安装的匹配声线。若没有对应英式/美式声线，会提示，可在 Windows 语言设置中自行添加；不会静默换口音。无网络请求，无音频磁盘缓存。
- 收藏按钮显示“下一版”，尚不保存；Learning、真实 AI 和全系统输入法未进入本轮。
- 自有词表仍有限；输入和释义覆盖不是完整词典。旧 Primary/Shadow、上下文补全与五步引导保持。

请先试用本版；确认后才继续 v0.0.5 Learning。你提及的两处未来改进留待 v0.1.0 后讨论。
