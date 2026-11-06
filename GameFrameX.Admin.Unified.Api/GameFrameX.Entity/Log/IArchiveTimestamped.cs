namespace GameFrameX.Entity.Log;

/// <summary>
/// 归档时间标记接口。审计留存作业按 <see cref="ArchivedTime"/> 计算归档表的保留期。
/// </summary>
public interface IArchiveTimestamped
{
    /// <summary>
    /// 归档时间（UTC 秒级时间戳）
    /// </summary>
    long? ArchivedTime { get; set; }
}
