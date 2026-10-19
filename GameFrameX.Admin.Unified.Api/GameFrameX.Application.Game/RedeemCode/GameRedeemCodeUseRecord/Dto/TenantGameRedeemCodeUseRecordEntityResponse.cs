namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeUseRecord.Dto;

/// <summary>
/// 兑换记录输出参数
/// </summary>
public class TenantGameRedeemCodeUseRecordEntityResponse : IBasePageResponse<Entity.Tenant.Game.TenantGameRedeemCodeUseRecordEntity>
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
    /// 礼包码的唯一字符串。
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 角色ID
    /// </summary>
    public long PlayerId { get; set; }

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

    /// <summary>
    /// 角色ID
    /// </summary>
    public string PlayerIdString { get; set; } = null!;

    /// <summary>
    /// 兑换码渠道/分组ID（来自关联的 TenantGameRedeemCodeEntity.ChannelId）
    /// </summary>
    public long? ChannelId { get; set; }

    /// <summary>
    /// 兑换码渠道/分组名称（来自关联的 TenantGameRedeemCodeChannelEntity.Name）
    /// </summary>
    public string ChannelName { get; set; } = null!;
}