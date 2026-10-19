using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Routing;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.System;

/// <summary>
/// 系统作业触发器表
/// </summary>
[SugarTable(null, "系统作业触发器表")]
[PlatformTable]
[IncrementTable]
public sealed class SystemJobTriggerEntity : EntityBase
{
    /// <summary>
    /// 触发器Id
    /// </summary>
    [SugarColumn(ColumnDescription = "触发器Id", Length = EntityStringLength.Length256)]
    public string TriggerId { get; set; } = string.Empty;

    /// <summary>
    /// 作业Id
    /// </summary>
    [SugarColumn(ColumnDescription = "作业Id", Length = EntityStringLength.Length256)]
    public string JobId { get; set; } = string.Empty;

    /// <summary>
    /// 触发器类型FullName
    /// </summary>
    [SugarColumn(ColumnDescription = "触发器类型", Length = EntityStringLength.Length256)]
    public string? TriggerType { get; set; }

    /// <summary>
    /// 程序集Name
    /// </summary>
    [SugarColumn(ColumnDescription = "程序集", Length = EntityStringLength.Length256)]
    public string? AssemblyName { get; set; } = "GameFrameX.Core";

    /// <summary>
    /// 参数
    /// </summary>
    [SugarColumn(ColumnDescription = "参数", Length = EntityStringLength.Length256)]
    public string? Args { get; set; }

    /// <summary>
    /// 描述信息
    /// </summary>
    [SugarColumn(ColumnDescription = "描述信息", Length = EntityStringLength.Length256)]
    public string? Description { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    [SugarColumn(ColumnDescription = "状态", DefaultValue = "0")]
    public TriggerStatus Status { get; set; } = TriggerStatus.Ready;

    /// <summary>
    /// 起始时间
    /// </summary>
    [SugarColumn(ColumnDescription = "起始时间")]
    public long? StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    [SugarColumn(ColumnDescription = "结束时间")]
    public long? EndTime { get; set; }

    /// <summary>
    /// 最近运行时间
    /// </summary>
    [SugarColumn(ColumnDescription = "最近运行时间")]
    public long? LastRunTime { get; set; }

    /// <summary>
    /// 下一次运行时间
    /// </summary>
    [SugarColumn(ColumnDescription = "下一次运行时间")]
    public long? NextRunTime { get; set; }

    /// <summary>
    /// 触发次数
    /// </summary>
    [SugarColumn(ColumnDescription = "触发次数", DefaultValue = "0")]
    public long NumberOfRuns { get; set; }

    /// <summary>
    /// 最大触发次数（0:不限制，n:N次）
    /// </summary>
    [SugarColumn(ColumnDescription = "最大触发次数", DefaultValue = "0")]
    public long MaxNumberOfRuns { get; set; }

    /// <summary>
    /// 出错次数
    /// </summary>
    [SugarColumn(ColumnDescription = "出错次数", DefaultValue = "0")]
    public long NumberOfErrors { get; set; }

    /// <summary>
    /// 最大出错次数（0:不限制，n:N次）
    /// </summary>
    [SugarColumn(ColumnDescription = "最大出错次数", DefaultValue = "0")]
    public long MaxNumberOfErrors { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    [SugarColumn(ColumnDescription = "重试次数", DefaultValue = "0")]
    public int NumRetries { get; set; }

    /// <summary>
    /// 重试间隔时间（ms）
    /// </summary>
    [SugarColumn(ColumnDescription = "重试间隔时间(ms)", DefaultValue = "0")]
    public int RetryTimeout { get; set; } = 1000;

    /// <summary>
    /// 是否立即启动
    /// </summary>
    [SugarColumn(ColumnDescription = "是否立即启动", DefaultValue = "false")]
    public bool StartNow { get; set; } = true;

    /// <summary>
    /// 是否启动时执行一次
    /// </summary>
    [SugarColumn(ColumnDescription = "是否启动时执行一次", DefaultValue = "false")]
    public bool RunOnStart { get; set; } = false;

    /// <summary>
    /// 是否在启动时重置最大触发次数等于一次的作业
    /// </summary>
    [SugarColumn(ColumnDescription = "是否重置触发次数", DefaultValue = "false")]
    public bool ResetOnlyOnce { get; set; } = true;

    /// <summary>
    /// 更新时间
    /// </summary>
    [SugarColumn(ColumnDescription = "更新时间")]
    public long? UpdatedTime { get; set; }
}