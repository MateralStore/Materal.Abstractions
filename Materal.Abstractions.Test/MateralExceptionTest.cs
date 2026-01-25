namespace Materal.Abstractions.Test;

/// <summary>
/// MateralException类的单元测试
/// </summary>
[TestClass]
public class MateralExceptionTest
{
    /// <summary>
    /// 测试默认构造函数：创建实例不抛出异常
    /// </summary>
    [TestMethod]
    public void Constructor_WhenCalled_CreatesInstanceWithoutThrowing_Test()
    {
        // Arrange & Act
        var exception = new MateralException();

        // Assert
        Assert.IsNotNull(exception);
    }

    /// <summary>
    /// 测试带消息的构造函数：设置正确的Message属性
    /// </summary>
    [TestMethod]
    public void Constructor_WhenMessageProvided_SetsMessageProperty_Test()
    {
        // Arrange
        string expectedMessage = "Test exception message";

        // Act
        var exception = new MateralException(expectedMessage);

        // Assert
        Assert.AreEqual(expectedMessage, exception.Message);
    }

    /// <summary>
    /// 测试带消息和内部异常的构造函数：设置正确的Message属性
    /// </summary>
    [TestMethod]
    public void Constructor_WhenMessageAndInnerExceptionProvided_SetsMessageProperty_Test()
    {
        // Arrange
        string expectedMessage = "Test exception message";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new MateralException(expectedMessage, innerException);

        // Assert
        Assert.AreEqual(expectedMessage, exception.Message);
    }

    /// <summary>
    /// 测试带消息和内部异常的构造函数：设置正确的InnerException属性
    /// </summary>
    [TestMethod]
    public void Constructor_WhenMessageAndInnerExceptionProvided_SetsInnerExceptionProperty_Test()
    {
        // Arrange
        string expectedMessage = "Test exception message";
        var expectedInnerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new MateralException(expectedMessage, expectedInnerException);

        // Assert
        Assert.AreSame(expectedInnerException, exception.InnerException);
    }

    /// <summary>
    /// 测试GetDetailMessage方法：返回Message属性
    /// </summary>
    [TestMethod]
    public void GetDetailMessage_WhenCalled_ReturnsMessageProperty_Test()
    {
        // Arrange
        string expectedMessage = "Test message";
        var exception = new MateralException(expectedMessage);

        // Act
        string result = exception.GetDetailMessage(string.Empty);

        // Assert
        Assert.AreEqual(expectedMessage, result);
    }

    /// <summary>
    /// 测试GetDetailMessage方法：前缀不影响返回值
    /// </summary>
    [TestMethod]
    public void GetDetailMessage_WithPrefix_ReturnsMessageWithoutPrefix_Test()
    {
        // Arrange
        string expectedMessage = "Test message";
        var exception = new MateralException(expectedMessage);
        string prefix = "PREFIX";

        // Act
        string result = exception.GetDetailMessage(prefix);

        // Assert
        Assert.AreEqual(expectedMessage, result);
    }

    /// <summary>
    /// 测试GetDetailMessage方法：前缀为空时正常工作
    /// </summary>
    [TestMethod]
    public void GetDetailMessage_WithEmptyPrefix_ReturnsMessage_Test()
    {
        // Arrange
        string expectedMessage = "Test message";
        var exception = new MateralException(expectedMessage);

        // Act
        string result = exception.GetDetailMessage(string.Empty);

        // Assert
        Assert.AreEqual(expectedMessage, result);
    }

    /// <summary>
    /// 测试GetDetailMessage方法：前缀为null时正常工作
    /// </summary>
    [TestMethod]
    public void GetDetailMessage_WithNullPrefix_ReturnsMessage_Test()
    {
        // Arrange
        string expectedMessage = "Test message";
        var exception = new MateralException(expectedMessage);

        // Act
        string result = exception.GetDetailMessage(null!);

        // Assert
        Assert.AreEqual(expectedMessage, result);
    }

    /// <summary>
    /// 测试ToString方法：调用GetErrorMessage扩展方法
    /// </summary>
    [TestMethod]
    public void ToString_WhenCalled_ReturnsFormattedErrorMessage_Test()
    {
        // Arrange
        string expectedMessage = "Test message";
        var exception = new MateralException(expectedMessage);

        // Act
        string result = exception.ToString();

        // Assert
        Assert.Contains(exception.GetType().FullName!, result);
        Assert.Contains(expectedMessage, result);
    }

    /// <summary>
    /// 测试继承的GetDetailMessage：子类可以重写方法
    /// </summary>
    [TestMethod]
    public void GetDetailMessage_WhenOverriddenInDerivedClass_ReturnsCustomMessage_Test()
    {
        // Arrange
        const string customMessage = "Custom detailed message";
        var derivedException = new CustomMateralException("Base message");

        // Act
        string result = derivedException.GetDetailMessage(string.Empty);

        // Assert
        Assert.AreEqual(customMessage, result);
    }

    /// <summary>
    /// 自定义MateralException子类，用于测试重写的GetDetailMessage
    /// </summary>
    private class CustomMateralException(string message) : MateralException(message)
    {
        public override string GetDetailMessage(string prefix)
        {
            return "Custom detailed message";
        }
    }
}
