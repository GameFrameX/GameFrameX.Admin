using System.ComponentModel.DataAnnotations;

namespace GameFrameX.Hub.Api.Dto.GameResourceVersion;

/// <summary>
/// 资源包版本更新请求（Hub.Api 后台接口契约）
/// </summary>
public class AssetPackageVersionRequest
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
    /// 资源包版本
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string AssetPackageVersion { get; set; } = null!;
}