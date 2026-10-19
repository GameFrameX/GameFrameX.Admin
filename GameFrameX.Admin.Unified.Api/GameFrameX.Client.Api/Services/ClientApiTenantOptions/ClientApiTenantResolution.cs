namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// 当前请求解析出的租户 / 应用作用域。
/// </summary>
public sealed class ClientApiTenantResolution
{
    /// <summary>
    /// 租户 ID（来自中间件，权威来源）。
    /// </summary>
    public long TenantId { get; init; }

    /// <summary>
    /// 应用 ID；null 表示当前请求未携带应用上下文，只能读取租户默认配置。
    /// </summary>
    public long? AppId { get; init; }
}