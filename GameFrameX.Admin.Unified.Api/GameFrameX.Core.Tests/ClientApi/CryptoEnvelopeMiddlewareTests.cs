using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// CryptoEnvelopeMiddleware 向量等价测试（change C5 server-envelope-middleware）。
/// 全部期望值逐字抄录 canting 仓 gfx-doc/dev/crypto-envelope/archived/C4-client-crypto-envelope/test-vectors.json
/// （事实源唯一，禁止本地重算）。
/// <para>结构与客户端 C4 harness 同构（双端同一期望值）：<b>派生组</b>（HKDF RFC 5869 A.1 + c2s/s2c）
/// 与<b>信封组</b>（E1/E3 字节级 + E2 deflate 语义，AES key = 向量 key_hex 原始 session key）分开锚定；
/// 中间件集成用例按生产口径（先 HKDF c2s/s2c 派生再 GCM，对齐客户端 ApiServerHttpRequest 管线）自造信封往返，
/// 若中间件漏派生/派错方向（raw key / s2c 当 c2s）这些用例必败。</para>
/// <para>密钥环用 <see cref="FakeSessionKeyResolver"/> 内存桩：向量 key 为假 key（字节 0x00-0x1f 顺序值），
/// NotBeforeTime=0 / ExpireTime=null（永久生效）不受真实时钟影响；到期/禁用用例单独覆写。</para>
/// </summary>
public sealed class CryptoEnvelopeMiddlewareTests
{
    private const long TestAppId = 1001;

    // ===== HKDF RFC 5869 A.1 官方向量（test-vectors.json hkdf_rfc5869_a1） =====
    private const string A1IkmHex = "0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b";
    private const string A1SaltHex = "000102030405060708090a0b0c";
    private const string A1InfoHex = "f0f1f2f3f4f5f6f7f8f9";
    private const string A1OkmHex = "3cb25f25faacd57a90434f64d0362f2a2d2d0a90cf1a5a4c5db02d56ecc4c5bf34007208d5b887185865";

    // ===== c2s/s2c 派生组（test-vectors.json derive，salt=空） =====
    private const string DeriveSessionKeyHex = "000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f";
    private const string DeriveSendKeyC2sHex = "4b0b6d07926d530d4702c0d7cb6a9f5d65e27163fb78f2397a6e370ce76fa93a";
    private const string DeriveRecvKeyS2cHex = "699c6951db061a3a21c807ab4b47da8635c3c63e9dc3710793cdba9804085d6b";

    // ===== E1 raw 精确向量（常规 path + query 域外） =====
    private const string VectorKid = "sk-vector-a-001";
    private const ulong E1Mid = 1;
    private const long E1Ts = 1755244800;
    private const string E1Path = "/cola";
    private const string E1IvBase64 = "AAECAwQFBgcICQoL";
    private const string E1CtBase64 = "R3n0c6CJrnSve7X83psUCaGrOaNCxPow0srgl4ouqbtHuA==";
    private const string E1AadHex = "656e76310a2f636f6c610a313735353234343830300a31";
    private const string E1PayloadHex = "7b2268656c6c6f223a22776f726c64227d";
    private const string E1EnvelopeJson = "{\"v\":1,\"kid\":\"sk-vector-a-001\",\"mid\":1,\"ts\":1755244800,\"iv\":\"AAECAwQFBgcICQoL\",\"ct\":\"R3n0c6CJrnSve7X83psUCaGrOaNCxPow0srgl4ouqbtHuA==\"}";

    // ===== E3 raw 精确向量（多段 path + 非 ASCII payload） =====
    private const ulong E3Mid = 5;
    private const long E3Ts = 1755244804;
    private const string E3Path = "/api/GameAppVersion/GetInfo";
    private const string E3CtBase64 = "R3n0V6SLpW7sJvKpi8uc1S4wEbMV1eleRTU4eQiKFvmbtVITC/Yd2lA=";
    private const string E3AadHex = "656e76310a2f6170692f47616d6541707056657273696f6e2f476574496e666f0a313735353234343830340a35";
    private const string E3PayloadHex = "7b224c616e6775616765223a22e4b8ade69687e5aeb6227d";
    private const string E3EnvelopeJson = "{\"v\":1,\"kid\":\"sk-vector-a-001\",\"mid\":5,\"ts\":1755244804,\"iv\":\"AAECAwQFBgcICQoL\",\"ct\":\"R3n0V6SLpW7sJvKpi8uc1S4wEbMV1eleRTU4eQiKFvmbtVITC/Yd2lA=\"}";

    // ===== E2 deflate 语义向量（ct 不锁字节，校验 = 解密得 flag=0x01 + raw deflate，inflate 后等于 payload） =====
    private const ulong E2Mid = 2;
    private const long E2Ts = 1755244801;
    private const string E2IvHex = "101112131415161718191a1b";
    private const string E2PayloadHex = "7b22726f7773223a5b7b226964223a302c2273636f7265223a307d2c7b226964223a312c2273636f7265223a377d2c7b226964223a322c2273636f7265223a31347d2c7b226964223a332c2273636f7265223a32317d2c7b226964223a342c2273636f7265223a32387d2c7b226964223a352c2273636f7265223a33357d2c7b226964223a362c2273636f7265223a34327d2c7b226964223a372c2273636f7265223a34397d2c7b226964223a382c2273636f7265223a35367d2c7b226964223a392c2273636f7265223a36337d2c7b226964223a31302c2273636f7265223a37307d2c7b226964223a31312c2273636f7265223a37377d2c7b226964223a31322c2273636f7265223a38347d2c7b226964223a31332c2273636f7265223a39317d2c7b226964223a31342c2273636f7265223a39387d2c7b226964223a31352c2273636f7265223a3130357d2c7b226964223a31362c2273636f7265223a3131327d2c7b226964223a31372c2273636f7265223a3131397d2c7b226964223a31382c2273636f7265223a3132367d2c7b226964223a31392c2273636f7265223a3133337d2c7b226964223a32302c2273636f7265223a3134307d2c7b226964223a32312c2273636f7265223a3134377d2c7b226964223a32322c2273636f7265223a3135347d2c7b226964223a32332c2273636f7265223a3136317d2c7b226964223a32342c2273636f7265223a3136387d2c7b226964223a32352c2273636f7265223a3137357d2c7b226964223a32362c2273636f7265223a3138327d2c7b226964223a32372c2273636f7265223a3138397d2c7b226964223a32382c2273636f7265223a3139367d2c7b226964223a32392c2273636f7265223a3230337d2c7b226964223a33302c2273636f7265223a3231307d2c7b226964223a33312c2273636f7265223a3231377d2c7b226964223a33322c2273636f7265223a3232347d2c7b226964223a33332c2273636f7265223a3233317d2c7b226964223a33342c2273636f7265223a3233387d2c7b226964223a33352c2273636f7265223a3234357d2c7b226964223a33362c2273636f7265223a3235327d2c7b226964223a33372c2273636f7265223a3235397d2c7b226964223a33382c2273636f7265223a3236367d2c7b226964223a33392c2273636f7265223a3237337d5d7d";
    private const string E2OracleCtBase64 = "fLNKLSeKC6OJpLW1eVgbzamOBJ4xi8Z7Z6RaV4CKSftA0Mpz3rvnQAyJr05vYuIxrGOiKcxuqIcH4casPVzb6dcgtmlxyAABDzf2kzYXPBzQb0ejiKuO7O6jjeozErkaId7C1/UoM5M9r/nDCbTQ8cP6WCC9oyH8vnpKSTcC8Q+XleXiFK8uzWAmLGyUNBXsI7XJ1pD6njk2uAm7qi55pg3h6i481UjJ4Nl96kJGBUlNQe17rVo0DX+C7b7+E4cqJdOEjc3BRpAWKOL+lDxQv1Ms/KsLrYhOjgY6X+ukfs6NITdXMvo=";

    private static CryptoEnvelopeOptions CreateOptions(bool enabled, List<long>? testTenantIds = null)
    {
        return new CryptoEnvelopeOptions
        {
            IsEnabled = enabled,
            TestTenantIds = testTenantIds ?? new List<long>(),
        };
    }

    private static DefaultHttpContext CreateEnvelopeContext(string envelopeJson, string path = E1Path, string? query = "?op=BLLogin&userid=10086", string? cryptoHeader = "1", long? appId = TestAppId, long? tenantId = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Method = "POST";
        context.Request.Path = path;
        if (!string.IsNullOrEmpty(query))
        {
            context.Request.QueryString = new QueryString(query);
        }

        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(envelopeJson));
        if (cryptoHeader != null)
        {
            context.Request.Headers[CryptoEnvelopeMiddleware.CryptoHeader] = cryptoHeader;
        }

        if (appId.HasValue)
        {
            context.Request.Headers[GlobalConst.AppIdHeader] = appId.Value.ToString();
        }

        if (tenantId.HasValue)
        {
            context.Items[TenantContextAccessor.TenantIdKey] = tenantId.Value;
        }

        return context;
    }

    /// <summary>
    /// 双活密钥环桩：向量 key（永久生效，不受真实时钟影响）为默认；用例可按需覆写状态。
    /// </summary>
    private static FakeSessionKeyResolver CreateKeyRing()
    {
        var resolver = new FakeSessionKeyResolver();
        resolver.AddKey(TestAppId, VectorKid, DeriveSessionKeyHex, true, 0, null);
        return resolver;
    }

    private static Task InvokeAsync(DefaultHttpContext context, CryptoEnvelopeOptions options, Func<Task> onNext, ISessionKeyResolver? keyResolver = null)
    {
        var middleware = new CryptoEnvelopeMiddleware(_ => onNext());
        return middleware.InvokeAsync(context, Options.Create(options), keyResolver ?? CreateKeyRing(), NullLogger<CryptoEnvelopeMiddleware>.Instance);
    }

    /// <summary>
    /// 按生产口径自造信封 JSON（中间件集成用例）：payload 前加压缩标记后，用 <b>HKDF c2s 派生 key</b> 加密
    /// （对齐客户端 ApiServerHttpRequest 生产管线 GetSendKey 口径）。中间件若漏派生/派错方向必解不开。
    /// </summary>
    private static string BuildEnvelopeJson(string path, ulong mid, long ts, byte[] payload, bool deflate)
    {
        var c2s = CryptoEnvelopeMiddleware.DeriveDirectionalKey(HexToBytes(DeriveSessionKeyHex), "c2s");
        var iv = HexToBytes(E2IvHex);
        var content = deflate ? DeflateRaw(payload) : payload;
        var plain = new byte[1 + content.Length];
        plain[0] = deflate ? CryptoEnvelopeMiddleware.FlagDeflate : CryptoEnvelopeMiddleware.FlagRaw;
        Buffer.BlockCopy(content, 0, plain, 1, content.Length);
        var ct = CraftCtBase64(c2s, iv, plain, CryptoEnvelopeMiddleware.BuildAad(path, ts, mid));
        return "{\"v\":1,\"kid\":\"" + VectorKid + "\",\"mid\":" + mid.ToString() + ",\"ts\":" + ts.ToString()
               + ",\"iv\":\"" + Convert.ToBase64String(iv) + "\",\"ct\":\"" + ct + "\"}";
    }

    /// <summary>raw deflate 压缩（DeflateStream 原语，与客户端 DeflateRaw / python zlib -15 同格式）。</summary>
    private static byte[] DeflateRaw(byte[] data)
    {
        using (var output = new MemoryStream())
        {
            using (var deflate = new DeflateStream(output, CompressionLevel.Optimal, true))
            {
                deflate.Write(data, 0, data.Length);
            }

            return output.ToArray();
        }
    }

    /// <summary>测试内独立加密路径（构造信封 ct / 负例明文），不经过被测代码的 Encode 辅助。</summary>
    private static string CraftCtBase64(byte[] key, byte[] iv, byte[] plain, byte[] aad)
    {
        var ct = new byte[plain.Length];
        var tag = new byte[CryptoEnvelopeMiddleware.TagBytes];
        using (var aes = new AesGcm(key, CryptoEnvelopeMiddleware.TagBytes))
        {
            aes.Encrypt(iv, plain, ct, tag, aad);
        }

        var combined = new byte[plain.Length + CryptoEnvelopeMiddleware.TagBytes];
        Buffer.BlockCopy(ct, 0, combined, 0, plain.Length);
        Buffer.BlockCopy(tag, 0, combined, plain.Length, CryptoEnvelopeMiddleware.TagBytes);
        return Convert.ToBase64String(combined);
    }

    private static async Task<int> ReadEnvelopeCodeAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using (var document = await JsonDocument.ParseAsync(context.Response.Body))
        {
            return document.RootElement.GetProperty("code").GetInt32();
        }
    }

    private static async Task<byte[]> ReadRequestBodyAsync(DefaultHttpContext context)
    {
        context.Request.Body.Position = 0;
        using (var ms = new MemoryStream())
        {
            await context.Request.Body.CopyToAsync(ms);
            return ms.ToArray();
        }
    }

    private static async Task<byte[]> ReadResponseBodyAsync(DefaultHttpContext context)
    {
        context.Response.Body.Position = 0;
        using (var ms = new MemoryStream())
        {
            await context.Response.Body.CopyToAsync(ms);
            return ms.ToArray();
        }
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

    private static string BytesToHex(byte[] bytes)
    {
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    // ==================== HKDF / 派生 / AAD（向量锚定） ====================

    [Fact]
    public void Hkdf_Rfc5869A1_OfficialVector()
    {
        var okm = CryptoEnvelopeMiddleware.HkdfSha256(HexToBytes(A1IkmHex), HexToBytes(A1SaltHex), HexToBytes(A1InfoHex), A1OkmHex.Length / 2);
        Assert.Equal(A1OkmHex, BytesToHex(okm));
    }

    [Fact]
    public void DeriveDirectionalKey_C2sS2c_MatchVector()
    {
        // salt=空（RFC 缺省 hashLen 零字节）由 DeriveDirectionalKey 传 null 覆盖，与客户端手写实现零共享锚定
        var sessionKey = HexToBytes(DeriveSessionKeyHex);
        Assert.Equal(DeriveSendKeyC2sHex, BytesToHex(CryptoEnvelopeMiddleware.DeriveDirectionalKey(sessionKey, "c2s")));
        Assert.Equal(DeriveRecvKeyS2cHex, BytesToHex(CryptoEnvelopeMiddleware.DeriveDirectionalKey(sessionKey, "s2c")));
    }

    [Fact]
    public void BuildAad_E1_E3_MatchVectorBytes()
    {
        Assert.Equal(E1AadHex, BytesToHex(CryptoEnvelopeMiddleware.BuildAad(E1Path, E1Ts, E1Mid)));
        Assert.Equal(E3AadHex, BytesToHex(CryptoEnvelopeMiddleware.BuildAad(E3Path, E3Ts, E3Mid)));
    }

    [Fact]
    public void IsValidKid_WhitelistSemantics()
    {
        Assert.True(CryptoEnvelopeMiddleware.IsValidKid("sk-vector-a-001"));
        Assert.True(CryptoEnvelopeMiddleware.IsValidKid("a._-Z9"));
        Assert.False(CryptoEnvelopeMiddleware.IsValidKid(string.Empty));
        Assert.False(CryptoEnvelopeMiddleware.IsValidKid("bad kid"));
        Assert.False(CryptoEnvelopeMiddleware.IsValidKid("kid\"quote"));
        Assert.False(CryptoEnvelopeMiddleware.IsValidKid("a\\b"));
        Assert.False(CryptoEnvelopeMiddleware.IsValidKid(new string('a', 65)));
        Assert.True(CryptoEnvelopeMiddleware.IsValidKid(new string('a', 64)));
    }

    // ==================== 信封解析（受控 JSON） ====================

    [Fact]
    public void Parse_E1Envelope_MatchesVectorFields()
    {
        var parsed = CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(E1EnvelopeJson), out var fields);
        Assert.True(parsed);
        Assert.Equal(VectorKid, fields.Kid);
        Assert.Equal(E1Mid, fields.Mid);
        Assert.Equal(E1Ts, fields.Ts);
        Assert.Equal(E1IvBase64, Convert.ToBase64String(fields.Iv));
        Assert.Equal(E1CtBase64, Convert.ToBase64String(fields.Ct));
    }

    [Fact]
    public void Parse_RejectsVersion2_BackslashAndIllegalKid()
    {
        var v2 = E1EnvelopeJson.Replace("\"v\":1", "\"v\":2");
        Assert.False(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(v2), out _));
        var escaped = E1EnvelopeJson.Replace(VectorKid, "a\\b");
        Assert.False(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(escaped), out _));
        var illegalKid = E1EnvelopeJson.Replace(VectorKid, "bad kid");
        Assert.False(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(illegalKid), out _));
        Assert.False(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes("{\"v\":1}"), out _));
    }

    // ==================== 信封向量（字节级 / 语义级，AES key = 向量 key_hex） ====================

    [Fact]
    public void Core_E1Vector_RawKeyDecryptsToPlaintext()
    {
        Assert.True(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(E1EnvelopeJson), out var fields));
        Assert.True(CryptoEnvelopeMiddleware.TryOpenEnvelope(HexToBytes(DeriveSessionKeyHex), fields, E1Path, out var payload));
        Assert.Equal(E1PayloadHex, BytesToHex(payload));
    }

    [Fact]
    public void Core_E3Vector_MultiSegmentPath_NonAsciiDecrypts()
    {
        Assert.True(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(E3EnvelopeJson), out var fields));
        Assert.True(CryptoEnvelopeMiddleware.TryOpenEnvelope(HexToBytes(DeriveSessionKeyHex), fields, E3Path, out var payload));
        Assert.Equal(E3PayloadHex, BytesToHex(payload));
        Assert.Equal("{\"Language\":\"中文家\"}", Encoding.UTF8.GetString(payload));
    }

    [Fact]
    public void Core_E2DeflateOracle_InflatesToPayload()
    {
        // ct 为 python cryptography oracle 产物（raw deflate 流跨实现不锁字节）：解密得 flag=0x01 + raw deflate，
        // inflate 后必须等于向量 payload —— 跨实现（C# 解密端 vs python 加密端）锚
        var envelope = "{\"v\":1,\"kid\":\"" + VectorKid + "\",\"mid\":" + E2Mid.ToString() + ",\"ts\":" + E2Ts.ToString()
                       + ",\"iv\":\"" + Convert.ToBase64String(HexToBytes(E2IvHex)) + "\",\"ct\":\"" + E2OracleCtBase64 + "\"}";
        Assert.True(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(envelope), out var fields));
        Assert.True(CryptoEnvelopeMiddleware.TryOpenEnvelope(HexToBytes(DeriveSessionKeyHex), fields, E1Path, out var payload));
        Assert.Equal(E2PayloadHex, BytesToHex(payload));
    }

    [Fact]
    public void Core_VectorAadPathTamper_DecryptFails()
    {
        // AAD 绑 path（向量字节级负例）：E1 信封换 path 解密必败（GCM tag 验证）
        Assert.True(CryptoEnvelopeMiddleware.TryParseEnvelope(Encoding.UTF8.GetBytes(E1EnvelopeJson), out var fields));
        Assert.False(CryptoEnvelopeMiddleware.TryOpenEnvelope(HexToBytes(DeriveSessionKeyHex), fields, "/cola2", out _));
    }

    // ==================== 中间件：请求侧解密（生产口径 c2s 派生 + flag/压缩） ====================

    [Fact]
    public async Task Request_ValidEnvelope_DecryptsToPlaintextBody()
    {
        // E1 参数组（path /cola + query 域外断言）+ 派生 c2s 加密；中间件漏派生/方向派错必解不开
        var envelope = BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false);
        var nextCalled = false;
        var context = CreateEnvelopeContext(envelope);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        var body = await ReadRequestBodyAsync(context);
        Assert.Equal(E1PayloadHex, BytesToHex(body));
        Assert.Equal(body.Length, context.Request.ContentLength);
    }

    [Fact]
    public async Task Request_MultiSegmentPath_NonAsciiPayload_Decrypts()
    {
        // E3 参数组：多段 path（AAD 精确取请求 path，不含 query）
        var envelope = BuildEnvelopeJson(E3Path, E3Mid, E3Ts, HexToBytes(E3PayloadHex), false);
        var nextCalled = false;
        var context = CreateEnvelopeContext(envelope, E3Path, null);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
        var body = await ReadRequestBodyAsync(context);
        Assert.Equal(E3PayloadHex, BytesToHex(body));
    }

    [Fact]
    public async Task Request_DeflateEnvelope_InflatesToPayload()
    {
        // E2 语义（中间件级）：flag=0x01 + raw deflate → 解压后明文进管线
        var envelope = BuildEnvelopeJson(E1Path, E2Mid, E2Ts, HexToBytes(E2PayloadHex), true);
        var nextCalled = false;
        var context = CreateEnvelopeContext(envelope);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
        var body = await ReadRequestBodyAsync(context);
        Assert.Equal(E2PayloadHex, BytesToHex(body));
    }

    // ==================== 负例：AAD/ct/v/kid/flag 篡改必败 + 拒收码分级 ====================

    [Fact]
    public async Task AadPathMismatch_RejectedWith60106()
    {
        var nextCalled = false;
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false), "/other");

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(CryptoEnvelopeMiddleware.EnvelopeDecryptRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task TamperedTs_RejectedWith60106()
    {
        var envelope = BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false).Replace("1755244800", "1755244809");
        var nextCalled = false;
        var context = CreateEnvelopeContext(envelope);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(CryptoEnvelopeMiddleware.EnvelopeDecryptRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task TamperedMid_RejectedWith60106()
    {
        var envelope = BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false).Replace("\"mid\":1", "\"mid\":9");
        var nextCalled = false;
        var context = CreateEnvelopeContext(envelope);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(CryptoEnvelopeMiddleware.EnvelopeDecryptRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task TamperedCt_RejectedWith60106()
    {
        // ct 篡改一字节（首字符替换，仍为合法 base64）→ tag 验证必败
        var envelope = BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false);
        var nextCalled = false;
        var context = CreateEnvelopeContext(ReplaceFirstCtByte(envelope));

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(CryptoEnvelopeMiddleware.EnvelopeDecryptRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task Version2_RejectedWith60106()
    {
        var nextCalled = false;
        var context = CreateEnvelopeContext(E1EnvelopeJson.Replace("\"v\":1", "\"v\":2"));

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(CryptoEnvelopeMiddleware.EnvelopeDecryptRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task IllegalFlag_RejectedWith60106()
    {
        // flag ∉ {0x00, 0x01}：解密成功但压缩标记非法必须拒收
        var c2s = CryptoEnvelopeMiddleware.DeriveDirectionalKey(HexToBytes(DeriveSessionKeyHex), "c2s");
        var iv = HexToBytes(E2IvHex);
        var aad = CryptoEnvelopeMiddleware.BuildAad(E1Path, E1Ts, 77);
        var ct = CraftCtBase64(c2s, iv, new byte[] { 0x02, 0x78, }, aad);
        var envelope = "{\"v\":1,\"kid\":\"" + VectorKid + "\",\"mid\":77,\"ts\":" + E1Ts.ToString()
                       + ",\"iv\":\"" + Convert.ToBase64String(iv) + "\",\"ct\":\"" + ct + "\"}";
        var nextCalled = false;
        var context = CreateEnvelopeContext(envelope);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(CryptoEnvelopeMiddleware.EnvelopeDecryptRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task UnknownKid_RejectedWith60104()
    {
        var nextCalled = false;
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false).Replace(VectorKid, "sk-not-exist"));

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(CryptoEnvelopeMiddleware.UnknownKidRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task MissingAppHeader_RejectedWith60104()
    {
        var nextCalled = false;
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false), appId: null);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.False(nextCalled);
        Assert.Equal(CryptoEnvelopeMiddleware.UnknownKidRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task DisabledKey_RejectedWith60105()
    {
        var ring = CreateKeyRing();
        ring.AddKey(TestAppId, VectorKid, DeriveSessionKeyHex, false, 0, null);
        var nextCalled = false;
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false));

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, ring);

        Assert.False(nextCalled);
        Assert.Equal(StatusCodes.Status400BadRequest, context.Response.StatusCode);
        Assert.Equal(CryptoEnvelopeMiddleware.KeyDisabledOrExpiredRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    [Fact]
    public async Task ExpiredKey_RejectedWith60105()
    {
        var ring = CreateKeyRing();
        ring.AddKey(TestAppId, VectorKid, DeriveSessionKeyHex, true, 0, 1755244800); // 到期时刻 = 向量 ts（已过）
        var nextCalled = false;
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false));

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        }, ring);

        Assert.False(nextCalled);
        Assert.Equal(CryptoEnvelopeMiddleware.KeyDisabledOrExpiredRejectedCode, await ReadEnvelopeCodeAsync(context));
    }

    // ==================== 灰度门控：开关 / 无头放行 / 测试租户旁路 ====================

    [Fact]
    public async Task EnabledFalse_CompletelyBypassed()
    {
        var nextCalled = false;
        var context = CreateEnvelopeContext("not-json-at-all");

        await InvokeAsync(context, CreateOptions(false), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
        Assert.Equal(StatusCodes.Status200OK, context.Response.StatusCode);
        Assert.Equal("not-json-at-all", Encoding.UTF8.GetString(await ReadRequestBodyAsync(context)));
    }

    [Fact]
    public async Task NoCryptoHeader_PlaintextPassesThrough()
    {
        var nextCalled = false;
        var context = CreateEnvelopeContext("{\"op\":\"BLLogin\"}", cryptoHeader: null);

        await InvokeAsync(context, CreateOptions(true), () =>
        {
            nextCalled = true;
            return Task.CompletedTask;
        });

        Assert.True(nextCalled);
        Assert.Equal("{\"op\":\"BLLogin\"}", Encoding.UTF8.GetString(await ReadRequestBodyAsync(context)));
    }

    [Fact]
    public async Task TestTenant_BypassesEnvelopeEvenWhenEnabled()
    {
        // 命中 TestTenantIds 的测试租户：请求不解密（body 原样进管线）、响应不加密（明文直出）
        var nextCalled = false;
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false), tenantId: 999);

        await InvokeAsync(context, CreateOptions(true, new List<long> { 999, }), async () =>
        {
            nextCalled = true;
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("PLAIN"));
        });

        Assert.True(nextCalled);
        // 原信封 JSON 未被解密改写（body 仍以 { 开头而非明文 payload）
        var body = await ReadRequestBodyAsync(context);
        Assert.StartsWith("{\"v\":1", Encoding.UTF8.GetString(body));
        Assert.Equal("PLAIN", Encoding.UTF8.GetString(await ReadResponseBodyAsync(context)));
    }

    // ==================== 响应侧加密：s2c 往返（双端契约） ====================

    [Fact]
    public async Task DecryptedRequest_ResponseEncryptedAsEnvelope()
    {
        var envelope = BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false);
        var context = CreateEnvelopeContext(envelope);

        await InvokeAsync(context, CreateOptions(true), async () =>
        {
            context.Response.StatusCode = StatusCodes.Status200OK;
            await context.Response.Body.WriteAsync(Encoding.UTF8.GetBytes("{\"code\":0,\"data\":\"PONG\"}"));
        });

        var responseBytes = await ReadResponseBodyAsync(context);
        Assert.True(CryptoEnvelopeMiddleware.TryParseEnvelope(responseBytes, out var fields));
        // mid 回显请求信封 mid、kid 回显；ts 现生成
        Assert.Equal(VectorKid, fields.Kid);
        Assert.Equal(E1Mid, fields.Mid);

        // 客户端解密路径：按「其请求 path + 信封自报 mid/ts」构 AAD，s2c key（方向与请求解密相反）；
        // 中间件若用 c2s 加密响应或 AAD 用了响应 path 以外字段，此处必败
        var s2c = CryptoEnvelopeMiddleware.DeriveDirectionalKey(HexToBytes(DeriveSessionKeyHex), "s2c");
        Assert.True(CryptoEnvelopeMiddleware.TryOpenEnvelope(s2c, fields, E1Path, out var payload));
        Assert.Equal("{\"code\":0,\"data\":\"PONG\"}", Encoding.UTF8.GetString(payload));
        Assert.Equal(responseBytes.Length, context.Response.ContentLength);
    }

    [Fact]
    public async Task DecryptedRequest_EmptyResponse_StaysEmpty()
    {
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false));

        await InvokeAsync(context, CreateOptions(true), () => Task.CompletedTask);

        Assert.Equal(0, (await ReadResponseBodyAsync(context)).Length);
    }

    [Fact]
    public async Task RejectedEnvelope_ErrorBodyIsCamelCaseCodeMessage()
    {
        var context = CreateEnvelopeContext(BuildEnvelopeJson(E1Path, E1Mid, E1Ts, HexToBytes(E1PayloadHex), false).Replace(VectorKid, "sk-not-exist"));

        await InvokeAsync(context, CreateOptions(true), () => Task.CompletedTask);

        context.Response.Body.Position = 0;
        using (var document = await JsonDocument.ParseAsync(context.Response.Body))
        {
            Assert.True(document.RootElement.TryGetProperty("code", out _));
            Assert.True(document.RootElement.TryGetProperty("message", out _));
        }
    }

    // ==================== 租约判定纯函数（照 C2 Judge 口径） ====================

    [Fact]
    public void Judge_LeaseAndEnableSemantics()
    {
        var now = 1755244800L;
        Assert.Equal(SessionKeyLookupResult.Active, SessionKeyResolver.Judge(Key(0, null, true), now));
        Assert.Equal(SessionKeyLookupResult.Active, SessionKeyResolver.Judge(Key(now, null, true), now));
        Assert.Equal(SessionKeyLookupResult.Active, SessionKeyResolver.Judge(Key(0, now + 1, true), now));
        Assert.Equal(SessionKeyLookupResult.Disabled, SessionKeyResolver.Judge(Key(0, null, false), now));
        Assert.Equal(SessionKeyLookupResult.NotYetValid, SessionKeyResolver.Judge(Key(now + 1, null, true), now));
        Assert.Equal(SessionKeyLookupResult.Expired, SessionKeyResolver.Judge(Key(0, now, true), now));
        return;

        static TenantSessionKeyEntity Key(long? notBefore, long? expire, bool enabled)
        {
            return new TenantSessionKeyEntity { NotBeforeTime = notBefore, ExpireTime = expire, IsEnabled = enabled, };
        }
    }

    /// <summary>把信封 JSON 的 ct 首个 base64 字符换成同字符集另一字符（仍合法 base64，长度不变）。</summary>
    private static string ReplaceFirstCtByte(string envelopeJson)
    {
        var marker = "\"ct\":\"";
        var index = envelopeJson.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var changed = envelopeJson[index] == 'A' ? 'B' : 'A';
        return envelopeJson[..index] + changed + envelopeJson[(index + 1)..];
    }

    /// <summary>
    /// 密钥环内存桩：按 (AppId, Kid) 查找，租约/启用判定复用 <see cref="SessionKeyResolver.Judge"/>（与生产同一路径）。
    /// </summary>
    private sealed class FakeSessionKeyResolver : ISessionKeyResolver
    {
        private readonly List<TenantSessionKeyEntity> _keys = new();

        public void AddKey(long appId, string kid, string secret, bool enabled, long? notBefore, long? expire)
        {
            _keys.RemoveAll(x => x.AppId == appId && x.Kid == kid);
            _keys.Add(new TenantSessionKeyEntity
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

        public Task<SessionKeyLookup> ResolveAsync(long appId, string kid)
        {
            var entity = _keys.FirstOrDefault(x => x.AppId == appId && x.Kid == kid);
            if (entity == null)
            {
                return Task.FromResult(new SessionKeyLookup(SessionKeyLookupResult.NotFound, kid, null));
            }

            var result = SessionKeyResolver.Judge(entity, TimerHelper.UnixTimeSeconds());
            return Task.FromResult(new SessionKeyLookup(result, kid, result == SessionKeyLookupResult.Active ? entity.Secret : null));
        }
    }
}