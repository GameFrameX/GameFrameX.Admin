using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.RemoteConfig;

/// <summary>
/// 客户端远程配置内容键定义表
/// </summary>
[SugarTable(null, "客户端远程配置内容键定义表")]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppRemoteConfigKeyEntity : EntityBasic
{
    /// <summary>
    /// JSON key 名称
    /// </summary>
    [SugarColumn(ColumnDescription = "JSON key 名称", Length = EntityStringLength.Length256)]
    public string KeyName { get; set; } = null!;

    /// <summary>
    /// 显示名称
    /// </summary>
    [SugarColumn(ColumnDescription = "显示名称", Length = EntityStringLength.Length256)]
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// 值类型：string、bool、long
    /// </summary>
    [SugarColumn(ColumnDescription = "值类型", Length = EntityStringLength.Length256)]
    public string ValueType { get; set; } = "string";

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