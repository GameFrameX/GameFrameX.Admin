using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.Log;

/// <summary>
/// 平台访问日志归档表（主库 system_log_visit_archive_entity 归属）。字段与 <see cref="SystemLogVisitEntity"/> 一致，
/// 另加归档时间；由审计留存作业把超期热数据搬入。
/// </summary>
[SugarTable(null, "平台访问日志归档表")]
[IncrementTable]
[PlatformTable]
public class SystemLogVisitArchiveEntity : LogVisitFields, IArchiveTimestamped
{
    /// <summary>
    /// 归档时间（UTC 秒级时间戳）
    /// </summary>
    [SugarColumn(ColumnDescription = "归档时间")]
    public long? ArchivedTime { get; set; }
}
