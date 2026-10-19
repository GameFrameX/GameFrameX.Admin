namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeChannel.Dto;

/// <summary>
/// 兑换码渠道分页查询输入参数
/// </summary>
public class PageTenantGameRedeemCodeChannelEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id（可选过滤，按当前应用筛选兑换码渠道）
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}