using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.CommerceCenter.GameOrderFulfillment.Dto;

/// <summary>
/// 权益发货账本分页查询输入参数。过滤字段严格锁定见 change BuildPageQuery 表。
/// </summary>
public class PageGameOrderFulfillmentEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 来源订单 Id（账本 OrderId 为 long，非订单表的字符串 OrderId）。
    /// </summary>
    public long? OrderId { get; set; }

    /// <summary>
    /// 角色 Id。
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 支付渠道/Provider（模糊匹配）。
    /// </summary>
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 第三方支付交易号（等值匹配）。
    /// </summary>
    public string ProviderTransactionId { get; set; } = null!;

    /// <summary>
    /// 发放类型（Purchase/RefundAsync/CompensateAsync）。
    /// </summary>
    public FulfillmentTypeEnum? Type { get; set; }

    /// <summary>
    /// 发放状态（账本 FulfillmentStatusEnum，非 OrderStatusEnum）。
    /// </summary>
    public FulfillmentStatusEnum? Status { get; set; }

    /// <summary>
    /// 商品 Id。
    /// </summary>
    public long? Sku { get; set; }

    /// <summary>
    /// 账号。
    /// </summary>
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// 操作人（&gt;0 人工 / =0 系统自动）。
    /// </summary>
    public long? OperatorId { get; set; }

    /// <summary>
    /// 创建时间范围（[0]=起始, [1]=结束）。
    /// </summary>
    public List<long?> CreateTimeRange { get; set; } = null!;
}
