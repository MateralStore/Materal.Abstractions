using System.Collections.Concurrent;
using System.Diagnostics;

namespace Materal.Abstractions;

/// <summary>
/// 异常扩展
/// </summary>
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
    /// <param name="exception"></param>
    /// <param name="getDetailMessage"></param>
    /// <param name="maxDepth"></param>
    /// <returns></returns>
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
    /// <param name="exception"></param>
    /// <param name="prefix"></param>
    /// <param name="getDetailMessage"></param>
    /// <param name="maxDepth"></param>
    /// <returns></returns>
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
    /// <param name="prefix">前缀</param>
    /// <param name="getDetailMessage">获取详细消息的委托</param>
    /// <returns>异常消息</returns>
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
    /// <param name="prefix">前缀</param>
    /// <returns>详细消息</returns>
    private static string? GetDetailMessageByReflection(Exception exception, string prefix)
    {
        try
        {
            Type exceptionType = exception.GetType();
            static MethodInfo? GetGetMethod(Type type) => type.GetMethod(nameof(MateralException.GetDetailMessage), [typeof(string)]);
            MethodInfo? methodInfo = _detailMessageMethodCache.GetOrAdd(exceptionType, GetGetMethod);
            if (methodInfo is not null && methodInfo.ReturnType == typeof(string))
            {
                return methodInfo.Invoke(exception, [prefix]) as string;
            }
        }
        catch
        {
            Debug.WriteLine($"反射调用 GetDetailMessage 方法失败: {exception.Message}");
        }
        return null;
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
        public int Value
        {
            get => _value;
            set => _value = value;
        }

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
