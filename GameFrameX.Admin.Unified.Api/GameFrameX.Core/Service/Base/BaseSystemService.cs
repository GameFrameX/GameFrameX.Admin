using GameFrameX.Core.Service.User;

namespace GameFrameX.Core.Service.Base;

/// <summary>系统服务的当前用户及资源操作权限。</summary>
public abstract class BaseSystemService : IAdminApiController
{
    protected readonly CurrentUserContext CurrentUserContext;

    protected BaseSystemService(CurrentUserContext currentUserContext)
    {
        CurrentUserContext = currentUserContext;
    }

    protected virtual bool CheckPermission(long? createdId = null)
    {
        return CurrentUserContext.SuperAdmin ||
               (createdId.HasValue && createdId.Value == CurrentUserContext.UserId);
    }
}
