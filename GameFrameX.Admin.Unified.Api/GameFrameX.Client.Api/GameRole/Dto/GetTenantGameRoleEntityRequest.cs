namespace GameFrameX.Client.Api.GameRole.Dto;

/// <summary>
/// 游戏角色分页查询输入参数
/// </summary>
public sealed class GetTenantGameRoleEntityRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// 区服Id
    /// </summary>
    public long AreaId { get; set; }
}