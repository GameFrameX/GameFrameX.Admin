using GameFrameX.Entity.Base;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 游戏兑换码
/// </summary>
[SugarTable(null, "游戏兑换码")]
[SugarIndex("idx_tenant_game_redeem_code_tenant_app", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameRedeemCodeEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// <para>来源：Admin 创建兑换码由后台当前应用上下文写入。</para>
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    [SugarColumn(ColumnDescription = "区服ID", DefaultValue = "0")]
    public long AreaId { get; set; }

    /// <summary>
    /// 兑换码渠道ID
    /// </summary>
    [SugarColumn(ColumnDescription = "兑换码渠道ID")]
    public long? ChannelId { get; set; }

    /// <summary>
    /// 兑换码渠道
    /// </summary>
    [Navigate(NavigateType.OneToOne, nameof(ChannelId))]
    public TenantGameRedeemCodeChannelEntity Channel { get; set; } = null!;

    /// <summary>
    /// 礼包码的唯一字符串。
    /// </summary>
    [SugarColumn(ColumnDescription = "兑换码")]
    public string Code { get; set; } = null!;

    /// <summary>
    /// 对礼包内容的描述。
    /// </summary>
    [SugarColumn(ColumnDescription = "描述")]
    public string Description { get; set; } = null!;

    /// <summary>
    /// 表示礼包码是否有效。
    /// </summary>
    [SugarColumn(ColumnDescription = "是否激活", DefaultValue = "false")]
    public bool IsActive { get; set; }

    /// <summary>
    /// 礼包码的可使用次数限制。
    /// </summary>
    [SugarColumn(ColumnDescription = "可使用次数", DefaultValue = "0")]
    public int UsageLimit { get; set; }

    /// <summary>
    /// 现在已使用次数。
    /// </summary>
    [SugarColumn(ColumnDescription = "已使用次数", DefaultValue = "0")]
    public int UsedCount { get; set; }

    /// <summary>
    /// 礼包码有效的开始时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "有效开始时间", DefaultValue = "0")]
    public long StartTime { get; set; }

    /// <summary>
    /// 礼包码有效的结束时间。
    /// </summary>
    [SugarColumn(ColumnDescription = "失效时间", DefaultValue = "0")]
    public long EndTime { get; set; }

    /// <summary>
    /// 礼包中包含的物品或权益的详细列表。
    /// </summary>
    [SugarColumn(ColumnDescription = "礼包内容")]
    public string Contents { get; set; } = null!; // 可能是JSON序列化的字符串
}