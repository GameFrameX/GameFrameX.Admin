using System.Collections.Concurrent;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Payment.Apple.AppStoreServer;

/// <summary>
/// Apple App Store Server JWS 签名验证器
/// 从 JWS header 提取 x5c 证书链，校验到 Apple Root CA，验证 ECDSA 签名
/// </summary>
public static class AppleJwsVerifier
{
    /// <summary>
    /// 已解析的 Root CA 证书缓存，key 为原始证书内容字符串
    /// </summary>
    private static readonly ConcurrentDictionary<string, X509Certificate2> _rootCaCache = new();

    /// <summary>
    /// 验证 Apple JWS 签名并返回解码后的 payload JSON 字符串（接受 PEM/Base64 DER 格式的 Root CA）
    /// </summary>
    /// <param name="jws">JWS 字符串（header.payload.signature）</param>
    /// <param name="rootCaContent">Apple Root CA 证书内容（PEM 格式或 Base64 编码 DER）</param>
    /// <returns>解码后的 payload JSON 字符串</returns>
    /// <exception cref="SecurityException">签名验证失败时抛出</exception>
    public static string VerifyAndDecode(string jws, string rootCaContent)
    {
        if (string.IsNullOrWhiteSpace(rootCaContent))
        {
            throw new SecurityException("Apple Root CA certificate content is empty");
        }

        var rootCa = _rootCaCache.GetOrAdd(rootCaContent, LoadCertificate);
        return VerifyAndDecode(jws, rootCa);
    }

    /// <summary>
    /// 验证 Apple JWS 签名并返回解码后的 payload JSON 字符串
    /// </summary>
    /// <param name="jws">JWS 字符串（header.payload.signature）</param>
    /// <param name="rootCa">受信任的 Apple Root CA 证书</param>
    /// <returns>解码后的 payload JSON 字符串</returns>
    /// <exception cref="SecurityException">签名验证失败时抛出</exception>
    public static string VerifyAndDecode(string jws, X509Certificate2 rootCa)
    {
        if (string.IsNullOrWhiteSpace(jws))
        {
            throw new SecurityException("JWS is null or empty");
        }

        var parts = jws.Split('.');
        if (parts.Length != 3)
        {
            throw new SecurityException("Invalid JWS format: expected 3 parts");
        }

        // 1. 解码 header，提取 x5c 证书链
        string headerJson;
        try
        {
            headerJson = Base64UrlEncoder.Decode(parts[0]);
        }
        catch (Exception ex)
        {
            throw new SecurityException("Failed to decode JWS header", ex);
        }

        JwsHeader? header;
        try
        {
            header = JsonSerializer.Deserialize<JwsHeader>(headerJson);
        }
        catch (Exception ex)
        {
            throw new SecurityException("Failed to parse JWS header JSON", ex);
        }

        if (header?.X5c == null || header.X5c.Count == 0)
        {
            throw new SecurityException("No x5c certificate chain in JWS header");
        }

        if (!string.IsNullOrEmpty(header.Alg) && header.Alg != "ES256")
        {
            throw new SecurityException($"Unexpected JWS algorithm: {header.Alg}, expected ES256");
        }

        // 2. 从 x5c 加载证书
        List<X509Certificate2> certificates;
        try
        {
            certificates = header.X5c
                                 .Select(b64 => new X509Certificate2(Convert.FromBase64String(b64)))
                                 .ToList();
        }
        catch (Exception ex)
        {
            throw new SecurityException("Failed to parse x5c certificates", ex);
        }

        var leafCert = certificates[0];

        // 3. 校验证书链是否信任到 Apple Root CA
        try
        {
            using (var chain = new X509Chain())
            {
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(rootCa);

                // 中间证书放入 ExtraStore
                for (var i = 1; i < certificates.Count; i++)
                {
                    chain.ChainPolicy.ExtraStore.Add(certificates[i]);
                }

                if (!chain.Build(leafCert))
                {
                    var errors = string.Join("; ", chain.ChainStatus.Select(s => s.StatusInformation));
                    throw new SecurityException($"Certificate chain validation failed: {errors}");
                }
            }
        }
        finally
        {
            // 清理临时证书（leafCert 在签名验证后清理）
            for (var i = 1; i < certificates.Count; i++)
            {
                certificates[i].Dispose();
            }
        }

        // 4. 使用叶子证书公钥验证 JWS 签名
        try
        {
            var ecdsa = leafCert.GetECDsaPublicKey();
            if (ecdsa == null)
            {
                throw new SecurityException("Leaf certificate does not contain an ECDSA public key");
            }

            var securityKey = new ECDsaSecurityKey(ecdsa);
            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = securityKey,
            };

            var handler = new JsonWebTokenHandler();
            var result = handler.ValidateToken(jws, parameters);

            if (!result.IsValid)
            {
                throw new SecurityException("JWS signature verification failed", result.Exception);
            }
        }
        finally
        {
            leafCert.Dispose();
        }

        // 5. 返回解码后的 payload
        return Base64UrlEncoder.Decode(parts[1]);
    }

    /// <summary>
    /// 自动识别格式并加载证书（PEM 或 Base64 DER）
    /// </summary>
    private static X509Certificate2 LoadCertificate(string content)
    {
        var trimmed = content.Trim();

        if (trimmed.Contains("BEGIN CERTIFICATE"))
        {
            return X509Certificate2.CreateFromPem(trimmed);
        }

        return X509CertificateLoader.LoadCertificate(Convert.FromBase64String(trimmed));
    }

    private class JwsHeader
    {
        [JsonPropertyName("alg")] public string Alg { get; set; } = null!;

        [JsonPropertyName("x5c")] public List<string> X5c { get; set; } = null!;
    }
}