# v0.0.6 表达服务依赖决定

2026-10-06 用户明确批准 OpenAI API `gpt-5.4-mini`、.NET 自带 `HttpClient`、当前计费与所述隐私/缓存条件，并表示自行在本机配置 API Key。本版不引入新 NuGet 包；此前已批准的 SQLite 等依赖保持原样。ChatGPT/Codex 登录本身不等于 OpenAI API Key，也不代表 API 已充值。

技术边界：仅用户在设置中明确开启 Cloud assistance、当前处于 Normal、存在本机 `OPENAI_API_KEY`、活动表达是完整句子且不超过 500 字符时才发送。自动句子 Shadow 经 600ms debounce；Tab 详情中的 Natural Expression 按需调用。请求只包含当前句子、源/目标语言、操作说明；不包含其他编辑段落、历史对话、收藏词库、仓库文档、源码或密钥本身。一次请求最长等待 8 秒，不重试。每次发送及接收均经过隐私/会话/修订检查。密钥只存在运行进程环境与授权请求头；不打包、不持久化、不写日志。

请求使用 `/v1/responses`、`store:false`，应用内不做持久文本缓存。`store:false` 关闭 Responses 的后续检索状态，但并不等于供应商的所有安全监测数据即时清零。OpenAI 官方说明，API 输入/输出默认不用于训练，除非账户主动选择共享；标准滥用监测日志可保留最多约 30 天，特殊法律或安全情形按其政策处理。需更严格的数据驻留或零保留时，另做供应商与账户级审查，不由本原型承诺。

批准时官方模型页列出的文本价格为输入 **US$0.75 / 100 万 tokens**、输出 **US$4.50 / 100 万 tokens**；费用随实际用量计费，平台价格可变化，本程序不设置账户级预算。输出上限 180 tokens，输入限当前 500 字符以内的句子。账户预算和密钥轮换由 API 账户持有人在平台控制。

来源：[模型与价格](https://developers.openai.com/api/docs/models/gpt-5.4-mini)、[Responses API](https://developers.openai.com/api/reference/resources/responses/methods/create)、[API 数据控制](https://developers.openai.com/api/docs/guides/your-data)。

验收限制：本机当前未检测到 `OPENAI_API_KEY`，因此可验证请求格式、模拟服务、隐私与 UI 降级，但不能把真实语义质量标为 PASS；用户配置密钥后需按 `tests/semantic/cases.md` 做真实表达与人工判断。
