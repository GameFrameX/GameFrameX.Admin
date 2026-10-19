using System.Text;
using GameFrameX.Core.SignatureAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace GameFrameX.Core.Tests.SignatureAuth;

/// <summary>
/// CanonicalRequestBuilder 单元测试（C267-A）。
/// 覆盖：字典序、percent-encoding（RFC 3986 保留字）、空 value、剔除 Signature 参数、
/// Unicode key/value、重复 key 保留出现顺序、v2 message 字段顺序、空 body 常量、
/// HMAC-SHA256 已知向量（RFC 4231 风格第三方事实源）。
/// </summary>
public sealed class CanonicalRequestBuilderTests
{
    private static QueryCollection Query(params (string Key, string Value)[] pairs)
    {
        var dict = new Dictionary<string, StringValues>(StringComparer.Ordinal);
        foreach (var (key, value) in pairs)
        {
            if (dict.TryGetValue(key, out var existing))
            {
                var merged = existing.ToList();
                merged.Add(value);
                dict[key] = new StringValues(merged.ToArray());
            }
            else
            {
                dict[key] = new StringValues(value);
            }
        }

        return new QueryCollection(dict);
    }

    [Fact]
    public void PercentEncode_LeavesUnreservedAndEncodesReserved()
    {
        // RFC 3986 §2.3 unreserved 原样保留
        Assert.Equal("abcXYZ0189-._~", CanonicalRequestBuilder.PercentEncode("abcXYZ0189-._~"));

        // §2.2 reserved 与空格、非 ASCII 必须编码（%XX 大写十六进制）
        Assert.Equal("%3A%2F%3F%23%5B%5D%40%21%24%26%27%28%29%2A%2B%2C%3B%3D", CanonicalRequestBuilder.PercentEncode(":/?#[]@!$&'()*+,;="));
        Assert.Equal("%20", CanonicalRequestBuilder.PercentEncode(" "));
        Assert.Equal("%E4%B8%AD", CanonicalRequestBuilder.PercentEncode("中"));
    }

    [Fact]
    public void CanonicalQuery_OrdersAlphabetically()
    {
        // 乱序传入 → 按 key Ordinal 字典序输出（解决前置代理改写参数顺序破签）
        var query = Query(("userid", "10086"), ("op", "BLLogin"));
        Assert.Equal("op=BLLogin&userid=10086", CanonicalRequestBuilder.CanonicalQuery(query));
    }

    [Fact]
    public void CanonicalQuery_PercentEncodesReservedChars()
    {
        // key / value 含保留字（如 + / 空格 / 中文）必须编码后拼接
        var query = Query(("a+b", "x/y"), ("kw", "红色 战机"));
        Assert.Equal("a%2Bb=x%2Fy&kw=%E7%BA%A2%E8%89%B2%20%E6%88%98%E6%9C%BA", CanonicalRequestBuilder.CanonicalQuery(query));
    }

    [Fact]
    public void CanonicalQuery_StripsSignatureParam()
    {
        // Signature 参数不进待签串（防循环依赖）
        var query = Query(("Signature", "v2;k;1;1;abc="), ("op", "BLLogin"));
        Assert.Equal("op=BLLogin", CanonicalRequestBuilder.CanonicalQuery(query));
    }

    [Fact]
    public void CanonicalQuery_EmptyValueKeepsEquals()
    {
        var query = Query(("op", ""), ("a", "1"));
        Assert.Equal("a=1&op=", CanonicalRequestBuilder.CanonicalQuery(query));
    }

    [Fact]
    public void CanonicalQuery_DuplicateKeysKeepValueOrder()
    {
        // 重复 key：key 排序、value 保留出现顺序（不二次排序打乱语义）
        var query = Query(("tag", "beta"), ("tag", "alpha"), ("op", "x"));
        Assert.Equal("op=x&tag=beta&tag=alpha", CanonicalRequestBuilder.CanonicalQuery(query));
    }

    [Fact]
    public void CanonicalQuery_EmptyCollection_ReturnsEmptyString()
    {
        Assert.Equal(string.Empty, CanonicalRequestBuilder.CanonicalQuery(Query()));
        Assert.Equal(string.Empty, CanonicalRequestBuilder.CanonicalQuery(new QueryCollection()));
    }

    [Fact]
    public void BuildV2Message_FieldsInSpecOrder()
    {
        var body = Encoding.UTF8.GetBytes("{\"hello\":\"world\"}");
        var message = CanonicalRequestBuilder.BuildV2Message("POST", "/cola", "op=BLLogin&userid=10086", "sk-a", 1755244800, "7", body);
        Assert.Equal("v2\nPOST\n/cola\nop=BLLogin&userid=10086\nsk-a\n1755244800\n7\n93a23971a914e5eacbf0a8d25154cda309c3c1c72fbb9914d47c60f3cb681588", message);
    }

    [Fact]
    public void BodyHashHex_EmptyBody_MatchesSha256Constant()
    {
        Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                     CanonicalRequestBuilder.BodyHashHex(Array.Empty<byte>()));
    }

    [Fact]
    public void HmacSha256_MatchesKnownVector()
    {
        // 公开已知向量：HMAC-SHA256(key="key", msg="The quick brown fox jumps over the lazy dog")
        var mac = CanonicalRequestBuilder.HmacSha256(Encoding.UTF8.GetBytes("key"), "The quick brown fox jumps over the lazy dog");
        Assert.Equal("f7bc83f430538424b13298e6aa6fb143ef4d59a14946175997479dbc2d1a3cd8", Convert.ToHexString(mac).ToLowerInvariant());
    }

    [Fact]
    public void HmacSha256Base64_MatchesKnownVector()
    {
        var base64 = CanonicalRequestBuilder.HmacSha256Base64(Encoding.UTF8.GetBytes("key"), "The quick brown fox jumps over the lazy dog");
        Assert.Equal("97yD9DBThCSxMpjmqm+xQ+9NWaFJRhdZl0edvC0aPNg=", base64);
    }
}