# Materal.Abstractions

Materal 基础抽象包，为 .NET 应用程序提供通用的抽象基类和扩展方法。

## 功能特性

- **异常处理增强**：提供强大的异常处理扩展方法，支持格式化异常信息、递归处理内部异常、自定义消息格式等功能
- **基础异常类**：`MateralException` 作为所有自定义异常的基类
- **日期时间枚举**：提供常用的日期时间单位枚举
- **多框架支持**：支持 .NET Standard 2.0/2.1 以及 .NET 8.0/9.0/10.0

## 快速开始

### 安装

```xml
<PackageReference Include="Materal.Abstractions" Version="[VERSION]" />
```

或者使用 .NET CLI：

```bash
dotnet add package Materal.Abstractions
```

### 基本使用

```csharp
using Materal.Abstractions;

// 使用异常处理扩展
try
{
    throw new MateralException("测试异常");
}
catch (Exception ex)
{
    Console.WriteLine(ex.GetErrorMessage());
}

// 使用日期时间枚举
DateTimeUnitEnum unit = DateTimeUnitEnum.DayUnit;
```

## 文档

有关更详细的文档和使用示例，请查看我们的 [Wiki](https://github.com/Materal/Materal.Abstractions/wiki)。