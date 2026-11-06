namespace GameFrameX.Application.CommerceCenter.GameRedeemCodeUseRecord.Dto;

/// <summary>
/// 兑换记录分页查询输入参数
/// </summary>
public class PageGameRedeemCodeUseRecordEntityRequest : BaseAreaPageRequest
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
    /// 兑换码渠道/分组ID（关联 GameRedeemCodeEntity.ChannelId 过滤）
    /// </summary>
    public long? ChannelId { get; set; }

}