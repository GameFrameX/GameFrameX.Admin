using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Application.Game.App.GameAppPackage.Dto;

/// <summary>
/// 游戏应用分包输出参数
/// </summary>
public class GameAppPackageResponse : IBasePageResponse<TenantGameAppPackageEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 游戏应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 分包名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 平台编码快照
    /// </summary>
    public string PlatformCode { get; set; } = null!;

    /// <summary>
    /// 平台名称快照
    /// </summary>
    public string PlatformName { get; set; } = null!;

    /// <summary>
    /// 渠道Id
    /// </summary>
    public long ChannelId { get; set; }

    /// <summary>
    /// 渠道编码快照
    /// </summary>
    public string ChannelCode { get; set; } = null!;

    /// <summary>
    /// 渠道名称快照
    /// </summary>
    public string ChannelName { get; set; } = null!;

    /// <summary>
    /// 母包Id
    /// </summary>
    public long? ParentPackageId { get; set; }

    /// <summary>
    /// 分包类型
    /// </summary>
    public PackageKindEnum PackageKind { get; set; }

    /// <summary>
    /// 平台继承模式
    /// </summary>
    public InheritanceModeEnum PlatformInheritanceMode { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string PackageVersion { get; set; } = null!;

    /// <summary>
    /// 是否默认分包
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedName { get; set; } = null!;

    /// <summary>
    /// 更新时间
    /// </summary>
    public long UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string UpdatedName { get; set; } = null!;

    /// <summary>
    /// 乐观锁版本。前端按 <c>rowVersion</c> 回传，避免与业务包版本 <see cref="PackageVersion"/> 混淆。
    /// </summary>
    public long RowVersion { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;
}