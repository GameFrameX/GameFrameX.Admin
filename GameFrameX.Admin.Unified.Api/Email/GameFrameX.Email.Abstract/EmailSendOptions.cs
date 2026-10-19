namespace GameFrameX.Email.Abstract;

/// <summary>
/// 邮件发送输入参数。
/// <para>屏蔽各 Provider 私有 SDK 类型（TencentCloud Credential / SmtpClient 等），所有调用方通过本模型与 <see cref="IEmailSender"/> 交互。</para>
/// <para>字段按渠道需求提供，Provider 实现按需读取自身关心的子集，忽略无关字段。</para>
/// </summary>
public sealed class EmailSendOptions
{
    /// <summary>
    /// 发件人邮箱地址（必填）。
    /// </summary>
    public string FromEmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// 发件人显示名（可选；SMTP 发送会使用，模板型渠道如 TencentCloud SES 走 FromEmailAddress 完整形式）。
    /// </summary>
    public string? FromDisplayName { get; set; }

    /// <summary>
    /// 收件人邮箱地址（必填）。
    /// </summary>
    public string ReceiverEmail { get; set; } = string.Empty;

    /// <summary>
    /// 邮件主题（必填）。
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// 邮件正文（必填；TencentCloud SES 作为 TemplateData.username 兜底使用）。
    /// </summary>
    public string Body { get; set; } = string.Empty;

    /// <summary>
    /// Provider 模板 ID。模板型渠道（如 TencentCloud SES）必填；SMTP 渠道忽略。
    /// </summary>
    public ulong? TemplateId { get; set; }

    /// <summary>
    /// Provider 模板变量 JSON 字符串。模板型渠道使用；SMTP 渠道忽略。
    /// </summary>
    public string? TemplateDataJson { get; set; }

    /// <summary>
    /// Provider 凭据 SecretId（TencentCloud SES 必填；SMTP 作为用户名场景可选）。
    /// </summary>
    public string? SecretId { get; set; }

    /// <summary>
    /// Provider 凭据 SecretKey（TencentCloud SES 必填；SMTP 作为密码场景可选）。
    /// </summary>
    public string? SecretKey { get; set; }

    /// <summary>
    /// SMTP 主机（Provider=Smtp 必填）。
    /// </summary>
    public string? SmtpHost { get; set; }

    /// <summary>
    /// SMTP 端口（Provider=Smtp 必填）。
    /// </summary>
    public int? SmtpPort { get; set; }

    /// <summary>
    /// SMTP 启用 SSL。
    /// </summary>
    public bool? SmtpEnableSsl { get; set; }

    /// <summary>
    /// SMTP 使用默认凭据（true 时忽略 <see cref="SmtpUserName"/> / <see cref="SmtpPassword"/>）。
    /// </summary>
    public bool? SmtpUseDefaultCredentials { get; set; }

    /// <summary>
    /// SMTP 用户名。
    /// </summary>
    public string? SmtpUserName { get; set; }

    /// <summary>
    /// SMTP 密码。
    /// </summary>
    public string? SmtpPassword { get; set; }
}