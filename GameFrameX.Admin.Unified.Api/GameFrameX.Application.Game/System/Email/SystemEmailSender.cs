using GameFrameX.Core.Service.Message;
using Microsoft.Extensions.Logging;

namespace GameFrameX.Application.Game.System.Email;

/// <summary>
/// 平台系统邮件发送实现。
/// <para>从主库 <c>SystemEmailProviderEntity</c> 读取 Provider 配置；缺配置 / 缺凭据返回 <c>false</c> 并记录警告。</para>
/// <para>不得用于租户玩家邮件，也不得作为租户业务邮件的 fallback。</para>
/// </summary>
public sealed class SystemEmailSender : ISystemEmailSender
{
    private readonly ISqlSugarClient _sqlSugar;
    private readonly ILogger<SystemEmailSender> _logger;

    /// <inheritdoc />
    public string? LastSelectedDefaultToEmail { get; private set; }

    /// <summary>
    /// </summary>
    public SystemEmailSender(ISqlSugarClient sqlSugar, ILogger<SystemEmailSender> logger)
    {
        _sqlSugar = sqlSugar;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<bool> SendAsync(SystemEmailPurpose purpose, string? receiverEmail, string subject, string body)
    {
        var provider = await SystemEmailProviderSelector.SelectAsync(_sqlSugar, (int)purpose);
        if (provider == null)
        {
            _logger.LogWarning("平台系统邮件未配置，purpose={Purpose}；已拦截。", purpose);
            return false;
        }

        LastSelectedDefaultToEmail = provider.DefaultToEmail;

        var target = string.IsNullOrWhiteSpace(receiverEmail) ? provider.DefaultToEmail : receiverEmail;
        if (string.IsNullOrWhiteSpace(target))
        {
            _logger.LogWarning("平台系统邮件收件人为空，purpose={Purpose}；已拦截。", purpose);
            return false;
        }

        try
        {
            var result = await SystemEmailDispatcher.SendAsync(provider, target, subject, body);
            if (!result.IsSuccess)
            {
                _logger.LogError("平台系统邮件发送失败：purpose={Purpose}, provider={Provider}, receiver={Receiver}, reason={Reason}", purpose, provider.Provider, target, result.FailureReason);
            }

            return result.IsSuccess;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "平台系统邮件发送失败：purpose={Purpose}, provider={Provider}, receiver={Receiver}", purpose, provider.Provider, target);
            return false;
        }
    }
}