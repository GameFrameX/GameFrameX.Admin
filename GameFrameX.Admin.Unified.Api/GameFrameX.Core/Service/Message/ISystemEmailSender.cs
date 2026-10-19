namespace GameFrameX.Core.Service.Message;

/// <summary>
/// 平台系统邮件发送服务。
/// <para>配置存主库 / SuperAdmin-only，不依赖当前请求租户上下文。</para>
/// <para>关键约束：本服务不得用于租户玩家邮件，也不得作为租户业务邮件的 fallback。</para>
/// <para>缺配置 / 缺凭据 / 发送失败返回 <c>false</c> 并记录警告，不抛业务异常；测试发送由 <c>SystemEmailProviderService.SendTestEmail</c> 暴露。</para>
/// </summary>
public interface ISystemEmailSender
{
    /// <summary>
    /// 按平台系统邮件用途发送邮件。
    /// </summary>
    /// <param name="purpose">平台系统邮件用途。</param>
    /// <param name="receiverEmail">收件人邮箱；缺省时由实现方按 <c>SystemEmailProviderEntity.DefaultToEmail</c> 兜底。</param>
    /// <param name="subject">邮件主题。</param>
    /// <param name="body">邮件正文。</param>
    /// <returns>发送是否成功；缺配置 / 缺凭据 / 发送失败返回 false。</returns>
    Task<bool> SendAsync(SystemEmailPurpose purpose, string? receiverEmail, string subject, string body);

    /// <summary>
    /// 最近一次发送时所选 Provider 的 <c>DefaultToEmail</c>。仅供 <c>SystemMessageService</c> 解析缺省收件人；调用方不应依赖此字段做业务判断。
    /// </summary>
    string? LastSelectedDefaultToEmail { get; }
}