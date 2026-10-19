using GameFrameX.Core.Service.Message;

namespace GameFrameX.Client.Api.Services.Email;

/// <summary>
/// 租户业务邮件发送服务。
/// <para>从租户库 <c>TenantEmailConfigEntity</c> 按 <c>TenantId + AppId + Purpose</c> 优先级读取；缺租户上下文或缺配置时返回 <c>false</c>，禁止 fallback 到 <see cref="ISystemEmailSender"/>。</para>
/// <para>调用方必须显式传入 <see cref="TenantEmailPurpose"/>，禁止无用途的 <c>SendAsync()</c>。</para>
/// </summary>
public interface ITenantEmailSender
{
    /// <summary>
    /// 按租户业务邮件用途发送模板邮件。
    /// </summary>
    /// <param name="purpose">租户业务邮件用途，调用方必须显式表达。</param>
    /// <param name="receiverEmail">收件人邮箱。</param>
    /// <param name="templateId">腾讯云邮件模板 ID。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="templateData">模板变量。</param>
    /// <returns>发送成功返回 true；缺租户上下文 / 缺配置 / 发送失败返回 false，不抛业务异常。</returns>
    Task<bool> SendTemplateAsync(TenantEmailPurpose purpose, string receiverEmail, ulong templateId, string subject, IDictionary<string, string> templateData);

    /// <summary>
    /// 获取当前租户配置的发件人地址；缺租户上下文或未配置返回 null。
    /// </summary>
    Task<string?> GetFromEmailAddressAsync();

    /// <summary>
    /// 获取当前租户配置（按 <c>Purpose</c> 优先选择）；缺租户上下文或未配置返回 null。
    /// </summary>
    /// <param name="purpose">租户业务邮件用途。传 0 视为通配，匹配 <c>Purpose=0</c> 的配置。</param>
    Task<TenantEmailOptionsSnapshot?> GetSnapshotAsync(TenantEmailPurpose purpose);
}