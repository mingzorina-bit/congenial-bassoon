# v0.0.6 窗口试用清单

1. 无 `OPENAI_API_KEY`：启动、`youhua`、Space/Shift+Enter、Hover、Tab/Esc、Library 可用；设置明确提示密钥未检测到。不得把 Mock 输出当成真实 AI。
2. 有 Key、Cloud 关闭：完整中文句子不产生网络调用；开启 Cloud 后粘贴完整句子，等待唯一英文 Shadow；Tab 后才请求 Natural Expression。输入未完的词或句段不请求。
3. 输入 S1 后迅速改为 S2；较迟的 S1 结果不可出现在 S2。Tab/Esc、提交、Private、模拟 Secure 切换都使旧结果失效。Private 可继续本地输入；Secure 的云端、学习写入、内容日志、持久文本缓存均为零。
4. 断网、超时、HTTP 错误、无效响应或无 Key：Primary/本地 Shadow 仍可用；UI 不出现原始错误、供应商品牌错误或密钥。快响应不闪 loading；慢响应约 850ms 后才出现轻提示。
5. 搜索正常日志与本地 DB，不应找到 `PRIVACY_TEST_UNIQUE_7F3` 原文。第二台干净 Windows 机器及 10–20 分钟真人试用在 v0.1.0-alpha 继续。

记录版本、机器、是否配置 Key、场景、实际表现及问题。若没有真实 API Key，只能记录本地/Mock 路径，语义测试保持待执行。
