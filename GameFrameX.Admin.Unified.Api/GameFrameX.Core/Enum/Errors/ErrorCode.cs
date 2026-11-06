namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 租户管理错误码（段 306，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum ErrorCode
{
    /// <summary>
    /// 禁止删除默认租户
    /// </summary>
    [ErrorText("禁止删除默认租户", "Deleting the default tenant is forbidden")]
    DeleteDefaultForbidden = 30600001,

    /// <summary>
    /// 已存在同名的租户
    /// </summary>
    [ErrorText("已存在同名的租户", "A tenant with the same name already exists")]
    NameAlreadyExists = 30600002,

    /// <summary>
    /// 已存在同名的管理员角色
    /// </summary>
    [ErrorText("已存在同名的管理员角色", "A tenant administrator with the same name already exists")]
    AdminNameAlreadyExists = 30600003,

    /// <summary>
    /// 默认租户状态禁止修改
    /// </summary>
    [ErrorText("默认租户状态禁止修改", "The default tenant status cannot be changed")]
    DefaultStatusChangeForbidden = 30600004,

    /// <summary>
    /// 禁止创建此类型的数据库
    /// </summary>
    [ErrorText("禁止创建此类型的数据库", "Creating this type of database is forbidden")]
    DbTypeForbidden = 30600005,

    /// <summary>
    /// 租户已禁用
    /// </summary>
    [ErrorText("租户已禁用", "Business is disabled")]
    Disabled = 30600006,

    /// <summary>
    /// 普通租户禁止使用ID隔离
    /// </summary>
    [ErrorText("普通租户禁止使用ID隔离", "ID isolation is forbidden for normal tenants")]
    IdIsolationForbiddenForNormal = 30600007,

    /// <summary>
    /// 租户数据库已创建，禁止重复创建
    /// </summary>
    [ErrorText("租户数据库已创建，禁止重复创建", "The tenant database has already been created")]
    DatabaseAlreadyCreated = 30600008,

    /// <summary>
    /// 租户已过期
    /// </summary>
    [ErrorText("租户已过期", "Business has expired")]
    Expired = 30600009,

    /// <summary>
    /// 租户菜单同步手动触发仅超级管理员可用
    /// </summary>
    [ErrorText("租户菜单同步手动触发仅超级管理员可用", "Manual tenant menu sync trigger is reserved for the super admin")]
    MenuSyncSuperAdminOnly = 30600010,

    /// <summary>
    /// 租户不存在或不在菜单同步范围
    /// </summary>
    [ErrorText("租户不存在或不在菜单同步范围", "The tenant does not exist or is out of the menu sync scope")]
    MenuSyncNotInScope = 30600011,

    /// <summary>
    /// 套餐不存在
    /// </summary>
    [ErrorText("套餐不存在", "The package does not exist")]
    MenuSyncPackageNotFound = 30600012,

    /// <summary>
    /// 企业版套餐菜单清单由启动自动收敛维护，禁止手工编辑
    /// </summary>
    [ErrorText("企业版套餐菜单清单由启动自动收敛维护，禁止手工编辑", "The enterprise package menu list is startup-converged and read-only for manual editing")]
    EnterprisePackageMenuReadOnly = 30600013,

    /// <summary>
    /// 套餐不存在或未启用
    /// </summary>
    [ErrorText("套餐不存在或未启用", "The package does not exist or is not enabled")]
    PackageNotUsable = 30600014,

    /// <summary>
    /// 默认租户套餐禁止修改
    /// </summary>
    [ErrorText("默认租户套餐禁止修改", "The default tenant package cannot be changed")]
    DefaultPackageChangeForbidden = 30600015,
}