using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.System;

/// <summary>
/// 系统作业集群表
/// </summary>
[SugarTable(null, "系统作业集群表")]
[PlatformTable]
[IncrementTable]
public sealed class SystemJobClusterEntity : EntityBase
{
    /// <summary>
    /// 作业集群Id
    /// </summary>
    [SugarColumn(ColumnDescription = "作业集群Id", Length = EntityStringLength.Length256)]
    public string ClusterId { get; set; } = string.Empty;

    /// <summary>
    /// 描述信息
    /// </summary>
    [SugarColumn(ColumnDescription = "描述信息", Length = EntityStringLength.Length256)]
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态", DefaultValue = "0")]
    public ClusterStatus Status { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public long? UpdatedTime { get; set; }
}