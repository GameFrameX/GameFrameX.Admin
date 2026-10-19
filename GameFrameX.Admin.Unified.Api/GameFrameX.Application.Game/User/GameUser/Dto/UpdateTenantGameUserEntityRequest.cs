using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.User.GameUser.Dto;

/// <summary>
/// 游戏账号更新输入参数
/// </summary>
public class UpdateTenantGameUserEntityRequest : BaseUpdateRequest
{
    public virtual string? UserName { get; set; }
    public virtual string? Password { get; set; }
    public virtual GameUserStatus? Status { get; set; }
    public virtual string? Description { get; set; }
}