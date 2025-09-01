namespace Materal.Abstractions;

/// <summary>
/// 日期时间单位枚举
/// </summary>
public enum DateTimeUnitEnum
{
    /// <summary>
    /// 年
    /// </summary>
    [Description("年")]
    YearUnit = 0,
    /// <summary>
    /// 月
    /// </summary>
    [Description("月")]
    MonthUnit = 1,
    /// <summary>
    /// 日
    /// </summary>
    [Description("日")]
    DayUnit = 2,
    /// <summary>
    /// 时
    /// </summary>
    [Description("时")]
    HourUnit = 3,
    /// <summary>
    /// 分
    /// </summary>
    [Description("分")]
    MinuteUnit = 4,
    /// <summary>
    /// 秒
    /// </summary>
    [Description("秒")]
    SecondUnit = 5,
    /// <summary>
    /// 毫秒
    /// </summary>
    [Description("毫秒")]
    MillisecondUnit = 6
}
