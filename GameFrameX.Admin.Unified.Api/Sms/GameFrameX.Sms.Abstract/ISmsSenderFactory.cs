namespace GameFrameX.Sms.Abstract;

/// <summary>
/// 短信发送器工厂抽象。
/// <para>由 <see cref="SmsSenderRegistry"/> 实现；调用方不直接 new 实现，仅通过 <c>SmsSenderRegistry</c> 入口获取 <see cref="ISmsSender"/>。</para>
/// </summary>
public interface ISmsSenderFactory
{
    /// <summary>
    /// 按渠道名解析已注册的 <see cref="ISmsSender"/>；未注册返回 false。
    /// </summary>
    /// <param name="providerName">渠道名（如 <c>Aliyun</c>），大小写不敏感。</param>
    /// <param name="sender">解析得到的发送器。</param>
    bool TryResolve(string providerName, out ISmsSender sender);

    /// <summary>
    /// 列出全部已注册渠道名集合。
    /// </summary>
    IReadOnlyCollection<string> ListProviders();
}