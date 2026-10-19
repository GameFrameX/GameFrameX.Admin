namespace GameFrameX.Core.Service.PlatformPasswordPolicy;

/// <summary>
/// 平台密码策略配置常量（C173）。承载 SystemConfigEntity 固定记录编码与历史兼容默认值。
/// </summary>
public static class PlatformPasswordPolicyConfigConstants
{
    /// <summary>
    /// SystemConfigEntity 中保存平台密码策略的固定编码。通用配置页禁止维护，仅由 SuperAdmin 专用 API 读写。
    /// </summary>
    public const string ConfigCode = "platform_password_policy_config";

    /// <summary>
    /// 序列化后配置最大长度，与 SystemConfigEntity.Value 列长度（2048）保持同余量。
    /// </summary>
    public const int MaxSerializedLength = 2048;

    /// <summary>
    /// SystemConfigEntity 固定记录排序号（接 captcha=20 / rateLimit=30 / logging=40 之后）。
    /// </summary>
    public const int OrderNo = 50;

    /// <summary>
    /// SystemConfigEntity 固定记录分组（与其它平台私有配置同属 Security）。
    /// </summary>
    public const string GroupCode = "Security";

    /// <summary>
    /// SystemConfigEntity 固定记录名称。
    /// </summary>
    public const string ConfigName = "平台密码策略配置";

    /// <summary>
    /// SystemConfigEntity 固定记录备注。
    /// </summary>
    public const string ConfigRemark = "平台私有密码强度策略运行时配置";

    /// <summary>
    /// 默认密码强度正则。StrongPassword 开启时用于校验新密码：须包含数字、小写、大写、特殊字符，长度 6-16。
    /// 注：历史 Cryptogram:PasswordStrengthValidation 值的字符类未闭合（\] 被转义为字面 ]），
    /// 因 StrongPassword 默认 false 从未执行而未暴露；此处修正为合法闭合，特殊字符集保持原注释意图。
    /// </summary>
    public const string DefaultPasswordStrengthValidation = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[~@#$%*\-+=:,?\\\[\]\{\}]).{6,16}$";

    /// <summary>
    /// 默认密码强度提示，等价历史 Cryptogram 密码强度提示值。校验失败时回显。
    /// </summary>
    public const string DefaultPasswordStrengthValidationMsg = "密码必须包含大小写字母、数字和特殊字符的组合，长度在6-16之间";
}