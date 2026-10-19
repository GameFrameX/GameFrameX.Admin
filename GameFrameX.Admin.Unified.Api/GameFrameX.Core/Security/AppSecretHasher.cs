using System.Security.Cryptography;

namespace GameFrameX.Core.Security;

/// <summary>
/// 应用密钥哈希工具：PBKDF2-SHA256 + 随机 salt，常量时间比对。
/// <para>存储格式：<c>PBKDF2-SHA256.{iterations}.{base64(salt)}.{base64(hash)}</c></para>
/// <para>用于替代 TenantGameAppEntity.AppSecret 的明文存储与明文比对。</para>
/// <para>共享于 Admin（写库）与 Client API（校验）两侧，故置于 GameFrameX.Core。</para>
/// </summary>
public static class AppSecretHasher
{
    /// <summary>
    /// 默认算法标识
    /// </summary>
    public const string Algorithm = "PBKDF2-SHA256";

    private const int SaltBytes = 16;
    private const int HashBytes = 32;
    private const int Iterations = 100_000;

    /// <summary>
    /// 对明文密钥计算哈希，返回自解析格式字符串
    /// </summary>
    /// <param name="secret">明文密钥</param>
    /// <returns>存储格式哈希字符串</returns>
    public static string Hash(string secret)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltBytes);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(secret ?? string.Empty),
            salt,
            Iterations,
            HashAlgorithmName.SHA256,
            HashBytes);

        return string.Join('.', Algorithm, Iterations, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
    }

    /// <summary>
    /// 用常量时间比对验证明文密钥是否匹配存储哈希
    /// </summary>
    /// <param name="secret">待验证明文密钥</param>
    /// <param name="storedHash">存储的哈希字符串</param>
    /// <returns>匹配返回 true；格式非法或不匹配返回 false</returns>
    public static bool Verify(string secret, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('.');
        if (parts.Length != 4 || parts[0] != Algorithm)
        {
            return false;
        }

        if (!int.TryParse(parts[1], out var iterations) || iterations <= 0)
        {
            return false;
        }

        byte[] salt;
        byte[] expected;
        try
        {
            salt = Convert.FromBase64String(parts[2]);
            expected = Convert.FromBase64String(parts[3]);
        }
        catch
        {
            return false;
        }

        var actual = Rfc2898DeriveBytes.Pbkdf2(
            Encoding.UTF8.GetBytes(secret ?? string.Empty),
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}