namespace Materal.Abstractions;

/// <summary>
/// Materal基础异常类
/// </summary>
public class MateralException : Exception
{
    /// <summary>
    /// 初始化 <see cref="MateralException"/> 类的新实例。
    /// </summary>
    /// <remarks>
    /// 此构造函数创建一个没有错误消息的异常实例。
    /// 通常在不需要提供具体错误信息时使用。
    /// </remarks>
    public MateralException()
    {
    }
    /// <summary>
    /// 使用指定的错误消息初始化 <see cref="MateralException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    /// <remarks>
    /// 此构造函数创建一个带有指定错误消息的异常实例。
    /// 当需要向调用者提供具体的错误信息时使用此构造函数。
    /// </remarks>
    public MateralException(string message) : base(message)
    {
    }
    /// <summary>
    /// 使用指定的错误消息和对作为此异常原因的内部异常的引用来初始化 <see cref="MateralException"/> 类的新实例。
    /// </summary>
    /// <param name="message">描述错误的消息。</param>
    /// <param name="innerException">导致当前异常的异常，如果未指定内部异常，则为空引用。</param>
    /// <remarks>
    /// 此构造函数创建一个异常实例，该实例包含错误消息和对内部异常的引用。
    /// 当需要保留原始异常信息并添加额外上下文时使用此构造函数。
    /// </remarks>
    public MateralException(string message, Exception innerException) : base(message, innerException)
    {
    }
    /// <inheritdoc/>
    public override string ToString() => this.GetErrorMessage();
    /// <summary>
    /// 获取异常的详细消息
    /// </summary>
    /// <param name="prefix">消息前缀，用于格式化输出时的缩进或标识。</param>
    /// <returns>格式化后的异常详细消息。</returns>
    /// <remarks>
    /// 此方法用于自定义异常类型的详细消息格式，通常与 <see cref="ExceptionExtensions.GetErrorMessage(Exception, Func{Exception, string, string}, int)"/> 方法配合使用。
    /// 通过重写此方法，可以让不同的异常类型提供特定的格式化输出，使异常信息更加丰富和易读。
    /// <para>
    /// 使用示例：
    /// <code>
    /// public class CustomException : MateralException
    /// {
    ///     public CustomException(string message) : base(message) { }
    ///     
    ///     public override string GetDetailMessage(string prefix)
    ///     {
    ///         return $"自定义异常: {Message}";
    ///     }
    /// }
    /// </code>
    /// </para>
    /// </remarks>
    public virtual string GetDetailMessage(string prefix) => Message;
}
