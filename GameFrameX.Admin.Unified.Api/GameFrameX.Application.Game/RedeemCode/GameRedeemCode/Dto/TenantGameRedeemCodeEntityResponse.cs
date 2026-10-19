namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCode.Dto;

/// <summary>
/// 兑换码输出参数
/// </summary>
public class TenantGameRedeemCodeEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameRedeemCodeEntity>
{
    /// <summary>
    /// Id
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 应用Id。事实数据应用作用域（TenantId + AppId），必须 &gt; 0。
    /// </summary>
    public long AppId { get; set; }

    /// <summary>
    /// 区服ID
    /// </summary>
    public long AreaId { get; set; }

    /// <summary>
    /// 兑换码渠道ID
    /// </summary>
    public long? ChannelId { get; set; }

    /// <summary>
    /// 礼包码的唯一字符串。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 对礼包内容的描述。
    /// </summary>
    public string Description { get; set; } = null!;

    /// <summary>
    /// 表示礼包码是否有效。
    /// </summary>
    public bool IsActive { get; set; }

    /// <summary>
    /// 礼包码的可使用次数限制。
    /// </summary>
    public int UsageLimit { get; set; }

    /// <summary>
    /// 现在已使用次数。
    /// </summary>
    public int UsedCount { get; set; }

    /// <summary>
    /// 礼包码有效的开始时间。
    /// </summary>
    public long StartTime { get; set; }

    /// <summary>
    /// 礼包码有效的结束时间。
    /// </summary>
    public long EndTime { get; set; }

    /// <summary>
    /// 礼包中包含的物品或权益的详细列表。
    /// </summary>
    public string Contents { get; set; } = null!;

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