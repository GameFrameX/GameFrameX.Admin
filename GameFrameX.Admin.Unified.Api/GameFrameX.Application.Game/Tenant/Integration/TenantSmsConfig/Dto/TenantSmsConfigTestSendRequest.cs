namespace GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig.Dto;

/// <summary>
/// 租户 SMS 测试发送输入参数。
/// </summary>
public class TenantSmsConfigTestSendRequest
{
    /// <summary>
    /// 配置 ID。必须显式指定租户 SMS 配置 ID，避免依赖运行期租户上下文。
    /// </summary>
    [Required(ErrorMessage = "测试发送必须指定租户 SMS 配置 ID")]
    public long Id { get; set; }

    /// <summary>
    /// 目标手机号。
    /// </summary>
    [Required(ErrorMessage = "目标手机号不能为空")]
    [Phone(ErrorMessage = "目标手机号格式不正确")]
    public string Phone { get; set; } = null!;

    /// <summary>
    /// 短信模板 Code（可选）。留空则使用配置存储值；两者皆空返回明确错误。
    /// </summary>
    public string? TemplateCode { get; set; }
}