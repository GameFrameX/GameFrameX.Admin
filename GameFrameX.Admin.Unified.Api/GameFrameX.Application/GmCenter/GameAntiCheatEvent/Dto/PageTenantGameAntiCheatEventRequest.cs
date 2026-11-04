using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.GmCenter.GameAntiCheatEvent.Dto;

/// <summary>
/// 反作弊事件分页查询输入。
/// </summary>
public class PageTenantGameAntiCheatEventRequest : BasePageRequest
{
    /// <summary>
    /// Provider 过滤。可空。
    /// </summary>
    public AntiCheatProviderEnum? Provider { get; set; }

    /// <summary>
    /// 风险等级过滤。可空。
    /// </summary>
    public RiskLevelEnum? RiskLevel { get; set; }

    /// <summary>
    /// 人工复核状态过滤。可空。
    /// </summary>
    public ManualReviewStateEnum? ManualReviewState { get; set; }

    /// <summary>
    /// 指向对象类型过滤。可空。
    /// </summary>
    public AntiCheatTargetTypeEnum? TargetType { get; set; }

    /// <summary>
    /// 指向对象标识过滤（精确匹配）。可空。
    /// </summary>
    public string? TargetId { get; set; }
}