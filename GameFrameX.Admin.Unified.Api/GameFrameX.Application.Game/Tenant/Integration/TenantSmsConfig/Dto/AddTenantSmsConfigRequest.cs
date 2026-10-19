namespace GameFrameX.Application.Game.Tenant.Integration.TenantSmsConfig.Dto;

/// <summary>
/// 租户 SMS 配置新增输入参数。
/// </summary>
public class AddTenantSmsConfigRequest : BaseAddRequest
{
    /// <summary>
    /// 应用 ID。0 表示租户默认 SMS 配置；大于 0 表示应用级覆盖。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// Provider 类型。当前仅支持 <c>Aliyun</c>。
    /// </summary>
    [Required(ErrorMessage = "Provider类型不能为空")]
    public string Provider { get; set; } = "Aliyun";

    /// <summary>
    /// 阿里云 AccessKeyId（敏感字段，入库明文，API 输出脱敏）。
    /// <para>禁止提交 <c>******</c>；脱敏占位仅用于 UpdateAsync 保留原值。</para>
    /// </summary>
    [Required(ErrorMessage = "AccessKeyId不能为空")]
    public string AccessKeyId { get; set; } = null!;

    /// <summary>
    /// 阿里云 AccessKeySecret（敏感字段，入库明文，API 输出脱敏）。
    /// <para>禁止提交 <c>******</c>；脱敏占位仅用于 UpdateAsync 保留原值。</para>
    /// </summary>
    [Required(ErrorMessage = "AccessKeySecret不能为空")]
    public string AccessKeySecret { get; set; } = null!;

    /// <summary>
    /// 短信签名。
    /// </summary>
    [Required(ErrorMessage = "短信签名不能为空")]
    public string SignName { get; set; } = null!;

    /// <summary>
    /// 短信模板 Code（阿里云 SMS_ 开头）。可为空（<c>SendTestAsync</c> 时由调用方指定）。
    /// </summary>
    public string? TemplateCode { get; set; }

    /// <summary>
    /// 配置选择优先级（数值越大越优先）。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用。禁用配置不参与 SMS 解析和测试发送。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    public string Remark { get; set; } = null!;
}