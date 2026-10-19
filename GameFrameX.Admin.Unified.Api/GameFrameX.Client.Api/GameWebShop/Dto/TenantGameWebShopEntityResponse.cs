namespace GameFrameX.Client.Api.GameWebShop.Dto;

/// <summary>
/// 游戏角色输出参数
/// </summary>
public sealed class TenantGameWebShopEntityResponse : EntityBaseId
{
    /// <summary>
    /// 商品名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 商品图
    /// </summary>
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
    public string Price { get; set; } = null!;

    /// <summary>
    /// 实际价格
    /// </summary>
    public string RealPrice { get; set; } = null!;

    /// <summary>
    /// 道具列表
    /// </summary>
    public string ItemJson { get; set; } = null!;

    /// <summary>
    /// 权重
    /// </summary>
    public string Weight { get; set; } = null!;
}