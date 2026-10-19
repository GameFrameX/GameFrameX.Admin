using System.Security.Cryptography;

namespace GameFrameX.Core.SignatureAuth;

/// <summary>
/// 传输签名规范化与 HMAC 计算的单一公共工具（C267 全仓唯一加签/验签实现）。
/// <para>消息串格式（<see cref="SignatureSpec"/> 常量、微信支付 v3 风格逐字段 \n 分隔）：
/// <c>v2\n{method}\n{path}\n{canonicalQuery}\n{keyId}\n{ts}\n{mid}\n{hex(sha256(body))}</c>。</para>
/// <para>单一性约束（C267 守护测试锁定）：全仓禁止业务/信封代码直接 <c>new HMACSHA256(...)</c> / <c>new HMACSHA1(...)</c>，
/// 必须走 <see cref="HmacSha256(byte[], string)"/> / <see cref="HmacSha256Base64(byte[], string)"/>；
/// v2 message 拼装仅此一份。第三方协议实现（腾讯云 COS HMAC-SHA1）在 ObjectStorage 模块期外豁免。</para>
/// <para>标准来源：RFC 2104（HMAC）+ RFC 3986 §2.1/§3.4（percent-encoding + canonical query）
/// + AWS SigV4 canonical query 实践 + RFC 4648 §4（Base64）+ FIPS 180-4（SHA-256）。</para>
/// </summary>
public static class CanonicalRequestBuilder
{
    /// <summary>
    /// RFC 3986 §2.1 percent-encoding：保留字仅 ALPHA / DIGIT / "-" / "." / "_" / "~"（§2.3 unreserved），
    /// 其余（含 §2.2 reserved、空格与非 ASCII）一律 %XX 大写十六进制。
    /// <para><see cref="Uri.EscapeDataString"/> 是 UTF-8 严格实现，行为符合本规格；
    /// 禁止换成 <c>HttpUtility.UrlEncode</c>（表单风格 + 编码空格为 "+"）或 <c>Uri.EscapeUriString</c>（保留 ? # 等，不符合 §3.4）。</para>
    /// </summary>
    /// <param name="s">待编码字符串。</param>
    /// <returns>percent-encoded 字符串；入参 null 视为空串。</returns>
    public static string PercentEncode(string s)
    {
        return Uri.EscapeDataString(s ?? string.Empty);
    }

    /// <summary>
    /// canonical query（RFC 3986 §3.4 + AWS SigV4 canonical query 实践）：
    /// 1) 剔除 <c>Signature</c> 参数（防把签名本身拼进待签串造成循环依赖）；
    /// 2) 按 key 字典序（Ordinal）排列；
    /// 3) 空 value 也要拼 <c>key=</c>；
    /// 4) key / value 各做 <see cref="PercentEncode"/>；
    /// 5) 用 "&amp;" 拼接；重复 key 的多个 value 保留出现顺序（稳定排序）。
    /// <para>解决 C267 前的 ponytail：query 按原始顺序进签，前置代理改写参数顺序/编码即破签。</para>
    /// </summary>
    /// <param name="query">请求 query 集合。</param>
    /// <returns>canonical query 串（无 "?" 前缀）；空集合返回空串。</returns>
    public static string CanonicalQuery(IQueryCollection query)
    {
        if (query == null || query.Count == 0)
        {
            return string.Empty;
        }

        var pairs = new List<EncodedQueryPair>();
        foreach (var item in query)
        {
            if (string.Equals(item.Key, SignatureSpec.SignatureHeader, StringComparison.Ordinal))
            {
                continue;
            }

            var encodedKey = PercentEncode(item.Key);
            foreach (var value in item.Value)
            {
                pairs.Add(new EncodedQueryPair(encodedKey, encodedKey + "=" + PercentEncode(value ?? string.Empty)));
            }
        }

        // Enumerable.OrderBy 为稳定排序：同 key 的重复 value 保留出现顺序
        return string.Join("&", pairs.OrderBy(p => p.EncodedKey, StringComparer.Ordinal).Select(p => p.Encoded));
    }

    /// <summary>
    /// body 摘要：SHA-256 → hex 小写（FIPS 180-4 + RFC 4648 §8 hex 风格）。空 body 得常量
    /// e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855。
    /// </summary>
    /// <param name="body">请求 body 字节。</param>
    /// <returns>64 字符 hex 小写摘要。</returns>
    public static string BodyHashHex(byte[] body)
    {
        return Convert.ToHexString(SHA256.HashData(body)).ToLowerInvariant();
    }

    /// <summary>
    /// v2 message 串（全仓唯一拼装实现），与客户端 NetLib.ApiServerHttpRequest 逐字节对齐。
    /// <para>顺序：v2 \n {method} \n {path} \n {canonicalQuery} \n {keyId} \n {ts} \n {mid} \n {hex(sha256(body))}。</para>
    /// <para>keyId 进 message 防「签名挪用」降级攻击；mid 段为字符串段位
    /// （Client.Api 场景为单调递增消息 ID，Hub OpenAccess 场景为 nonce 随机串）。</para>
    /// </summary>
    /// <param name="method">HTTP 方法（大写）。</param>
    /// <param name="path">URL 路径（TrimEnd('/') 后）。</param>
    /// <param name="canonicalQuery"><see cref="CanonicalQuery"/> 产物（无 "?" 前缀）。</param>
    /// <param name="keyId">密钥身份（Client.Api 为密钥环 Kid；Hub OpenAccess 为 accessKey）。</param>
    /// <param name="ts">Unix 秒时间戳。</param>
    /// <param name="mid">消息段位（消息 ID 或 nonce）。</param>
    /// <param name="body">body 字节。</param>
    /// <returns>待签名 message 串。</returns>
    public static string BuildV2Message(string method, string path, string canonicalQuery, string keyId, long ts, string mid, byte[] body)
    {
        return string.Join(SignatureSpec.FieldSeparator,
                           SignatureSpec.ProtocolPrefix,
                           method,
                           path,
                           canonicalQuery,
                           keyId,
                           ts.ToString(),
                           mid,
                           BodyHashHex(body));
    }

    /// <summary>
    /// HMAC-SHA256（RFC 2104）字节计算 —— 全仓唯一 HMAC 实现。
    /// <para>业务/信封代码禁止直接 <c>new HMACSHA256(...)</c> / <c>new HMACSHA1(...)</c>（C267 守护测试锁定）。</para>
    /// </summary>
    /// <param name="secret">密钥字节。</param>
    /// <param name="message">待签消息串（UTF-8 编码后参与计算）。</param>
    /// <returns>32 字节 MAC。</returns>
    public static byte[] HmacSha256(byte[] secret, string message)
    {
        using (var hmac = new HMACSHA256(secret))
        {
            return hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
        }
    }

    /// <summary>
    /// Base64(HMAC-SHA256)（RFC 2104 + RFC 4648 §4）—— envelope/信封侧与传输签名侧共用的唯一出口，
    /// 取代历史上散落在各旧 helper / handler 中的多份 HMAC 实现。
    /// </summary>
    /// <param name="secret">密钥字节。</param>
    /// <param name="message">待签消息串。</param>
    /// <returns>Base64 编码签名。</returns>
    public static string HmacSha256Base64(byte[] secret, string message)
    {
        return Convert.ToBase64String(HmacSha256(secret, message));
    }

    /// <summary>
    /// canonical query 排序中间结构（encoded key 供 Ordinal 排序、encoded pair 供拼接）。
    /// </summary>
    private sealed class EncodedQueryPair
    {
        public EncodedQueryPair(string encodedKey, string encoded)
        {
            EncodedKey = encodedKey;
            Encoded = encoded;
        }

        public string EncodedKey { get; }

        public string Encoded { get; }
    }
}