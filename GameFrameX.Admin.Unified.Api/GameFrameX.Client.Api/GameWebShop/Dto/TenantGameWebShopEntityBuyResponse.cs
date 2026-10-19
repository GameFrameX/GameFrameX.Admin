namespace GameFrameX.Client.Api.GameWebShop.Dto;

/// <summary>
/// 游戏角色输出参数
/// </summary>
public sealed class TenantGameWebShopEntityBuyResponse
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Html { get; set; } = null!;

    /// <summary>
    /// 订单号
    /// </summary>
    public string OrderId { get; set; } = null!;
}