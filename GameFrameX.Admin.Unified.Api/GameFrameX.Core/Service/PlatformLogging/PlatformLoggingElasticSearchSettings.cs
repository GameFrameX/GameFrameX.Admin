namespace GameFrameX.Core.Service.PlatformLogging;

/// <summary>
/// ElasticSearch 日志参数。IsEnabled 关闭可同 Host 动态生效；从启动期关闭态开启、或调整 ServerUris/DefaultIndex 需重启。
/// </summary>
public sealed class PlatformLoggingElasticSearchSettings
{
    /// <summary>
    /// 是否启用 ES 日志。默认关闭，等价旧 Logging.json 安全默认。
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>
    /// ES 服务地址列表（可含凭据）。DetailAsync 接口不回显明文，统一掩码。重启生效。
    /// </summary>
    public List<string> ServerUris { get; set; } = new();

    /// <summary>
    /// ES 默认索引。重启生效。
    /// </summary>
    public string DefaultIndex { get; set; } = "adminnet";
}