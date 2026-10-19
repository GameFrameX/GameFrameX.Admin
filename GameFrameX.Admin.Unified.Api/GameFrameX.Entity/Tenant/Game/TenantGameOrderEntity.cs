using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏订单
/// </summary>
[SugarTable(null, "游戏订单")]
[SugarIndex("ux_game_order_apple_transaction_id", nameof(AppleTransactionId), OrderByType.Asc, true)]
// ProviderTransactionId 唯一约束保持不变：第三方支付渠道（AliPay trade_no / Xsolla order.id / Google OrderId / Apple AppleTransactionId）
// 由 Provider 自身全局唯一，加入 AppId 反而会放宽约束导致跨应用潜在重复处理风险，故不引入 AppId 复合（详见 app-scope-backend-foundation.md 3.4 与本 change notes.md）。
[SugarIndex("ux_game_order_provider_transaction", nameof(PayChannel), OrderByType.Asc, nameof(ProviderTransactionId), OrderByType.Asc, true)]
[SugarIndex("idx_game_order_apple_original_transaction_id", nameof(AppleOriginalTransactionId), OrderByType.Asc)]
[SugarIndex("idx_tenant_game_order_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
// 首页聚合「今日订单」按 CreatedTime 过滤计数走索引（C321 / GFX-730）
[SugarIndex("idx_tenant_game_order_created_time", nameof(CreatedTime), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameOrderEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Client API 下单由 <c>AppResolutionMiddleware.AppIdKey</c> 写入；网页商城回调由路由 <c>{appId}</c> 写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 订单状态
    /// </summary>
    [SugarColumn(ColumnDescription = "订单状态", DefaultValue = "0")]
    public OrderStatusEnum Status { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    [SugarColumn(ColumnDescription = "角色ID")]
    public string RoleId { get; set; } = null!;

    /// <summary>
    /// 金钱数量
    /// </summary>
    [SugarColumn(ColumnDescription = "金钱数量", DefaultValue = "0")]
    public uint Amount { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [SugarColumn(ColumnDescription = "内容", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string Content { get; set; } = null!;

    /// <summary>
    /// 区服Id
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 第三方订单id
    /// </summary>
    [SugarColumn(ColumnDescription = "第三方订单id", IsNullable = true)]
    public string OrderId { get; set; } = null!;

    /// <summary>
    /// 商品Id
    /// </summary>
    [SugarColumn(ColumnDescription = "商品", DefaultValue = "0")]
    public long Sku { get; set; }

    /// <summary>
    /// 渠道商品 ID。旧 GameUserPay 协议使用字符串商品号；常规订单继续使用 <see cref="Sku"/> 数值商品 ID。
    /// </summary>
    [SugarColumn(ColumnDescription = "渠道商品ID", Length = EntityStringLength.Length256, IsNullable = true)]
    public string ProductId { get; set; } = null!;

    /// <summary>
    /// 支付渠道,由客户端传入
    /// </summary>
    [SugarColumn(ColumnDescription = "支付渠道")]
    public string PayChannel { get; set; } = null!;

    /// <summary>
    /// 客户端所属渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端所属渠道")]
    public string Channel { get; set; } = null!;

    /// <summary>
    /// 客户端子渠道
    /// </summary>
    [SugarColumn(ColumnDescription = "客户端所属子渠道", IsNullable = true)]
    public string SubChannel { get; set; } = null!;

    /// <summary>
    /// 全局平台
    /// </summary>
    [SugarColumn(ColumnDescription = "全局平台")]
    public string Platform { get; set; } = null!;

    /// <summary>
    /// 国家/地区代码
    /// </summary>
    [SugarColumn(ColumnDescription = "国家/地区代码", Length = EntityStringLength.Length256, IsNullable = true)]
    public string Country { get; set; } = null!;

    /// <summary>
    /// 全局平台
    /// </summary>
    [SugarColumn(ColumnDescription = "是否为沙盒")]
    public bool? IsSandbox { get; set; }

    /// <summary>
    /// 所处的关卡
    /// </summary>
    [SugarColumn(ColumnDescription = "所处的关卡")]
    public string AdventureSuffix { get; set; } = null!;

    /// <summary>
    /// 所属账号
    /// </summary>
    [SugarColumn(ColumnDescription = "所属账号")]
    public string AccountId { get; set; } = null!;


    /// <summary>
    /// 角色注册时间
    /// </summary>
    [SugarColumn(ColumnDescription = "角色注册时间")]
    public long? RoleRegisterTime { get; set; }

    /// <summary>
    /// Apple 交易ID
    /// </summary>
    [SugarColumn(ColumnDescription = "Apple交易ID", IsNullable = true)]
    public string AppleTransactionId { get; set; } = null!;

    /// <summary>
    /// Apple 原始交易ID
    /// </summary>
    [SugarColumn(ColumnDescription = "Apple原始交易ID", IsNullable = true)]
    public string AppleOriginalTransactionId { get; set; } = null!;

    /// <summary>
    /// Apple 收据环境
    /// </summary>
    [SugarColumn(ColumnDescription = "Apple收据环境", IsNullable = true)]
    public string AppleReceiptEnvironment { get; set; } = null!;

    /// <summary>
    /// Apple 验证时间
    /// </summary>
    [SugarColumn(ColumnDescription = "Apple验证时间", IsNullable = true)]
    public long? AppleVerifiedTime { get; set; }

    /// <summary>
    /// 第三方支付交易号（幂等去重）。AliPay=trade_no / Xsolla=order.id / Google=OrderId；Apple 复用 AppleTransactionId。
    /// 配合 (PayChannel, ProviderTransactionId) 复合唯一约束，数据库层面防重复处理。
    /// </summary>
    [SugarColumn(ColumnDescription = "第三方支付交易号", IsNullable = true)]
    public string ProviderTransactionId { get; set; } = null!;

    /// <summary>
    /// 验真结果留档（渠道验真输出 JSON；未验真为 NULL）
    /// </summary>
    [SugarColumn(ColumnDescription = "验真结果留档", ColumnDataType = StaticConfig.CodeFirst_BigString, IsNullable = true)]
    public string ReceiptJson { get; set; } = null!;

    /// <summary>
    /// 实付金额（分；GameUserPay Report 侧回填）
    /// </summary>
    [SugarColumn(ColumnDescription = "实付金额(分)", IsNullable = true)]
    public long? RealMoney { get; set; }

    /// <summary>
    /// 币种（GameUserPay Report 侧回填）
    /// </summary>
    [SugarColumn(ColumnDescription = "币种", Length = EntityStringLength.Length256, IsNullable = true)]
    public string Currency { get; set; } = null!;

    /// <summary>
    /// 下单时玩家关卡（GameUserPay Report 侧回填）
    /// </summary>
    [SugarColumn(ColumnDescription = "下单时玩家关卡", IsNullable = true)]
    public int? Level { get; set; }

    /// <summary>
    /// 支付场景（GameUserPay Report 侧回填）
    /// </summary>
    [SugarColumn(ColumnDescription = "支付场景", Length = EntityStringLength.Length256, IsNullable = true)]
    public string StrScene { get; set; } = null!;
}