namespace GameFrameX.Client.Api.Services.Email;

/// <summary>
/// 租户业务邮件配置选择的作用域候选（应用 + 用途）。
/// </summary>
internal sealed class EmailScopeCandidate
{
    /// <summary>应用 ID，0 表示通配。</summary>
    public long AppId { get; }

    /// <summary>邮件用途，0 表示通配。</summary>
    public int Purpose { get; }

    public EmailScopeCandidate(long appId, int purpose)
    {
        AppId = appId;
        Purpose = purpose;
    }
}