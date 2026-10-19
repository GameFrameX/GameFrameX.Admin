namespace GameFrameX.ObjectStorage.Abstract;

/// <summary>
/// 对象存储 Adapter 运行时上下文。由 Factory 从租户配置解析得到，承载 Provider 名、
/// 反序列化后的 Option 实例、Bucket、公共访问域名与路径模板。
/// </summary>
public sealed class ObjectStorageAdapterContext
{
    /// <summary>标准 Provider 名。</summary>
    public string Provider { get; set; } = null!;

    /// <summary>反序列化后的 Option 实例（具体类型见各 Provider 选项类），Adapter 内部按需强转。</summary>
    public object Options { get; set; } = null!;

    /// <summary>Bucket / 存储空间名（来自 Option，便于 Adapter 直接取用）。</summary>
    public string Bucket { get; set; } = null!;

    /// <summary>公共访问域名（来自 Option，可为空）。</summary>
    public string PublicBaseUrl { get; set; } = null!;

    /// <summary>路径模板，如 Upload/{yyyy}/{MM}/{dd}。</summary>
    public string PathTemplate { get; set; } = null!;

    /// <summary>原始 ConfigJson（调试/日志用）。</summary>
    public string RawConfigJson { get; set; } = null!;
}