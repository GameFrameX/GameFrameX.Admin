namespace GameFrameX.Core.Job;

/// <summary>
/// 持久化层与 <see cref="历史框架.Schedule"/> 类型之间 <c>*Time</c> 字段的唯一转换入口。
/// <para>
/// 强制规则（C144）：项目内任何业务 <c>*Time</c> 字段一律存储为 <c>long?</c>，表示 UTC 秒级 Unix 时间戳。
/// 与 <c>历史框架.Schedule.Trigger / JobDetail</c> 交互时必须经过本类，禁止再使用 Mapster <c>Adapt/MapTo</c>
/// 或 <c>历史框架.Schedule.TriggerBuilder.LoadFrom</c>、<c>历史框架.Schedule.JobBuilder.LoadFrom</c> 这类隐式
/// 跨类型映射直接处理时间字段（已确认其在缺少显式转换规则时会抛
/// <c>System.InvalidCastException: Invalid cast from 'DateTime' to 'Int64'</c>）。
/// </para>
/// </summary>
public static class ScheduleTimeBridge
{
    /// <summary>
    /// 将 <see cref="DateTime"/>? 按 UTC 秒级 Unix 时间戳编码为 <c>long?</c>。
    /// <para>
    /// 输入的 <c>Kind</c> 一律按 UTC 解释（与项目内 "DateTime.Now 统一 UTC" 约定一致），避免本地时区偏移混入持久化值。
    /// 输入为 <c>null</c> 时返回 <c>null</c>。
    /// </para>
    /// </summary>
    public static long? ToUnixSeconds(DateTime? value)
    {
        return TimerHelper.DateTimeToSeconds(value is { } dateTime ? DateTime.SpecifyKind(dateTime, DateTimeKind.Utc) : null, true);
    }

    /// <summary>
    /// 将 <c>long?</c> 解码为 UTC 秒级 Unix 时间戳对应的 <see cref="DateTime"/>?。
    /// <para>
    /// 返回值的 <c>Kind</c> 始终为 <see cref="DateTimeKind.Utc"/>。输入为 <c>null</c> 时返回 <c>null</c>。
    /// </para>
    /// </summary>
    public static DateTime? FromUnixSeconds(long? value)
    {
        return TimerHelper.TimestampSecondsToDateTime(value, true);
    }
}