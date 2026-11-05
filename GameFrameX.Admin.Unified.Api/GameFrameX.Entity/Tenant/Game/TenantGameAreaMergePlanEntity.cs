using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>主库合服计划及阶段报告。</summary>
[SugarTable(null, "合服计划")]
[TenantTable]
[IncrementTable]
public class TenantGameAreaMergePlanEntity : EntityBasic
{
    /// <summary>目标服。</summary>
    [SugarColumn(ColumnDescription = "目标服", DefaultValue = "0")]
    public long TargetServerId { get; set; }

    /// <summary>源服集合。</summary>
    [SugarColumn(ColumnDescription = "源服集合", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string SourceServerIds { get; set; } = string.Empty;

    /// <summary>Server 计划标识。</summary>
    [SugarColumn(ColumnDescription = "Server 计划标识")]
    public string PlanId { get; set; } = string.Empty;

    /// <summary>状态。</summary>
    [SugarColumn(ColumnDescription = "状态", DefaultValue = "0")]
    public GameAreaMergePlanStatus Status { get; set; }

    /// <summary>预演报告快照。</summary>
    [SugarColumn(ColumnDescription = "预演报告快照", IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? DryRunReportSnapshot { get; set; }

    /// <summary>执行报告快照。</summary>
    [SugarColumn(ColumnDescription = "执行报告快照", IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? ExecutionReportSnapshot { get; set; }

    /// <summary>申请人。</summary>
    [SugarColumn(ColumnDescription = "申请人", DefaultValue = "0")]
    public long ApplicantOperatorId { get; set; }

    /// <summary>复核人。</summary>
    [SugarColumn(ColumnDescription = "复核人", IsNullable = true)]
    public long? ReviewerOperatorId { get; set; }

    /// <summary>原因。</summary>
    [SugarColumn(ColumnDescription = "原因")]
    public string Reason { get; set; } = string.Empty;

    /// <summary>备份标识。</summary>
    [SugarColumn(ColumnDescription = "备份标识", IsNullable = true)]
    public string? BackupIdentifier { get; set; }

    /// <summary>已确认备份。</summary>
    [SugarColumn(ColumnDescription = "已确认备份", DefaultValue = "false")]
    public bool BackupConfirmed { get; set; }

    /// <summary>预演时间。</summary>
    [SugarColumn(ColumnDescription = "预演时间", IsNullable = true)]
    public long? DryRunTime { get; set; }

    /// <summary>批准时间。</summary>
    [SugarColumn(ColumnDescription = "批准时间", IsNullable = true)]
    public long? ApprovedTime { get; set; }

    /// <summary>开始执行时间。</summary>
    [SugarColumn(ColumnDescription = "开始执行时间", IsNullable = true)]
    public long? ExecutionStartedTime { get; set; }

    /// <summary>完成时间。</summary>
    [SugarColumn(ColumnDescription = "完成时间", IsNullable = true)]
    public long? CompletedTime { get; set; }

    /// <summary>失败时间。</summary>
    [SugarColumn(ColumnDescription = "失败时间", IsNullable = true)]
    public long? FailedTime { get; set; }

    /// <summary>失败原因。</summary>
    [SugarColumn(ColumnDescription = "失败原因", IsNullable = true, ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? FailureMessage { get; set; }

}
