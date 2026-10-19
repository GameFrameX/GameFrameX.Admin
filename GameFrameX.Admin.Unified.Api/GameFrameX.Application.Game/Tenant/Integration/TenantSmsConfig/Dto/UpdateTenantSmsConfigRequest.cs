namespace GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig.Dto;

/// <summary>
/// 租户 SMS 配置更新输入参数。
/// <para>敏感字段提交 <c>******</c> 时表示保留原值，避免脱敏回写覆盖真实密钥。</para>
/// </summary>
public class UpdateTenantSmsConfigRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用 ID。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// Provider 类型。
    /// </summary>
    [Required(ErrorMessage = "Provider类型不能为空")]
    public string Provider { get; set; } = "Aliyun";

    /// <summary>
    /// 阿里云 AccessKeyId。传 <c>******</c> 时保留原值。
    /// </summary>
    public string AccessKeyId { get; set; } = null!;

    /// <summary>
    /// 阿里云 AccessKeySecret。传 <c>******</c> 时保留原值。
    /// </summary>
    public string AccessKeySecret { get; set; } = null!;

    /// <summary>
    /// 短信签名。
    /// </summary>
    [Required(ErrorMessage = "短信签名不能为空")]
    public string SignName { get; set; } = null!;

    /// <summary>
    /// 短信模板 Code。
    /// </summary>
    public string? TemplateCode { get; set; }

    /// <summary>
    /// 配置选择优先级。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;
}