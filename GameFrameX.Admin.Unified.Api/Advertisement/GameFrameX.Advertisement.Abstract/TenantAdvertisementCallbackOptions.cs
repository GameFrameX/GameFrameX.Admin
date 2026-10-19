namespace GameFrameX.Advertisement.Abstract;

/// <summary>
/// 租户广告回调配置快照。null 引用表示该作用域未配置广告回调材料。
/// </summary>
public sealed class TenantAdvertisementCallbackOptions
{
    /// <summary>
    /// 广告平台 Provider。
    /// </summary>
    public string? Provider { get; init; }

    /// <summary>
    /// 广告平台后台配置的回调 URL。
    /// </summary>
    public string? CallbackUrl { get; init; }

    /// <summary>
    /// 签名密钥或共享密钥。
    /// </summary>
    public string? SigningSecret { get; init; }

    /// <summary>
    /// 验签公钥或验证材料原文。
    /// </summary>
    public string? VerificationKeys { get; init; }

    /// <summary>
    /// 参数作用说明 JSON。
    /// </summary>
    public string? ParameterPurposeJson { get; init; }

    /// <summary>
    /// 广告回调动态配置 JSON。
    /// </summary>
    public string? ConfigJson { get; init; }

    /// <summary>
    /// 按 Provider schema 解析出的动态配置参数。
    /// </summary>
    public IReadOnlyDictionary<string, string> ConfigValues { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>
    /// Pangle 回调验签密钥（= <see cref="SigningSecret"/>，Pangle WebHook 出口沿用的字段名）。
    /// </summary>
    public string? PangleSecurityKey { get; init; }

    /// <summary>
    /// AdMob 回调验签的 key_id → base64 key 映射（解析自 <see cref="VerificationKeys"/>）。
    /// </summary>
    public IReadOnlyDictionary<string, string> AdMobKeys { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}