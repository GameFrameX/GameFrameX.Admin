using GameFrameX.Entity.Enum;

namespace GameFrameX.Core.Service.TenantMenuSync;

/// <summary>
/// 租户套餐菜单天花板（ceiling）形态。
/// </summary>
[AdminEnum]
public enum TenantMenuSyncCeilingKind
{
    /// <summary>
    /// 无 ceiling（PackageId == null）：模板角色按 SyncRoles 纯 C189 收敛，自定义角色不夹逼不删除。
    /// </summary>
    None = 1,

    /// <summary>
    /// 有 ceiling（套餐存在）：模板角色期望集与闭包取交集，自定义角色被闭包夹逼。
    /// </summary>
    Bounded = 2,

    /// <summary>
    /// 套餐悬空（不存在 / 软删 / 禁用）：跳过同步并置 failed（distinct 错误语义），不删不补。
    /// </summary>
    Dangling = 3,
}
