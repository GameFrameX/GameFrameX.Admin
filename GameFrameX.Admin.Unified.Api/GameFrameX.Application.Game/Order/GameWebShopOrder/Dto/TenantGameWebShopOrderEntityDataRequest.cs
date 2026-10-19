namespace GameFrameX.Application.Game.Order.GameWebShopOrder.Dto;

/// <summary>
/// 游戏商城订单实体数据
/// </summary>
public class TenantGameWebShopOrderEntityDataRequest
{
    /// <summary>
    /// 商品名称
    /// </summary>
    [Required(ErrorMessage = "商品名称不能为空")]
    public string Name { get; set; } = null!;

    /// <summary>
    /// 商品图
    /// </summary>
    [Required(ErrorMessage = "商品图不能为空")]
    public string Logo { get; set; } = null!;

    /// <summary>
    /// 商品描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 商品关键词
    /// </summary>
    public string KeyWords { get; set; } = null!;

    /// <summary>
    /// 零售价
    /// </summary>
    [Required(ErrorMessage = "零售价不能为空")]
    public string Price { get; set; } = null!;

    /// <summary>
    /// 实际价格
    /// </summary>
    [Required(ErrorMessage = "实际价格不能为空")]
    public string RealPrice { get; set; } = null!;

    /// <summary>
    /// 道具列表
    /// </summary>
    [Required(ErrorMessage = "道具列表不能为空")]
    public string ItemJson { get; set; } = null!;

    /// <summary>
    /// 是否上架
    /// </summary>
    public bool IsUp { get; set; }

    /// <summary>
    /// 权重
    /// </summary>
    public int Weight { get; set; }
}