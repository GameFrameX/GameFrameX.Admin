namespace GameFrameX.Core.Middleware;

/// <summary>
/// 公开 provider 回调路由上下文。
/// <para>这些字段只用于在控制器构造前定位租户库和读取 provider 验签配置，不代表业务授权；订单、广告发放等业务动作仍必须通过 provider 签名或 JWT 校验。</para>
/// </summary>
public sealed record PublicCallbackRouteContext(
    long TenantId,
    long AppId,
    string Provider,
    string CallbackKind,
    long? PackageId)
{
    /// <summary>
    /// HttpContext.Items 中保存已解析公开回调上下文的键。
    /// </summary>
    public const string ItemKey = "PublicCallbackRouteContext";
}