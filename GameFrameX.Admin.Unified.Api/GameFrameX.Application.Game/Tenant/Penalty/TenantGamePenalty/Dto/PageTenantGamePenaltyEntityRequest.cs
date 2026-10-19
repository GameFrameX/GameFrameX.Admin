using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.Penalty.TenantGamePenalty.Dto;

/// <summary>
/// 处罚分页查询输入参数。
/// </summary>
public class PageTenantGamePenaltyEntityRequest : BasePageRequest
{
    /// <summary>
    /// 应用Id。应用级处罚分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }

    /// <summary>
    /// 处罚对象类型（可选过滤）。
    /// </summary>
    public PenaltyTargetTypeEnum? TargetType { get; set; }

    /// <summary>
    /// 对象标识（可选过滤，精确匹配）。
    /// </summary>
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 处罚状态（可选过滤）。
    /// </summary>
    public PenaltyStatusEnum? Status { get; set; }

    /// <summary>
    /// 处罚动作（可选过滤）。
    /// </summary>
    public PenaltyActionTypeEnum? ActionType { get; set; }
}