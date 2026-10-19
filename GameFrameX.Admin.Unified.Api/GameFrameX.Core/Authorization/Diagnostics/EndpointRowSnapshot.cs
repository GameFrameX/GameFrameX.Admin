namespace GameFrameX.Core.Authorization.Diagnostics;

/// <summary>
/// DB 端点行快照（来自 <c>global_api_endpoint</c>）。
/// </summary>
public sealed class EndpointRowSnapshot
{
    /// <summary>端点主键 Id（DB 代理主键，稳定身份）。</summary>
    public long Id { get; set; }

    /// <summary>归一化 HTTP 方法（大写）。</summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>规范化路由（api/... 小写）。</summary>
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>是否公开。</summary>
    public bool IsPublic { get; set; }

    /// <summary>是否仅需登录态。</summary>
    public bool IsAuthenticatedOnly { get; set; }

    /// <summary>是否孤儿（DB 有行但本次扫描无活端点）。</summary>
    public bool IsOrphan { get; set; }

    /// <summary>是否启用。</summary>
    public bool IsEnabled { get; set; }
}