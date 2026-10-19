namespace GameFrameX.Application.Game.Tenant.Integration.TenantEmailConfig.Dto;

/// <summary>
/// 租户业务邮件配置更新输入参数。
/// <para>敏感字段提交 <c>******</c> 时表示保留原值，避免脱敏回写覆盖真实密钥。</para>
/// </summary>
public class UpdateTenantEmailConfigRequest : TenantEmailConfigBaseRequest, ICrudUpdateRequest
{
    /// <summary>
    /// 主键Id
    /// </summary>
    [Required(ErrorMessage = "主键Id不能为空")]
    [Range(1, long.MaxValue, ErrorMessage = "主键Id必须大于0")]
    public long Id { get; set; }

    /// <summary>
    /// 客户端读取记录时的版本号，用于阻止过期数据覆盖较新的修改。
    /// </summary>
    [Required(ErrorMessage = "版本号不能为空")]
    [Range(0, long.MaxValue, ErrorMessage = "版本号不能小于0")]
    public long? RowVersion { get; set; }

    /// <summary>
    /// 邮件服务商 SecretId。传 <c>******</c> 时保留原值。
    /// </summary>
    public string FromEmailSecretId { get; set; } = null!;

    /// <summary>
    /// 邮件服务商 SecretKey。传 <c>******</c> 时保留原值。
    /// </summary>
    public string FromEmailSecretKey { get; set; } = null!;
}