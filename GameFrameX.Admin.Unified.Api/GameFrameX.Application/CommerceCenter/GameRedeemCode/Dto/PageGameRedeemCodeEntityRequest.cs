namespace GameFrameX.Application.CommerceCenter.GameRedeemCode.Dto;

/// <summary>
/// 兑换码分页查询输入参数
/// </summary>
public class PageGameRedeemCodeEntityRequest : BaseAreaPageRequest
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

}