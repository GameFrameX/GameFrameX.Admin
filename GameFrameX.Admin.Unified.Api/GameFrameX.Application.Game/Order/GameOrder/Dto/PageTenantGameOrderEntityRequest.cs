using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Order.GameOrder.Dto;

/// <summary>
/// 游戏支付订单分页查询输入参数
/// </summary>
public class PageTenantGameOrderEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 订单状态
    /// </summary>
    public OrderStatusEnum? Status { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 金钱数量
    /// </summary>
    public int? Amount { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// 第三方订单id
    /// </summary>
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 商品
    /// </summary>
    public long? Sku { get; set; }

    /// <summary>
    /// 支付渠道
    /// </summary>
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 客户端所属渠道
    /// </summary>
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 全局平台
    /// </summary>
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public long? CreateTime { get; set; }

    /// <summary>
    /// 创建时间范围
    /// </summary>
    public List<long?> CreateTimeRange { get; set; } = null!;

    /// <summary>
    /// 是否为沙盒
    /// </summary>
    public bool? IsSandbox { get; set; }

    /// <summary>
    /// 所属账号
    /// </summary>
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// 应用Id。应用级订单分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }
}