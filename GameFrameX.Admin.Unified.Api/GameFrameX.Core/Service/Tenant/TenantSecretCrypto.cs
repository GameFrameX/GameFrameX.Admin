using System.Collections.Concurrent;
using GameFrameX.Core.Service.Tenant.ObjectStorage;
using GameFrameX.Foundation.Encryption;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 租户业务凭证静态加密唯一收口（C370 / GFX-829）：除连接串外的可逆凭证（邮件 Provider 凭据、开放接口 AccessSecret、
/// 对象存储 ConfigJson 敏感字段等）落库前加密、使用前解密都必须经过本类，禁止调用方自行判断密文形态。
/// <para>密文格式为 <c>enc:sec:v1:&lt;Base64&gt;</c>，底层复用 Foundation.Encryption 的 <see cref="AesHelper"/>（AES-GCM + PBKDF2 600,000 迭代派生）；
/// 密钥取环境变量 <see cref="KeyEnvironmentVariable"/>（GFX_TENANT_SECRET_ENC_KEY），与连接串密钥 <c>GFX_TENANT_CONN_ENC_KEY</c> 相互独立。</para>
/// <para>前缀刻意区别于 <see cref="TenantConnectionCrypto"/> 的 <c>enc:v1:</c>：两类密文互不认领，防止连接串密文被凭证收口误当明文透传（反之亦然）。</para>
/// <para>兼容语义：无 <c>enc:sec:v1:</c> 前缀的存量值视为明文直通；未配置密钥时写入侧保持明文直存（降级模式，首次打一条告警），
/// 读取侧遇到带前缀密文则抛错提示补配密钥，绝不静默把密文当明文使用（fail-closed）。</para>
/// <para>性能约束：PBKDF2 派生开销大（单次数百毫秒级），加解密只允许发生在「配置加载」边界（进程级缓存 / 单次加载语义），
/// 禁止每请求解密；对象存储 ConfigJson 的字段级收口与缓存见 <see cref="TenantObjectStorageConfigCrypto"/>。</para>
/// </summary>
public static class TenantSecretCrypto
{
    /// <summary>
    /// 密文前缀（sec = 业务凭证域；v1 = AesHelper 当前负载格式版本）。变更前缀语义必须升版本号而非复用。
    /// </summary>
    public const string EncryptedPrefix = "enc:sec:v1:";

    /// <summary>
    /// 加密密钥的环境变量名。三 host 与回填执行器共用同一密钥，否则跨 host 解密失败。
    /// </summary>
    public const string KeyEnvironmentVariable = "GFX_TENANT_SECRET_ENC_KEY";

    /// <summary>
    /// 降级模式（未配置密钥）告警只打一次的哨兵：0 = 未告警，1 = 已告警。
    /// </summary>
    private static int _degradedModeWarned;

    /// <summary>
    /// 加密业务凭证明文用于落库。幂等：空值与已加密值（含连接串域的 <c>enc:v1:</c> 值，视为明文原样返回）原样透传；
    /// 未配置密钥时明文直存（降级模式，保证无密钥部署可用，进程生命周期内告警一次）。
    /// </summary>
    /// <param name="plainSecret">明文凭证（可为 null，透传）</param>
    /// <returns><c>enc:sec:v1:</c> 前缀密文，或降级模式下的原值</returns>
    public static string? Encrypt(string? plainSecret)
    {
        if (string.IsNullOrWhiteSpace(plainSecret) || IsEncrypted(plainSecret))
        {
            return plainSecret;
        }

        var key = Environment.GetEnvironmentVariable(KeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(key))
        {
            // 降级模式：无密钥时明文直存，与存量部署兼容；读取侧无前缀直通，行为一致。
            if (Interlocked.CompareExchange(ref _degradedModeWarned, 1, 0) == 0)
            {
                Console.WriteLine($"[TenantSecretCrypto] 警告：环境变量 {KeyEnvironmentVariable} 未配置，业务凭证以明文落库（降级模式）。配置后新写入即密文，存量明文由回填执行器加密。");
            }

            return plainSecret;
        }

        return EncryptedPrefix + AesHelper.Encrypt(plainSecret, key);
    }

    /// <summary>
    /// 解密落库的业务凭证用于构建外部调用。无前缀明文直通（存量兼容，含连接串域 <c>enc:v1:</c> 值同样透传由调用方自查）；
    /// 带前缀密文在缺密钥或解密失败（密钥不一致 / 数据被篡改）时抛 <see cref="InvalidOperationException"/>，错误文案指向环境变量，不回显任何密文片段。
    /// </summary>
    /// <param name="storedSecret">凭证列存储值（可为 null，透传）</param>
    /// <returns>可用的明文凭证</returns>
    public static string? Decrypt(string? storedSecret)
    {
        if (string.IsNullOrWhiteSpace(storedSecret) || !IsEncrypted(storedSecret))
        {
            return storedSecret;
        }

        var key = Environment.GetEnvironmentVariable(KeyEnvironmentVariable);
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException($"业务凭证已加密（{EncryptedPrefix} 前缀），但环境变量 {KeyEnvironmentVariable} 未配置，无法解密。");
        }

        try
        {
            return AesHelper.Decrypt(storedSecret[EncryptedPrefix.Length..], key);
        }
        catch (Exception exception) when (exception is FormatException || exception is System.Security.Cryptography.CryptographicException || exception is ArgumentException)
        {
            // 不把密文片段带进异常，防止密文被日志二次扩散。
            throw new InvalidOperationException($"业务凭证解密失败：{KeyEnvironmentVariable} 与加密时使用的密钥不一致，或存储值已损坏。");
        }
    }

    /// <summary>
    /// 进程级「密文 → 明文」缓存（C372 / GFX-838）：PBKDF2 单次数百毫秒，无独立缓存的运行时读取点
    /// （开放访问中间件、票据签发、邮件快照等每请求读取）经本缓存把派生摊销到「每 distinct 密文一次」。
    /// 与 C370/C371 JSON 加密器的 DecryptCache 同款模式。
    /// <para>ponytail: 缓存无上限，容量受库内 distinct 密文数约束（密文含随机盐，每行每密文唯一）；
    /// 密钥轮换（更换环境变量值）后旧明文残留缓存，须重启进程生效；GetOrAdd 抛出（fail-closed）不落缓存。</para>
    /// </summary>
    private static readonly ConcurrentDictionary<string, string> DecryptCache = new(StringComparer.Ordinal);

    /// <summary>
    /// 带进程级缓存的 <see cref="Decrypt"/>：同一密文全进程只做一次 PBKDF2 派生，后续调用零派生直取明文。
    /// 语义与 <see cref="Decrypt"/> 完全一致（无前缀明文直通、密文缺密钥抛 <see cref="InvalidOperationException"/>），
    /// 供运行时高频读取点使用；一次性 / 低频路径直接用 <see cref="Decrypt"/> 即可。
    /// </summary>
    /// <param name="storedSecret">凭证列存储值（可为 null，透传）</param>
    /// <returns>可用的明文凭证</returns>
    public static string? DecryptCached(string? storedSecret)
    {
        if (string.IsNullOrWhiteSpace(storedSecret) || !IsEncrypted(storedSecret))
        {
            return storedSecret;
        }

        return DecryptCache.GetOrAdd(storedSecret, static cipher => Decrypt(cipher) ?? string.Empty);
    }

    /// <summary>
    /// 判断存储值是否为业务凭证密文形态（读取侧兼容分支与测试断言共用）。
    /// </summary>
    public static bool IsEncrypted(string? storedSecret)
    {
        return storedSecret != null && storedSecret.StartsWith(EncryptedPrefix, StringComparison.Ordinal);
    }

    /// <summary>
    /// 判断加密密钥是否已配置（回填执行器与部署自检用：未配置时回填空转，直接跳过）。
    /// </summary>
    internal static bool IsKeyConfigured()
    {
        return !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(KeyEnvironmentVariable));
    }
}
