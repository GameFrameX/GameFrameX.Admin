namespace GameFrameX.Core.Enum.Errors;

/// <summary>
/// Admin 组织架构错误码（段 307，见 <see cref="ErrorSegmentRegistry"/>；C265 自旧 D 码体系平移）。
/// </summary>
public enum OrganizationErrorCode
{
    /// <summary>
    /// 上级组织不存在
    /// </summary>
    [ErrorText("上级组织不存在", "Parent organization does not exist")]
    ParentOrganizationNotFound = 30700001,

    /// <summary>
    /// 当前组织Id不能与上级组织Id相同
    /// </summary>
    [ErrorText("当前组织Id不能与上级组织Id相同", "Organization ID cannot equal its parent ID")]
    OrganizationIdEqualsParentForbidden = 30700002,

    /// <summary>
    /// 已有相同运营组织,编码或名称相同
    /// </summary>
    [ErrorText("已有相同运营组织,编码或名称相同", "An organization with the same code or name already exists")]
    OrganizationAlreadyExists = 30700003,

    /// <summary>
    /// 没有权限操作组织
    /// </summary>
    [ErrorText("没有权限操作组织", "No permission to operate on the organization")]
    NoPermissionForOrganizationOperation = 30700004,

    /// <summary>
    /// 该组织下有用户禁止删除
    /// </summary>
    [ErrorText("该组织下有用户禁止删除", "The organization has users and cannot be deleted")]
    OrganizationHasUsersDeleteForbidden = 30700005,

    /// <summary>
    /// 附属组织下有用户禁止删除
    /// </summary>
    [ErrorText("附属组织下有用户禁止删除", "The affiliated organization has users and cannot be deleted")]
    AffiliatedOrganizationHasUsersDeleteForbidden = 30700006,

    /// <summary>
    /// 下级组织下有用户禁止删除
    /// </summary>
    [ErrorText("下级组织下有用户禁止删除", "The child organization has users and cannot be deleted")]
    ChildOrganizationHasUsersDeleteForbidden = 30700007,

    /// <summary>
    /// 租户默认组织禁止删除
    /// </summary>
    [ErrorText("租户默认组织禁止删除", "The tenant default organization cannot be deleted")]
    TenantDefaultOrganizationDeleteForbidden = 30700008,

    /// <summary>
    /// 禁止增加根节点组织
    /// </summary>
    [ErrorText("禁止增加根节点组织", "Creating root organizations is forbidden")]
    RootOrganizationCreateForbidden = 30700009,
}