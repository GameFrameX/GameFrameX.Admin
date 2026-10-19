using System.Text;
using GameFrameX.Core.SignatureAuth;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Hub.Api.Middleware;

namespace GameFrameX.Core.Tests.HubApi;

/// <summary>
/// TenantGameAppOpenAccessAuthMiddleware 签名段单元测试（C267-C：规范 B → 规范 A 迁移）。
/// 覆盖：v2 单头解析（合法/旧格式拒绝/坏 Base64）、canonical 消息串验签通过、
/// body 篡改拒（body 入签证据）、按原始乱序 query 拼串签名拒（canonical 强制证据）。
/// </summary>
public sealed class TenantGameAppOpenAccessAuthMiddlewareTests
{
    private const string AccessKey = "ak-hub-vector-001";
    private const string AccessSecret = "GFX-C267-HUB-OPENACCESS-SECRET";
    private const long Ts = 1755244800;
    private const string Nonce = "n-9f3c1b";
    private const string Body = "{\"package\":\"pkg-1001\",\"status\":\"ok\"}";

    private static TenantGameAppOpenAccessEntity Credential()
    {
        return new TenantGameAppOpenAccessEntity
        {
            AccessKey = AccessKey,
            AccessSecret = AccessSecret,
            IsEnabled = true,
        };
    }

    /// <summary>模拟客户端 SDK 按 C267 spec（canonical query + body 入签）计算 v2 签名。</summary>
    private static byte[] ClientSign(string method, string path, string canonicalQuery, byte[] body)
    {
        var message = CanonicalRequestBuilder.BuildV2Message(method, path, canonicalQuery, AccessKey, Ts, Nonce, body);
        return CanonicalRequestBuilder.HmacSha256(Encoding.UTF8.GetBytes(AccessSecret), message);
    }

    [Fact]
    public void TryParseSignatureHeader_ValidV2Header_ParsesAllSegments()
    {
        var signatureBase64 = ClientSign("POST", "/api/PackageBuildReport", "appId=9&channel=ios", Encoding.UTF8.GetBytes(Body));
        var header = "v2;" + AccessKey + ";" + Ts + ";" + Nonce + ";" + Convert.ToBase64String(signatureBase64);

        var parsed = TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader(header, out var accessKey, out var timestamp, out var nonce, out var provided);

        Assert.True(parsed);
        Assert.Equal(AccessKey, accessKey);
        Assert.Equal(Ts, timestamp);
        Assert.Equal(Nonce, nonce);
        Assert.Equal(signatureBase64, provided);
    }

    [Fact]
    public void TryParseSignatureHeader_LegacyOrMalformed_Rejected()
    {
        // C267 前的 4 个独立头（accessKey/timestamp/nonce/sign）不再支持：缺 Signature 单头直接解析失败；
        // 段数不足 / 非 v2 前缀 / 坏 Base64 同样拒绝
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader(null, out _, out _, out _, out _));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader(string.Empty, out _, out _, out _, out _));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader("v1;" + AccessKey + ";" + Ts + ";" + Nonce + ";AAAA", out _, out _, out _, out _));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader("v2;" + AccessKey + ";" + Ts + ";" + Nonce, out _, out _, out _, out _));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader("v2;" + AccessKey + ";not-a-ts;" + Nonce + ";AAAA", out _, out _, out _, out _));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader("v2;;" + Ts + ";" + Nonce + ";AAAA", out _, out _, out _, out _));
        Assert.False(TenantGameAppOpenAccessAuthMiddleware.TryParseSignatureHeader("v2;" + AccessKey + ";" + Ts + ";" + Nonce + ";***not-base64***", out _, out _, out _, out _));
    }

    [Fact]
    public void VerifySignature_ClientCanonicalMessage_Passes()
    {
        var body = Encoding.UTF8.GetBytes(Body);
        var provided = ClientSign("POST", "/api/PackageBuildReport", "appId=9&channel=ios", body);

        var verified = TenantGameAppOpenAccessAuthMiddleware.VerifySignature(
            Credential(), "POST", "/api/PackageBuildReport", "appId=9&channel=ios", Ts, Nonce, body, provided);

        Assert.True(verified);
    }

    [Fact]
    public void VerifySignature_TamperedBody_Rejected()
    {
        // body 入签（C267 前规范 B 不把 body 进签 —— 这是本迁移的关键收紧点）：篡改 body 后原签名必失效
        var body = Encoding.UTF8.GetBytes(Body);
        var provided = ClientSign("POST", "/api/PackageBuildReport", "appId=9", body);

        var verified = TenantGameAppOpenAccessAuthMiddleware.VerifySignature(
            Credential(), "POST", "/api/PackageBuildReport", "appId=9", Ts, Nonce,
            Encoding.UTF8.GetBytes("{\"package\":\"pkg-1001\",\"status\":\"tampered\"}"), provided);

        Assert.False(verified);
    }

    [Fact]
    public void VerifySignature_RawOrderSignedClient_Rejected()
    {
        // 客户端按「原始乱序 query 串」拼消息签名（未按 canonical 排序）→ 拒绝，
        // 证明 Hub 侧同样强制 canonical query（与 SignatureValidationMiddleware 同一规范）
        var body = Encoding.UTF8.GetBytes(Body);
        var rawOrderMessage = "v2\nPOST\n/api/PackageBuildReport\nchannel=ios&appId=9\n" + AccessKey + "\n" + Ts + "\n" + Nonce + "\n"
                              + CanonicalRequestBuilder.BodyHashHex(body);
        var provided = CanonicalRequestBuilder.HmacSha256(Encoding.UTF8.GetBytes(AccessSecret), rawOrderMessage);

        var verified = TenantGameAppOpenAccessAuthMiddleware.VerifySignature(
            Credential(), "POST", "/api/PackageBuildReport", "appId=9&channel=ios", Ts, Nonce, body, provided);

        Assert.False(verified);
    }
}