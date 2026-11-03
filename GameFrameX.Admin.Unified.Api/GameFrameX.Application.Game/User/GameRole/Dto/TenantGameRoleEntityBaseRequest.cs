namespace GameFrameX.Application.Game.User.GameRole.Dto;

/// <summary>
/// 游戏角色基础输入参数
/// </summary>
public class TenantGameRoleEntityBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 角色Id
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 用户ID
    /// </summary>
    public virtual long UserId { get; set; }

    /// <summary>
    /// 区服Id
    /// </summary>
    public virtual long AreaId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public virtual string Description { get; set; } = null!;

    /// <summary>
    /// 角色当前的等级
    /// </summary>
    public virtual long? Level { get; set; }

    /// <summary>
    /// 角色最后一次登录时间
    /// </summary>
    public virtual long? LastLoginTime { get; set; }
}