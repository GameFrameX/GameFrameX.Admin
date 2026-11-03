using GameFrameX.Client.Api.Services.PackageVersion;
namespace GameFrameX.Client.Api.Services.RemoteConfig;

/// <summary>
/// 客户端远程配置投放匹配请求。
/// </summary>
public sealed class RemoteConfigDeliveryRequest : IPackageRequest
{
    /// <summary>
    /// 平台。
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 包名。
    /// </summary>
    public string PackageName { get; set; } = null!;

    /// <summary>
    /// 应用版本。
    /// </summary>
    public string AppVersion { get; set; } = null!;

    /// <summary>
    /// 语言。
    /// </summary>
    public string Language { get; set; } = null!;

    /// <summary>
    /// 渠道。
    /// </summary>
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 设备稳定标识。缺失时只允许命中全量投放记录。
    /// </summary>
    public string? DeviceId { get; set; }
}