namespace GameFrameX.Client.Api.GameAssetPackageVersion.Dto;

/// <summary>
/// 
/// </summary>
public class AssetPackageVersionResponse
{
    /// <summary>
    /// 语言
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 资源版本
    /// </summary>
    public string Version { get; set; } = null!;

    /// <summary>
    /// 资源包名称
    /// </summary>
    public string AssetPackageName { get; set; } = null!;

    /// <summary>
    /// 资源包路径
    /// </summary>
    public string AssetPackagePath { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 下载根路径
    /// </summary>
    public string RootPath { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 游戏版本
    /// </summary>
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 渠道名称
    /// </summary>
    public string Channel { get; set; } = null!;
}