# Materal.Abstractions

Materal 基础抽象包，提供优雅的异常处理机制和统一的异常消息格式化能力。

## 核心功能

- **统一异常基类** - `MateralException` 作为框架异常的基础类型
- **智能消息格式化** - 自动递归处理异常及内部异常，生成结构化错误信息
- **灵活自定义** - 支持通过继承或委托自定义异常消息格式
- **高性能** - 反射缓存优化，线程安全设计
- **多框架支持** - 兼容 .NET Standard 2.0/2.1 和 .NET 8.0/9.0/10.0

## 快速开始

```bash
dotnet add package Materal.Abstractions
```

```csharp
using Materal.Abstractions;

// 基础使用
throw new MateralException("操作失败");

// 格式化异常消息
try { ... }
catch (Exception ex)
{
    Console.WriteLine(ex.GetErrorMessage());
}

// 自定义异常
public class BusinessException : MateralException
{
    public string ErrorCode { get; }
    public override string GetDetailMessage(string prefix)
        => $"[{ErrorCode}] {Message}";
}
```

## 📖 文档

详细文档请查看 [Wiki](../../wiki)，包含：
- 完整 API 参考
- 高级用法示例
- 最佳实践指南
- 测试说明
