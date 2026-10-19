namespace GameFrameX.Application.Game.System.Email.Dto;

/// <summary>
/// 平台系统邮件 Provider 配置输出参数（敏感字段已脱敏）。
/// </summary>
public class SystemEmailProviderResponse : IBasePageResponse<GameFrameX.Entity.System.SystemEmailProviderEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 平台系统邮件用途（SystemEmailPurpose）。0 表示默认配置。
    /// </summary>
    public int Purpose { get; set; }

    /// <summary>
    /// Provider 类型。当前支持 TencentCloudSes（腾讯云 SES）和 Smtp。
    /// </summary>
    public string Provider { get; set; } = "TencentCloudSes";

    /// <summary>
    /// SMTP 主机。Provider=Smtp 时必填。
    /// </summary>
    public string? Host { get; set; }

    /// <summary>
    /// SMTP 端口。Provider=Smtp 时必填。
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// SMTP 启用 SSL。Provider=Smtp 时使用。
    /// </summary>
    public bool? EnableSsl { get; set; }

    /// <summary>
    /// SMTP 使用默认凭据。Provider=Smtp 时使用。
    /// </summary>
    public bool? UseDefaultCredentials { get; set; }

    /// <summary>
    /// SMTP 用户名（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// SMTP 密码（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Provider 区域（如腾讯云 SES 区域 ap-hongkong）。
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// 腾讯云 SES SecretId（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string? SecretId { get; set; }

    /// <summary>
    /// 腾讯云 SES SecretKey（敏感字段，API 输出脱敏，日志禁止明文）。
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// 平台默认发件人邮箱地址。
    /// </summary>
    public string? DefaultFromEmail { get; set; }

    /// <summary>
    /// 平台默认发件人显示名称。
    /// </summary>
    public string? DefaultFromName { get; set; }

    /// <summary>
    /// 平台默认收件人邮箱；缺省时使用业务调用方传入的 receiver。
    /// </summary>
    public string? DefaultToEmail { get; set; }

    /// <summary>
    /// 腾讯云 SES 模板 ID。Provider=TencentCloudSes 时必填；SMTP 发送不使用。
    /// </summary>
    public ulong? TemplateId { get; set; }

    /// <summary>
    /// 腾讯云 SES 模板变量 JSON。Provider=TencentCloudSes 时写入 TemplateData；SMTP 发送不使用。
    /// </summary>
    public string? TemplateDataJson { get; set; }

    /// <summary>
    /// 配置选择优先级。同一 Purpose 匹配层中按此值降序选择；数值越大越优先。
    /// </summary>
    public int Priority { get; set; } = 100;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }

    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; set; }
}