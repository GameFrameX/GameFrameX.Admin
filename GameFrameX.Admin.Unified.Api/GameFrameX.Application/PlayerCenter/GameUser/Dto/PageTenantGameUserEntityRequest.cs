using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.PlayerCenter.GameUser.Dto;

/// <summary>
/// 游戏账号分页查询输入参数
/// </summary>
public class PageTenantGameUserEntityRequest : BasePageRequest
{
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