using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using GameFrameX.Foundation.Json;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Core.Tests.Payment;

/// <summary>
/// Apple JWS 测试材料：自建 Root CA + 叶子证书链 + ES256 JWS 签名（C271 Server API 验证路径测试用）。
/// <para>与 ClientApi 嵌套实现同源逻辑，独立为共享文件供支付域测试复用。</para>
/// </summary>
public sealed class AppleJwsTestMaterial : IDisposable
{
    private readonly ECDsa _leafKey;
    private readonly X509Certificate2 _leafCertificate;
    private readonly X509Certificate2 _rootCertificate;

    private AppleJwsTestMaterial(ECDsa leafKey, X509Certificate2 leafCertificate, X509Certificate2 rootCertificate)
    {
        _leafKey = leafKey;
        _leafCertificate = leafCertificate;
        _rootCertificate = rootCertificate;
        RootCertificatePem = rootCertificate.ExportCertificatePem();
    }

    /// <summary>
    /// Root CA 证书 PEM（作为被测配置的 ApplePaymentRootCertificate）
    /// </summary>
    public string RootCertificatePem { get; }

    /// <summary>
    /// 自建证书链并生成签名材料（root 密钥仅签发期存活；leaf 私钥所有权转移给本实例，随 Dispose 释放）
    /// </summary>
    public static AppleJwsTestMaterial Create()
    {
        var now = DateTimeOffset.UtcNow;
        using (var rootKey = ECDsa.Create(ECCurve.NamedCurves.nistP256))
        {
            var rootRequest = new CertificateRequest("CN=C271 Apple Root", rootKey, HashAlgorithmName.SHA256);
            rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
            rootRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.KeyCertSign | X509KeyUsageFlags.DigitalSignature, true));
            using (var rootWithKey = rootRequest.CreateSelfSigned(now.AddDays(-1), now.AddDays(1)))
            {
                var rootCertificate = X509CertificateLoader.LoadCertificate(rootWithKey.Export(X509ContentType.Cert));

                // leaf 私钥不随本方法释放：所有权转移给 AppleJwsTestMaterial（Sign 持续使用，Dispose 统一回收）
                var leafKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
                var leafRequest = new CertificateRequest("CN=C271 Apple Leaf", leafKey, HashAlgorithmName.SHA256);
                leafRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
                leafRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));
                var serial = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                var leafCertificate = leafRequest.Create(rootWithKey, now.AddDays(-1), now.AddDays(1), serial);

                return new AppleJwsTestMaterial(leafKey, leafCertificate, rootCertificate);
            }
        }
    }

    /// <summary>
    /// 用叶子证书私钥对 payload 签发合法 ES256 JWS（x5c 携带完整证书链，可通过 AppleJwsVerifier 验签）
    /// </summary>
    public string Sign<T>(T payload)
    {
        var headerJson = JsonSerializer.Serialize(new
        {
            alg = "ES256",
            x5c = new[]
            {
                Convert.ToBase64String(_leafCertificate.RawData),
                Convert.ToBase64String(_rootCertificate.RawData),
            },
        });
        var payloadJson = JsonHelper.Serialize(payload);
        var signingInput = Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(headerJson)) + "." + Base64UrlEncoder.Encode(Encoding.UTF8.GetBytes(payloadJson));
        var signature = _leafKey.SignData(Encoding.UTF8.GetBytes(signingInput), HashAlgorithmName.SHA256, DSASignatureFormat.IeeeP1363FixedFieldConcatenation);
        return signingInput + "." + Base64UrlEncoder.Encode(signature);
    }

    /// <summary>
    /// 释放证书与密钥资源
    /// </summary>
    public void Dispose()
    {
        _leafCertificate.Dispose();
        _rootCertificate.Dispose();
        _leafKey.Dispose();
    }
}