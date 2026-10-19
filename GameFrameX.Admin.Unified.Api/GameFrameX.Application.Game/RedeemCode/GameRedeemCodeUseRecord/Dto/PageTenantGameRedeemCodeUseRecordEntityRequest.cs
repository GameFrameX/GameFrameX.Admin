namespace GameFrameX.Application.Game.RedeemCode.GameRedeemCodeUseRecord.Dto;

/// <summary>
/// 兑换记录分页查询输入参数
/// </summary>
public class PageTenantGameRedeemCodeUseRecordEntityRequest : BaseAreaPageRequest
{
    /// <summary>
    /// 兑换码
    /// </summary>
    public string Code { get; set; } = null!;

    /// <summary>
    /// 角色ID
    /// </summary>
    public long? PlayerId { get; set; }

    /// <summary>
    /// 兑换码渠道/分组ID（关联 TenantGameRedeemCodeEntity.ChannelId 过滤）
    /// </summary>
    public long? ChannelId { get; set; }

    /// <summary>
    /// 应用Id（可选过滤，按当前应用筛选兑换记录）
    /// </summary>
    public long? AppId { get; set; }
}