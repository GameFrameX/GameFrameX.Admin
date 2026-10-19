namespace GameFrameX.Client.Api.Services;

/// <summary>
/// 公开端点要求的租户与应用上下文。
/// </summary>
public sealed class TenantAppContext
{
    /// <summary>租户 ID。</summary>
    public long TenantId { get; }

    /// <summary>应用 ID。</summary>
    public long AppId { get; }

    public TenantAppContext(long tenantId, long appId)
    {
        TenantId = tenantId;
        AppId = appId;
    }
}