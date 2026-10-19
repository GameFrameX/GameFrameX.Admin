using System.Net.Mail;
using GameFrameX.Email.Abstract;

namespace GameFrameX.Email.Smtp;

/// <summary>
/// SMTP 邮件发送实现。
/// <para>承接原 <c>SystemEmailDispatcher.SendViaSmtpAsync</c> 的发送能力，现以 <see cref="IEmailSender"/> 形态注册到 <see cref="EmailSenderRegistry"/>，
/// 取代此前静态 switch 中的 <c>Smtp</c> 分支。</para>
/// <para>发送路径继续使用 <see cref="System.Net.Mail.SmtpClient"/>（BCL 直连），不引入 <c>FluentEmail.Smtp</c> 等第三方封装。</para>
/// </summary>
[EmailProvider("Smtp", "SMTP")]
public sealed class SmtpEmailSender : IEmailSender
{
    /// <inheritdoc />
    public string ProviderName
    {
        get { return "Smtp"; }
    }

    /// <inheritdoc />
    public async Task<EmailSendResult> SendAsync(EmailSendOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            return EmailSendResult.Fail(EmailSendFailureReason.InvalidInput);
        }

        if (string.IsNullOrWhiteSpace(options.SmtpHost) || !options.SmtpPort.HasValue)
        {
            return EmailSendResult.Fail(EmailSendFailureReason.MissingCredentials);
        }

        if (string.IsNullOrWhiteSpace(options.FromEmailAddress))
        {
            return EmailSendResult.Fail(EmailSendFailureReason.MissingCredentials);
        }

        if (string.IsNullOrWhiteSpace(options.ReceiverEmail))
        {
            return EmailSendResult.Fail(EmailSendFailureReason.InvalidInput);
        }

        if (string.IsNullOrWhiteSpace(options.Subject))
        {
            return EmailSendResult.Fail(EmailSendFailureReason.InvalidInput);
        }

        try
        {
            using (var client = new SmtpClient(options.SmtpHost, options.SmtpPort.Value))
            {
                client.EnableSsl = options.SmtpEnableSsl ?? false;
                client.UseDefaultCredentials = options.SmtpUseDefaultCredentials ?? false;
                if (!client.UseDefaultCredentials &&
                    !string.IsNullOrEmpty(options.SmtpUserName))
                {
                    client.Credentials = new System.Net.NetworkCredential(options.SmtpUserName, options.SmtpPassword ?? string.Empty);
                }

                using (var message = new MailMessage
                       {
                           From = new MailAddress(options.FromEmailAddress, options.FromDisplayName ?? string.Empty),
                           Subject = options.Subject,
                           Body = options.Body ?? string.Empty,
                           IsBodyHtml = false,
                       })
                {
                    message.To.Add(options.ReceiverEmail);
                    cancellationToken.ThrowIfCancellationRequested();
                    await client.SendMailAsync(message, cancellationToken).ConfigureAwait(false);
                }
            }

            return EmailSendResult.Ok();
        }
        catch (Exception ex)
        {
            return EmailSendResult.Fail(EmailSendFailureReason.UpstreamError, ex);
        }
    }
}