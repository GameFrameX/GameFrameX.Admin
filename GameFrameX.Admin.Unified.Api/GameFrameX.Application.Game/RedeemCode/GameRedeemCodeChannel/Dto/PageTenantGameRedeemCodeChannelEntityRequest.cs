namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeChannel.Dto;

/// <summary>
/// 兑换码渠道分页查询输入参数
/// </summary>
public class PageTenantGameRedeemCodeChannelEntityRequest : BasePageRequest
{

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;
}