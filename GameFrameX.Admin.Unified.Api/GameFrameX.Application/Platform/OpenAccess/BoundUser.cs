namespace GameFrameX.Application.Platform.OpenAccess;

/// <summary>开放接口身份绑定的用户。</summary>
public sealed class BoundUser
{
    public UserEntity User { get; }

    public BoundUser(UserEntity user)
    {
        User = user;
    }
}
