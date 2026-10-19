using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.AntiCheat.TenantGameAntiCheatEvent.Dto;

/// <summary>
/// 反作弊事件输出投影。
/// </summary>
public class TenantGameAntiCheatEventResponse
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public long AppId { get; set; }

    public AntiCheatProviderEnum Provider { get; set; }

    public string? ProviderName { get; set; }

    public string? ProviderEventId { get; set; }

    public AntiCheatTargetTypeEnum TargetType { get; set; }

    public string TargetId { get; set; } = null!;

    public RiskLevelEnum RiskLevel { get; set; }

    public string EvidenceSummary { get; set; } = null!;

    public AntiCheatSuggestedActionEnum SuggestedAction { get; set; }

    public string? SuggestedActionNote { get; set; }

    public ManualReviewStateEnum ManualReviewState { get; set; }

    public long? ReviewerId { get; set; }

    public long? ReviewedTime { get; set; }

    public string? ReviewDecision { get; set; }

    public string? ReviewNote { get; set; }

    public long OccurredTime { get; set; }

    public string IdempotencyKey { get; set; } = null!;

    public long? RowVersion { get; set; }

    public long? CreatedTime { get; set; }

    public long? UpdateTime { get; set; }
}