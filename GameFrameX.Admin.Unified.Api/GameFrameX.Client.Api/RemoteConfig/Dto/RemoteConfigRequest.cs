using GameFrameX.Client.Api.Services.PackageVersion;
using System.ComponentModel.DataAnnotations;
using GameFrameX.Client.Api.Services;

namespace GameFrameX.Client.Api.RemoteConfig.Dto;

/// <summary>
///
/// </summary>
public class RemoteConfigRequest : IPackageRequest
{
    /// <summary>
    /// 语言
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Language { get; set; } = null!;

    /// <summary>
    /// 模块名称
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 平台
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Platform { get; set; } = null!;

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
    /// 设备稳定标识，用于记录级灰度取模；大小写按服务端统一转小写处理。
    /// </summary>
    public string? DeviceId { get; set; }
}