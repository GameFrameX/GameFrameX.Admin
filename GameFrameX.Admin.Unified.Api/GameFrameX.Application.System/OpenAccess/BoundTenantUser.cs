namespace GameFrameX.Application.System.OpenAccess;

/// <summary>
/// 开放接口身份绑定的租户用户与组织解析结果。
/// </summary>
public sealed class BoundTenantUser
{
    /// <summary>绑定的租户用户。</summary>
    public TenantUserEntity User { get; }

    /// <summary>绑定的租户组织（可能为空）。</summary>
    public TenantOrganizationEntity Organization { get; }

    public BoundTenantUser(TenantUserEntity user, TenantOrganizationEntity org)
    {
        User = user;
        Organization = org;
    }
}