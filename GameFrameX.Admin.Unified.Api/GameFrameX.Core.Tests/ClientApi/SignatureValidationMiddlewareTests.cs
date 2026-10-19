using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.SignatureAuth;
using GameFrameX.Client.Api.GameAssetPackageVersion;
using GameFrameX.Core.Cache;
using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// SignatureValidationMiddleware 单元测试。
/// v1 向量取自 C1-hmac-foundation-migration/test-vectors.json（固定历史 ts，用大时间窗跑通验签路径）；
/// v2 向量取自 C2-sig-keyring-server/test-vectors.json（W1-W6 六条双密钥组，期望值逐字抄录，禁止本地重算——
/// W1/W2 同参数双 key 是双密钥共存锚）。
/// <para>密钥环用 <see cref="FakeSignatureKeyResolver"/> 内存桩：NotBeforeTime=0 / ExpireTime=null（永久生效）
/// 不受真实时钟影响；到期/未生效用例用向量历史时刻（1755244800，必然早于当前时间）构造。</para>
/// </summary>
public sealed class SignatureValidationMiddlewareTests
{
    private const string TestKey = "GFX-C1-VECTOR-TEST-KEY";

    private const long V1Ts = 1755244800;
    private const ulong V1Mid = 1;
    private const string V1Body = "{\"hello\":\"world\"}";
    private const string V1Signature = "xvcjY+eEGdZu+qGaqDqSrjTXYiSpqkmftxV1lyU1iuU=";

    // ===== C2 v2 双密钥向量常量（抄录自 C2 test-vectors.json，事实源唯一） =====
    private const string KeyAId = "sk-vector-a-001";
    private const string KeyA = "GFX-C2-VECTOR-TEST-KEY-A";
    private const string KeyBId = "sk-vector-b-002";
    private const string KeyB = "GFX-C2-VECTOR-TEST-KEY-B";
    private const long TestAppId = 1001;
    private const string W1Signature = "gpRxwTuAYGBKjsCeTYqx7IWhTjp07AsjUL9d5Wz8RfA=";
    private const string W2Signature = "Su1Np4XQN4CisUiAdf1JbIxZBzgSsZJH+nDHWlTf5GQ=";
    private const string W3Signature = "OrisTusDdIuqJtg8THzKBjQmC0CXD/4dFvgARPTmezw=";
    private const string W4Signature = "YLaPQd4riYpXruGwLAZ+1DkjDGbe277oMj5w541B0j8=";
    private const string W5Signature = "vr1z88+4+1qVy1tfN5CvmEhSfAHOOAOh+9ykFXlA/ZU=";
    private const string W6Signature = "FDIZ1kJweEIU8XVhmRlbBeNuVk0v2q2AMq9jHVN50+k=";

    private static SignatureValidationOptions CreateOptions(bool enabled, int tolerance = int.MaxValue / 2)
    {
        return new SignatureValidationOptions
        {
            IsEnabled = enabled,
            Secret = TestKey,
            TimestampToleranceSeconds = tolerance,
        };
    }

    private static DefaultHttpContext CreateContext(string? signature, string body = V1Body)
    {
        return CreateContext("POST", "/cola", "?op=BLLogin&userid=10086", signature, body, null);
    }

    private static DefaultHttpContext CreateContext(string? signature, string body, long? appId)
    {
        return CreateContext("POST", "/cola", "?op=BLLogin&userid=10086", signature, body, appId);
    }

    private static DefaultHttpContext CreateContext(string method, string path, string? query, string? signature, string body, long? appId)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream(); // 默认 Stream.Null 会吞掉错误信封，无法断言
        context.Request.Method = method;
        context.Request.Path = path;
        if (!string.IsNullOrEmpty(query))
        {
            context.Request.QueryString = new QueryString(query);
        }

        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        if (signature != null)
        {
            context.Request.Headers[SignatureValidationMiddleware.ColaSignatureHeader] = signature;
        }

        if (appId.HasValue)
        {
            context.Request.Headers[GlobalConst.AppIdHeader] = appId.Value.ToString();
        }

        return context;
    }

    private static string V2Header(string keyId, long ts, ulong mid, string signatureBase64)
    {
        return $"v2;{keyId};{ts};{mid};{signatureBase64}";
    }

    private static byte[] HexToBytes(string hex)
    {
        var bytes = new byte[hex.Length / 2];
        for (var i = 0; i < bytes.Length; i++)
        {
            bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
        }

        return bytes;
    }

    private static byte[] W6Body()
    {
        // W6 向量 body：2048 字节 = "0123456789abcdef" 重复 128 次（hex 事实源里的 3031...6566 循环）
        return Encoding.ASCII.GetBytes(string.Concat(Enumerable.Repeat("0123456789abcdef", 128)));
    }

    private static DefaultHttpContext CreateV2Context(string keyId, long ts, ulong mid, string signatureBase64, string method = "POST", string path = "/cola", string? query = "?op=BLLogin&userid=10086", byte[]? body = null, long? appId = TestAppId)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = method;
        context.Request.Path = path;
        if (!string.IsNullOrEmpty(query))
        {
            context.Request.QueryString = new QueryString(query);
        }

        context.Request.Body = new MemoryStream(body ?? Encoding.UTF8.GetBytes(V1Body));
        context.Request.Headers[SignatureValidationMiddleware.ColaSignatureHeader] = V2Header(keyId, ts, mid, signatureBase64);
        if (appId.HasValue)
        {
            context.Request.Headers[GlobalConst.AppIdHeader] = appId.Value.ToString();
        }

        return context;
    }

    /// <summary>
    /// 双活密钥环桩：keyA/keyB 同时在场且永久生效（NotBefore=0 / Expire=null，不受真实时钟影响）；
    /// keyA 状态可按用例覆写（禁用/到期共存用例）。
    /// </summary>
    private static FakeSignatureKeyResolver CreateDualKeyRing()
    {
        var resolver = new FakeSignatureKeyResolver();
        resolver.AddKey(TestAppId, KeyAId, KeyA, true, 0, null);
        resolver.AddKey(TestAppId, KeyBId, KeyB, true, 0, null);
        return resolver;
    }

    private static Task InvokeAsync(DefaultHttpContext context, SignatureValidationOptions options, MemoryCacheService cache, Func<bool> onNext, ISignatureKeyResolver? keyResolver = null, IGameAppRiskEnforcementModeResolver? riskResolver = null)
    {
        var middleware = new SignatureValidationMiddleware(_ =>
        {
            onNext();
            return Task.CompletedTask;
        });
        // C261 起 RiskEnforcementModeResolver 改为方法内 RequestServices 按需解析（避免参数注入在无租户上下文请求上触发租户仓储构造）
        context.RequestServices = new ServiceCollection()
                                  .AddSingleton(riskResolver ?? new FakeRiskEnforcementModeResolver())
                                  .BuildServiceProvider();
        return middleware.InvokeAsync(context, Options.Create(options), cache, keyResolver ?? new FakeSignatureKeyResolver(), NullLogger<SignatureValidationMiddleware>.Instance);
    }

    private static async Task<int> ReadEnvelopeCodeAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using (var document = await JsonDocument.ParseAsync(context.Response.Body))
        {
            return document.RootElement.GetProperty("code").GetInt32();
        }
    }

    // ==================== C1 既有用例（v1 兼容回归，不删不改） ====================

    [Fact]
    public async Task ValidSignature_V1Vector_Passes()
    {
        var nextCalled = false;
        var context = CreateContext("v1;" + V1Ts + ";" + V1Mid + ";" + V1Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task ValidSignature_V2EmptyBodyVector_Passes()
    {
        // test-vectors.json V2：空 body 走 sha256("")=e3b0... 常量分支
        var nextCalled = false;
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = "POST";
        context.Request.Path = "/redis";
        context.Request.QueryString = new QueryString("?op=RSHeartBeat&userid=1");
        context.Request.Body = new MemoryStream(Array.Empty<byte>());
        context.Request.Headers[SignatureValidationMiddleware.ColaSignatureHeader] = "v1;1755244801;2;zmGudK2l/ajynuumf8bPph16hgAxojDNNJVME4+IRug=";

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task TamperedBody_RejectedWith60101()
    {
        var nextCalled = false;
        var context = CreateContext("v1;" + V1Ts + ";" + V1Mid + ";" + V1Signature, "{\"hello\":\"tampered\"}");

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task ReplayedMessageId_RejectedWith60101()
    {
        var cache = new MemoryCacheService();
        var context = CreateContext("v1;" + V1Ts + ";" + V1Mid + ";" + V1Signature);
        await InvokeAsync(context, CreateOptions(true), cache, () => true);

        var replayContext = CreateContext("v1;" + V1Ts + ";" + V1Mid + ";" + V1Signature);
        var nextCalled = false;
        await InvokeAsync(replayContext, CreateOptions(true), cache, () => nextCalled = true);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, replayContext.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(replayContext));
    }

    [Fact]
    public async Task TimestampOutOfRange_RejectedWith60101()
    {
        // 默认 300s 窗口 + 固定历史时间戳（V1Ts），必然超窗
        var nextCalled = false;
        var context = CreateContext("v1;" + V1Ts + ";" + V1Mid + ";" + V1Signature);

        await InvokeAsync(context, CreateOptions(true, 300), new MemoryCacheService(), () => nextCalled = true);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task MissingHeader_RejectedWith60101()
    {
        var nextCalled = false;
        var context = CreateContext(null);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task DisabledByDefault_PassesWithoutSignature()
    {
        var nextCalled = false;
        var context = CreateContext(null);

        await InvokeAsync(context, CreateOptions(false), new MemoryCacheService(), () => nextCalled = true);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task PublicCallbackRequest_ExemptFromValidation()
    {
        var nextCalled = false;
        var context = CreateContext(null);
        context.Items[PublicCallbackRouteContext.ItemKey] = new object();

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.True(nextCalled);
    }

    // ==================== C2 新增：v2 密钥环验签（向量 W1-W6） ====================

    [Fact]
    public async Task V2_W1Vector_KeyA_Passes()
    {
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, W1Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task V2_W2Vector_KeyB_SameParams_Passes()
    {
        // 双密钥共存锚：与 W1 完全同参数（method/path/query/ts/mid/body），仅 key 不同，两把 key 均须验签通过
        var nextCalled = false;
        var context = CreateV2Context(KeyBId, 1755244800, 1, W2Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task V2_W3Vector_EmptyBody_Passes()
    {
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244801, 2, W3Signature, path: "/redis", query: "?op=RSHeartBeat&userid=1", body: Array.Empty<byte>());

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task V2_W4Vector_NonAsciiBody_Passes()
    {
        var nextCalled = false;
        var context = CreateV2Context(KeyBId, 1755244802, 3, W4Signature, query: "?op=BLUpdUserBranch&userid=20000", body: HexToBytes("7b226e616d65223a22e9a490e58e85e78ea9e5aeb6227d"));

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task V2_W5Vector_EmptyQueryMultiSegmentPath_Passes()
    {
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244804, 5, W5Signature, path: "/api/GameAppVersion/GetInfo", query: null, body: Encoding.UTF8.GetBytes("{\"Language\":\"zh\"}"));

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task V2_W6Vector_LargeBody_Passes()
    {
        var nextCalled = false;
        var context = CreateV2Context(KeyBId, 1755244803, 4, W6Signature, query: "?op=BLLogin&userid=99999", body: W6Body());

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public void BuildV2SignatureMessage_MatchesVectorMessage()
    {
        // message 构造逐字节对齐向量事实源（W1 的 message 串）；C267 起拼装收口到 CanonicalRequestBuilder（全仓唯一）
        var message = CanonicalRequestBuilder.BuildV2Message("POST", "/cola", "op=BLLogin&userid=10086", KeyAId, 1755244800, "1", Encoding.UTF8.GetBytes(V1Body));
        Assert.Equal("v2\nPOST\n/cola\nop=BLLogin&userid=10086\nsk-vector-a-001\n1755244800\n1\n93a23971a914e5eacbf0a8d25154cda309c3c1c72fbb9914d47c60f3cb681588", message);
    }

    [Fact]
    public async Task V2_ReorderedUrlQuery_SameCanonicalSignature_Passes()
    {
        // C267 活体验证：URL query 乱序（?userid=...&op=...）但 canonical 后与 W1 一致 → W1 签名依然通过。
        // 证明服务端按 canonical（字典序）验签、不再依赖客户端原始参数顺序（代理改写不再破签）。
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, W1Signature, query: "?userid=10086&op=BLLogin");

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task V2_SignedWithRawOrderQuery_Rejected()
    {
        // 反向对照：客户端按「原始乱序串」拼 message 签名（canonical 与原始不一致时）→ 必然 401，
        // 证明服务端确实按 canonical 拼装而非宽松匹配（规范 A 升级行为差异的另一面）
        var message = "v2\nPOST\n/cola\nuserid=10086&op=BLLogin\n" + KeyAId + "\n1755244800\n1\n"
                      + CanonicalRequestBuilder.BodyHashHex(Encoding.UTF8.GetBytes(V1Body));
        var signature = CanonicalRequestBuilder.HmacSha256Base64(Encoding.UTF8.GetBytes(KeyA), message);
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, signature, query: "?userid=10086&op=BLLogin");

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    // ==================== C2 新增：拒签码分级（60102 / 60103） ====================

    [Fact]
    public async Task V2_UnknownKeyId_RejectedWith60102()
    {
        var nextCalled = false;
        var context = CreateV2Context("sk-not-exist", 1755244800, 1, W1Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.UnknownKeyIdRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task V2_MissingAppHeader_RejectedWith60102()
    {
        // 无 GameFrameX-App-Id 头 → 无法定位密钥域（AppId 内联读方案）
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, W1Signature, appId: null);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.UnknownKeyIdRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task V2_DisabledKey_RejectedWith60103()
    {
        var ring = CreateDualKeyRing();
        ring.AddKey(TestAppId, KeyAId, KeyA, false, 0, null); // keyA 禁用（同 Kid 覆写）
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, W1Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, ring);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.KeyDisabledOrExpiredRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task V2_ExpiredKey_RejectedWith60103()
    {
        var ring = CreateDualKeyRing();
        ring.AddKey(TestAppId, KeyAId, KeyA, true, 0, 1755244800); // 到期时刻 = 向量 ts（已过）
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, W1Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, ring);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.KeyDisabledOrExpiredRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task V2_TamperedBody_RejectedWith60101()
    {
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, W1Signature, body: Encoding.UTF8.GetBytes("{\"hello\":\"tampered\"}"));

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task V2_SignatureMovedToOtherKeyId_RejectedWith60101()
    {
        // keyId 进 message 防挪用：keyB 的签名配 keyA 的 keyId —— keyId 段不匹配必然验签失败（而非借用成功）
        var nextCalled = false;
        var context = CreateV2Context(KeyAId, 1755244800, 1, W2Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task V2_ReplayedMid_RejectedWith60101()
    {
        var cache = new MemoryCacheService();
        var ring = CreateDualKeyRing();
        var first = CreateV2Context(KeyAId, 1755244800, 1, W1Signature);
        await InvokeAsync(first, CreateOptions(true), cache, () => true, ring);

        var replay = CreateV2Context(KeyAId, 1755244800, 1, W1Signature);
        var nextCalled = false;
        await InvokeAsync(replay, CreateOptions(true), cache, () => nextCalled = true, ring);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, replay.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(replay));
    }

    // ==================== C2 新增：v1 兼容遍历（App 域密钥环 + 常量回退） ====================

    [Fact]
    public async Task V1_SignedWithKeyringKey_TraversesActiveSetAndPasses()
    {
        // v1 无 keyId：中间件应在 App 域生效密钥集合内遍历（非 C1 常量签的名也要能过）
        var signature = SignV1(KeyA, "POST", "/cola", "op=BLLogin&userid=10086", 1755244800, 20, V1Body);
        var nextCalled = false;
        var context = CreateContext("v1;1755244800;20;" + signature, V1Body, TestAppId);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task V1_UnknownKey_FallsBackToConstantAndFails()
    {
        // ring 内没有能验过这把签名的 key，也不匹配 C1 常量 → 60101（常量回退不豁免失败签名）
        var signature = SignV1("GFX-UNKNOWN-KEY", "POST", "/cola", "op=BLLogin&userid=10086", 1755244800, 21, V1Body);
        var nextCalled = false;
        var context = CreateContext("v1;1755244800;21;" + signature, V1Body, TestAppId);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    // ==================== C2 新增：双密钥共存（R5）与灰度分流 ====================

    [Fact]
    public async Task DualKeyCoexistence_DisableKeyA_KeyBStillPasses()
    {
        // 禁用 keyA 不影响 keyB 验签（单 key 失效不殃及全应用）
        var ring = CreateDualKeyRing();
        ring.AddKey(TestAppId, KeyAId, KeyA, false, 0, null);
        var nextCalled = false;
        var context = CreateV2Context(KeyBId, 1755244800, 1, W2Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, ring);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task AuditOnlyMode_BadSignature_StillCallsNext()
    {
        // App 级 RiskEnforcementMode=AuditOnly：校验失败仅记录不阻断
        var risk = new FakeRiskEnforcementModeResolver { Mode = RiskEnforcementModeEnum.AuditOnly, };
        var nextCalled = false;
        var context = CreateContext("v1;" + V1Ts + ";" + V1Mid + ";" + V1Signature, "{\"hello\":\"tampered\"}");

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, riskResolver: risk);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task AuditOnlyMode_MissingHeader_StillCallsNext()
    {
        var risk = new FakeRiskEnforcementModeResolver { Mode = RiskEnforcementModeEnum.AuditOnly, };
        var nextCalled = false;
        var context = CreateContext(null);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, riskResolver: risk);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task FailOpenMode_UnknownKeyId_StillCallsNext()
    {
        var risk = new FakeRiskEnforcementModeResolver { Mode = RiskEnforcementModeEnum.FailOpen, };
        var nextCalled = false;
        var context = CreateV2Context("sk-not-exist", 1755244800, 1, W1Signature);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing(), risk);

        Assert.True(nextCalled);
    }

    [Fact]
    public async Task EnabledFalse_V2BadSignature_CompletelyBypassed()
    {
        // 灰度总开关 false = 完全旁路（现网默认，与 C1 行为一致）
        var nextCalled = false;
        var context = CreateV2Context("sk-not-exist", 1755244800, 1, "AAAA");

        await InvokeAsync(context, CreateOptions(false), new MemoryCacheService(), () => nextCalled = true, CreateDualKeyRing());

        Assert.True(nextCalled);
    }

    // ==================== C379 新增：Tier 0 公开只读端点豁免（标记命中先于风险模式分流） ====================

    /// <summary>
    /// 构造「路由已匹配打标 action」的上下文：endpoint metadata 携带 <see cref="PublicStartupEndpointAttribute"/>。
    /// </summary>
    private static DefaultHttpContext CreateMarkedContext(string? signature, string body = V1Body)
    {
        var context = CreateContext(signature, body);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(new PublicStartupEndpointAttribute()), "PublicStartupEndpoint"));
        return context;
    }

    /// <summary>
    /// 构造「路由已匹配未打标 action」的上下文：endpoint 存在但 metadata 无标记——
    /// 证明豁免的判别依据是标记本身而非 endpoint 是否存在。
    /// </summary>
    private static DefaultHttpContext CreateUnmarkedContext(string? signature, string body = V1Body)
    {
        var context = CreateContext(signature, body);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, EndpointMetadataCollection.Empty, "UnmarkedEndpoint"));
        return context;
    }

    [Fact]
    public async Task PublicStartupEndpoint_FailClosedMissingHeader_Exempted()
    {
        // ① IsEnabled=true + FailClosed + 标记端点缺签名头 → 放行（豁免先于风险模式分流，FailClosed 不打断启动流程）；
        //    FakeRiskEnforcementModeResolver 默认即 FailClosed（与中间件默认安全口径一致）
        var nextCalled = false;
        var context = CreateMarkedContext(null);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task UnmarkedEndpoint_FailClosedMissingHeader_RejectedWith60101()
    {
        // ② 同条件未标记端点缺签名头 → 401 + 60101（签名语义零变化）；
        //    GetEndpoint() 为 null 的安全降级（不豁免）由既有 MissingHeader_RejectedWith60101 等用例覆盖（未 SetEndpoint 即 null）
        var nextCalled = false;
        var context = CreateUnmarkedContext(null);

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task PublicStartupEndpoint_ForgedSignatureHeader_StillExempted()
    {
        // ③ 标记端点带伪造签名头 → 仍放行：豁免即完全跳过验签，不做「带错签名反而拒」的语义
        var nextCalled = false;
        var context = CreateMarkedContext("v1;1755244800;99;AAAA");

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
    }

    [Fact]
    public async Task RegisterDeviceAction_FailClosedMissingHeader_RejectedWith60101()
    {
        // ④ RegisterDevice 归 Tier 1（写副作用，不打标）：用真实 action 的反射元数据构造 endpoint metadata，
        //    证明生产端点真实携带的特性集合不触发豁免，走正常验签（缺头 401）
        var method = typeof(GameAssetPackageVersionController).GetMethod(nameof(GameAssetPackageVersionController.RegisterDevice))!;
        var attributes = method.GetCustomAttributes(inherit: false);
        var nextCalled = false;
        var context = CreateContext(null);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(attributes), method.Name));

        await InvokeAsync(context, CreateOptions(true), new MemoryCacheService(), () => nextCalled = true);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
        Assert.Equal(SignatureValidationMiddleware.SignatureRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    // ==================== C2 新增：租约判定纯函数 ====================

    [Fact]
    public void Judge_LeaseAndEnableSemantics()
    {
        var now = 1755244800L;
        Assert.Equal(SignatureKeyLookupResult.Active, SignatureKeyResolver.Judge(Key(0, null, true), now));
        Assert.Equal(SignatureKeyLookupResult.Active, SignatureKeyResolver.Judge(Key(now, null, true), now)); // NotBefore == now 生效
        Assert.Equal(SignatureKeyLookupResult.Active, SignatureKeyResolver.Judge(Key(0, now + 1, true), now)); // now < Expire 生效
        Assert.Equal(SignatureKeyLookupResult.Disabled, SignatureKeyResolver.Judge(Key(0, null, false), now));
        Assert.Equal(SignatureKeyLookupResult.NotYetValid, SignatureKeyResolver.Judge(Key(now + 1, null, true), now));
        Assert.Equal(SignatureKeyLookupResult.Expired, SignatureKeyResolver.Judge(Key(0, now, true), now)); // now == Expire 即到期
        return;

        static TenantAppSignatureKeyEntity Key(long? notBefore, long? expire, bool enabled)
        {
            return new TenantAppSignatureKeyEntity { NotBeforeTime = notBefore, ExpireTime = expire, IsEnabled = enabled, };
        }
    }

    private static string SignV1(string secret, string method, string path, string query, long ts, ulong mid, string body)
    {
        var message = SignatureValidationMiddleware.BuildSignatureMessage(method, path, query, ts, mid, Encoding.UTF8.GetBytes(body));
        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
        }
    }

    /// <summary>
    /// 密钥环内存桩：按 (AppId, Kid) 查找，租约/启用判定复用 <see cref="SignatureKeyResolver.Judge"/>（与生产同一路径）。
    /// </summary>
    private sealed class FakeSignatureKeyResolver : ISignatureKeyResolver
    {
        private readonly List<TenantAppSignatureKeyEntity> _keys = new();

        public void AddKey(long appId, string kid, string secret, bool enabled, long? notBefore, long? expire)
        {
            _keys.RemoveAll(x => x.AppId == appId && x.Kid == kid);
            _keys.Add(new TenantAppSignatureKeyEntity
            {
                AppId = appId,
                Kid = kid,
                Secret = secret,
                IsEnabled = enabled,
                NotBeforeTime = notBefore,
                ExpireTime = expire,
                CreatedTime = 1000,
            });
        }

        public Task<SignatureKeyLookup> ResolveAsync(long appId, string kid)
        {
            var entity = _keys.FirstOrDefault(x => x.AppId == appId && x.Kid == kid);
            if (entity == null)
            {
                return Task.FromResult(new SignatureKeyLookup(SignatureKeyLookupResult.NotFound, kid, null));
            }

            var result = SignatureKeyResolver.Judge(entity, TimerHelper.UnixTimeSeconds());
            return Task.FromResult(new SignatureKeyLookup(result, kid, result == SignatureKeyLookupResult.Active ? entity.Secret : null));
        }

        public Task<IReadOnlyList<SignatureKeyMaterial>> GetActiveKeysAsync(long appId)
        {
            var now = TimerHelper.UnixTimeSeconds();
            IReadOnlyList<SignatureKeyMaterial> active = _keys.Where(x => x.AppId == appId && SignatureKeyResolver.Judge(x, now) == SignatureKeyLookupResult.Active)
                                                              .OrderByDescending(x => x.CreatedTime)
                                                              .Select(x => new SignatureKeyMaterial(x.Kid, x.Secret))
                                                              .ToList();
            return Task.FromResult(active);
        }
    }

    /// <summary>
    /// 风险生效模式桩：固定返回 <see cref="Mode"/>，默认 FailClosed（与中间件默认安全口径一致）。
    /// </summary>
    private sealed class FakeRiskEnforcementModeResolver : IGameAppRiskEnforcementModeResolver
    {
        public RiskEnforcementModeEnum Mode { get; set; } = RiskEnforcementModeEnum.FailClosed;

        public RiskEnforcementModeEnum Resolve(long appId)
        {
            return Mode;
        }
    }
}