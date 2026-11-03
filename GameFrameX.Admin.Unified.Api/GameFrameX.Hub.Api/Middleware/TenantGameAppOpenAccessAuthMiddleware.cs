using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GameFrameX.Core.Const;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SignatureAuth;
using GameFrameX.Entity.Tenant;
using GameFrameX.Entity.Tenant.Game.App;

namespace GameFrameX.Hub.Api.Middleware;

/// <summary>
/// 主库开放访问凭证鉴权中间件。
/// <para>机器通道独立于 JWT：校验凭证生命周期、HMAC v2、时间窗、nonce 与接口范围，
/// 然后绑定主库操作人供审计使用。</para>
/// </summary>
public class TenantGameAppOpenAccessAuthMiddleware
{
    private readonly RequestDelegate _next;

    private const string NonceCachePrefix = "gameAppOpenAccess:nonce:";

    /// <summary>
    /// 时间戳允许偏差（±秒），与 C181 Signature 默认 ±5min 一致。
    /// </summary>
    private const int AllowedDriftSeconds = 300;

    public TenantGameAppOpenAccessAuthMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// 中间件入口。
    /// </summary>
    /// <param name="context">HTTP 上下文。</param>
    /// <param name="cacheService">缓存服务（nonce 去重）。</param>
    public async Task InvokeAsync(HttpContext context, SystemCacheService cacheService)
    {
        var credentialRepo = context.RequestServices.GetRequiredService<Repository<TenantGameAppOpenAccessEntity>>();
        var userRepo = context.RequestServices.GetRequiredService<Repository<TenantUserEntity>>();

        // 2. 鉴权头（C267 起单头 Signature: v2;{accessKey};{ts};{nonce};{base64sig}，与传输签名规范统一）
        var signatureHeader = context.Request.Headers[SignatureSpec.SignatureHeader].FirstOrDefault();
        if (!TryParseSignatureHeader(signatureHeader, out var accessKey, out var timestamp, out var nonce, out var providedSignature))
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "缺少或非法的签名头（Signature: v2;{accessKey};{ts};{nonce};{sig}）");
            return;
        }

        // 3. 时间窗（防重放第一道）
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (timestamp < now - AllowedDriftSeconds || timestamp > now + AllowedDriftSeconds)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "timestamp 超出允许偏差范围");
            return;
        }

        // 主库按访问标识查凭证。
        var credential = await credentialRepo.AsQueryable().FirstAsync(c => c.AccessKey == accessKey);
        if (credential == null)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "凭证无效");
            return;
        }

        // C372（GFX-838）：开放访问凭证读取边界解密（C371 起库内密文；无前缀明文直通兼容存量行）。
        // 实体为本次查询的请求本地实例，改写无副作用；进程级密文缓存兜底，避免每请求 PBKDF2 派生。
        credential.AccessSecret = TenantSecretCrypto.DecryptCached(credential.AccessSecret) ?? string.Empty;

        // 5. 生命周期校验
        if (credential.IsEnabled != true)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "凭证已禁用");
            return;
        }

        if (credential.RevokedTime != null)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "凭证已撤销");
            return;
        }

        if (credential.ExpiresTime.HasValue && credential.ExpiresTime.Value <= now)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "凭证已过期");
            return;
        }

        // 6. HMAC 签名校验（C267：复用 CanonicalRequestBuilder.BuildV2Message —— v2 message 单一拼装，
        //    body 入签 + canonical query；比较改字节级 FixedTimeEquals，替代原字符串序数比较）
        context.Request.EnableBuffering();
        byte[] bodyBytes;
        using (var buffer = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(buffer);
            bodyBytes = buffer.ToArray();
        }

        context.Request.Body.Position = 0;

        var method = context.Request.Method;
        var path = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        var canonicalQuery = CanonicalRequestBuilder.CanonicalQuery(context.Request.Query);
        if (!VerifySignature(credential, method, path, canonicalQuery, timestamp, nonce, bodyBytes, providedSignature))
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "签名无效");
            return;
        }

        // 7. nonce 去重（缓存窗口为偏差范围的 2 倍）
        var nonceKey = NonceCachePrefix + accessKey + "|" + nonce;
        if (cacheService.ExistKey(nonceKey))
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "重复的请求");
            return;
        }

        cacheService.Set(nonceKey, string.Empty, TimeSpan.FromSeconds(AllowedDriftSeconds * 2));

        // 8. 接口范围（Scopes）校验 —— 越界 403
        if (!IsRouteInScopes(context.Request.Path, credential.ScopesJson))
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status403Forbidden, "接口范围越权");
            return;
        }

        // 10. 查绑定用户（审计操作人）
        var user = await userRepo.AsQueryable().FirstAsync(u => u.Id == credential.BindUserId);
        if (user == null)
        {
            await MiddlewareResponseWriter.WriteRejectedAsync(context, StatusCodes.Status401Unauthorized, "凭证绑定用户不存在");
            return;
        }

        // 11. 构造 ClaimsPrincipal，供审计列 AOP 与下游授权使用
        var identity = new ClaimsIdentity("TenantGameAppOpenAccess");
        identity.AddClaim(new Claim(ClaimConst.UserId, credential.BindUserId + ""));
        identity.AddClaim(new Claim(ClaimConst.Account, user.Account ?? string.Empty));
        identity.AddClaim(new Claim(ClaimConst.RealName, user.RealName ?? string.Empty));
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }

    /// <summary>
    /// 解析单头 <c>Signature: v2;{accessKey};{ts};{nonce};{base64sig}</c>（C267 格式）。
    /// </summary>
    /// <param name="headerValue">签名头原始值。</param>
    /// <param name="accessKey">输出：凭证 AccessKey（keyId 段位）。</param>
    /// <param name="timestamp">输出：Unix 秒时间戳。</param>
    /// <param name="nonce">输出：唯一随机数（mid 段位）。</param>
    /// <param name="providedSignature">输出：Base64 解码后的签名字节。</param>
    /// <returns>五段齐全且格式合法返回 true。</returns>
    internal static bool TryParseSignatureHeader(string? headerValue, out string accessKey, out long timestamp, out string nonce, out byte[] providedSignature)
    {
        accessKey = string.Empty;
        timestamp = 0;
        nonce = string.Empty;
        providedSignature = Array.Empty<byte>();
        if (string.IsNullOrEmpty(headerValue))
        {
            return false;
        }

        var parts = headerValue.Split(';');
        if (parts.Length != 5 || parts[0] != SignatureSpec.ProtocolPrefix
                              || string.IsNullOrEmpty(parts[1]) || !long.TryParse(parts[2], out timestamp)
                              || string.IsNullOrEmpty(parts[3]))
        {
            return false;
        }

        try
        {
            providedSignature = Convert.FromBase64String(parts[4]);
        }
        catch (FormatException)
        {
            return false;
        }

        accessKey = parts[1];
        nonce = parts[3];
        return true;
    }

    /// <summary>
    /// v2 消息串验签（C267）：accessKey 充当 keyId、nonce 充当 mid 段位，body 入签；
    /// 消息串拼装与 HMAC 计算均委托 <see cref="CanonicalRequestBuilder"/>（全仓唯一实现）；
    /// 比较为字节级恒时（先比长度再 FixedTimeEquals）。
    /// </summary>
    internal static bool VerifySignature(TenantGameAppOpenAccessEntity credential, string method, string path, string canonicalQuery, long timestamp, string nonce, byte[] body, byte[] providedSignature)
    {
        var message = CanonicalRequestBuilder.BuildV2Message(method, path, canonicalQuery, credential.AccessKey, timestamp, nonce, body);
        var expected = CanonicalRequestBuilder.HmacSha256(Encoding.UTF8.GetBytes(credential.AccessSecret), message);
        return providedSignature.Length == expected.Length && CryptographicOperations.FixedTimeEquals(providedSignature, expected);
    }

    /// <summary>
    /// 校验请求路由是否在凭证 ScopesJson 允许范围内。
    /// <para>scope 值为路由前缀（不含 /api/），例如 "PackageBuildReport" 命中 <c>/api/PackageBuildReport</c>。</para>
    /// </summary>
    /// <param name="path">请求路径。</param>
    /// <param name="scopesJson">凭证接口范围 JSON 数组字符串。</param>
    /// <returns>命中返回 true。</returns>
    internal static bool IsRouteInScopes(PathString path, string scopesJson)
    {
        if (string.IsNullOrWhiteSpace(scopesJson))
        {
            return false;
        }

        List<string> scopes;
        try
        {
            scopes = JsonSerializer.Deserialize<List<string>>(scopesJson) ?? new List<string>();
        }
        catch (JsonException)
        {
            return false;
        }

        return scopes.Any(s => !string.IsNullOrWhiteSpace(s)
                               && path.StartsWithSegments("/api/" + s, StringComparison.OrdinalIgnoreCase));
    }
}