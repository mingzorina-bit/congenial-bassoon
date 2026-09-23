# 自有本地词表 v0.0.2

source_id: own-curated-v002
Type: DATA; snapshot: 2026-09-23; origin: 本项目编写，未抓取/复制第三方词典。
Purpose: 本地 word/phrase Shadow 试用。文件 own-v0.0.2.tsv，字段 primary、target language、text、sourceId、entryId。
License/right holder: 项目自有内容（OWN）；可修改、随项目原型分发与本地读取；不包含第三方共享许可数据；无音频、IPA、例句或模型。
releaseStatus: PROTOTYPE_ONLY（语义覆盖有限，尚非商业质量词典）。
review: 每行有来源和稳定 entryId；禁止缺失来源/未知来源行。词级最多三项；短语采用完整条目，不能拆词拼译。顺序是自有词表针对该词义的静态优先级，无用户偏好/学习排名。
Approval: 冻结 D-07 与 v0.0.2 计划的 curated 路线，本轮用户明确要求执行下一里程碑；没有引入外部数据依赖。

语义复核：优化 optimize/improve/refine，均为动词改进义；油画 oil painting，为独立名词义；优化方案 optimization plan，完整短语条目；继续优化 further improve，动词短语；学习语言 learn a language，完整动宾结构。缺失词义（如 有话）不强造英文。
反向 optimize→优化 用于验证 Core 目标语言不被写死成 EN；当前真实拼音 Host 尚未进行英文上下文识别，不能据此宣称 AC-11 已实现。
