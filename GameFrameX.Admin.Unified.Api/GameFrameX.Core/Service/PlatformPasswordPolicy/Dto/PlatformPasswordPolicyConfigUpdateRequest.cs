namespace GameFrameX.Core.Service.PlatformPasswordPolicy.Dto;

/// <summary>
/// 平台密码策略配置更新输入（SuperAdmin-only）。
/// </summary>
public sealed class PlatformPasswordPolicyConfigUpdateRequest
{
    /// <summary>是否开启密码强度验证。</summary>
    public bool StrongPassword { get; set; } = false;

    /// <summary>密码强度验证正则表达式（必须可编译）。</summary>
    [Range(1, 1024)]
    public string PasswordStrengthValidation { get; set; } = PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidation;

    /// <summary>密码强度验证失败提示。</summary>
    [Range(1, 1024)]
    public string PasswordStrengthValidationMsg { get; set; } = PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidationMsg;
}