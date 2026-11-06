namespace GameFrameX.Client.Api.Services.ClientApiOptions;

/// <summary>
/// 租户区服信息输出策略。
/// </summary>
public sealed class AreaPolicyOptions
{
    /// <summary>
    /// 是否在区服列表中返回 HttpManageUrl 运维地址。
    /// </summary>
    public bool EnableReturnHttpManager { get; init; }
}