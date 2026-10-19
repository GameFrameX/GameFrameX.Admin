using GameFrameX.Foundation.Encryption;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 租户库连接串加解密唯一收口：system_tenant.connection 落库前加密、建连前解密都必须经过本类，禁止调用方自行判断密文形态。
/// <para>密文格式为 <c>enc:v1:&lt;Base64&gt;</c>，底层复用 Foundation.Encryption 的 <see cref="AesHelper"/>（AES-GCM + PBKDF2）；
/// 密钥取环境变量 <see cref="KeyEnvironmentVariable"/>（GFX_TENANT_CONN_ENC_KEY）。</para>
/// <para>兼容语义：无 <c>enc:v1:</c> 前缀的存量值视为明文直通；未配置密钥时写入侧保持明文直存（降级模式），
/// 读取侧遇到带前缀密文则抛错提示补配密钥，绝不静默把密文当明文建连。</para>
/// </summary>
public static class TenantConnectionCrypto
{
    /// <summary>
    /// 密文前缀（v1 = AesHelper 当前负载格式版本）；变更前缀语义必须升版本号而非复用。
    /// </summary>
    public const string EncryptedPrefix = "enc:v1:";

    /// <summary>
    /// 加密密钥的环境变量名。三 host 与任意建库入口共用同一密钥，否则跨 host 解密失败。
    /// </summary>
    public const string KeyEnvironmentVariable = "GFX_TENANT_CONN_ENC_KEY";

    /// <summary>
    /// 加密租户库连接串用于落库。幂等：空值与已加密值原样返回；未配置密钥时明文直存（降级模式，保证无密钥部署可用）。
    /// </summary>
    /// <param name="plainConnection">明文连接串（可为 null，透传）</param>
    /// <returns><c>enc:v1:</c> 前缀密文，或降级模式下的原值</returns>
    public static string? Encrypt(string? plainConnection)
    {
        if (string.IsNullOrWhiteSpace(plainConnection) || IsEncrypted(plainConnection))
        {
            return plainConnection;
        }

        var key = Environment.GetEnvironmentVariable(KeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(key))
        {
            // 降级模式：无密钥时明文直存，与存量部署兼容；读取侧无前缀直通，行为一致。
            return plainConnection;
        }

        return EncryptedPrefix + AesHelper.Encrypt(plainConnection, key);
    }

    /// <summary>
    /// 解密落库的租户库连接串用于建连。无前缀明文直通（存量兼容）；带前缀密文在缺密钥或解密失败（密钥不一致/数据被篡改）时抛 <see cref="InvalidOperationException"/>。
    /// </summary>
    /// <param name="storedConnection">system_tenant.connection 存储值（可为 null，透传）</param>
    /// <returns>可建连的明文连接串</returns>
    public static string? Decrypt(string? storedConnection)
    {
        if (string.IsNullOrWhiteSpace(storedConnection) || !IsEncrypted(storedConnection))
        {
            return storedConnection;
        }

        var key = Environment.GetEnvironmentVariable(KeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException($"租户库连接串已加密（{EncryptedPrefix} 前缀），但环境变量 {KeyEnvironmentVariable} 未配置，无法解密。");
        }

        try
        {
            return AesHelper.Decrypt(storedConnection[EncryptedPrefix.Length..], key);
        }
        catch (Exception exception) when (exception is FormatException || exception is System.Security.Cryptography.CryptographicException || exception is ArgumentException)
        {
            // 不把密文片段带进异常，防止密文被日志二次扩散。
            throw new InvalidOperationException($"租户库连接串解密失败：{KeyEnvironmentVariable} 与加密时使用的密钥不一致，或存储值已损坏。");
        }
    }

    /// <summary>
    /// 判断存储值是否为密文形态（读取侧兼容分支与测试断言共用）。
    /// </summary>
    public static bool IsEncrypted(string? storedConnection)
    {
        return storedConnection != null && storedConnection.StartsWith(EncryptedPrefix, StringComparison.Ordinal);
    }
}
