namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// 租户 Game 登录票据策略（非 key 部分）。
/// </summary>
public sealed class TenantGameLoginTicketPolicyOptions
{
    /// <summary>
    /// 票据固定到期时间（Unix 秒）；0 表示永不过期，非 0 值已按平台默认值和 30 天上限归一化。
    /// </summary>
    public long TicketExpireTime { get; init; }

    /// <summary>
    /// 是否允许首次签发自动创建租户应用级 RSA key。
    /// </summary>
    public bool EnableKeyAutoCreate { get; init; }
}