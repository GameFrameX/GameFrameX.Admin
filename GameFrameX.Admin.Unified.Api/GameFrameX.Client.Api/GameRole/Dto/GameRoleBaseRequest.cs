namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 游戏角色基础输入参数
/// </summary>
public class TenantGameRoleEntityBaseRequest
{
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
}