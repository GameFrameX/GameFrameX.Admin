using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.PlayerCenter.GamePenaltyAppeal.Dto;

/// <summary>
/// 申诉分页查询输入参数。
/// </summary>
public class PageGamePenaltyAppealEntityRequest : BasePageRequest
{
    /// <summary>
    /// 关联处罚 Id（可选过滤）。
    /// </summary>
    public long? PenaltyId { get; set; }

    /// <summary>
    /// 申诉状态（可选过滤）。
    /// </summary>
    public AppealStatusEnum? Status { get; set; }
}