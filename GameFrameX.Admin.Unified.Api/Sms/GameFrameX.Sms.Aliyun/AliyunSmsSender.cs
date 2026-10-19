using GameFrameX.Sms.Abstract;

namespace GameFrameX.Sms.Aliyun;

/// <summary>
/// 阿里云短信发送实现。
/// <para>承接原 <c>TenantSmsConfigEntityService.SendTest</c> 中的 <c>AlibabaCloud.SDK.Dysmsapi20170525</c> SDK 调用，
/// 现以 <see cref="ISmsSender"/> 形态注册到 <see cref="SmsSenderRegistry"/>，
/// 取代原 CRUD 服务内联构造 <c>SendSmsRequest</c> 与 <c>CreateAliyunClient</c> 的耦合。</para>
/// </summary>
/// <remarks>
/// <para>阿里云约定：<c>SendSms</c> 返回 <c>Body.Code == "OK" &amp;&amp; Body.Message == "OK"</c> 视为成功，否则视为失败并把上游 Code/Message 透传到 <see cref="SmsSendResult.UpstreamErrorCode"/> / <see cref="SmsSendResult.UpstreamErrorMessage"/>。</para>
/// <para>凭据与签名由调用方经 <see cref="SmsSendOptions"/> 传入；本类不持有任何静态凭据。</para>
/// </remarks>
[SmsProvider("Aliyun", "阿里云短信")]
public sealed class AliyunSmsSender : ISmsSender
{
    private const string DefaultEndpoint = "dysmsapi.aliyuncs.com";

    /// <inheritdoc />
    public string ProviderName
    {
        get { return "Aliyun"; }
    }

    /// <inheritdoc />
    public Task<SmsSendResult> SendAsync(SmsSendOptions options, CancellationToken cancellationToken = default)
    {
        if (options == null)
        {
            return Task.FromResult(SmsSendResult.Fail(SmsSendFailureReason.InvalidInput));
        }

        if (string.IsNullOrWhiteSpace(options.AccessKeyId) || string.IsNullOrWhiteSpace(options.AccessKeySecret))
        {
            return Task.FromResult(SmsSendResult.Fail(SmsSendFailureReason.MissingCredentials));
        }

        if (string.IsNullOrWhiteSpace(options.PhoneNumbers))
        {
            return Task.FromResult(SmsSendResult.Fail(SmsSendFailureReason.InvalidInput));
        }

        if (string.IsNullOrWhiteSpace(options.SignName))
        {
            return Task.FromResult(SmsSendResult.Fail(SmsSendFailureReason.MissingCredentials));
        }

        if (string.IsNullOrWhiteSpace(options.TemplateCode))
        {
            return Task.FromResult(SmsSendResult.Fail(SmsSendFailureReason.MissingCredentials));
        }

        try
        {
            var request = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
            {
                PhoneNumbers = options.PhoneNumbers,
                SignName = options.SignName,
                TemplateCode = options.TemplateCode,
                TemplateParam = string.IsNullOrWhiteSpace(options.TemplateParamJson) ? "{}" : options.TemplateParamJson,
                OutId = Guid.NewGuid().ToString("N"),
            };

            var client = CreateClient(options.AccessKeyId, options.AccessKeySecret, options.Endpoint);
            var response = client.SendSms(request);
            if (response?.Body != null && response.Body.Code == "OK" && response.Body.Message == "OK")
            {
                return Task.FromResult(SmsSendResult.Ok());
            }

            return Task.FromResult(SmsSendResult.Fail(
                                       SmsSendFailureReason.UpstreamError,
                                       null,
                                       response?.Body?.Code,
                                       response?.Body?.Message));
        }
        catch (Exception ex)
        {
            return Task.FromResult(SmsSendResult.Fail(SmsSendFailureReason.UpstreamError, ex));
        }
    }

    /// <summary>
    /// 创建阿里云短信客户端。
    /// </summary>
    private static AlibabaCloud.SDK.Dysmsapi20170525.Client CreateClient(string accessKeyId, string accessKeySecret, string? endpoint)
    {
        var config = new AlibabaCloud.OpenApiClient.Models.Config
        {
            AccessKeyId = accessKeyId,
            AccessKeySecret = accessKeySecret,
            Endpoint = string.IsNullOrWhiteSpace(endpoint) ? DefaultEndpoint : endpoint,
        };
        return new AlibabaCloud.SDK.Dysmsapi20170525.Client(config);
    }
}