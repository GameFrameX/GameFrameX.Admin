using GameFrameX.Client.Api.Services.PackageVersion;
using System.ComponentModel.DataAnnotations;
using GameFrameX.Client.Api.Services;

namespace GameFrameX.Client.Api.GameAppVersion.Dto;

/// <summary>
///
/// </summary>
public class GameAppVersionRequest : IPackageRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Language { get; set; } = null!;

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
    /// 主渠道名称
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Platform { get; set; } = null!;


}