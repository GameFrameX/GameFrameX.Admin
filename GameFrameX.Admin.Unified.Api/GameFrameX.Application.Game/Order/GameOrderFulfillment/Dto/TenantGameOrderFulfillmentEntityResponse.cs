using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Order.GameOrderFulfillment.Dto;

/// <summary>
/// 权益发货账本输出参数（C120）。投影暴露实体全字段：
/// Type/Status/FailureReason/IdempotencyKey/OperatorId/ItemsJson/订单快照（OrderId/PayChannel/ProviderTransactionId/Sku/RoleId/AreaId/AccountId/Amount）/审计列。
/// </summary>
public class TenantGameOrderFulfillmentEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameOrderFulfillmentEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），跟随订单 AppId，必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 来源订单 Id（TenantGameOrderEntity.Id）。
    /// </summary>
    public long OrderId { get; set; }

    /// <summary>
    /// 支付渠道/Provider（alipay/apple/google/xsolla），来自订单。
    /// </summary>
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 第三方支付交易号，来自订单，组成幂等键。可空（无第三方交易号时幂等键退化为 order{OrderId}）。
    /// </summary>
    public string ProviderTransactionId { get; set; } = null!;

    /// <summary>
    /// 商品 Id，来自订单 Sku。
    /// </summary>
    public long Sku { get; set; }

    /// <summary>
    /// 角色 Id，来自订单。
    /// </summary>
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 区服 Id，来自订单。
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 账号，来自订单。
    /// </summary>
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// 金额（分），来自订单。
    /// </summary>
    public uint Amount { get; set; }

    /// <summary>
    /// 道具明细 JSON（PropertyItemResponse[] 序列化），来自订单 Content 反序列化的 RewardsItem。
    /// </summary>
    public string ItemsJson { get; set; } = null!;

    /// <summary>
    /// 发放类型（Purchase/RefundAsync/CompensateAsync）。
    /// </summary>
    public FulfillmentTypeEnum Type { get; set; }

    /// <summary>
    /// 发放状态（Pending/Success/Failed/Voided）。
    /// </summary>
    public FulfillmentStatusEnum Status { get; set; }

    /// <summary>
    /// 幂等键。
    /// </summary>
    public string IdempotencyKey { get; set; } = null!;

    /// <summary>
    /// 失败原因（仅 Failed 状态写）。
    /// </summary>
    public string FailureReason { get; set; } = null!;

    /// <summary>
    /// 操作人。0=系统自动（支付回调触发）；&gt;0=人工操作（Admin 用户 Id）。
    /// </summary>
    public long OperatorId { get; set; }

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