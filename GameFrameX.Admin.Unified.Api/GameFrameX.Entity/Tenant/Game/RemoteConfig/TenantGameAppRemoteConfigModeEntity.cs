using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.RemoteConfig;

/// <summary>
/// 客户端远程配置模式定义表
/// </summary>
[SugarTable(null, "客户端远程配置模式定义表")]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppRemoteConfigModeEntity : EntityBasic
{
    /// <summary>
    /// 模式编码
    /// </summary>
    [SugarColumn(ColumnDescription = "模式编码", Length = EntityStringLength.Length256)]
    public string ModeCode { get; set; } = null!;

    /// <summary>
    /// 显示名称
    /// </summary>
    [SugarColumn(ColumnDescription = "显示名称", Length = EntityStringLength.Length256)]
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    [SugarColumn(ColumnDescription = "描述", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string? Description { get; set; }

    /// <summary>
    /// 排序序号
    /// </summary>
    [SugarColumn(ColumnDescription = "排序序号", DefaultValue = "0")]
    public int SortOrder { get; set; } = 0;
}