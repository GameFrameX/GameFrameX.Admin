using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Entity.Tenant;

/// <summary>
/// 系统在线用户表
/// </summary>
[SugarTable(null, "租户库在线用户表")]
[TenantTable]
[IncrementTable]
public sealed class TenantOnlineUserEntity : EntityTenantBase
{
    /// <summary>
    /// 连接Id
    /// </summary>
    [SugarColumn(ColumnDescription = "连接Id")]
    public string? ConnectionId { get; set; }

    /// <summary>
    /// 用户Id
    /// </summary>
    [SugarColumn(ColumnDescription = "用户Id", DefaultValue = "0")]
    public long UserId { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    [SugarColumn(ColumnDescription = "账号", Length = EntityStringLength.Length256)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 真实姓名
    /// </summary>
    [SugarColumn(ColumnDescription = "真实姓名", Length = EntityStringLength.Length256)]
    public string? RealName { get; set; }

    /// <summary>
    /// 连接时间
    /// </summary>
    [SugarColumn(ColumnDescription = "连接时间")]
    public long? Time { get; set; }

    /// <summary>
    /// 连接IP
    /// </summary>
    [SugarColumn(ColumnDescription = "连接IP", Length = EntityStringLength.Length512)]
    public string? Ip { get; set; }

    /// <summary>
    /// 浏览器
    /// </summary>
    [SugarColumn(ColumnDescription = "浏览器", Length = EntityStringLength.Length256)]
    public string? Browser { get; set; }

    /// <summary>
    /// 操作系统
    /// </summary>
    [SugarColumn(ColumnDescription = "操作系统", Length = EntityStringLength.Length256)]
    public string? Os { get; set; }
}