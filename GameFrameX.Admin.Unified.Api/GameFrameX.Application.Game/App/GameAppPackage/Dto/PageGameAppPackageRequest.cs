namespace GameFrameX.Application.Game.App.GameAppPackage.Dto;

/// <summary>
/// 游戏应用分包分页查询输入参数
/// </summary>
public class PageGameAppPackageRequest : BasePageRequest
{
    /// <summary>
    /// 游戏应用Id
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 分包名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 分包版本号
    /// </summary>
    public string PackageVersion { get; set; } = null!;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 母包Id
    /// </summary>
    public long? ParentPackageId { get; set; }
}