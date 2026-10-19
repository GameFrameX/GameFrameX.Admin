namespace GameFrameX.Email.TencentSes;

/// <summary>
/// 腾讯云 SES 邮件发送输入模型（工程内部使用）。
/// <para>本类型仅在 <c>GameFrameX.Email.TencentSes</c> 工程内可见；调用方通过 <see cref="Abstract.IEmailSender"/> + <see cref="Abstract.EmailSendOptions"/> 交互。</para>
/// </summary>
public sealed class SendEmailTemplate
{
    /// <summary>
    /// 发件人邮箱地址，格式 <c>"昵称 &lt;邮箱地址&gt;"</c>。
    /// </summary>
    public string FromEmailAddress { get; set; } = null!;

    /// <summary>
    /// 腾讯云 SES 模板 ID。
    /// </summary>
    public ulong TemplateID { get; set; }

    /// <summary>
    /// 邮件主题。
    /// </summary>
    public string Subject { get; set; } = null!;

    /// <summary>
    /// 收件人邮箱地址。
    /// </summary>
    public string ReceiverEmail { get; set; } = null!;

    /// <summary>
    /// 模板变量数据。
    /// </summary>
    public SendEmailTemplateData TemplateData { get; set; } = new();
}