namespace GameFrameX.Application.Game.App.GameAppPackage.Dto;

/// <summary>
/// 游戏应用分包选择输出
/// </summary>
public class GameAppPackageListResponse
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
    /// 包名
    /// </summary>
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 分包版本号
    /// </summary>
    public string PackageVersion { get; set; } = null!;

    /// <summary>
    /// 是否默认分包
    /// </summary>
    public bool IsDefault { get; set; }
}