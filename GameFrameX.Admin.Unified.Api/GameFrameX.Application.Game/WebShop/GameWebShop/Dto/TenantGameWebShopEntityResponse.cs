namespace GameFrameX.Application.Game.WebShop.GameWebShop.Dto;

/// <summary>
/// 游戏邮件输出参数
/// </summary>
public class TenantGameWebShopEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameWebShopEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// 详细描述
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 应用Id
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 商品图
    /// </summary>
    public string Logo { get; set; } = null!;

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
    /// 是否上架
    /// </summary>
    public bool IsUp { get; set; }

    /// <summary>
    /// 销量
    /// </summary>
    public int? Sales { get; set; }

    /// <summary>
    /// 权重
    /// </summary>
    public int? Weight { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public long CreatedTime { get; set; }

    /// <summary>
    /// 创建人
    /// </summary>
    public string? CreatedName { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public long? UpdateTime { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public string? UpdatedName { get; set; }

    /// <summary>
    /// 行版本号（乐观锁）
    /// </summary>
    public long? RowVersion { get; set; }
}