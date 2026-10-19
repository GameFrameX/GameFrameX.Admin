using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Const.MenuSeed;

/// <summary>
/// 租户默认角色的菜单同步预设。
/// </summary>
[Flags]
[AdminEnum]
public enum TenantMenuSyncRole
{
    /// <summary>
    /// 不同步给租户默认角色。
    /// </summary>
    None = 0,

    /// <summary>
    /// 租户管理员。
    /// </summary>
    TenantAdmin = 1 << 0,

    /// <summary>
    /// 运营负责人。
    /// </summary>
    OpsManager = 1 << 1,

    /// <summary>
    /// 客服 GM。
    /// </summary>
    GmSupport = 1 << 2,

    /// <summary>
    /// 内容运营。
    /// </summary>
    ContentOps = 1 << 3,

    /// <summary>
    /// 区服运维。
    /// </summary>
    ServerOps = 1 << 4,

    /// <summary>
    /// 支付运营。
    /// </summary>
    PaymentOps = 1 << 5,

    /// <summary>
    /// 市场活动。
    /// </summary>
    MarketingOps = 1 << 6,

    /// <summary>
    /// 数据分析。
    /// </summary>
    DataAnalyst = 1 << 7,

    /// <summary>
    /// 审计安全。
    /// </summary>
    AuditSecurity = 1 << 8,

    /// <summary>
    /// 应用配置管理员。
    /// </summary>
    AppConfigAdmin = 1 << 9,
}