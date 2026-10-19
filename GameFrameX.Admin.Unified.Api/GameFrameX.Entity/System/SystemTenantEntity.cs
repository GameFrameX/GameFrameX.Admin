using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Routing;

namespace GameFrameX.Entity.System;

/// <summary>
/// 系统租户表
/// </summary>
[SugarTable(null, "系统租户表")]
[PlatformTable]
[IncrementTable]
public class SystemTenantEntity : EntityBasic
{
    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 机构Id
    /// </summary>
    [SugarColumn(ColumnDescription = "机构Id", DefaultValue = "0")]
    public long OrganizationId { get; set; }

    /// <summary>
    /// 主机
    /// </summary>
    [SugarColumn(ColumnDescription = "主机", Length = EntityStringLength.Length256)]
    public string? Host { get; set; }

    /// <summary>
    /// 租户类型
    /// </summary>
    [SugarColumn(ColumnDescription = "租户类型", DefaultValue = "0")]
    public TenantTypeEnum TenantType { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库类型", DefaultValue = "0")]
    public DbType DbType { get; set; }

    /// <summary>
    /// 数据库连接
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库连接", Length = EntityStringLength.Length512)]
    public string? Connection { get; set; }

    /// <summary>
    /// 数据库标识
    /// </summary>
    [SugarColumn(ColumnDescription = "数据库标识", Length = EntityStringLength.Length256)]
    public string? ConfigId { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length256)]
    public string? Remark { get; set; }

    /// <summary>
    /// 套餐等级Id
    /// </summary>
    [SugarColumn(ColumnDescription = "套餐等级Id", IsNullable = true)]
    public long? PackageId { get; set; }

    /// <summary>
    /// 到期时间（秒级 Unix 时间戳）
    /// </summary>
    [SugarColumn(ColumnDescription = "到期时间", IsNullable = true)]
    public long? ExpireTime { get; set; }

    /// <summary>
    /// 过渡期天数（覆盖套餐默认值，null 使用套餐默认）
    /// </summary>
    [SugarColumn(ColumnDescription = "过渡期天数", IsNullable = true)]
    public int? GracePeriodDays { get; set; }
}