using GameFrameX.Core.Const.MenuSeed;

namespace GameFrameX.Core.Const;

/// <summary>
/// 租户管理员默认菜单授权清单。
/// </summary>
public static class TenantAdminMenuDefaults
{
    /// <summary>
    /// 默认租户管理员菜单与按钮权限，由菜单定义中的租户同步策略派生。
    /// </summary>
    public static IReadOnlyList<long> MenuIds
    {
        get { return MenuSeedCatalog.TenantAdminMenuIds; }
    }
}