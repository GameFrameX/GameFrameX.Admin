namespace GameFrameX.Core.Authorization;

/// <summary>
/// 请求期端点权限条目（U6/C42 缓存值）：由 <c>ApiEndpointCatalogSync</c> 在启动时构建并预热，
/// JwtHandler 请求期按 RoutePath 查得，做端点→绑定按钮→已授权 MenuId 反向鉴权。
/// </summary>
public sealed class EndpointPermissionEntry
{
    /// <summary>端点代理 Id（稳定身份）。</summary>
    public long EndpointId { get; set; }

    /// <summary>HTTP Method（对账键的一部分）。</summary>
    public string HttpMethod { get; set; } = string.Empty;

    /// <summary>规范化路由（对账键，非授权令牌）。</summary>
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>是否公开（[AllowAnonymous]）—— 请求期短路放行。</summary>
    public bool IsPublic { get; set; }

    /// <summary>是否仅需登录态（白名单）—— 请求期短路放行。</summary>
    public bool IsAuthenticatedOnly { get; set; }

    /// <summary>该端点绑定的按钮 MenuId 集（global_menu_endpoint）。OR 语义。</summary>
    public HashSet<long> BoundMenuIds { get; set; } = new();
}