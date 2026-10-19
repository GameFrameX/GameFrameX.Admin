namespace GameFrameX.Application.Game.User.GameUser.Dto;

/// <summary>
/// 游戏账号删除输入参数
/// </summary>
public class DeleteTenantGameUserEntityRequest : BaseIdRequest
{
    /// <summary>
    /// 当前应用Id。封禁/解封前校验账号在当前应用下存在角色归属。
    /// </summary>
    public long? AppId { get; set; }
}