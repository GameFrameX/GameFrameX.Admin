using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Log;

/// <summary>
/// 审计日志公共字段基类。不映射物理表，供 <see cref="SystemLogAuditEntity"/> 与 <see cref="TenantLogAuditEntity"/> 复用字段。
/// </summary>
public class LogAuditFields : EntityBasic
{
    /// <summary>
    /// 表名
    /// </summary>
    [SugarColumn(ColumnDescription = "表名", Length = EntityStringLength.Length256)]
    public virtual string TableName { get; set; } = string.Empty;

    /// <summary>
    /// 列名
    /// </summary>
    [SugarColumn(ColumnDescription = "列名", Length = EntityStringLength.Length256)]
    public virtual string ColumnName { get; set; } = string.Empty;

    /// <summary>
    /// 新值
    /// </summary>
    [SugarColumn(ColumnDescription = "新值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? NewValue { get; set; }

    /// <summary>
    /// 旧值
    /// </summary>
    [SugarColumn(ColumnDescription = "旧值", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string? OldValue { get; set; }

    /// <summary>
    /// 操作方式（新增、更新、删除）
    /// </summary>
    [SugarColumn(ColumnDescription = "操作方式", DefaultValue = "0")]
    public DataOpTypeEnum Operate { get; set; }

    /// <summary>
    /// 审计时间
    /// </summary>
    [SugarColumn(ColumnDescription = "审计时间")]
    public long? AuditTime { get; set; }
}