using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.AntiCheat.TenantGameAntiCheatEvent.Dto;

/// <summary>
/// 反作弊事件分页查询输入。
/// </summary>
public class PageTenantGameAntiCheatEventRequest : BasePageRequest
{
    /// <summary>
    /// 应用 Id。应用级反作弊事件分页必须显式传入当前应用，缺失或小于等于 0 会被服务层拒绝。
    /// </summary>
    public long? AppId { get; set; }

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