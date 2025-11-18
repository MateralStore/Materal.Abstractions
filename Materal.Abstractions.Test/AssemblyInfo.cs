using Microsoft.VisualStudio.TestTools.UnitTesting;

// 启用测试并行化以提高测试执行性能
// Workers = 0 表示使用所有可用的处理器核心
// Scope = MethodLevel 表示在方法级别并行执行测试
[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.MethodLevel)]
