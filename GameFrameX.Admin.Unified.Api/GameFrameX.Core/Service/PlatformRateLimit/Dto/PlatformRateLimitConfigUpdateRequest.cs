namespace GameFrameX.Core.Service.PlatformRateLimit.Dto;

/// <summary>
/// 平台限流配置更新输入。
/// </summary>
public sealed class PlatformRateLimitConfigUpdateRequest
{
    /// <summary>
    /// 是否启用全局 IP 固定窗口限流。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 单个固定窗口允许的请求数。
    /// </summary>
    [Range(1, 100000)]
    public int PermitLimit { get; set; } = 20;

    /// <summary>
    /// 固定窗口秒数。
    /// </summary>
    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 1;

    /// <summary>
    /// 排队请求数量。
    /// </summary>
    [Range(0, 100000)]
    public int QueueLimit { get; set; }
}