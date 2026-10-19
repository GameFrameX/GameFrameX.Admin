namespace GameFrameX.Client.Api.GameWebShop.Dto;

/// <summary>
/// 购买一个商品
/// </summary>
public sealed class TenantGameWebShopEntityBuyRequest
{
    /// <summary>
    /// 商品id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 区服id
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 支付方式
    /// </summary>
    public string Type { get; set; } = null!;
}