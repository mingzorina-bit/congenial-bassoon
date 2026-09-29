# v0.0.4 发音依赖提案

状态：用户于 2026-09-29 明确批准新增 CMUdict 数据及本轮英式音标缺失提示；其余沿用已批准技术栈。2026-09-29。

建议使用 Carnegie Mellon 官方 CMUdict，固定 commit `74790861f652b15e4ac49015a90074ad62a27690`，仅提取原型已有英语词汇的 ARPAbet 发音，使用可测试的确定性映射显示宽式 IPA。不是 LLM 生成音标。每个词保存原始 ARPAbet、locale=en-US、sourceId、源版本。数据单独存放，随包保留官方 LICENSE 与转换说明，不混入自有词库。

官方来源：https://github.com/cmusphinx/cmudict 。LICENSE 允许带声明的源码/二进制再分发及修改；商业发布仍执行统一数据复核。下载后记录原始文件 SHA256；提取脚本、有限源记录、license 一同交付。运行时离线读取，不抓词典网页、不下载音频、不缓存用户输入。

英式设置继续切换 IPA 查询 locale 与 Windows voice；在尚无批准的英式音标数据时明确显示“暂无英式音标”，绝不把美式 IPA 当英式。此项为试用包已知覆盖限制，完整英式 IPA 的验收不能标 PASS。继续评估独立许可的英式来源；不为补齐展示引入未经审核的 GPL 数据合集。

WindowsSpeechProvider 使用已批准 Windows SDK 的系统 SpeechSynthesizer/MediaPlayer，无新增 NuGet 包。只在点击时调用已安装且语言地区匹配的 voice；没有对应 voice 时给可理解提示，不自动切成其他口音。音频仅保留当前内存流，不写磁盘，无云 API key/云服务费用。

已批准，允许按上述固定范围集成并上传有限数据及许可。

