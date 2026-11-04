namespace GameFrameX.Application.Platform.OpenAccess;

/// <summary>开放接口身份绑定的用户。</summary>
public sealed class BoundTenantUser
{
    public TenantUserEntity User { get; }

    public BoundTenantUser(TenantUserEntity user)
    {
        User = user;
    }
}
