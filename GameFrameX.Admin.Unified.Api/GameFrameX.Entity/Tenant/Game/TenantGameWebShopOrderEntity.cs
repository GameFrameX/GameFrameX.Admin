using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 网页商城,只有网页商城发送失败才会新增
/// </summary>
[SugarTable(null, "网页商城订单")]
[TenantTable]
[IncrementTable]
public class TenantGameWebShopOrderEntity : EntityBasicSelect
{

    /// <summary>
    /// 网页商城商品id
    /// </summary>
    [SugarColumn(ColumnDescription = "网页商城商品id", DefaultValue = "0")]
    public long GameShopId { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    [SugarColumn(ColumnDescription = "角色id", DefaultValue = "0")]
    public long RoleId { get; set; }

    /// <summary>
    /// 区服id
    /// </summary>
    [SugarColumn(ColumnDescription = "区服id", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 失败原因
    /// </summary>
    [SugarColumn(ColumnDescription = "失败原因")]
    public string Reason { get; set; } = null!;

    /// <summary>
    /// 是否已经补发
    /// </summary>
    [SugarColumn(ColumnDescription = "是否补发", DefaultValue = "false")]
    public bool OrderStatus { get; set; }

    /// <summary>
    /// 订单id
    /// </summary>
    [SugarColumn(ColumnDescription = "订单id")]
    public string OrderId { get; set; } = null!;
}