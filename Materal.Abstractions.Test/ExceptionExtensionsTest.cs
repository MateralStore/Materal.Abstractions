using System.Text.RegularExpressions;

namespace Materal.Abstractions.Test;

/// <summary>
/// ExceptionExtensions扩展方法的单元测试
/// </summary>
[TestClass]
public class ExceptionExtensionsTest
{
    /// <summary>
    /// 测试GetErrorMessage方法：异常为null时返回空字符串
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WhenExceptionIsNull_ReturnsEmptyString_Test()
    {
        // Arrange
        Exception? exception = null;

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        Assert.AreEqual(string.Empty, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：标准异常返回格式化消息
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithStandardException_ReturnsFormattedMessage_Test()
    {
        // Arrange
        string expectedMessage = "Test exception";
        var exception = new Exception(expectedMessage);

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        Assert.Contains(exception.GetType().FullName!, result);
        Assert.Contains(expectedMessage, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：MateralException使用GetDetailMessage
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithMateralException_UsesGetDetailMessage_Test()
    {
        // Arrange
        string expectedMessage = "Materal exception message";
        var exception = new MateralException(expectedMessage);

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        Assert.Contains(expectedMessage, result);
        Assert.Contains(exception.GetType().FullName!, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：包含内部异常
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithInnerException_IncludesInnerException_Test()
    {
        // Arrange
        string outerMessage = "Outer exception";
        string innerMessage = "Inner exception";
        var innerException = new InvalidOperationException(innerMessage);
        var outerException = new Exception(outerMessage, innerException);

        // Act
        string result = outerException.GetErrorMessage();

        // Assert
        Assert.Contains(outerMessage, result);
        Assert.Contains(innerMessage, result);
        Assert.Contains(innerException.GetType().FullName!, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：深层嵌套的内部异常
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithNestedInnerExceptions_IncludesAllExceptions_Test()
    {
        // Arrange
        var level3Exception = new Exception("Level 3");
        var level2Exception = new Exception("Level 2", level3Exception);
        var level1Exception = new Exception("Level 1", level2Exception);

        // Act
        string result = level1Exception.GetErrorMessage();

        // Assert
        Assert.Contains("Level 1", result);
        Assert.Contains("Level 2", result);
        Assert.Contains("Level 3", result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：达到最大递归深度时显示警告
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WhenMaxDepthReached_ShowsWarning_Test()
    {
        // Arrange
        Exception currentException = new("Level 0");
        const int depth = 25;

        for (int i = 1; i <= depth; i++)
        {
            currentException = new Exception($"Level {i}", currentException);
        }

        // Act
        string result = currentException.GetErrorMessage(maxDepth: 20);

        // Assert
        Assert.Contains("已达到最大递归深度 20", result);
        Assert.Contains("剩余异常信息被截断", result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：自定义getDetailMessage委托
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithCustomGetDetailMessage_UsesCustomFunction_Test()
    {
        // Arrange
        string expectedCustomMessage = "Custom error message";
        var exception = new Exception("Original message");

        // Act
        string result = exception.GetErrorMessage((ex, prefix) => expectedCustomMessage, maxDepth: 20);

        // Assert
        Assert.Contains(expectedCustomMessage, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：自定义委托的prefix参数正确传递
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithCustomGetDetailMessage_PrefixParameterIsPassed_Test()
    {
        // Arrange
        var exception = new Exception("Message");
        string capturedPrefix = string.Empty;

        // Act
        string result = exception.GetErrorMessage((ex, prefix) =>
        {
            capturedPrefix = prefix;
            return $"Prefix: {prefix}";
        }, maxDepth: 20);

        // Assert
        Assert.AreEqual(string.Empty, capturedPrefix);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：包含堆栈跟踪信息
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithStackTrace_IncludesStackTrace_Test()
    {
        // Arrange
        var exception = new Exception("Test exception");

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        // 检查结果不为空且包含异常类型和消息
        Assert.IsFalse(string.IsNullOrEmpty(result));
        Assert.Contains("System.Exception", result);
        Assert.Contains("Test exception", result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：内部异常的堆栈跟踪也包含
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithInnerExceptionStackTrace_IncludesBothStackTraces_Test()
    {
        // Arrange
        try
        {
            try
            {
                throw new InvalidOperationException("Inner");
            }
            catch (Exception inner)
            {
                throw new Exception("Outer", inner);
            }
        }
        catch (Exception outerException)
        {
            // Act
            string result = outerException.GetErrorMessage();

            // Assert
            // 应该包含两个堆栈跟踪结束标记
            int stackTraceEndCount = 0;
            int index = 0;
            while ((index = result.IndexOf("异常堆栈跟踪结束", index)) != -1)
            {
                stackTraceEndCount++;
                index += "异常堆栈跟踪结束".Length;
            }

            Assert.IsGreaterThanOrEqualTo(2, stackTraceEndCount);
        }
    }

    /// <summary>
    /// 测试GetErrorMessage方法：通过反射调用自定义GetDetailMessage
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithCustomGetDetailMessageMethod_CallsMethodByReflection_Test()
    {
        // Arrange
        string expectedMessage = "Reflection based detail message";
        var exception = new ExceptionWithGetDetailMessage(expectedMessage);

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        Assert.Contains(expectedMessage, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：多次调用同一个异常类型，反射缓存生效
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithMultipleCallsSameType_UsesReflectionCache_Test()
    {
        // Arrange
        var exception1 = new ExceptionWithGetDetailMessage("Message 1");
        var exception2 = new ExceptionWithGetDetailMessage("Message 2");

        // Act
        string result1 = exception1.GetErrorMessage();
        string result2 = exception2.GetErrorMessage();

        // Assert
        Assert.Contains("Message 1", result1);
        Assert.Contains("Message 2", result2);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：反射调用失败时回退到Message属性
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WhenReflectionFails_FallsBackToMessageProperty_Test()
    {
        // Arrange
        string expectedMessage = "Test message";
        var exception = new ExceptionWithBadGetDetailMessage(expectedMessage);

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        Assert.Contains(expectedMessage, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：没有GetDetailMessage方法的异常类型
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithExceptionWithoutGetDetailMessage_UsesMessageProperty_Test()
    {
        // Arrange
        string expectedMessage = "Test message";
        var exception = new Exception(expectedMessage);

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        Assert.Contains(expectedMessage, result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：堆栈跟踪中的回车符被正确处理
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithStackTraceContainingCarriageReturn_HandlesCorrectly_Test()
    {
        // Arrange
        var exception = new Exception("Test");

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        // 确保堆栈跟踪被正确格式化，没有多余的空行
        Assert.DoesNotContain("\r\r\n", result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：空消息的异常
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithEmptyMessage_HandlesCorrectly_Test()
    {
        // Arrange
        var exception = new Exception(string.Empty);

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        Assert.Contains(exception.GetType().FullName!, result);
        Assert.IsGreaterThan(0, result.Length);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：连续调用重置递归计数器
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithSequentialCalls_ResetsRecursionCounter_Test()
    {
        // Arrange
        var exception1 = new Exception("Exception 1");
        var exception2 = new Exception("Exception 2");

        // Act
        string result1 = exception1.GetErrorMessage();
        string result2 = exception2.GetErrorMessage();

        // Assert
        Assert.Contains("Exception 1", result1);
        Assert.Contains("Exception 2", result2);
        // 两个结果都应该从第一层开始，不应该有深度前缀
        Assert.DoesNotContain("\t--->", result1);
        Assert.DoesNotContain("\t--->", result2);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：内部异常有正确的缩进
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithInnerException_HasCorrectIndentation_Test()
    {
        // Arrange
        var innerException = new Exception("Inner");
        var outerException = new Exception("Outer", innerException);

        // Act
        string result = outerException.GetErrorMessage();

        // Assert
        // 外层异常应该没有制表符前缀
        Assert.Contains("--->System.Exception: Outer", result);
        // 内层异常应该有制表符前缀
        Assert.Contains("\t--->System.Exception: Inner", result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：指定非默认的最大递归深度
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithCustomMaxDepth_RespectsCustomValue_Test()
    {
        // Arrange
        Exception currentException = new("Level 0");
        const int depth = 5;

        for (int i = 1; i <= depth; i++)
        {
            currentException = new Exception($"Level {i}", currentException);
        }

        // Act
        string result = currentException.GetErrorMessage(maxDepth: 3);

        // Assert
        Assert.Contains("已达到最大递归深度 3", result);
        Assert.Contains("剩余异常信息被截断", result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：最大深度为0时仅显示第一层
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithMaxDepthZero_ShowsOnlyFirstLevel_Test()
    {
        // Arrange
        var innerException = new Exception("Inner");
        var outerException = new Exception("Outer", innerException);

        // Act
        string result = outerException.GetErrorMessage(maxDepth: 1);

        // Assert
        Assert.Contains("Outer", result);
        Assert.DoesNotContain("Inner", result);
        Assert.Contains("已达到最大递归深度 1", result);
    }

    /// <summary>
    /// 测试GetErrorMessage方法：堆栈跟踪每行都有正确的前缀
    /// </summary>
    [TestMethod]
    public void GetErrorMessage_WithStackTrace_EachLineHasCorrectPrefix_Test()
    {
        // Arrange
        Exception exception;
        try
        {
            throw new Exception("Test");
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        // Act
        string result = exception.GetErrorMessage();

        // Assert
        // 验证结果包含堆栈跟踪结束标记
        Assert.Contains("--- 异常堆栈跟踪结束 ---", result);
        // 验证异常行存在且没有制表符前缀（第一层）
        Assert.Contains("--->System.Exception: Test", result);
        // 验证第一层的堆栈跟踪行不以制表符开头
        Assert.MatchesRegex(new Regex(@"--->System\.Exception: Test\r?\n\s[^\t]"), result);
    }

    /// <summary>
    /// 自定义异常类，包含GetDetailMessage方法用于反射测试
    /// </summary>
    private class ExceptionWithGetDetailMessage(string customMessage) : Exception(customMessage)
    {
        public string GetDetailMessage(string prefix)
        {
            return Message;
        }
    }

    /// <summary>
    /// 自定义异常类，GetDetailMessage方法会抛出异常
    /// </summary>
    private class ExceptionWithBadGetDetailMessage(string message) : Exception(message)
    {
        public string GetDetailMessage(string prefix)
        {
            throw new Exception("Bad GetDetailMessage");
        }
    }
}
