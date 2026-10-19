using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.Game.Tenant.AntiCheat.TenantGameAntiCheatEvent.Dto;

/// <summary>
/// 反作弊事件上报输入（SubmitAsync 接入）。不含复核相关字段——上报一律落 Pending，复核由 ReviewAsync 动作单独驱动。
/// </summary>
public class AddTenantGameAntiCheatEventRequest : BaseAddRequest
{
    /// <summary>
    /// 应用 Id（事实数据应用作用域，跟随上报来源应用）。
    /// </summary>
    [Required(ErrorMessage = "应用Id不能为空")]
    public long AppId { get; set; }

    /// <summary>
    /// 上报来源 Provider。
    /// </summary>
    public AntiCheatProviderEnum Provider { get; set; }

    /// <summary>
    /// Provider 名称。Provider=Custom 时必填；其它可空。
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Provider 侧事件/交易 Id。可空。
    /// </summary>
    public string? ProviderEventId { get; set; }

    /// <summary>
    /// 指向对象类型。
    /// </summary>
    public AntiCheatTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 指向对象标识。
    /// </summary>
    [Required(ErrorMessage = "指向对象标识不能为空")]
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 风险等级。
    /// </summary>
    public RiskLevelEnum RiskLevel { get; set; }

    /// <summary>
    /// 证据摘要（脱敏后的 Provider 原文摘要）。
    /// </summary>
    [Required(ErrorMessage = "证据摘要不能为空")]
    public string EvidenceSummary { get; set; } = null!;

    /// <summary>
    /// Provider 建议处置（仅建议，不自动生效）。
    /// </summary>
    public AntiCheatSuggestedActionEnum SuggestedAction { get; set; }

    /// <summary>
    /// 建议补充说明。可空。
    /// </summary>
    public string? SuggestedActionNote { get; set; }

    /// <summary>
    /// 事件发生时间（Provider 上报）。
    /// </summary>
    public long OccurredTime { get; set; }

    /// <summary>
    /// 幂等键。建议组成：{Provider}:{ProviderEventId}:{TargetType}:{TargetId}:{OccurredTime:O}。
    /// </summary>
    [Required(ErrorMessage = "幂等键不能为空")]
    public string IdempotencyKey { get; set; } = null!;
}