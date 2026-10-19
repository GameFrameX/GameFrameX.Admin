namespace GameFrameX.Core.Option;

/// <summary>
/// 缓存配置选项（内存缓存键前缀；Redis / Cluster 适配已随 C170 / GFX-171 退役）
/// </summary>
public sealed class CacheOptions
{
    /// <summary>
    /// 缓存前缀。默认空串：无配置节时所有键不加前缀，消费方无需空值防御。
    /// </summary>
    public string Prefix { get; set; } = string.Empty;
}
