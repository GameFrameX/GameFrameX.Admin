namespace GameFrameX.Core.Service.PlatformPasswordPolicy;

/// <summary>
/// 平台密码策略私有配置载荷（C173）。字段严格等于 strongPassword / passwordStrengthValidation /
/// passwordStrengthValidationMsg 三项，禁止未列字段。默认值等价历史 Cryptogram 密码策略值。
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlatformPasswordPolicySettings
{
    /// <summary>
    /// 是否开启密码强度验证。默认关闭，等价历史 StrongPassword 默认值（关闭）。
    /// 保存后同 Host 即时生效（TenantUserService.ChangePwdAsync 读 CryptogramUtility 镜像）。
    /// </summary>
    public bool StrongPassword { get; set; } = false;

    /// <summary>
    /// 密码强度验证正则表达式。StrongPassword=true 时用于校验新密码。默认等价历史密码强度正则。
    /// </summary>
    public string PasswordStrengthValidation { get; set; } = PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidation;

    /// <summary>
    /// 密码强度验证失败提示。校验不通过时回显。默认等价历史密码强度提示。
    /// </summary>
    public string PasswordStrengthValidationMsg { get; set; } = PlatformPasswordPolicyConfigConstants.DefaultPasswordStrengthValidationMsg;
}