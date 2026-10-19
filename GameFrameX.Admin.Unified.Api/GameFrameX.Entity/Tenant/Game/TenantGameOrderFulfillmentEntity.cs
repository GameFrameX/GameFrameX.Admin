using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 权益发货账本（权益事实源）。记录每一笔权益发放的完整生命周期：
/// 来源订单、Provider、商品、道具明细、角色/区服/账号、发放类型与状态、幂等键、失败原因、操作人审计。
/// <para>应用作用域事实数据（TenantId + AppId），AppId 跟随订单。账本状态机独立于订单 OrderStatusEnum。</para>
/// <para>幂等：ux_fulfillment_idempotency 在数据库层保证 IdempotencyKey 全局唯一，重复回调/重发不重复入账。</para>
/// </summary>
[SugarTable(null, "权益发货账本")]
[SugarIndex("ux_fulfillment_idempotency", nameof(IdempotencyKey), OrderByType.Asc, true)]
[SugarIndex("idx_fulfillment_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[SugarIndex("idx_fulfillment_order", nameof(OrderId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameOrderFulfillmentEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），跟随订单 AppId，必须 &gt; 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 来源订单 Id（TenantGameOrderEntity.Id）。
    /// </summary>
    [SugarColumn(ColumnDescription = "来源订单", DefaultValue = "0")]
    public long OrderId { get; set; }

    /// <summary>
    /// 支付渠道/Provider（alipay/apple/google/xsolla），来自订单。
    /// </summary>
    [SugarColumn(ColumnDescription = "支付渠道", Length = EntityStringLength.Length256)]
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 第三方支付交易号，来自订单，组成幂等键。可空（无第三方交易号时幂等键退化为 order{OrderId}）。
    /// </summary>
    [SugarColumn(ColumnDescription = "第三方支付交易号", IsNullable = true)]
    public string ProviderTransactionId { get; set; } = null!;

    /// <summary>
    /// 商品 Id，来自订单 Sku。
    /// </summary>
    [SugarColumn(ColumnDescription = "商品", DefaultValue = "0")]
    public long Sku { get; set; }

    /// <summary>
    /// 角色 Id，来自订单。
    /// </summary>
    [SugarColumn(ColumnDescription = "角色ID")]
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 区服 Id，来自订单。
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 账号，来自订单。
    /// </summary>
    [SugarColumn(ColumnDescription = "账号")]
    public string AccountId { get; set; } = null!;

    /// <summary>
    /// 金额（分），来自订单。
    /// </summary>
    [SugarColumn(ColumnDescription = "金额", DefaultValue = "0")]
    public uint Amount { get; set; }

    /// <summary>
    /// 道具明细 JSON（PropertyItemResponse[] 序列化），来自订单 Content 反序列化的 RewardsItem。
    /// </summary>
    [SugarColumn(ColumnDescription = "道具明细", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string ItemsJson { get; set; } = null!;

    /// <summary>
    /// 发放类型（Purchase/Refund/Compensate）。
    /// </summary>
    [SugarColumn(ColumnDescription = "发放类型", DefaultValue = "0")]
    public FulfillmentTypeEnum Type { get; set; }

    /// <summary>
    /// 发放状态（Pending/Success/Failed/Voided）。
    /// </summary>
    [SugarColumn(ColumnDescription = "发放状态", DefaultValue = "0")]
    public FulfillmentStatusEnum Status { get; set; }

    /// <summary>
    /// 幂等键。组成规则：
    /// <para>Purchase: purchase:{PayChannel}:{ProviderTransactionId}（ProviderTransactionId 为空退化为 purchase:{PayChannel}:order{OrderId}）</para>
    /// <para>Refund: refund:{PayChannel}:{ProviderTransactionId}（同上退化规则）</para>
    /// <para>Compensate: compensate:{OrderId}:{OperatorId}:{Guid前12位}（人工补偿每次新记录，含操作人审计）</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "幂等键", Length = EntityStringLength.Length512)]
    public string IdempotencyKey { get; set; } = null!;

    /// <summary>
    /// 失败原因（仅 Failed 状态写）。
    /// </summary>
    [SugarColumn(ColumnDescription = "失败原因", Length = EntityStringLength.Length2048, IsNullable = true)]
    public string FailureReason { get; set; } = null!;

    /// <summary>
    /// 操作人。0=系统自动（支付回调触发）；&gt;0=人工操作（Admin 用户 Id）。
    /// </summary>
    [SugarColumn(ColumnDescription = "操作人", DefaultValue = "0")]
    public long OperatorId { get; set; }
}