using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace GameFrameX.Client.Api.Services.GameLoginTicket;

/// <summary>
/// Game 登录票据签名 Key。
/// <para>HMAC fallback 随 C13 删除（P1 决策），生产签名唯一路径为租户应用级 RSA；本类不再保留 HMAC 构造工厂。</para>
/// </summary>
public sealed class GameLoginTicketSigningKey
{
    public const string AlgorithmRs256 = "RS256";

    private GameLoginTicketSigningKey(string kid, string algorithm, SigningCredentials signingCredentials, string publicKeyPem)
    {
        Kid = kid;
        Algorithm = algorithm;
        SigningCredentials = signingCredentials;
        PublicKeyPem = publicKeyPem;
    }

    /// <summary>
    /// Key ID。
    /// </summary>
    public string Kid { get; }

    /// <summary>
    /// 签名算法。
    /// </summary>
    public string Algorithm { get; }

    /// <summary>
    /// JWT 签名凭证。
    /// </summary>
    public SigningCredentials SigningCredentials { get; }

    /// <summary>
    /// PEM 格式公钥。
    /// </summary>
    public string PublicKeyPem { get; }

    public static GameLoginTicketSigningKey CreateRsa(string kid, string privateKeyPem, string publicKeyPem)
    {
        using (var rsa = RSA.Create())
        {
            rsa.ImportFromPem(privateKeyPem);
            var key = new RsaSecurityKey(rsa.ExportParameters(true))
            {
                KeyId = kid,
            };
            return new GameLoginTicketSigningKey(kid, AlgorithmRs256, new SigningCredentials(key, SecurityAlgorithms.RsaSha256), publicKeyPem);
        }
    }
}