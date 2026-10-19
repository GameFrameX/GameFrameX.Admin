using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game.App;

/// <summary>
/// 游戏应用分包
/// </summary>
[SugarTable(null, "游戏应用分包")]
[TenantTable]
[IncrementTable]
public class TenantGameAppPackageEntity : EntityBasicSelect
{
    public TenantGameAppPackageEntity()
    {
        IsEnabled = true;
        PackageKind = PackageKindEnum.Normal;
        PlatformInheritanceMode = InheritanceModeEnum.Override;
    }

    /// <summary>
    /// 游戏应用Id
    /// </summary>
    [SugarColumn(ColumnDescription = "游戏应用Id", DefaultValue = "0")]
    public long GameAppId { get; set; }

    /// <summary>
    /// 包名
    /// </summary>
    [SugarColumn(ColumnDescription = "包名", Length = EntityStringLength.Length512)]
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 平台编码快照
    /// </summary>
    [SugarColumn(ColumnDescription = "平台编码快照")]
    public string PlatformCode { get; set; } = null!;

    /// <summary>
    /// 平台名称快照
    /// </summary>
    [SugarColumn(ColumnDescription = "平台名称快照")]
    public string PlatformName { get; set; } = null!;

    /// <summary>
    /// 渠道Id
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道Id", DefaultValue = "0")]
    public long ChannelId { get; set; }

    /// <summary>
    /// 渠道编码快照
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道编码快照")]
    public string ChannelCode { get; set; } = null!;

    /// <summary>
    /// 渠道名称快照
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道名称快照")]
    public string ChannelName { get; set; } = null!;

    /// <summary>
    /// 母包Id
    /// </summary>
    [SugarColumn(ColumnDescription = "母包Id", IsNullable = true)]
    public long? ParentPackageId { get; set; }

    /// <summary>
    /// 分包类型
    /// </summary>
    [SugarColumn(ColumnDescription = "分包类型", DefaultValue = "0")]
    public PackageKindEnum PackageKind { get; set; }

    /// <summary>
    /// 平台继承模式
    /// </summary>
    [SugarColumn(ColumnDescription = "平台继承模式", DefaultValue = "0")]
    public InheritanceModeEnum PlatformInheritanceMode { get; set; }

    /// <summary>
    /// 资源版本号
    /// </summary>
    [SugarColumn(ColumnDescription = "资源版本号", IsNullable = true)]
    public string PackageVersion { get; set; } = null!;

    /// <summary>
    /// 是否默认分包
    /// </summary>
    [SugarColumn(ColumnDescription = "是否默认分包", DefaultValue = "false")]
    public bool IsDefault { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(ColumnDescription = "备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string Remark { get; set; } = null!;
}