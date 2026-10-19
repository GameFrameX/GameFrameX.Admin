namespace GameFrameX.Core.Service.PlatformRateLimit;

/// <summary>
/// 平台限流配置常量。
/// </summary>
public static class PlatformRateLimitConfigConstants
{
    /// <summary>
    /// SystemConfigEntity 中保存平台限流私有配置的固定编码。
    /// </summary>
    public const string ConfigCode = "platform_rate_limit_config";

    /// <summary>
    /// 序列化后的配置最大长度，与当前 SystemConfigEntity.Value 列长度保持安全余量。
    /// </summary>
    public const int MaxSerializedLength = 512;
}