using System.IO.Compression;
using System.Security.Cryptography;

namespace GameFrameX.Core.Middleware;

/// <summary>
/// 加密信封中间件（change C5 server-envelope-middleware，对端 = 客户端 NetLib.EnvelopeCrypto）。
/// <para>请求侧：X-Crypto: 1 头 → 解析信封 kid → 按 (AppId, Kid) 查会话密钥（<see cref="ISessionKeyResolver"/>）→
/// HKDF c2s 派生 → AAD 解密 → 明文重写 Request.Body 进管线；无 X-Crypto 头原样放行（灰度窗口双形态）。</para>
/// <para>响应侧：仅当请求是「信封且解密成功」时，响应 body 加密回信封（s2c key，方向与解密相反）；
/// 响应信封 AAD 的 path 用<b>请求 path</b>、mid 回显<b>请求信封 mid</b>、ts/iv 现生成、ct 前加压缩标记 0x00（raw，
/// 服务端响应不压缩）——客户端按「其请求 path + 信封自报 mid/ts」构 AAD 解密，双端契约，错一个字段客户端解不开。</para>
/// <para>拒收信封码（HTTP 400 + camelCase 小写信封，与 SignatureValidation 401 信封同形态）：
/// 60104 kid 未知 / 60105 key 禁用或租约到期 / 60106 信封解密失败（格式/AAD/tag）。</para>
/// <para>挂载点在 <see cref="SignatureValidationMiddleware"/> 之后：签名覆盖线上信封字节（sha256(envelope)），
/// 验签先于解密。双端测试向量：canting 仓 gfx-doc/dev/crypto-envelope/archived/C4-client-crypto-envelope/test-vectors.json。</para>
/// </summary>
public sealed class CryptoEnvelopeMiddleware
{
    /// <summary>
    /// 客户端加密声明头（仅启用会话密钥时随请求发送）。
    /// </summary>
    public const string CryptoHeader = "X-Crypto";

    /// <summary>
    /// 信封 kid 在 (AppId, Kid) 密钥域内未找到的信封 code。
    /// </summary>
    public const int UnknownKidRejectedCode = 60104;

    /// <summary>
    /// 会话密钥存在但被禁用或租约未生效/已到期的信封 code。
    /// </summary>
    public const int KeyDisabledOrExpiredRejectedCode = 60105;

    /// <summary>
    /// 信封解密失败（格式非法 / AAD 或 tag 失配 / 压缩标记非法）的信封 code。
    /// </summary>
    public const int EnvelopeDecryptRejectedCode = 60106;

    /// <summary>压缩标记：payload 未压缩（服务端响应恒用 raw）。</summary>
    public const byte FlagRaw = 0x00;

    /// <summary>压缩标记：payload 为 raw deflate（RFC 1951，无 zlib/gzip 头）。</summary>
    public const byte FlagDeflate = 0x01;

    /// <summary>GCM nonce 长度（字节）。</summary>
    public const int IvBytes = 12;

    /// <summary>GCM 认证标签长度（字节），附在 ct 尾。</summary>
    public const int TagBytes = 16;

    /// <summary>AES-256 会话密钥长度（字节）。</summary>
    public const int KeyBytes = 32;

    /// <summary>
    /// 解密成功请求的上下文标记（值 = <see cref="EnvelopeRequestState"/>），响应侧据此决定是否加密回信封。
    /// </summary>
    internal const string EnvelopeRequestStateItemKey = "CryptoEnvelope.RequestState";

    /// <summary>HKDF 双向分钥 info：client → server（请求解密方向）。</summary>
    internal const string C2sInfo = "c2s";

    /// <summary>HKDF 双向分钥 info：server → client（响应加密方向）。</summary>
    internal const string S2cInfo = "s2c";

    private readonly RequestDelegate _next;

    public CryptoEnvelopeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IOptions<CryptoEnvelopeOptions> optionsAccessor, ISessionKeyResolver keyResolver, ILogger<CryptoEnvelopeMiddleware> logger)
    {
        var options = optionsAccessor.Value;
        if (!options.IsEnabled)
        {
            await _next(context);
            return;
        }

        // 测试租户旁路：命中即按明文处理（Swagger/联调可用），请求不解密、响应不加密
        if (IsTestTenant(context, options))
        {
            await _next(context);
            return;
        }

        // 灰度窗口双形态：未启用加密的客户端不发 X-Crypto 头，原样放行
        if (context.Request.Headers[CryptoHeader].FirstOrDefault() != "1")
        {
            await _next(context);
            return;
        }

        byte[] envelopeBytes;
        using (var buffer = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(buffer);
            envelopeBytes = buffer.ToArray();
        }

        if (!TryParseEnvelope(envelopeBytes, out var fields))
        {
            await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status400BadRequest, EnvelopeDecryptRejectedCode, "信封格式非法或字段不完整");
            return;
        }

        var appId = TryResolveAppId(context);
        if (!appId.HasValue)
        {
            await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status400BadRequest, UnknownKidRejectedCode, "无法定位会话密钥域（缺少应用标识）");
            return;
        }

        var lookup = await keyResolver.ResolveAsync(appId.Value, fields.Kid);
        if (lookup.Result == SessionKeyLookupResult.NotFound)
        {
            await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status400BadRequest, UnknownKidRejectedCode, "信封 kid 未知");
            return;
        }

        if (lookup.Result != SessionKeyLookupResult.Active || lookup.Secret == null)
        {
            await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status400BadRequest, KeyDisabledOrExpiredRejectedCode, "会话密钥已禁用或租约未生效/已到期");
            return;
        }

        byte[] sessionKey;
        try
        {
            sessionKey = Convert.FromHexString(lookup.Secret);
        }
        catch (FormatException)
        {
            sessionKey = Array.Empty<byte>();
        }

        if (sessionKey.Length != KeyBytes)
        {
            // 存储侧密钥材料损坏（非 hex / 非 32B）：数据问题而非客户端问题，按解密失败拒收并留痕
            logger.LogError("会话密钥材料非法（长度 {Length}），kid={Kid} appId={AppId}", sessionKey.Length, fields.Kid, appId.Value);
            await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status400BadRequest, EnvelopeDecryptRejectedCode, "会话密钥材料非法");
            return;
        }

        var requestPath = context.Request.Path.Value?.TrimEnd('/') ?? string.Empty;
        if (!TryOpenEnvelope(DeriveDirectionalKey(sessionKey, C2sInfo), fields, requestPath, out var payload))
        {
            await MiddlewareResponseWriter.WriteEnvelopeRejectedAsync(context, StatusCodes.Status400BadRequest, EnvelopeDecryptRejectedCode, "信封解密失败（AAD 或密文校验未通过）");
            return;
        }

        // 明文重写 body（MemoryStream 可重读，供下游 MVC 绑定）；解密成功后响应侧要加密回信封
        context.Request.Body = new MemoryStream(payload, false);
        context.Request.ContentLength = payload.Length;
        context.Items[EnvelopeRequestStateItemKey] = new EnvelopeRequestState(fields.Kid, fields.Mid, sessionKey, requestPath);

        // 响应缓冲：_next 写入内存流，结束后按「是否解密请求」决定明文回灌还是加密回信封
        var originalBody = context.Response.Body;
        using (var responseBuffer = new MemoryStream())
        {
            context.Response.Body = responseBuffer;
            try
            {
                await _next(context);
            }
            finally
            {
                context.Response.Body = originalBody;
            }

            responseBuffer.Position = 0;
            if (context.Items[EnvelopeRequestStateItemKey] is EnvelopeRequestState state && responseBuffer.Length > 0)
            {
                var envelope = await EncryptResponseAsync(state, responseBuffer);
                if (!context.Response.HasStarted)
                {
                    context.Response.ContentLength = envelope.Length;
                }

                await originalBody.WriteAsync(envelope);
                return;
            }

            await responseBuffer.CopyToAsync(originalBody);
        }
    }

    /// <summary>
    /// 测试租户判定：TenantResolutionMiddleware 已写入的 TenantId 命中 <see cref="CryptoEnvelopeOptions.TestTenantIds"/>。
    /// </summary>
    private static bool IsTestTenant(HttpContext context, CryptoEnvelopeOptions options)
    {
        return options.TestTenantIds.Count > 0
               && context.Items.TryGetValue(GlobalConst.TenantIdContextKey, out var value)
               && value is long tenantId
               && options.TestTenantIds.Contains(tenantId);
    }

    /// <summary>
    /// 响应加密：s2c 派生 + raw 标记 + 新 ts/iv，AAD 用请求 path、mid 回显请求信封 mid（双端契约）。
    /// </summary>
    private static async Task<byte[]> EncryptResponseAsync(EnvelopeRequestState state, MemoryStream buffered)
    {
        byte[] plain;
        using (var ms = new MemoryStream())
        {
            await buffered.CopyToAsync(ms);
            plain = ms.ToArray();
        }

        var ts = TimerHelper.UnixTimeSeconds();
        var iv = RandomNumberGenerator.GetBytes(IvBytes);
        return EncodeEnvelope(DeriveDirectionalKey(state.SessionKey, S2cInfo), state.Kid, state.Mid, ts, state.RequestPath, iv, plain);
    }

    /// <summary>
    /// 内联读取 GameFrameX-App-Id 头定位密钥域（与 SignatureValidationMiddleware 同方案：不动管线顺序）。
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
    /// HKDF-SHA256（RFC 5869）：net10 内置实现，向量锚定与客户端手写实现一致（RFC 5869 A.1 + c2s/s2c 派生组）。
    /// salt 传 null = RFC 缺省（hashLen 零字节代入）。
    /// </summary>
    internal static byte[] HkdfSha256(byte[] ikm, byte[]? salt, byte[] info, int outputLength)
    {
        return HKDF.DeriveKey(HashAlgorithmName.SHA256, ikm, outputLength, salt, info);
    }

    /// <summary>
    /// 会话密钥双向分钥：sendKey = HKDF(sessionKey, info="c2s")，recvKey 同理 "s2c"（防 GCM nonce 跨向相撞）。
    /// </summary>
    internal static byte[] DeriveDirectionalKey(byte[] sessionKey, string info)
    {
        return HkdfSha256(sessionKey, null, Encoding.UTF8.GetBytes(info), KeyBytes);
    }

    /// <summary>
    /// 信封 AAD 构造：UTF8("env1\n" + path + "\n" + ts + "\n" + mid)，与客户端 EnvelopeCrypto.BuildAad 逐字节对齐。
    /// path 与签名规格同源（Uri.AbsolutePath.TrimEnd('/')），ts/mid 取信封字段值。
    /// </summary>
    internal static byte[] BuildAad(string path, long ts, ulong mid)
    {
        return Encoding.UTF8.GetBytes("env1\n" + path + "\n" + ts.ToString() + "\n" + mid.ToString());
    }

    /// <summary>
    /// kid 合法性：长度 1-64，字符限 [A-Za-z0-9._-]（信封受控 JSON 契约，与客户端 IsValidKeyId 同白名单）。
    /// </summary>
    internal static bool IsValidKid(string kid)
    {
        if (kid.Length < 1 || kid.Length > 64)
        {
            return false;
        }

        for (var i = 0; i < kid.Length; i++)
        {
            var c = kid[i];
            var ok = (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '.' || c == '_' || c == '-';
            if (!ok)
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 解析信封明文字段（不解密）：v 必须 ==1、kid 过白名单、iv 12B、ct &gt; 16B、base64 合法。
    /// <para>受控 JSON 契约：信封字符串值只有白名单 kid 与 base64，整串出现转义序列（'\'）一律拒收；
    /// 字段序不限、未知键容忍（前向兼容），比客户端手写扫描器宽、比通用解析器严。</para>
    /// </summary>
    internal static bool TryParseEnvelope(byte[] envelope, out EnvelopeFields fields)
    {
        fields = null!;
        string text;
        try
        {
            text = Encoding.UTF8.GetString(envelope);
        }
        catch (ArgumentOutOfRangeException)
        {
            return false;
        }

        if (text.IndexOf('\\') >= 0)
        {
            return false;
        }

        try
        {
            using (var document = JsonDocument.Parse(text))
            {
                var root = document.RootElement;
                if (root.ValueKind != JsonValueKind.Object
                    || !root.TryGetProperty("v", out var v) || v.ValueKind != JsonValueKind.Number || !v.TryGetInt32(out var version) || version != 1
                    || !root.TryGetProperty("kid", out var kidElement) || kidElement.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                var kid = kidElement.GetString()!;
                if (!IsValidKid(kid)
                    || !root.TryGetProperty("mid", out var midElement) || midElement.ValueKind != JsonValueKind.Number || !midElement.TryGetInt64(out var midSigned) || midSigned < 0
                    || !root.TryGetProperty("ts", out var tsElement) || tsElement.ValueKind != JsonValueKind.Number || !tsElement.TryGetInt64(out var ts)
                    || !root.TryGetProperty("iv", out var ivElement) || ivElement.ValueKind != JsonValueKind.String
                    || !root.TryGetProperty("ct", out var ctElement) || ctElement.ValueKind != JsonValueKind.String)
                {
                    return false;
                }

                byte[] iv;
                byte[] ct;
                try
                {
                    iv = Convert.FromBase64String(ivElement.GetString()!);
                    ct = Convert.FromBase64String(ctElement.GetString()!);
                }
                catch (FormatException)
                {
                    return false;
                }

                if (iv.Length != IvBytes || ct.Length <= TagBytes)
                {
                    return false;
                }

                fields = new EnvelopeFields(kid, (ulong)midSigned, ts, iv, ct);
                return true;
            }
        }
        catch (JsonException)
        {
            return false;
        }
    }

    /// <summary>
    /// 解密信封取业务字节：AAD 绑 path/ts/mid → GCM 开封 → 按压缩标记解压（0x00 直取 / 0x01 raw inflate）。
    /// flag 非法或 GCM 校验失败返回 false（调用方按 60106 拒收）。
    /// </summary>
    internal static bool TryOpenEnvelope(byte[] directionalKey, EnvelopeFields fields, string path, out byte[] payload)
    {
        payload = Array.Empty<byte>();
        var bodyLength = fields.Ct.Length - TagBytes;
        var plain = new byte[bodyLength];
        try
        {
            using (var aes = new AesGcm(directionalKey, TagBytes))
            {
                aes.Decrypt(fields.Iv, fields.Ct.AsSpan(0, bodyLength), fields.Ct.AsSpan(bodyLength), plain, BuildAad(path, fields.Ts, fields.Mid));
            }
        }
        catch (CryptographicException)
        {
            return false;
        }

        if (plain.Length == 0)
        {
            return false;
        }

        if (plain[0] == FlagRaw)
        {
            payload = plain[1..];
            return true;
        }

        if (plain[0] == FlagDeflate)
        {
            try
            {
                payload = InflateRaw(plain[1..]);
                return true;
            }
            catch (Exception)
            {
                payload = Array.Empty<byte>();
                return false;
            }
        }

        return false;
    }

    /// <summary>
    /// 生成信封 JSON 字节（响应侧）：字段序固定 v,kid,mid,ts,iv,ct、无空白（与客户端 EncodeEnvelope 同构输出）；
    /// ct 明文 = 0x00 raw 标记 + payload，整体 AES-GCM 加密（tag 16B 附尾）。
    /// </summary>
    internal static byte[] EncodeEnvelope(byte[] directionalKey, string kid, ulong mid, long ts, string path, byte[] iv, byte[] payload)
    {
        var plain = new byte[1 + payload.Length];
        plain[0] = FlagRaw;
        Buffer.BlockCopy(payload, 0, plain, 1, payload.Length);
        var combined = EncryptWithTag(directionalKey, iv, plain, BuildAad(path, ts, mid));
        var builder = new StringBuilder(96 + kid.Length + 32 + Convert.ToBase64String(combined).Length);
        builder.Append("{\"v\":1,\"kid\":\"").Append(kid)
               .Append("\",\"mid\":").Append(mid.ToString())
               .Append(",\"ts\":").Append(ts.ToString())
               .Append(",\"iv\":\"").Append(Convert.ToBase64String(iv))
               .Append("\",\"ct\":\"").Append(Convert.ToBase64String(combined))
               .Append("\"}");
        return Encoding.UTF8.GetBytes(builder.ToString());
    }

    /// <summary>
    /// AES-256-GCM 加密，返回 ct‖tag（信封 ct 字段的 base64 前身）。
    /// </summary>
    private static byte[] EncryptWithTag(byte[] key, byte[] iv, byte[] plain, byte[] aad)
    {
        var ciphertext = new byte[plain.Length];
        var tag = new byte[TagBytes];
        using (var aes = new AesGcm(key, TagBytes))
        {
            aes.Encrypt(iv, plain, ciphertext, tag, aad);
        }

        var combined = new byte[plain.Length + TagBytes];
        Buffer.BlockCopy(ciphertext, 0, combined, 0, ciphertext.Length);
        Buffer.BlockCopy(tag, 0, combined, ciphertext.Length, TagBytes);
        return combined;
    }

    /// <summary>raw deflate 解压（RFC 1951，与客户端 EnvelopeCrypto.InflateRaw / python zlib -15 同原语）。</summary>
    private static byte[] InflateRaw(byte[] data)
    {
        using (var input = new MemoryStream(data, false))
        {
            using (var inflate = new DeflateStream(input, CompressionMode.Decompress))
            {
                using (var output = new MemoryStream())
                {
                    inflate.CopyTo(output);
                    return output.ToArray();
                }
            }
        }
    }

    /// <summary>
    /// 信封明文字段（解析产物，v==1 已保证、kid 已过白名单、iv 12B、ct &gt; 16B 已保证）。
    /// </summary>
    internal sealed record EnvelopeFields(string Kid, ulong Mid, long Ts, byte[] Iv, byte[] Ct);

    /// <summary>
    /// 解密成功请求的响应侧状态：kid/mid 进响应信封（mid 回显）、sessionKey 供 s2c 派生、RequestPath 进响应 AAD。
    /// </summary>
    private sealed record EnvelopeRequestState(string Kid, ulong Mid, byte[] SessionKey, string RequestPath);
}