namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// 平台日志配置常量。
/// </summary>
public static class PlatformLoggingConfigConstants
{
    /// <summary>
    /// SystemConfigEntity 中保存平台私有日志配置的固定编码。
    /// </summary>
    public const string ConfigCode = "platform_logging_config";

    /// <summary>
    /// 序列化后的配置最大长度，与 SystemConfigEntity.Value 列长度（2048）保持同余量。
    /// </summary>
    public const int MaxSerializedLength = 2048;

    /// <summary>
    /// DetailAsync 输出 ES ServerUris 时的统一掩码占位，避免明文 URI/凭据经接口回显。
    /// </summary>
    public const string ServerUriMaskPlaceholder = "***";
}