namespace GameFrameX.Application.Game.Order.GameWebShopOrder.Dto;

/// <summary>
/// 网页商城订单输出参数
/// </summary>
public class TenantGameWebShopOrderEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameWebShopOrderEntity>
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
    /// 网页商城商品id
    /// </summary>
    public long GameShopId { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 区服id
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 是否已经补发
    /// </summary>
    public bool OrderStatus { get; set; }

    /// <summary>
    /// 订单id
    /// </summary>
    public string OrderId { get; set; } = null!;

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