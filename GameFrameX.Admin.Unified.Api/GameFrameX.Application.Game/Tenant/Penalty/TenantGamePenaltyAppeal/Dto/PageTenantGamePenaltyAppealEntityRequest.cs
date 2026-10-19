using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenaltyAppeal.Dto;

/// <summary>
/// 申诉分页查询输入参数。
/// </summary>
public class PageTenantGamePenaltyAppealEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id。应用级处罚申诉分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 关联处罚 Id（可选过滤）。
    /// </summary>
    public long? PenaltyId { get; set; }

    /// <summary>
    /// 申诉状态（可选过滤）。
    /// </summary>
    public AppealStatusEnum? Status { get; set; }
}