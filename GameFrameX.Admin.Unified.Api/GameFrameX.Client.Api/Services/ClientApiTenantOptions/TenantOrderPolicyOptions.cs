namespace GameFrameX.Client.Api.Services.ClientApiTenantOptions;

/// <summary>
/// 租户订单 / 支付测试策略。
/// </summary>
public sealed class TenantOrderPolicyOptions
{
    /// <summary>
    /// 是否启用沙箱环境充值直接发货。
    /// </summary>
    public bool EnableSandBoxRecharge { get; init; }
}