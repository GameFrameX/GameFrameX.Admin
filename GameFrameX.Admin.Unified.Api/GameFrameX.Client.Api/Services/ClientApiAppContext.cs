namespace GameFrameX.Client.Api.Services;

/// <summary>
/// Game.Api App 上下文解析（C11 支付域：AppId 事实数据 + 沙箱策略 / 支付配置的应用级作用域）
/// </summary>
public static class ClientApiAppContext
{
    /// <summary>
    /// HttpContext.Items 中保存已解析应用 ID 的键（与 Client.Api <c>AppResolutionMiddleware.AppIdKey</c> 同键，测试可直接写入）
    /// </summary>
    public const string AppIdKey = "AppId";

    /// <summary>
    /// 解析当前请求 AppId：优先中间件上下文 Items（App 凭证校验后写入）；
    /// 缺失时内联读 GameFrameX-App-Id 头——与 SignatureValidationMiddleware / CryptoEnvelopeMiddleware 同方案
    /// （本 host 的 AppResolutionMiddleware 仅在 App-Id + App-Secret 双头齐全时才校验拦截，游戏侧请求仅带 AppId 头、
    /// 走头回退解析；伪造该头只会错位密钥域导致验签失败。App 级鉴权由签名中间件兜底——但 C379 起 Tier 0
    /// 公开只读端点（PublicStartupEndpoint 标记）不依赖签名兜底，其边界是租户/应用上下文 + 公开数据）。
    /// </summary>
    /// <param name="context">当前 HTTP 上下文</param>
    /// <returns>应用 ID；解析不到返回 0（按租户默认作用域处理）</returns>
    public static long ResolveAppId(HttpContext? context)
    {
        if (context != null &&
            context.Items.TryGetValue(AppIdKey, out var item) &&
            long.TryParse(item?.ToString(), out var fromContext) &&
            fromContext > 0)
        {
            return fromContext;
        }

        var header = context?.Request.Headers[GlobalConst.AppIdHeader].FirstOrDefault();
        if (long.TryParse(header, out var fromHeader) && fromHeader > 0)
        {
            return fromHeader;
        }

        return 0;
    }
}