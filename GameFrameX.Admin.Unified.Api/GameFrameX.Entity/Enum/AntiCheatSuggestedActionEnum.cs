namespace GameFrameX.Entity.Enum;

/// <summary>
/// 反作弊建议处置（反作弊事件账本 TenantGameAntiCheatEventEntity.SuggestedAction）。
/// <para>严格锁定 4 值：这是 Provider 上报的<strong>建议</strong>，<strong>永不自动落处罚</strong>，必须经人工复核（Review）才能影响后续。
/// 建议集合与处罚登录阻断子集（Ban/RestrictLogin）对齐，便于复核通过后在 C11-D-followup 中映射为处罚。</para>
/// </summary>
[Description("反作弊建议处置")]
[AdminEnum]
public enum AntiCheatSuggestedActionEnum
{
    /// <summary>
    /// 无建议。
    /// </summary>
    [Description("无建议")] None = 1,

    /// <summary>
    /// 标记观察（仅记录，不阻断）。
    /// </summary>
    [Description("标记观察")] Flag = 2,

    /// <summary>
    /// 建议限制登录。
    /// </summary>
    [Description("建议限制登录")] RestrictLogin = 3,

    /// <summary>
    /// 建议封禁。
    /// </summary>
    [Description("建议封禁")] Ban = 4,
}