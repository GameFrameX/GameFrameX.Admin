namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// 单条漂移发现。
/// </summary>
public sealed class BindingDriftFinding
{
    /// <summary>漂移类别。</summary>
    public BindingDriftCategory Category { get; set; }

    /// <summary>严重级别（由类别查表派生）。</summary>
    public BindingDriftSeverity Severity { get; set; }

    /// <summary>归一化 HTTP 方法（若适用，否则空）。</summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>规范化路由（若适用，否则空）。</summary>
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>人类可读细节（含 Id/控制器等定位信息）。</summary>
    public string Detail { get; set; } = string.Empty;
}