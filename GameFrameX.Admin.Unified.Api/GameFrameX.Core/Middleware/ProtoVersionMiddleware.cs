namespace GameFrameX.Core.Middleware;

/// <summary>
/// 协议版本中间件（change C5 server-envelope-middleware，对端 = 客户端 X-Proto-Ver 恒发头）。
/// <para><b>缺头拒绝</b>：未上线无旧客户端，无版本头一律 400 拒绝，不做灰度放行；
/// endpoint metadata 实现 <see cref="IProtoVersionExempt"/> 的入口（外部平台 S2S 回调）豁免。</para>
/// <para><b>AuditOnly 语义</b>：头值不在白名单（<see cref="ProtoVersionOptions.AllowedVersions"/>）只记 Warning 日志放行，
/// <b>不拦截</b>——待不兼容 major 变更时才收紧为强制拒绝。</para>
/// <para>挂载点：管线最外层就近（TenantResolution 之后），先于签名/信封——版本观察与密钥态无关。</para>
/// </summary>
public sealed class ProtoVersionMiddleware
{
    /// <summary>
    /// 协议版本头（客户端 D2 起恒发：PROJECT_ID + "/" + PROTO_VER）。
    /// </summary>
    public const string ProtoVersionHeader = "X-Proto-Ver";

    private readonly RequestDelegate _next;

    public ProtoVersionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<ProtoVersionOptions> optionsAccessor, ILogger<ProtoVersionMiddleware> logger)
    {
        var headerValue = context.Request.Headers[ProtoVersionHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(headerValue))
        {
            if (context.GetEndpoint()?.Metadata.GetMetadata<IProtoVersionExempt>() != null)
            {
                // 外部平台 S2S 回调不带 X-Proto-Ver，豁免缺头拒绝
                await _next(context);
                return;
            }

            // 缺版本头一律拒绝；记录 path/appId 便于定位未携带头的接入方
            logger.LogWarning("缺失协议版本头，拒绝请求：path={Path} appId={AppId}", context.Request.Path, context.Request.Headers[GlobalConst.AppIdHeader].FirstOrDefault());
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status400BadRequest, "缺少协议版本头（X-Proto-Ver）");
            return;
        }

        var options = optionsAccessor.Value;
        if (!options.AllowedVersions.Contains(headerValue, StringComparer.Ordinal))
        {
            // AuditOnly：只告警不拦截（major 变更时才收紧）；记录 appId/租户便于定位未升级客户端
            logger.LogWarning("未知协议版本放行（AuditOnly）：protoVer={ProtoVer} allowed=[{Allowed}] path={Path} appId={AppId}",
                              headerValue, string.Join(",", options.AllowedVersions), context.Request.Path, context.Request.Headers[GlobalConst.AppIdHeader].FirstOrDefault());
        }

        await _next(context);
    }
}