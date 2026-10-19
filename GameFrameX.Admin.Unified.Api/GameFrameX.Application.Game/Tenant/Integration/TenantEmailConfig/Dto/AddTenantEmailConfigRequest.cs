namespace GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto;

/// <summary>
/// 租户业务邮件配置新增输入参数
/// </summary>
public class AddTenantEmailConfigRequest : TenantEmailConfigBaseRequest
{
    /// <summary>
    /// 邮件服务商 SecretId（敏感字段，入库明文，API 输出脱敏）。
    /// </summary>
    [Required(ErrorMessage = "SecretId不能为空")]
    public string FromEmailSecretId { get; set; } = null!;

    /// <summary>
    /// 邮件服务商 SecretKey（敏感字段，入库明文，API 输出脱敏）。
    /// </summary>
    [Required(ErrorMessage = "SecretKey不能为空")]
    public string FromEmailSecretKey { get; set; } = null!;
}