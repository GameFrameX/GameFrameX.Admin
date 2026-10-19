using GameFrameX.Foundation.Hash;

namespace GameFrameX.Application.Game.User.GameUser;

/// <summary>
/// 玩家账号密码哈希服务：纯 Argon2id（每用户随机盐 + OWASP 推荐成本参数，PHC 自描述格式）。
/// <para>复用 Foundation <see cref="PasswordHashHelper"/>；玩家 Password / EmailPassword 列一律以 <c>$argon2id$</c> PHC 串存储。</para>
/// <para>系统未上线、无存量玩家数据，故不留 PBKDF2/MD5 fallback、不做懒升级、不做兼容窗口（C198 → C215 迁移）。</para>
/// <para>服务本身无状态；登记为 Scoped（Admin / Hub 侧经 AddProjectDependencyInjection 的 ScopedTypes 白名单，
/// Client.Api 侧手动 AddScoped），两侧注入同一实现。</para>
/// <para>Hash/Verify 入口对 null 明文统一按空串处理（兼容 C198 既有测试矩阵 + 防止上层漏校验时 Foundation 抛
/// ArgumentNullException）；null storedHash 在 Verify 中直接返回 false（Foundation Verify 对 null 抛）。</para>
/// </summary>
public class TenantGameUserPasswordService
{
    /// <summary>
    /// 账号枚举防护诱饵哈希：登录路径账号不存在时，用它跑一次 <see cref="Verify"/>，
    /// 使响应耗时与"密码错误"路径一致（都是单次 Argon2id 验证），配合统一错误文案避免泄露账号是否存在。
    /// <para>进程级静态生成一次，不含任何真实账号信息。</para>
    /// </summary>
    public static readonly string EnumerationDecoyHash = PasswordHashHelper.Hash(PasswordHashAlgorithmKind.Argon2id, "enumeration-decoy");

    /// <summary>
    /// 对玩家明文密码计算 Argon2id 哈希，返回 PHC 自描述字符串（<c>$argon2id$v=...$salt$hash</c>）。
    /// </summary>
    /// <param name="plaintext">玩家明文密码；null 按空串处理</param>
    /// <returns>可入库存储的哈希字符串；每次调用因随机盐而不同</returns>
    public string Hash(string plaintext)
    {
        return PasswordHashHelper.Hash(PasswordHashAlgorithmKind.Argon2id, plaintext ?? string.Empty);
    }

    /// <summary>
    /// 常量时间比对待验证明文密码是否匹配存储哈希。
    /// <para>纯 Argon2id 校验：stored 非 <c>$argon2id$</c> PHC 前缀（含历史 PBKDF2/MD5 字符串、空、null）一律返回 false，不认旧格式。</para>
    /// </summary>
    /// <param name="plaintext">待验证的明文密码；null 按空串处理</param>
    /// <param name="storedHash">存储的 PHC 哈希字符串；null 直接返回 false</param>
    /// <returns>格式合法且匹配返回 true；格式非法或不匹配返回 false</returns>
    public bool Verify(string plaintext, string storedHash)
    {
        if (storedHash == null)
        {
            return false;
        }

        return PasswordHashHelper.Verify(plaintext ?? string.Empty, storedHash);
    }
}