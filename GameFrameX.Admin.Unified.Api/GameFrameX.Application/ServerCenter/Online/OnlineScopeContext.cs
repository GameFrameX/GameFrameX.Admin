namespace GameFrameX.Application.ServerCenter.Online;

/// <summary>
/// Admin 调用 Online 的区服上下文，只能由工厂验证区服存在后构造。
/// </summary>
public sealed class OnlineScopeContext
{
    // S3/C401 断链修复点
    /// <summary>区服标识。</summary>
    public long ServerId { get; }

    /// <summary>请求摘要，供审计与幂等上下文使用。</summary>
    public string RequestSummary { get; }

    internal OnlineScopeContext(long serverId, string requestSummary)
    {
        ServerId = serverId;
        RequestSummary = requestSummary ?? string.Empty;
    }
}
