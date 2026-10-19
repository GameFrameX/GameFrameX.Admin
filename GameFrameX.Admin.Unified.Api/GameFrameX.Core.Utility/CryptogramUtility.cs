using GameFrameX.Foundation.Hash;

namespace GameFrameX.Core.Utility;

/// <summary>
/// 密码哈希与密码强度策略静态工具。密码存储统一 Argon2id（Foundation <c>PasswordHashHelper</c>），不可逆；
/// 不再保留可逆的 SM2/SM4 与无盐 MD5 路径（C214）。
/// </summary>
public static class CryptogramUtility
{
    public static bool StrongPassword { get; private set; } // 是否开启密码强度验证
    public static string PasswordStrengthValidation { get; private set; } = string.Empty; // 密码强度验证正则表达式
    public static string PasswordStrengthValidationMsg { get; private set; } = string.Empty; // 密码强度验证提示

    /// <summary>
    /// 应用平台密码策略快照到静态属性（C173）。由 <c>PlatformPasswordPolicyConfigProvider.RefreshAsync</c>
    /// 在启动期与 SuperAdmin 保存后调用，使 <c>TenantUserService.ChangePwd</c> 读取的密码策略静态属性
    /// 始终反映 SystemConfigEntity 最新快照，实现同 Host 动态生效。
    /// </summary>
    /// <param name="strongPassword">是否开启密码强度验证</param>
    /// <param name="passwordStrengthValidation">密码强度验证正则表达式</param>
    /// <param name="passwordStrengthValidationMsg">密码强度验证失败提示</param>
    public static void ApplyPasswordPolicy(bool strongPassword, string? passwordStrengthValidation, string? passwordStrengthValidationMsg)
    {
        StrongPassword = strongPassword;
        PasswordStrengthValidation = passwordStrengthValidation ?? string.Empty;
        PasswordStrengthValidationMsg = passwordStrengthValidationMsg ?? string.Empty;
    }

    /// <summary>
    /// 账号枚举防护诱饵哈希（C216）：登录路径账号不存在时，用它跑一次假 <see cref="VerifyPassword"/>，
    /// 使响应耗时与"密码错误"路径对齐（都是单次 Argon2id 验证），配合统一错误响应避免泄露账号是否存在。
    /// 进程级静态生成一次，不含任何真实账号信息。
    /// </summary>
    public static readonly string EnumerationDecoyHash = PasswordHashHelper.Hash(PasswordHashAlgorithmKind.Argon2id, "enumeration-decoy");

    /// <summary>
    /// 密码哈希（Argon2id）。返回 Foundation <c>PasswordHashHelper</c> 的 PHC 自描述串（<c>$argon2id$</c> 前缀，
    /// 含随机盐 + OWASP 推荐成本参数），不可逆。系统未上线、无存量密码，直接全量 Argon2id，不认历史 MD5/SM2/SM4 密文。
    /// </summary>
    /// <param name="plaintext">明文密码。</param>
    /// <returns>Argon2id PHC 字符串。</returns>
    public static string HashPassword(string plaintext)
    {
        return PasswordHashHelper.Hash(PasswordHashAlgorithmKind.Argon2id, plaintext);
    }

    /// <summary>
    /// 密码校验。按 <paramref name="stored"/> 的 PHC 前缀自动识别算法；历史 MD5/SM2/SM4 密文格式未知，一律返回 false。
    /// 调用方对用户不存在场景应先跑一次假比对以抵御账号枚举。
    /// </summary>
    /// <param name="input">待校验的明文密码。</param>
    /// <param name="stored">已存储的 PHC 字符串。</param>
    /// <returns>匹配返回 true；密码错误或 stored 格式非法返回 false。</returns>
    public static bool VerifyPassword(string input, string stored)
    {
        return PasswordHashHelper.Verify(input, stored);
    }
}