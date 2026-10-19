using System.Text.Json;
using GameFrameX.Email.Abstract;
using TencentCloud.Common;
using TencentCloud.Common.Profile;
using TencentCloud.Ses.V20201002;
using TencentCloud.Ses.V20201002.Models;

namespace GameFrameX.Email.TencentSes;

/// <summary>
/// 腾讯云 SES 邮件发送实现。
/// <para>承接原 <c>GameFrameX.Core.TencentCloud.Email/EmailHelper</c> 的 SES 发送能力，
/// 现以 <see cref="IEmailSender"/> 形态注册到 <see cref="EmailSenderRegistry"/>，
/// 取代此前 <c>SystemEmailDispatcher</c> 静态 switch 中的 TencentCloudSes 分支。</para>
/// </summary>
[EmailProvider("TencentCloudSes", "腾讯云 SES")]
public sealed class TencentSesEmailSender : IEmailSender
{
    private const string DefaultRegion = "ap-hongkong";

    /// <inheritdoc />
    public string ProviderName
    {
        get { return "TencentCloudSes"; }
    }

    /// <inheritdoc />
    public Task<EmailSendResult> SendAsync(EmailSendOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            return Task.FromResult(EmailSendResult.Fail(EmailSendFailureReason.InvalidInput));
        }

        if (string.IsNullOrWhiteSpace(options.SecretId) || string.IsNullOrWhiteSpace(options.SecretKey))
        {
            return Task.FromResult(EmailSendResult.Fail(EmailSendFailureReason.MissingCredentials));
        }

        if (!options.TemplateId.HasValue || options.TemplateId.Value == 0)
        {
            return Task.FromResult(EmailSendResult.Fail(EmailSendFailureReason.MissingCredentials));
        }

        if (string.IsNullOrWhiteSpace(options.FromEmailAddress))
        {
            return Task.FromResult(EmailSendResult.Fail(EmailSendFailureReason.MissingCredentials));
        }

        if (string.IsNullOrWhiteSpace(options.ReceiverEmail))
        {
            return Task.FromResult(EmailSendResult.Fail(EmailSendFailureReason.InvalidInput));
        }

        if (string.IsNullOrWhiteSpace(options.Subject))
        {
            return Task.FromResult(EmailSendResult.Fail(EmailSendFailureReason.InvalidInput));
        }

        try
        {
            var template = new SendEmailTemplate
            {
                Subject = options.Subject,
                ReceiverEmail = options.ReceiverEmail,
                TemplateID = options.TemplateId.Value,
                FromEmailAddress = options.FromEmailAddress,
                TemplateData = BuildTemplateData(options.TemplateDataJson, options.Body ?? string.Empty),
            };
            var credential = new Credential
            {
                SecretId = options.SecretId,
                SecretKey = options.SecretKey,
            };

            return SendInternalAsync(template, credential, cancellationToken);
        }
        catch (Exception ex)
        {
            return Task.FromResult(EmailSendResult.Fail(EmailSendFailureReason.UpstreamError, ex));
        }
    }

    private static async Task<EmailSendResult> SendInternalAsync(SendEmailTemplate template, Credential credential, CancellationToken cancellationToken)
    {
        try
        {
            var clientProfile = new ClientProfile
            {
                HttpProfile = new HttpProfile
                {
                    Endpoint = "ses.ap-hongkong.tencentcloudapi.com",
                },
            };

            var client = new SesClient(credential, DefaultRegion, clientProfile);
            var request = new SendEmailRequest
            {
                FromEmailAddress = template.FromEmailAddress,
                Destination = new[] { template.ReceiverEmail, },
                Subject = template.Subject,
                Template = new Template
                {
                    TemplateID = template.TemplateID,
                    TemplateData = JsonSerializer.Serialize(template.TemplateData),
                },
                TriggerType = 1,
            };

            await client.SendEmail(request);
            return EmailSendResult.Ok();
        }
        catch (Exception ex)
        {
            return EmailSendResult.Fail(EmailSendFailureReason.UpstreamError, ex);
        }
    }

    /// <summary>
    /// 构造腾讯云 SES 模板数据。
    /// <para>当前通用模板数据对象只支持 <c>username</c> / <c>verification_code</c> / <c>expiry_minutes</c>；
    /// 自由正文通过 <c>username</c> 兜底，避免正文被静默丢弃。</para>
    /// </summary>
    private static SendEmailTemplateData BuildTemplateData(string? templateDataJson, string body)
    {
        var data = string.IsNullOrWhiteSpace(templateDataJson)
                       ? new SendEmailTemplateData()
                       : JsonSerializer.Deserialize<SendEmailTemplateData>(templateDataJson) ?? new SendEmailTemplateData();
        if (string.IsNullOrWhiteSpace(data.Username))
        {
            data.Username = body ?? string.Empty;
        }

        return data;
    }
}