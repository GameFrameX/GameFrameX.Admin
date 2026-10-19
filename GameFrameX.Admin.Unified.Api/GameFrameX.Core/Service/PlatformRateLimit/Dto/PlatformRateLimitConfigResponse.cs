namespace GameFrameX.Core.Service.PlatformRateLimit.Dto;

/// <summary>
/// 平台限流配置输出。
/// </summary>
public sealed class PlatformRateLimitConfigResponse
{
    /// <summary>
    /// 是否启用全局 IP 固定窗口限流。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 当前配置版本。
    /// </summary>
    public long Version { get; set; }

    /// <summary>
    /// 单个固定窗口允许的请求数。
    /// </summary>
    public int PermitLimit { get; set; } = 20;

    /// <summary>
    /// 固定窗口秒数。
    /// </summary>
    public int WindowSeconds { get; set; } = 1;

    /// <summary>
    /// 排队请求数量。
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// 最近更新时间。
    /// </summary>
    public long? UpdateTime { get; set; }
}