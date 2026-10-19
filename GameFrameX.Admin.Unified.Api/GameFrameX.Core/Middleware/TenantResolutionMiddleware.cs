namespace GameFrameX.Core.Middleware;

/// <summary>
/// 租户解析中间件：从租户请求头建立请求上下文（Client.Api / Game.Api 共享实现，C260 收敛，两 host 原逐字副本已回收）。
/// <para>写入的 Items 键契约见 <see cref="TenantContextAccessor"/>。租户头仅建立上下文，不构成鉴权：
/// 头存在性不代表验证成功（见 client-api spec C111）。</para>
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var tenantIdText = context.Request.Headers[GlobalConst.TenantIdHeader].FirstOrDefault();
        if (long.TryParse(tenantIdText, out var tenantId) && tenantId > 0)
        {
            context.Items[TenantContextAccessor.TenantIdKey] = tenantId;
        }

        var tenantSecret = context.Request.Headers[GlobalConst.TenantSecretHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(tenantSecret))
        {
            context.Items[TenantContextAccessor.TenantSecretKey] = tenantSecret;
        }

        var signature = context.Request.Headers[GlobalConst.SignatureHeader].FirstOrDefault();
        if (!string.IsNullOrWhiteSpace(signature))
        {
            context.Items[TenantContextAccessor.SignatureKey] = signature;
        }

        await _next(context);
    }
}