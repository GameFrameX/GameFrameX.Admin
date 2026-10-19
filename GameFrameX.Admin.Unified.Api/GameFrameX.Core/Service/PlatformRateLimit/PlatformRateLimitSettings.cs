namespace GameFrameX.Core.Service.PlatformRateLimit;

/// <summary>
/// 平台限流私有配置载荷。
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformRateLimitSettings
{
    /// <summary>
    /// 是否启用全局 IP 固定窗口限流。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 配置版本。每次保存递增，使后续请求切换到新的 limiter 分区。
    /// </summary>
    public long Version { get; set; } = 1;

    /// <summary>
    /// 单个固定窗口允许的请求数。
    /// </summary>
    public int PermitLimit { get; set; } = 20;

    /// <summary>
    /// 固定窗口秒数。
    /// </summary>
    public int WindowSeconds { get; set; } = 1;

    /// <summary>
    /// 排队请求数量。当前默认 0，与旧硬编码行为保持一致。
    /// </summary>
    public int QueueLimit { get; set; }
}