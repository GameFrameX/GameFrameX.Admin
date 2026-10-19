using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.User.GameUser.Dto;

/// <summary>
/// 游戏账号输出参数
/// </summary>
public class TenantGameUserEntityResponse
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string UserName { get; set; } = null!;

    /// <summary>
    /// 状态
    /// </summary>
    public GameUserStatus Status { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 创建人
    /// </summary>
    public string CreatedName { get; set; } = null!;

    /// <summary>
    /// 最后更新人
    /// </summary>
    public string UpdatedName { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public string CreateTime { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 行版本（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}