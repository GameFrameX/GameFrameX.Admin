namespace GameFrameX.Core.Service.PlatformPasswordPolicy.Dto;

/// <summary>
/// 平台密码策略配置输出（C173）。仅暴露密码强度策略字段，绝不回显 Cryptogram 密钥（CryptoType/PublicKey/PrivateKey）。
/// </summary>
public sealed class PlatformPasswordPolicyConfigResponse
{
    /// <summary>是否开启密码强度验证。</summary>
    public bool StrongPassword { get; set; } = false;

    /// <summary>密码强度验证正则表达式。</summary>
    public string PasswordStrengthValidation { get; set; } = PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidation;

    /// <summary>密码强度验证失败提示。</summary>
    public string PasswordStrengthValidationMsg { get; set; } = PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidationMsg;

    /// <summary>最近更新时间（Unix 秒）。</summary>
    public long? UpdateTime { get; set; }
}