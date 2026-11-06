namespace GameFrameX.Client.Api.Services.ClientApiOptions;

/// <summary>
/// 缺少租户上下文时抛出，禁止回落主库或进程级业务参数。
/// </summary>
public sealed class MissingContextException : InvalidOperationException
{
    /// <summary>
    /// </summary>
    public MissingContextException(string message) : base(message)
    {
    }
}