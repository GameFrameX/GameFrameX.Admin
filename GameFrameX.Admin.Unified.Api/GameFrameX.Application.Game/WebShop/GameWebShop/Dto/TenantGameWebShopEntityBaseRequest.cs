namespace GameFrameX.Application.Game.WebShop.GameWebShop.Dto;

/// <summary>
/// 游戏商城实体基类 - 包含 AddInput 和 UpdateInput 的公共字段
/// </summary>
public abstract class TenantGameWebShopEntityBaseRequest : BaseUpdateRequest
{
    /// <summary>
    /// 应用Id。事实数据应用作用域，必须 &gt; 0。
    /// <para>过渡期：后台运营当前应用上下文尚未建立，由前端手填。</para>
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

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

    /// <summary>
    /// 转换为共享数据类
    /// </summary>
    public TenantGameWebShopEntityDataRequest ToData()
    {
        return new TenantGameWebShopEntityDataRequest
        {
            Name = Name,
            Logo = Logo,
            Description = Description,
            KeyWords = KeyWords,
            Price = Price,
            RealPrice = RealPrice,
            ItemJson = ItemJson,
            IsUp = IsUp,
            Weight = Weight,
        };
    }
}