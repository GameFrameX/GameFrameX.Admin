namespace GameFrameX.Application.System.Logging;

/// <summary>
/// 日志时间戳查询范围。
/// </summary>
public sealed class LogTimestampRange
{
    /// <summary>开始时间戳。</summary>
    public long StartTimestamp { get; }

    /// <summary>结束时间戳。</summary>
    public long EndTimestamp { get; }

    public LogTimestampRange(long startTimestamp, long endTimestamp)
    {
        StartTimestamp = startTimestamp;
        EndTimestamp = endTimestamp;
    }
}