using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.User.GameUser.Dto;

/// <summary>
/// 游戏账号基础输入参数
/// </summary>
public class TenantGameUserEntityBaseRequest : BaseAddRequest
{
    /// <summary>
    /// 账号
    /// </summary>
    public virtual string UserName { get; set; } = null!;

    /// <summary>
    /// 密码
    /// </summary>
    public virtual string Password { get; set; } = null!;

    /// <summary>
    /// 状态
    /// </summary>
    public virtual GameUserStatus Status { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public virtual string Description { get; set; } = null!;

    /// <summary>
    /// 创建人
    /// </summary>
    public virtual string CreatedName { get; set; } = null!;

    /// <summary>
    /// 最后更新人
    /// </summary>
    public virtual string UpdatedName { get; set; } = null!;
}