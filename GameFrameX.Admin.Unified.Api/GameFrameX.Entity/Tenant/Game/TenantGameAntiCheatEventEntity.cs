using GameFrameX.Entity.Base;
using GameFrameX.Entity.Enum;

namespace GameFrameX.Entity.Tenant.Game;

/// <summary>
/// 反作弊事件账本（安全治理事件事实源）。记录反作弊 / 广告奖励 Provider 上报的每一笔风险事件：
/// 来源 Provider、风险等级、指向对象、证据摘要、<strong>建议</strong>处置、人工复核状态。
/// <para>应用作用域事实数据（TenantId + AppId），AppId 跟随上报来源应用。</para>
/// <para>铁律：SuggestedAction 仅是 Provider 建议，<strong>永不自动落处罚</strong>；必须经人工复核（ManualReviewState Pending → Approved/Rejected/Escalated）。
/// 本实体只承载事件与复核状态，不写处罚表——处罚创建/撤销属 C11-C（C115）职责。</para>
/// <para>幂等：ux_anticheat_idempotency 在数据库层保证 IdempotencyKey 唯一，重复上报不重复入账。</para>
/// </summary>
[SugarTable(null, "反作弊事件账本")]
[SugarIndex("ux_anticheat_idempotency", nameof(IdempotencyKey), OrderByType.Asc, true)]
[SugarIndex("idx_anticheat_tenant_app_level", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(RiskLevel), OrderByType.Asc)]
[SugarIndex("idx_anticheat_tenant_app_review", "TenantId", OrderByType.Asc, nameof(AppId), OrderByType.Asc, nameof(ManualReviewState), OrderByType.Asc)]
[TenantTable]
[IncrementTable]
public class TenantGameAntiCheatEventEntity : EntityBasic, IAppScoped
{
    /// <summary>
    /// 应用 Id。事实数据应用作用域（TenantId + AppId），跟随上报来源应用，必须大于 0。
    /// </summary>
    [SugarColumn(ColumnDescription = "应用Id", DefaultValue = "0")]
    public long AppId { get; set; }

    /// <summary>
    /// 上报来源 Provider（TopOn/GoogleAdMob/UnityAds/Custom）。
    /// </summary>
    [SugarColumn(ColumnDescription = "反作弊Provider", DefaultValue = "0")]
    public AntiCheatProviderEnum Provider { get; set; }

    /// <summary>
    /// Provider 名称。Provider=Custom 时必填；其它可空，仅辅助展示。
    /// </summary>
    [SugarColumn(ColumnDescription = "Provider名称", Length = EntityStringLength.Length256, IsNullable = true)]
    public string ProviderName { get; set; } = null!;

    /// <summary>
    /// Provider 侧事件/交易 Id（与 Provider 共同辅助追溯）。可空。
    /// </summary>
    [SugarColumn(ColumnDescription = "Provider事件Id", Length = EntityStringLength.Length256, IsNullable = true)]
    public string ProviderEventId { get; set; } = null!;

    /// <summary>
    /// 指向对象类型（Account/Role/Device/Ip）。
    /// </summary>
    [SugarColumn(ColumnDescription = "指向对象类型", DefaultValue = "0")]
    public AntiCheatTargetTypeEnum TargetType { get; set; }

    /// <summary>
    /// 指向对象标识（账号/角色 Id/设备标识/IP 字符串）。
    /// </summary>
    [SugarColumn(ColumnDescription = "指向对象标识", Length = EntityStringLength.Length256)]
    public string TargetId { get; set; } = null!;

    /// <summary>
    /// 风险等级（Low/Medium/High/Critical）。
    /// </summary>
    [SugarColumn(ColumnDescription = "风险等级", DefaultValue = "0")]
    public RiskLevelEnum RiskLevel { get; set; }

    /// <summary>
    /// 证据摘要（脱敏后的 Provider 原文摘要，不放原始 PII）。
    /// </summary>
    [SugarColumn(ColumnDescription = "证据摘要", ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public string EvidenceSummary { get; set; } = null!;

    /// <summary>
    /// Provider 建议处置（None/Flag/RestrictLogin/Ban）。<strong>仅建议，不自动生效。</strong>
    /// </summary>
    [SugarColumn(ColumnDescription = "建议处置", DefaultValue = "0")]
    public AntiCheatSuggestedActionEnum SuggestedAction { get; set; }

    /// <summary>
    /// 建议补充说明。可空。
    /// </summary>
    [SugarColumn(ColumnDescription = "建议说明", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string SuggestedActionNote { get; set; } = null!;

    /// <summary>
    /// 人工复核状态（Pending/Approved/Rejected/Escalated）。Submit 默认 Pending。
    /// </summary>
    [SugarColumn(ColumnDescription = "人工复核状态", DefaultValue = "0")]
    public ManualReviewStateEnum ManualReviewState { get; set; }

    /// <summary>
    /// 复核人 Id（Admin 用户 Id，来自 CurrentUserContext.UserId）。Review 时写入；未复核为 null。
    /// </summary>
    [SugarColumn(ColumnDescription = "复核人", IsNullable = true)]
    public long? ReviewerId { get; set; }

    /// <summary>
    /// 复核时间（UTC）。Review 时写入。
    /// </summary>
    [SugarColumn(ColumnDescription = "复核时间", IsNullable = true)]
    public long? ReviewedTime { get; set; }

    /// <summary>
    /// 复核结论短码（与 ManualReviewState 对齐：Approved/Rejected/Escalated）。可空。
    /// </summary>
    [SugarColumn(ColumnDescription = "复核结论", Length = EntityStringLength.Length256, IsNullable = true)]
    public string ReviewDecision { get; set; } = null!;

    /// <summary>
    /// 复核备注。可空。
    /// </summary>
    [SugarColumn(ColumnDescription = "复核备注", Length = EntityStringLength.Length1024, IsNullable = true)]
    public string ReviewNote { get; set; } = null!;

    /// <summary>
    /// 事件发生时间（Provider 上报，区别于 CreatedTime 入库时间）。
    /// </summary>
    [SugarColumn(ColumnDescription = "事件发生时间", DefaultValue = "0")]
    public long OccurredTime { get; set; }

    /// <summary>
    /// 幂等键。组成规则建议：{Provider}:{ProviderEventId}:{TargetType}:{TargetId}:{OccurredTime:O}；
    /// 由 Submit 端拼装，ux_anticheat_idempotency 全局唯一，重复上报返回既有事件，不重复入账。
    /// </summary>
    [SugarColumn(ColumnDescription = "幂等键", Length = EntityStringLength.Length256)]
    public string IdempotencyKey { get; set; } = null!;
}