namespace GameFrameX.Core.Middleware;

/// <summary>
/// 公开 provider 回调路由上下文。
/// <para>这些字段只标记 provider 与分包，不代表业务授权；业务动作仍必须通过 provider 签名或 JWT 校验。</para>
/// </summary>
public sealed record PublicCallbackRouteContext(
    string Provider,
    string CallbackKind,
    long? PackageId)
{
    /// <summary>
    /// HttpContext.Items 中保存已解析公开回调上下文的键。
    /// </summary>
    public const string ItemKey = "PublicCallbackRouteContext";
}