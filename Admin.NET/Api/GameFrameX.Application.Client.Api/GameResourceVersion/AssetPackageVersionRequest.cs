using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Application.Client.Api.GameResourceVersion;

/// <summary>
/// 
/// </summary>
public class AssetPackageVersionRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Language { get; set; }

    /// <summary>
    /// 资源包名称
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AssetPackageName { get; set; }

    /// <summary>
    /// 平台
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Platform { get; set; }

    /// <summary>
    /// 包名
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string PackageName { get; set; }

    /// <summary>
    /// 程序版本
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AppVersion { get; set; }

    /// <summary>
    /// 主渠道
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Channel { get; set; }
}