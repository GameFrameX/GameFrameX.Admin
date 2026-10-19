namespace GameFrameX.Core.Service.PlatformLogging.Dto;

/// <summary>
/// 平台日志配置更新输入。
/// ES <see cref="ElasticSearchServerUris"/>：若全部为掩码占位 <c>"***"</c> 视为「未变更」，保留库内原值；
/// 含非占位项则按非占位项替换；空列表视为清空。
/// </summary>
public sealed class PlatformLoggingConfigUpdateRequest
{
    public PlatformLoggingFileUpdateRequest File { get; set; } = new();

    public PlatformLoggingDatabaseUpdateRequest Database { get; set; } = new();

    public PlatformLoggingElasticSearchUpdateRequest ElasticSearch { get; set; } = new();

    public PlatformLoggingMonitorUpdateRequest Monitor { get; set; } = new();
}