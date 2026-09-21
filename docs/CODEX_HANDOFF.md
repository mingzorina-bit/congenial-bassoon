# 下一轮 Codex 启动说明
阅读 README、冻结规格、CR、实施计划、验收矩阵与 Source Manifest。
当前任务仅执行 v0.0.1 Candidate Foundation。先检查实际仓库环境并登记 Windows/toolchain/support matrix，再按计划构建最小可运行原型。新增外部依赖在引入前提交具体审核项；冻结技术方向不能当成无限授权。
实现 C++20 Core、真实 librime Primary、WinUI editor、基础键盘与可复现构建。测试可用 fake 隔离状态机，但真实 Primary 验收必须使用审核过的引擎和 schema/data。
不得实现 Shadow/AI/Learning/TSF，不得更改 Enter、Space 等冻结行为，不得为了完成里程碑伪造 AC PASS。
完成后提供改动清单、实际命令与结果、artifact、手工验证记录、依赖审核记录、未完成项和风险。到本里程碑边界停止扩展功能，等待验收后再推进 v0.0.2。
