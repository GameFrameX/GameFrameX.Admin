using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.User.GameUser.Dto;

/// <summary>
/// 游戏账号分页查询输入参数
/// </summary>
public class PageTenantGameUserEntityRequest : BasePageRequest
{
    /// <summary>
    /// 当前应用Id。游戏账号本身是租户级账号，后台应用工作台通过角色归属派生过滤账号。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 状态
    /// </summary>
    public GameUserStatus? Status { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string Name { get; set; } = null!;
}