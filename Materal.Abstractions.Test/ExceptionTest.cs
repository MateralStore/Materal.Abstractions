namespace Materal.Abstractions.Test
{
    [TestClass]
    public class ExceptionTest : MateralTestBase
    {
        /// <summary>
        /// 测试空异常
        /// </summary>
        [TestMethod]
        public void GetErrorMessage_NullException_ReturnsEmptyString()
        {
            // Arrange & Act
            string result = ExceptionExtensions.GetErrorMessage(null);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        /// <summary>
        /// 测试简单异常
        /// </summary>
        [TestMethod]
        public void GetErrorMessage_SimpleException_ReturnsFormattedMessage()
        {
            // Arrange
            Exception exception = new("测试异常");

            // Act
            string result = exception.GetErrorMessage();

            // Assert
            Assert.AreEqual("--->System.Exception: 测试异常\r\n", result);
        }

        /// <summary>
        /// 测试内部异常
        /// </summary>
        [TestMethod]
        public void GetErrorMessage_ExceptionWithInnerException_ReturnsNestedMessage()
        {
            // Arrange
            Exception innerException = new("内部异常");
            Exception outerException = new("外部异常", innerException);

            // Act
            string result = outerException.GetErrorMessage();

            // Assert
            Assert.Contains("System.Exception: 外部异常", result);
            Assert.Contains("--->System.Exception: 内部异常", result);
        }

        /// <summary>
        /// 测试MateralException
        /// </summary>
        [TestMethod]
        public void GetErrorMessage_MateralException_ReturnsMateralExceptionMessage()
        {
            // Arrange
            MateralException materalException = new("Materal异常");

            // Act
            string result = materalException.GetErrorMessage();

            // Assert
            Assert.Contains("Materal.Abstractions.MateralException: Materal异常", result);
        }

        /// <summary>
        /// 测试自定义消息格式
        /// </summary>
        [TestMethod]
        public void GetErrorMessage_WithCustomMessageFormatter_ReturnsCustomFormattedMessage()
        {
            // Arrange
            Exception exception = new("测试异常");

            // Act
            string result = exception.GetErrorMessage((ex, prefix) => $"自定义消息: {ex.Message}");

            // Assert
            Assert.Contains("自定义消息: 测试异常", result);
        }

        /// <summary>
        /// 测试递归深度限制
        /// </summary>
        [TestMethod]
        public void GetErrorMessage_DeepNestedException_RespectsRecursionDepthLimit()
        {
            // Arrange
            // 创建一个具有25层嵌套的异常结构
            Exception deepException = new("第25层异常");
            for (int i = 24; i >= 1; i--)
            {
                deepException = new Exception($"第{i}层异常", deepException);
            }

            // Act
            string result = deepException.GetErrorMessage();

            // Assert
            Assert.Contains("警告: 已达到最大递归深度 20", result);
        }

        /// <summary>
        /// 测试堆栈跟踪
        /// </summary>
        [TestMethod]
        public void GetErrorMessage_ExceptionWithStackTrace_IncludesStackTrace()
        {
            // Arrange
            string result;
            try
            {
                throw new Exception("测试堆栈跟踪");
            }
            catch (Exception ex)
            {
                // Act
                result = ex.GetErrorMessage();
            }

            // Assert
            Assert.Contains("--- 异常堆栈跟踪结束 ---", result);
        }
    }
}
