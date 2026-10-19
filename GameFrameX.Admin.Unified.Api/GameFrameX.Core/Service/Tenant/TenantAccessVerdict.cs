using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.Tenant;

/// <summary>
/// 租户访问门禁裁决结果。
/// </summary>
[AdminEnum]
public enum TenantAccessVerdict
{
    /// <summary>
    /// 放行。
    /// </summary>
    Allow,

    /// <summary>
    /// 拒绝：租户已禁用（对应错误码 Z1003，禁读禁写）。
    /// </summary>
    DenyDisabled,

    /// <summary>
    /// 拒绝：租户已过期且当前为写操作（对应错误码 Z1007，过期禁写放读）。
    /// </summary>
    DenyExpiredWrite,
}