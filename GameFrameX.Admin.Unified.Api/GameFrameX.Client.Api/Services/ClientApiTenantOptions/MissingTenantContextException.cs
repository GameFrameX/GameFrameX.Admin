namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// 缺少租户上下文时抛出，禁止回落主库或进程级业务参数。
/// </summary>
public sealed class MissingTenantContextException : InvalidOperationException
{
    /// <summary>
    /// </summary>
    public MissingTenantContextException(string message) : base(message)
    {
    }
}