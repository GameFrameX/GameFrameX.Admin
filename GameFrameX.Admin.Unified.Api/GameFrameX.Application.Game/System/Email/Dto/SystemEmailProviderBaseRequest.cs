namespace GameFrameX.Application.Game.System.Email.Dto;

/// <summary>
/// 平台系统邮件 Provider 配置共有输入字段。
/// <para>AddAsync/UpdateAsync 共用此基类承载 19 个业务字段；新增/更新接口的差仅为 UpdateAsync 追加 <c>Id</c>/<c>Version</c>。</para>
/// </summary>
public class SystemEmailProviderBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 平台系统邮件用途（<c>SystemEmailPurpose</c>）。0 表示默认系统邮件配置。
    /// </summary>
    public int Purpose { get; set; }

    /// <summary>
    /// Provider 类型。当前支持 <c>TencentCloudSes</c> 和 <c>Smtp</c>。
    /// </summary>
    [Required(ErrorMessage = "Provider类型不能为空")]
    public string Provider { get; set; } = "TencentCloudSes";

    /// <summary>
    /// SMTP 主机（Provider=Smtp 时必填）。
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// SMTP 端口（Provider=Smtp 时必填）。
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// SMTP 启用 SSL。
    /// </summary>
    public bool? EnableSsl { get; set; }

    /// <summary>
    /// SMTP 使用默认凭据。
    /// </summary>
    public bool? UseDefaultCredentials { get; set; }

    /// <summary>
    /// SMTP 用户名（敏感字段）。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// SMTP 密码（敏感字段）。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Provider 区域。
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// 腾讯云 SecretId（敏感字段）。
    /// </summary>
    public string? SecretId { get; set; }

    /// <summary>
    /// 腾讯云 SecretKey（敏感字段）。
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// 默认发件人邮箱。
    /// </summary>
    [Required(ErrorMessage = "默认发件人邮箱不能为空")]
    public string? DefaultFromEmail { get; set; }

    /// <summary>
    /// 默认发件人显示名。
    /// </summary>
    public string? DefaultFromName { get; set; }

    /// <summary>
    /// 默认收件人邮箱。
    /// </summary>
    public string? DefaultToEmail { get; set; }

    /// <summary>
    /// 腾讯云 SES 模板 ID。Provider=TencentCloudSes 时必填。
    /// </summary>
    public ulong? TemplateId { get; set; }

    /// <summary>
    /// 腾讯云 SES 模板变量 JSON。
    /// </summary>
    public string? TemplateDataJson { get; set; }

    /// <summary>
    /// 配置选择优先级（数值越大越优先）。仅用于配置选择，不代表邮件投递队列优先级。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用。禁用配置不参与系统邮件选择和测试发送。
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 备注。
    /// </summary>
    public string? Remark { get; set; }
}