namespace GameFrameX.Email.Abstract;

/// <summary>
/// 邮件发送器抽象。
/// <para>各服务商实现类（Tencent SES / SMTP / 后续扩展）实现本接口；通过 <see cref="EmailSenderRegistry"/> 按渠道名反射收集与发现。</para>
/// <para>实现类必须标注 <see cref="EmailProviderAttribute"/> 声明稳定 <see cref="EmailProviderAttribute.Name"/>，由 Registry 反射绑定。</para>
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// 渠道唯一标识（如 <c>TencentCloudSes</c> / <c>Smtp</c>），与 <see cref="EmailProviderAttribute.Name"/> 一致。
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// 发送邮件。
    /// </summary>
    /// <param name="options">发送参数；渠道实现按需读取自身字段。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>发送结果；缺凭据 / 输入不合法 / 上游错误通过 <see cref="EmailSendResult.FailureReason"/> 区分。</returns>
    Task<EmailSendResult> SendAsync(EmailSendOptions options, CancellationToken cancellationToken = default);
}