using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game.Release;

[SugarTable(null, "客户端资源包版本")]
[TenantTable]
[IncrementTable]
public sealed class TenantGameAppAssetPackageVersionEntity : EntityBasic
{
    /// <summary>
    /// 应用作用域，0 / null 表示租户默认配置
    /// </summary>
    [SugarColumn(ColumnDescription = "应用作用域", IsNullable = true)]
    public long? AppId { get; set; }

    /// <summary>
    /// 渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道名称", Length = EntityStringLength.Length256)]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    [SugarColumn(ColumnDescription = "平台", Length = EntityStringLength.Length256)]
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 游戏版本
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏版本", Length = EntityStringLength.Length256)]
    public string AppVersion { get; set; } = null!;


    /// <summary>
    /// 包名
    /// </summary>
    [SugarColumn(ColumnDescription = "包名", Length = EntityStringLength.Length256)]
    public string Package { get; set; } = null!;

    /// <summary>
    /// 语言
    /// </summary>
    [SugarColumn(ColumnDescription = "语言", Length = EntityStringLength.Length256)]
    public string Language { get; set; } = null!;

    /// <summary>
    /// 资源包名称
    /// </summary>
    [SugarColumn(ColumnDescription = "资源包名称", Length = EntityStringLength.Length256)]
    public string AssetPackageName { get; set; } = null!;

    /// <summary>
    /// 资源下载根路径
    /// </summary>
    [SugarColumn(ColumnDescription = "资源下载根路径", Length = EntityStringLength.Length256)]
    public string RootPath { get; set; } = null!;

    /// <summary>
    /// 资源版本
    /// </summary>
    [SugarColumn(ColumnDescription = "资源版本", Length = EntityStringLength.Length256)]
    public string ResourceVersion { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    [SugarColumn(ColumnDescription = "详细描述", Length = EntityStringLength.Length1024)]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 机器码分组id
    /// </summary>
    [SugarColumn(ColumnDescription = "机器码分组id")]
    public long? DeviceGroupId { get; set; }
}