using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.TenantPackage;

/// <summary>
/// 租户到期状态
/// </summary>
[AdminEnum]
public enum TenantExpirationStatus
{
    /// <summary>
    /// 正常
    /// </summary>
    Active,

    /// <summary>
    /// 过渡期中（已到期但仍在宽限期内）
    /// </summary>
    InGracePeriod,

    /// <summary>
    /// 已过期（过渡期结束）
    /// </summary>
    Expired,
}