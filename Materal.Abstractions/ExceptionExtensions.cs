using System.Collections.Concurrent;
using System.Diagnostics;

namespace Materal.Abstractions;

/// <summary>
/// 异常扩展
/// </summary>
/// <remarks>
/// 提供异常处理的扩展方法，包括格式化异常信息、获取详细消息等功能。
/// 支持通过反射查找异常类型中的GetDetailMessage方法来自定义消息格式。
/// </remarks>
public static class ExceptionExtensions
{
    private static readonly ConcurrentDictionary<Type, MethodInfo?> _detailMessageMethodCache = new();
    /// <summary>
    /// 默认递归深度
    /// </summary>
    private const int DefaultMaxRecursionDepth = 20;
    private static readonly AsyncLocal<Counter> _recursionCounter = new();
    /// <summary>
    /// 获得错误消息
    /// </summary>
    /// <param name="exception">要获取消息的异常。</param>
    /// <param name="getDetailMessage">自定义获取详细消息的委托。如果提供，将优先使用此委托而不是异常类型中的GetDetailMessage方法。</param>
    /// <param name="maxDepth">最大递归深度，用于限制内部异常的递归层数。</param>
    /// <returns>格式化后的异常消息，包括异常类型、消息、堆栈跟踪和内部异常信息。</returns>
    /// <remarks>
    /// 此方法会自动格式化异常信息，包括异常类型、消息、堆栈跟踪和内部异常。
    /// <para>
    /// 消息获取的优先级顺序如下：
    /// 1. 如果提供了getDetailMessage委托，则使用该委托获取消息
    /// 2. 如果异常是MateralException类型，则调用其GetDetailMessage方法
    /// 3. 通过反射查找异常类型中是否有GetDetailMessage方法，如果有则调用
    /// 4. 使用异常的默认Message属性
    /// </para>
    /// <para>
    /// 通过在自定义异常类型中实现GetDetailMessage(string prefix)方法，可以自定义异常消息的格式。
    /// </para>
    /// </remarks>
    public static string GetErrorMessage(this Exception? exception, Func<Exception, string, string>? getDetailMessage = null, int maxDepth = DefaultMaxRecursionDepth)
    {
        if (exception is null) return string.Empty;
        // 重置递归计数器，确保每次从顶层调用时都从0开始
        if (_recursionCounter.Value == null)
        {
            _recursionCounter.Value = new Counter();
        }
        else
        {
            _recursionCounter.Value.Value = 0;
        }
        return exception.GetErrorMessage(null, getDetailMessage, maxDepth);
    }
    /// <summary>
    /// 获得错误消息
    /// </summary>
    /// <param name="exception">要获取消息的异常。</param>
    /// <param name="prefix">消息前缀，用于格式化输出时的缩进或标识。</param>
    /// <param name="getDetailMessage">自定义获取详细消息的委托。</param>
    /// <param name="maxDepth">最大递归深度。</param>
    /// <returns>格式化后的异常消息。</returns>
    /// <remarks>
    /// 用于递归处理异常及其内部异常的消息获取。
    /// </remarks>
    private static string GetErrorMessage(this Exception? exception, string? prefix, Func<Exception, string, string>? getDetailMessage = null, int maxDepth = DefaultMaxRecursionDepth)
    {
        if (exception is null) return string.Empty;
        prefix ??= string.Empty;
        // 递增当前递归深度
        int currentDepth = _recursionCounter.Value!.Increment();
        StringBuilder errorMessage = new(1024);
        string message = GetExceptionMessage(exception, prefix, getDetailMessage);
        errorMessage.AppendLine($"{prefix}--->{exception.GetType().FullName}: {message}");
        if (exception.InnerException is not null && currentDepth < maxDepth)
        {
            string innerExceptionMessage = exception.InnerException.GetErrorMessage($"\t{prefix}", getDetailMessage, maxDepth);
            errorMessage.Append(innerExceptionMessage);
        }
        else if (exception.InnerException is not null && currentDepth >= maxDepth)
        {
            errorMessage.AppendLine($"{prefix}\t警告: 已达到最大递归深度 {maxDepth}，剩余异常信息被截断");
        }
        if (exception.StackTrace is not null)
        {
            AppendStackTrace(errorMessage, exception.StackTrace, prefix);
        }
        // 递减递归深度
        _recursionCounter.Value!.Decrement();
        return errorMessage.ToString();
    }
    /// <summary>
    /// 获取异常消息
    /// </summary>
    /// <param name="exception">异常</param>
    /// <param name="prefix">消息前缀</param>
    /// <param name="getDetailMessage">获取详细消息的委托</param>
    /// <returns>异常消息</returns>
    /// <remarks>
    /// 根据优先级顺序获取异常消息：
    /// 1. 如果提供了getDetailMessage委托，则使用该委托
    /// 2. 如果异常是MateralException类型，则调用其GetDetailMessage方法
    /// 3. 通过反射查找异常类型中的GetDetailMessage方法
    /// 4. 使用异常的默认Message属性
    /// </remarks>
    private static string GetExceptionMessage(Exception exception, string prefix, Func<Exception, string, string>? getDetailMessage)
    {
        if (getDetailMessage is not null) return getDetailMessage(exception, prefix);
        return exception switch
        {
            MateralException materalException => materalException.GetDetailMessage(prefix),
            _ => GetDetailMessageByReflection(exception, prefix) ?? exception.Message
        };
    }
    /// <summary>
    /// 通过反射获取详细消息
    /// </summary>
    /// <param name="exception">异常</param>
    /// <param name="prefix">消息前缀</param>
    /// <returns>详细消息，如果未找到GetDetailMessage方法或调用失败则返回null</returns>
    /// <remarks>
    /// 使用反射查找异常类型中是否有名为GetDetailMessage且参数为string类型的方法，
    /// 如果找到则调用该方法获取详细消息。
    /// <para>
    /// 缓存机制说明：
    /// 结果会被缓存到_detailMessageMethodCache字典中以提高性能。对于每个异常类型，
    /// 只会进行一次反射查找操作。如果某个类型没有实现GetDetailMessage方法，
    /// 缓存中会存储null值，后续对该类型的所有调用都会直接返回null，
    /// 避免重复进行反射查找操作。
    /// </para>
    /// <para>
    /// 性能优化：
    /// 1. 使用ConcurrentDictionary确保线程安全
    /// 2. 使用GetOrAdd方法避免重复的反射操作
    /// 3. 对于没有GetDetailMessage方法的类型，缓存null值避免重复查找
    /// </para>
    /// </remarks>
    private static string? GetDetailMessageByReflection(Exception exception, string prefix)
    {
        try
        {
            Type exceptionType = exception.GetType();
            static MethodInfo? GetGetMethod(Type type) => type.GetMethod(nameof(MateralException.GetDetailMessage), [typeof(string)]);
            MethodInfo? methodInfo = _detailMessageMethodCache.GetOrAdd(exceptionType, GetGetMethod);
            if (methodInfo is null || methodInfo.ReturnType != typeof(string)) return null;
            return methodInfo.Invoke(exception, [prefix]) as string;
        }
        catch
        {
            Debug.WriteLine($"反射调用 GetDetailMessage 方法失败: {exception.Message}");
            return null;
        }
    }
    /// <summary>
    /// 添加堆栈跟踪信息
    /// </summary>
    /// <param name="errorMessage">错误消息构建器</param>
    /// <param name="stackTrace">堆栈跟踪</param>
    /// <param name="prefix">前缀</param>
    private static void AppendStackTrace(StringBuilder errorMessage, string stackTrace, string prefix)
    {
        string[] stackTraces = stackTrace.Split('\n');
        foreach (string trace in stackTraces)
        {
            string formattedTrace = trace.Length > 0 && trace[^1] == '\r' ? trace[..^1] : trace;
            errorMessage.AppendLine($"{prefix}{formattedTrace}");
        }
        errorMessage.AppendLine($"{prefix}--- 异常堆栈跟踪结束 ---");
    }
    /// <summary>
    /// 线程安全的计数器类
    /// </summary>
    private class Counter
    {
        private int _value;
        /// <summary>
        /// 当前值
        /// </summary>
        public int Value { get => _value; set => _value = value; }
        /// <summary>
        /// 递增并返回递增后的值
        /// </summary>
        /// <returns>递增后的值</returns>
        public int Increment() => Interlocked.Increment(ref _value);
        /// <summary>
        /// 递减并返回递减后的值
        /// </summary>
        /// <returns>递减后的值</returns>
        public int Decrement() => Interlocked.Decrement(ref _value);
    }
}
