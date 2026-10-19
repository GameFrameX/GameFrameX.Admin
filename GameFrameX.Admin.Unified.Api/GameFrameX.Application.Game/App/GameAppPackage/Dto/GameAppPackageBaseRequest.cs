using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.App.GameAppPackage.Dto;

/// <summary>
/// 游戏应用分包共有输入字段
/// </summary>
public class GameAppPackageBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 游戏应用Id
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "游戏应用Id不能为空")]
    [AdaptMember("GameAppId")]
    public long AppId { get; set; }

    /// <summary>
    /// 分包名称
    /// </summary>
    [Required(ErrorMessage = "分包名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    [Required(ErrorMessage = "包名不能为空")]
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 平台编码快照
    /// </summary>
    [Required(ErrorMessage = "平台不能为空")]
    public string PlatformCode { get; set; } = null!;

    /// <summary>
    /// 平台名称快照
    /// </summary>
    public string PlatformName { get; set; } = null!;

    /// <summary>
    /// 渠道Id
    /// </summary>
    [Range(1, long.MaxValue, ErrorMessage = "渠道Id不能为空")]
    public long ChannelId { get; set; }

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
    public PackageKindEnum PackageKind { get; set; } = PackageKindEnum.Normal;

    /// <summary>
    /// 平台继承模式
    /// </summary>
    public InheritanceModeEnum PlatformInheritanceMode { get; set; } = InheritanceModeEnum.Override;

    /// <summary>
    /// 版本号
    /// </summary>
    public string PackageVersion { get; set; } = null!;

    /// <summary>
    /// 是否默认分包
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string Remark { get; set; } = null!;
}