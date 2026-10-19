namespace GameFrameX.Email.Abstract;

/// <summary>
/// 邮件发送器工厂抽象。
/// <para>由 <see cref="EmailSenderRegistry"/> 实现；调用方不直接 new 实现，仅通过 <c>EmailSenderRegistry</c> 入口获取 <see cref="IEmailSender"/>。</para>
/// </summary>
public interface IEmailSenderFactory
{
    /// <summary>
    /// 按渠道名解析已注册的 <see cref="IEmailSender"/>；未注册返回 false。
    /// </summary>
    /// <param name="providerName">渠道名（如 <c>TencentCloudSes</c> / <c>Smtp</c>），大小写不敏感。</param>
    /// <param name="sender">解析得到的发送器。</param>
    bool TryResolve(string providerName, out IEmailSender sender);

    /// <summary>
    /// 列出全部已注册渠道名集合。
    /// </summary>
    IReadOnlyCollection<string> ListProviders();
}