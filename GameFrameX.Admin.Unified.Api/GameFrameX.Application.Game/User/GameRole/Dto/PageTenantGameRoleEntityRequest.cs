namespace GameFrameX.Application.Game.User.GameRole.Dto;

/// <summary>
/// 游戏角色分页查询输入参数
/// </summary>
public class PageTenantGameRoleEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public long? UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public long? Id { get; set; }

    /// <summary>
    /// 是否是沙盒账户
    /// </summary>
    public bool? IsSandbox { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 角色当前的等级
    /// </summary>
    public long? Level { get; set; }

    /// <summary>
    /// 注册时间范围
    /// </summary>
    public List<long> RegisterTime { get; set; } = null!;

    /// <summary>
    /// 最后登录时间范围
    /// </summary>
    public List<long> LogoutTime { get; set; } = null!;

    /// <summary>
    /// 累充最小值(分)
    /// </summary>
    public int? RechargeMin { get; set; }

    /// <summary>
    /// 累充最大值(分)
    /// </summary>
    public int? RechargeMax { get; set; }

    /// <summary>
    /// 应用Id。应用级角色分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }
}