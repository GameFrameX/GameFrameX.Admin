namespace GameFrameX.Core.Middleware;

/// <summary>
/// 租户上下文统一访问器：三 host（Client / Game / Hub）共享的 <c>HttpContext.Items</c> 键契约与读取助手。
/// <para>任意来源（请求头 / JWT Claim / WebHook 请求头 / 公开回调路由）解析出的租户 Id，
/// 中间件一律写 <see cref="TenantIdKey"/>；<see cref="ISqlSugarTenantIdResolver"/> 接线与下游业务只认本键，
/// 不再各自定义键常量（Hub 专有键 <c>__hub_tenant_id</c> 已随 C260 移除）。</para>
/// <para>键值沿用 Client/Game 收敛前的原值 <c>TenantId</c>（与 <see cref="GameFrameX.Core.Const.ClaimConst.TenantId"/> 同值），
/// 保持既有 Items 语义不变。</para>
/// </summary>
public static class TenantContextAccessor
{
    /// <summary>
    /// HttpContext.Items 中保存已解析租户 ID 的键。
    /// </summary>
    public const string TenantIdKey = "TenantId";

    /// <summary>
    /// HttpContext.Items 中保存租户签名密钥原始 Header 值的键。
    /// </summary>
    public const string TenantSecretKey = "TenantSecret";

    /// <summary>
    /// HttpContext.Items 中保存请求内容签名原始 Header 值的键。
    /// </summary>
    public const string SignatureKey = "Signature";

    /// <summary>
    /// 读取当前请求已解析的租户 Id；未解析（无请求、键缺失或值不是 long）返回 null。
    /// <para>语义等价于收敛前各 Program.cs 手写接线中的 <c>Items[TenantIdKey] as long?</c>。</para>
    /// </summary>
    /// <param name="context">当前 HTTP 上下文；无请求背景时可传 null。</param>
    public static long? GetTenantId(HttpContext? context)
    {
        if (context != null
            && context.Items.TryGetValue(TenantIdKey, out var value)
            && value is long tenantId)
        {
            return tenantId;
        }

        return null;
    }
}