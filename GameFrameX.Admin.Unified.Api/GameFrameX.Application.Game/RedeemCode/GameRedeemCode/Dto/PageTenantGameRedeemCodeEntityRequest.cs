namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCode.Dto;

/// <summary>
/// 兑换码分页查询输入参数
/// </summary>
public class PageTenantGameRedeemCodeEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 兑换码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 是否激活
    /// </summary>
    public bool? IsActive { get; set; }

    /// <summary>
    /// 可使用次数
    /// </summary>
    public int? UsageLimit { get; set; }

    /// <summary>
    /// 已使用次数
    /// </summary>
    public int? UsedCount { get; set; }

    /// <summary>
    /// 兑换码渠道ID
    /// </summary>
    public long? ChannelId { get; set; }

    /// <summary>
    /// 应用Id（可选过滤，按当前应用筛选兑换码）
    /// </summary>
    public long? AppId { get; set; }
}