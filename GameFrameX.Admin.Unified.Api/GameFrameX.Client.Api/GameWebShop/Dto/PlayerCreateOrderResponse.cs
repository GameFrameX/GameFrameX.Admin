namespace GameFrameX.Client.Api.GameWebShop.Dto;

/// <summary>
/// 创建订单响应
/// </summary>
public sealed class PlayerCreateOrderResponse
{
    /// <summary>
    /// 角色ID
    /// </summary>
    public long RoleId { get; set; }

    /// <summary>
    /// 商品ID
    /// </summary>
    public long Sku { get; set; }

    /// <summary>
    /// 商品价格Id
    /// </summary>
    public long PriceId { get; set; }

    /// <summary>
    /// 真实价格
    /// </summary>
    public uint Amount { get; set; }

    /// <summary>
    /// 是否需要支付
    /// </summary>
    public bool IsNeedPay { get; set; }

    /// <summary>
    /// 获得的道具列表
    /// </summary>
    public List<PropertyItemResponse> RewardsItem { get; set; } = new();

    /// <summary>
    /// 最大购买次数
    /// </summary>
    public int MaxBuyCount { get; set; }

    /// <summary>
    /// 关卡的后缀
    /// </summary>
    public string AdventureSuffix { get; set; } = string.Empty;
}