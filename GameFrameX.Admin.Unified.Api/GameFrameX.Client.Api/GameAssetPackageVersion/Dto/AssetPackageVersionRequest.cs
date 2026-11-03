using GameFrameX.Client.Api.Services.PackageVersion;
using System.ComponentModel.DataAnnotations;
using GameFrameX.Client.Api.Services;

namespace GameFrameX.Client.Api.GameAssetPackageVersion.Dto;

/// <summary>
/// 资源包版本请求
/// </summary>
public class AssetPackageVersionRequest : IPackageRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Language { get; set; } = null!;

    /// <summary>
    /// 资源包名称
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AssetPackageName { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 包名
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 程序版本
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 主渠道
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 机器码
    /// </summary>
    public string DeviceUniqueIdentifier { get; set; } = null!;


}