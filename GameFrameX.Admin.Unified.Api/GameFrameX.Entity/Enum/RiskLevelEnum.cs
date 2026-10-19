namespace GameFrameX.Entity.Enum;

/// <summary>
/// 风险等级（反作弊事件账本 TenantGameAntiCheatEventEntity.RiskLevel）。
/// <para>严格锁定 4 值：由 Provider 上报，建议处置强度递增，仅作为人工复核参考，不自动决定处罚。</para>
/// </summary>
[Description("风险等级")]
[AdminEnum]
public enum RiskLevelEnum
{
    /// <summary>
    /// 低风险：建议 Flag 观察。
    /// </summary>
    [Description("低风险")] Low = 1,

    /// <summary>
    /// 中风险：建议观察 / 限制。
    /// </summary>
    [Description("中风险")] Medium = 2,

    /// <summary>
    /// 高风险：建议限制登录。
    /// </summary>
    [Description("高风险")] High = 3,

    /// <summary>
    /// 严重风险：建议封禁。
    /// </summary>
    [Description("严重风险")] Critical = 4,
}