using GameFrameX.Core.Service.Tenant;
using GameFrameX.Email.Abstract;

namespace GameFrameX.Application.Game.System.Email;

/// <summary>
/// 平台系统邮件分发器（路由薄层）。
/// <para>本类仅承担 Provider 名 → <see cref="IEmailSender"/> 的解析与转发；具体发送由 <see cref="EmailSenderRegistry"/> 反射收集的各渠道实现承担。</para>
/// <para>调用方传入已选中的 <see cref="GameFrameX.Entity.System.SystemEmailProviderEntity"/>；本类不读取数据库，避免重复选择策略。</para>
/// <para>凭证解密收口（C370）：落库密文（<c>enc:sec:v1:</c>）在 <see cref="BuildOptions"/> 构建发送参数时经
/// <see cref="TenantSecretCrypto.Decrypt"/> 解密——发信是所有 Provider 配置读取路径的唯一必经点，且为低频事件路径
/// （租户生命周期 / 账单 / 安全通知），每信一次解密的 PBKDF2 派生开销在秒级投递耗时内，不引入缓存。</para>
/// <para>入口校验（空配置 / 空收件人 / 未注册渠道）抛 <see cref="InvalidOperationException"/>；渠道发送结果（含失败原因）经 <see cref="EmailSendResult"/> 返回。</para>
/// </summary>
public static class SystemEmailDispatcher
{
    /// <summary>
    /// 按指定 Provider 配置发送邮件，返回渠道发送结果。
    /// <para>通过 <see cref="EmailSenderRegistry"/> 解析 <see cref="GameFrameX.Entity.System.SystemEmailProviderEntity.Provider"/> 字段并委托对应 <see cref="IEmailSender"/> 实现。</para>
    /// </summary>
    public static async Task<EmailSendResult> SendAsync(GameFrameX.Entity.System.SystemEmailProviderEntity provider, string receiverEmail, string subject, string body)
    {
        if (provider == null)
        {
            throw new InvalidOperationException("平台系统邮件 Provider 配置为空");
        }

        if (string.IsNullOrWhiteSpace(receiverEmail))
        {
            throw new InvalidOperationException("收件人邮箱不能为空");
        }

        if (string.IsNullOrWhiteSpace(provider.Provider))
        {
            throw new InvalidOperationException("平台系统邮件 Provider 类型不能为空");
        }

        if (!EmailSenderRegistry.TryGet(provider.Provider, out var sender))
        {
            throw new InvalidOperationException($"未注册的邮件 Provider：{provider.Provider}");
        }

        var options = BuildOptions(provider, receiverEmail, subject, body);
        return await sender.SendAsync(options);
    }

    /// <summary>
    /// 把 <see cref="GameFrameX.Entity.System.SystemEmailProviderEntity"/> 映射为 <see cref="EmailSendOptions"/>，按渠道字段屏蔽差异。
    /// 凭证列在此解密（C370 收口点，见类注释）。
    /// </summary>
    private static EmailSendOptions BuildOptions(GameFrameX.Entity.System.SystemEmailProviderEntity provider, string receiverEmail, string subject, string body)
    {
        return new EmailSendOptions
        {
            FromEmailAddress = provider.DefaultFromEmail ?? string.Empty,
            FromDisplayName = provider.DefaultFromName,
            ReceiverEmail = receiverEmail,
            Subject = subject,
            Body = body,
            TemplateId = provider.TemplateId,
            TemplateDataJson = provider.TemplateDataJson,
            SecretId = TenantSecretCrypto.Decrypt(provider.SecretId),
            SecretKey = TenantSecretCrypto.Decrypt(provider.SecretKey),
            SmtpHost = provider.Host,
            SmtpPort = provider.Port,
            SmtpEnableSsl = provider.EnableSsl,
            SmtpUseDefaultCredentials = provider.UseDefaultCredentials,
            SmtpUserName = TenantSecretCrypto.Decrypt(provider.UserName),
            SmtpPassword = TenantSecretCrypto.Decrypt(provider.Password),
        };
    }
}