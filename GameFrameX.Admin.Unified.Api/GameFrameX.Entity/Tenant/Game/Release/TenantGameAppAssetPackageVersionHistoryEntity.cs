using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.Release;

/// <summary>
/// 客户端资源包版本变更历史（分包级流水，作用域跟随外键）。
/// 通过 <see cref="VersionId"/> 关联 <see cref="TenantGameAppAssetPackageVersionEntity"/>，
/// 作用域隐式继承对应资源包版本记录的 AppId + Channel + Platform + Package + Language，不冗余 AppId；
/// 查询入口必须带 VersionId 过滤，由外键链路保证隔离，不应被当成可跨版本扫描的应用数据。
/// </summary>
[SugarTable(null, "客户端资源包版本记录")]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppAssetPackageVersionHistoryEntity : EntityBasic
{
    /// <summary>
    /// 资源包版本Id
    /// </summary>
    [SugarColumn(ColumnDescription = "资源包版本Id", DefaultValue = "0")]
    public long VersionId { get; set; }

    /// <summary>
    /// 旧资源版本
    /// </summary>
    [SugarColumn(ColumnDescription = "旧资源版本", Length = EntityStringLength.Length256)]
    public string OldVersion { get; set; } = null!;


    /// <summary>
    /// 新资源版本
    /// </summary>
    [SugarColumn(ColumnDescription = "新资源版本", Length = EntityStringLength.Length256)]
    public string ResourceVersion { get; set; } = null!;
}