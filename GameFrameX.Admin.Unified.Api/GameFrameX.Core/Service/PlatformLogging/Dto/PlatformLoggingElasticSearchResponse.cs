namespace GameFrameX.Core.Service.PlatformLogging.Dto;

/// <summary>
/// ES 日志输出。ServerUris 元素统一掩码为 <c>"***"</c>，仅保留数量，不回显明文。
/// </summary>
public sealed class PlatformLoggingElasticSearchResponse
{
    public bool IsEnabled { get; set; } = false;

    public List<string> ServerUris { get; set; } = new();

    public string DefaultIndex { get; set; } = "adminnet";
}