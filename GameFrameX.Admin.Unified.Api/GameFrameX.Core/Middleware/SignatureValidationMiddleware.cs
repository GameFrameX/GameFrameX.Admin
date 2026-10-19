using System.Security.Cryptography;
using GameFrameX.Core.Extension.Cache;
using GameFrameX.Core.SignatureAuth;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 传输签名验证中间件（C1 hmac-foundation-migration 落地 v1；C2 sig-keyring-server 扩展 v2 密钥环；
/// C267 v2 query 升级 RFC 3986 canonical 并收口 HMAC 计算到 <see cref="CanonicalRequestBuilder"/>）。
/// 校验客户端 "Signature" 头的单层 HMAC-SHA256 签名 —— UTF-8 输入、Base64 输出（RFC 2104 + RFC 4648 §4）。
/// <para>v1 头 = "v1;&lt;ts&gt;;&lt;mid&gt;;&lt;base64&gt;"，message = "v1" \n METHOD \n path \n query \n ts \n mid \n hex(sha256(body))，
/// query 为原始串（C1 客户端行为，过渡逃生通道语义，C267 canonical 仅作用于 v2），
/// 密钥为全局常量（<see cref="SignatureValidationOptions.Secret"/>，过渡期回退路径，C3 全量切换后退役）；</para>
/// <para>v2 头 = "v2;&lt;keyId&gt;;&lt;ts&gt;;&lt;mid&gt;;&lt;base64&gt;"，message = v2 \n METHOD \n path \n <b>canonicalQuery</b> \n keyId \n ts \n mid \n hex(sha256(body))
/// （拼装与 HMAC 计算均委托 <see cref="CanonicalRequestBuilder"/>，规范正文 gfx-doc/dev/signature/spec.md）——
/// keyId 进 message 防「签名挪用」降级攻击（把 A key 的签名配 B 的 keyId 重放必然验签失败），
/// query 按 RFC 3986 canonical（key 字典序 + percent-encoding），前置代理改写参数顺序/编码不再破签，
/// 密钥来自 App 级密钥环（<see cref="ISignatureKeyResolver"/>，按 (AppId, Kid) 定位）。</para>
/// <para>启用语义（C2 起）：<see cref="SignatureValidationOptions.IsEnabled"/> 降级为灰度总开关 ——
/// false = 完全旁路（现网默认，与 C1 行为一致）；true = 验签失败后按 App 级
/// <see cref="RiskEnforcementModeEnum"/> 分流：FailClosed 拒绝（默认安全），AuditOnly/FailOpen 记录后放行。</para>
/// <para>拒签信封码分级（HTTP 401 + camelCase 小写信封，与 C1 同形态）：60101 签名不符（含缺头/格式/时间窗/重放）、
/// 60102 keyId 未知、60103 key 禁用或租约到期。</para>
/// <para>双端测试向量：canting 仓 gfx-doc/dev/hmac-signature/changes/C2-sig-keyring-server/test-vectors.json。</para>
/// </summary>
public sealed class SignatureValidationMiddleware
{
    /// <summary>
    /// cola 客户端实际使用的签名头名（注意与 GlobalConst.SignatureHeader = "GameFrameX-Signature" 是两个不同的头）。
    /// C267 起与 Hub OpenAccess 单头统一，常量收敛到 <see cref="SignatureSpec.SignatureHeader"/>。
    /// </summary>
    public const string ColaSignatureHeader = SignatureSpec.SignatureHeader;

    /// <summary>
    /// 验签失败信封 code：签名不符 / 头格式非法 / 时间窗外 / 疑似重放（客户端 ApiMessage 据此 401 分流）。
    /// </summary>
    public const int SignatureRejectedCode = 60101;

    /// <summary>
    /// v2 keyId 在 (AppId, Kid) 密钥域内未找到的信封 code。
    /// </summary>
    public const int UnknownKeyIdRejectedCode = 60102;

    /// <summary>
    /// v2 key 存在但被禁用或租约未生效/已到期的信封 code。
    /// </summary>
    public const int KeyDisabledOrExpiredRejectedCode = 60103;

    private readonly RequestDelegate _next;

    public SignatureValidationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<SignatureValidationOptions> optionsAccessor, ICache cache, ISignatureKeyResolver keyResolver, ILogger<SignatureValidationMiddleware> logger)
    {
        var options = optionsAccessor.Value;
        if (!options.IsEnabled)
        {
            await _next(context);
            return;
        }

        // 第三方 provider 回调（PublicCallbackTenantResolutionMiddleware 已打标）没有我们的签名，豁免
        if (context.Items.ContainsKey(PublicCallbackRouteContext.ItemKey))
        {
            await _next(context);
            return;
        }

        // 启动期公开只读端点（Tier 0，C379）：远程配置 / 程序版本 / 资源版本等运营性公开数据端点打标豁免。
        // 豁免必须先于 App 风险模式分流——这类端点是登录前第一波请求，客户端本就不在启动期做签名，
        // 若进 FailClosed 分流会打断所有存量客户端启动流程。命中标记即完全跳过验签（含伪造签名头，不做
        // "带错签名反而拒"语义）；安全边界为 RequireTenantAppContext 上下文解析 + TLS，不改变其他中间件行为。
        // GetEndpoint() 为 null（无路由匹配）时安全降级不豁免，交由后续链路按缺签名头处理。
        if (context.GetEndpoint()?.Metadata.GetMetadata<PublicStartupEndpointAttribute>() != null)
        {
            await _next(context);
            return;
        }

        var rejection = await TryValidateAsync(context, options, cache, keyResolver);
        if (rejection == null)
        {
            await _next(context);
            return;
        }

        var appId = TryResolveAppId(context);
        // 风险模式分流器依赖租户实体仓储（TenantGameAppEntity，C259 起构造 fail-closed：无租户上下文禁止回落主库），
        // 必须按需解析而不能走 InvokeAsync 参数注入——否则旁路请求（如 /health 探针）在无租户上下文时也会触发仓储构造而 500。
        var riskEnforcementModeResolver = context.RequestServices.GetRequiredService<IGameAppRiskEnforcementModeResolver>();
        var mode = ResolveEnforcementMode(riskEnforcementModeResolver, appId);
        if (mode != RiskEnforcementModeEnum.FailClosed)
        {
            // AuditOnly / FailOpen：灰度观察期 —— 校验失败只记录不阻断，业务照常执行
            logger.LogWarning("传输签名校验未通过，按 App 风险模式放行：mode={Mode} appId={AppId} code={Code} reason={Reason} path={Path}",
                              mode, appId, rejection.Code, rejection.Message, context.Request.Path);
            await _next(context);
            return;
        }

        await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status401Unauthorized, rejection.Code, rejection.Message);
    }

    /// <summary>
    /// 执行验签；返回 null 表示通过，否则返回 (信封 code, 原因)。
    /// </summary>
    private static async Task<SignatureRejection?> TryValidateAsync(HttpContext context, SignatureValidationOptions options, ICache cache, ISignatureKeyResolver keyResolver)
    {
        var headerValue = context.Request.Headers[ColaSignatureHeader].FirstOrDefault();
        if (string.IsNullOrEmpty(headerValue))
        {
            return new SignatureRejection(SignatureRejectedCode, "缺少签名头");
        }

        var parts = headerValue.Split(';');
        string? keyId = null;
        long ts;
        ulong mid;
        if (parts.Length == 4 && parts[0] == "v1")
        {
            if (!long.TryParse(parts[1], out ts) || !ulong.TryParse(parts[2], out mid))
            {
                return new SignatureRejection(SignatureRejectedCode, "签名头格式非法");
            }
        }
        else if (parts.Length == 5 && parts[0] == "v2")
        {
            keyId = parts[1];
            if (string.IsNullOrEmpty(keyId) || !long.TryParse(parts[2], out ts) || !ulong.TryParse(parts[3], out mid))
            {
                return new SignatureRejection(SignatureRejectedCode, "签名头格式非法");
            }
        }
        else
        {
            return new SignatureRejection(SignatureRejectedCode, "签名头格式非法");
        }

        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (Math.Abs(now - ts) > options.TimestampToleranceSeconds)
        {
            return new SignatureRejection(SignatureRejectedCode, "签名时间戳超出容忍窗口");
        }

        // 字节级读取 body（EnableBuffering 后回卷，供下游 MVC 重新绑定）
        context.Request.EnableBuffering();
        byte[] bodyBytes;
        using (var buffer = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(buffer);
            bodyBytes = buffer.ToArray();
        }

        context.Request.Body.Position = 0;

        var signatureBase64 = parts[^1];
        byte[] provided;
        try
        {
            provided = Convert.FromBase64String(signatureBase64);
        }
        catch (FormatException)
        {
            return new SignatureRejection(SignatureRejectedCode, "签名 Base64 非法");
        }

        var method = context.Request.Method;
        var path = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        var query = context.Request.QueryString.Value?.TrimStart('?') ?? string.Empty;
        // C267：v2 消息串 query 段走 RFC 3986 canonical（key 字典序 + percent-encoding），
        // 不再依赖客户端原始顺序 —— 前置代理改写参数顺序/编码不再破签；v1 保持原始串（逃生通道）
        var canonicalQuery = CanonicalRequestBuilder.CanonicalQuery(context.Request.Query);

        if (keyId == null)
        {
            // v1 过渡期：C1 全局常量优先回退（客户端未注入密钥环时行为与 C1 完全一致），
            // 再遍历 App 域生效密钥环（无 keyId 寻址，只能逐把尝试）
            var message = BuildSignatureMessage(method, path, query, ts, mid, bodyBytes);
            if (VerifyHmac(Encoding.UTF8.GetBytes(options.Secret), message, provided))
            {
                return RegisterReplayGuard(cache, "sig:mid:v1:" + mid.ToString(), options, SignatureRejectedCode);
            }

            var appId = TryResolveAppId(context);
            if (appId.HasValue)
            {
                var activeKeys = await keyResolver.GetActiveKeysAsync(appId.Value);
                foreach (var key in activeKeys)
                {
                    if (VerifyHmac(Encoding.UTF8.GetBytes(key.Secret), message, provided))
                    {
                        return RegisterReplayGuard(cache, "sig:mid:v1:" + mid.ToString(), options, SignatureRejectedCode);
                    }
                }
            }

            return new SignatureRejection(SignatureRejectedCode, "签名不匹配");
        }

        // v2：按 (AppId, Kid) 精确择 key；keyId 进 message 串防签名挪用
        var resolvedAppId = TryResolveAppId(context);
        if (!resolvedAppId.HasValue)
        {
            return new SignatureRejection(UnknownKeyIdRejectedCode, "无法定位签名密钥域（缺少应用标识）");
        }

        var lookup = await keyResolver.ResolveAsync(resolvedAppId.Value, keyId);
        if (lookup.Result == SignatureKeyLookupResult.NotFound)
        {
            return new SignatureRejection(UnknownKeyIdRejectedCode, "签名 keyId 未知");
        }

        if (lookup.Result != SignatureKeyLookupResult.Active || lookup.Secret == null)
        {
            return new SignatureRejection(KeyDisabledOrExpiredRejectedCode, "签名密钥已禁用或租约未生效/已到期");
        }

        var v2Message = CanonicalRequestBuilder.BuildV2Message(method, path, canonicalQuery, keyId, ts, mid.ToString(), bodyBytes);
        if (!VerifyHmac(Encoding.UTF8.GetBytes(lookup.Secret), v2Message, provided))
        {
            return new SignatureRejection(SignatureRejectedCode, "签名不匹配");
        }

        return RegisterReplayGuard(cache, "sig:mid:v2:" + keyId + ":" + mid.ToString(), options, SignatureRejectedCode);
    }

    /// <summary>
    /// 验签通过后记账 mid：避免垃圾请求预占未来的 MessageId 空间。
    /// <para>replay key 按版本 + keyId 分域：v2 的 message 串含 keyId，同 mid 不同 key 的两条签名本就是不同消息，
    /// 不能让一把 key 的记账误伤另一把 key 的合法请求。</para>
    /// <para>ponytail: ContainsKey+Set 非原子（微秒级竞态窗口）且为进程内缓存，多实例部署不共享；
    /// 纵深防御由恒时 HMAC 比较兜底。升级路径：Redis SETNX + 分布式缓存。</para>
    /// </summary>
    private static SignatureRejection? RegisterReplayGuard(ICache cache, string replayKey, SignatureValidationOptions options, int rejectedCode)
    {
        if (cache.ContainsKey(replayKey))
        {
            return new SignatureRejection(rejectedCode, "消息 ID 已使用（疑似重放）");
        }

        cache.Set(replayKey, true, TimeSpan.FromSeconds(options.ReplayTtlSeconds));
        return null;
    }

    /// <summary>
    /// 解析 App 级风险生效模式；解析器异常（如租户库不可达）按默认安全 FailClosed 处理。
    /// </summary>
    private static RiskEnforcementModeEnum ResolveEnforcementMode(IGameAppRiskEnforcementModeResolver resolver, long? appId)
    {
        try
        {
            // appId 缺失传 0：与 resolver 内「应用不存在 → FailClosed」的默认安全口径一致
            return resolver.Resolve(appId ?? 0);
        }
        catch (Exception)
        {
            return RiskEnforcementModeEnum.FailClosed;
        }
    }

    /// <summary>
    /// 内联读取 GameFrameX-App-Id 头定位密钥域（C2 定稿：不动管线顺序）。
    /// <para>AppResolutionMiddleware 不前移的原因：它做 AppSecret 凭证校验 + 租户归属校验并写 AppId 上下文，
    /// 前移会让「应用凭证失败」先于「签名失败」返回且信封格式不同（PascalCase Code/Message vs 客户端依赖的
    /// camelCase code/message），改变 C1 已上线的拒签契约；而验签本身即持密钥证明，无需依赖 AppSecret 校验结果，
    /// 伪造 AppId 头只会导致密钥域错位、验签必然失败，无放大面。</para>
    /// </summary>
    private static long? TryResolveAppId(HttpContext context)
    {
        var headerValue = context.Request.Headers[GlobalConst.AppIdHeader].FirstOrDefault();
        if (long.TryParse(headerValue, out var appId) && appId > 0)
        {
            return appId;
        }

        return null;
    }

    /// <summary>
    /// v1 message（C1 历史规格，过渡逃生通道保留）：query 保持客户端原始串
    /// （Uri.Query.TrimStart('?')），与客户端 NetLib.ApiServerHttpRequest 逐字节对齐。
    /// body 摘要走 <see cref="CanonicalRequestBuilder.BodyHashHex"/>（C267 消重 SHA-256 计算）。
    /// </summary>
    internal static string BuildSignatureMessage(string method, string path, string query, long ts, ulong mid, byte[] body)
    {
        // ponytail: v1 按原始串对齐（C1 客户端行为），若前置代理改写 URL 编码或参数顺序两端会不一致；
        // v2 已升级 canonical（见 TryValidateAsync），此路径 C3 全量切换后随 v1 一起退役
        return $"v1\n{method}\n{path}\n{query}\n{ts}\n{mid}\n{CanonicalRequestBuilder.BodyHashHex(body)}";
    }

    /// <summary>
    /// HMAC-SHA256 重算 + 恒时比较（先比长度再 FixedTimeEquals，避免长度差异短路泄露计时信号）。
    /// <para>计算走 <see cref="CanonicalRequestBuilder.HmacSha256(byte[], string)"/> —— 全仓唯一 HMAC 实现（C267）。</para>
    /// </summary>
    private static bool VerifyHmac(byte[] secret, string message, byte[] provided)
    {
        var actual = CanonicalRequestBuilder.HmacSha256(secret, message);
        return provided.Length == actual.Length && CryptographicOperations.FixedTimeEquals(provided, actual);
    }

    /// <summary>
    /// 验签拒绝结论（信封 code + 原因）；null 表示验签通过。
    /// </summary>
    private sealed record SignatureRejection(int Code, string Message);
}