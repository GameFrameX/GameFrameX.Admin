namespace GameFrameX.Core.Service.PlatformCaptcha;

/// <summary>
/// 平台图形验证码配置常量。
/// </summary>
public static class PlatformCaptchaConfigConstants
{
    /// <summary>
    /// SystemConfigEntity 中保存平台验证码私有配置的固定编码。
    /// </summary>
    public const string ConfigCode = "platform_captcha_config";

    /// <summary>
    /// 动态验证码存储键前缀。
    /// </summary>
    public const string StorageKeyPrefix = "platform:captcha:";

    /// <summary>
    /// 序列化后的配置最大长度，与当前 SystemConfigEntity.Value 列长度保持一致。
    /// </summary>
    public const int MaxSerializedLength = 2048;
}