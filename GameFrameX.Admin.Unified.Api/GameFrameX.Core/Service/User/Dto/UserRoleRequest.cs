namespace GameFrameX.Core.Service.User.Dto;

/// <summary>
/// 授权用户角色
/// </summary>
public class UserRoleRequest
{
    /// <summary>
    /// 用户Id
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 角色Id集合
    /// </summary>
    public List<long> RoleIdList { get; set; } = null!;
}