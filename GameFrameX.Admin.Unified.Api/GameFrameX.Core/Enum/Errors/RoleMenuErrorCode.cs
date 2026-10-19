namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 角色与菜单错误码（段 301，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum RoleMenuErrorCode
{
    /// <summary>
    /// 禁止删除系统管理员角色
    /// </summary>
    [ErrorText("禁止删除系统管理员角色", "Deleting the system administrator role is forbidden")]
    DeleteSystemAdminRoleForbidden = 30100001,

    /// <summary>
    /// 菜单已存在
    /// </summary>
    [ErrorText("菜单已存在", "Menu already exists")]
    MenuAlreadyExists = 30100002,

    /// <summary>
    /// 权限标识格式为空
    /// </summary>
    [ErrorText("权限标识格式为空", "Permission code is empty")]
    PermissionCodeEmpty = 30100003,

    /// <summary>
    /// 禁止本节点与父节点相同
    /// </summary>
    [ErrorText("禁止本节点与父节点相同", "A node cannot be its own parent")]
    NodeEqualsParentForbidden = 30100004,

    /// <summary>
    /// 路由名称重复
    /// </summary>
    [ErrorText("路由名称重复", "Route name is duplicated")]
    RouteNameDuplicate = 30100005,
}