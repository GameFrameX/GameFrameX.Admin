namespace GameFrameX.Entity.Enum;

/// <summary>
/// 处罚动作。处罚事实源（TenantGamePenaltyEntity）的动作分类。
/// <para>严格锁定 5 值；登录阻断集合严格等于 {Ban, RestrictLogin}。</para>
/// <para>禁言与限制发言语义同一，合并为单个 Mute；Mute / RestrictTrade / RestrictMatch 不阻断登录与支付，仅落账。</para>
/// </summary>
[Description("处罚动作")]
[AdminEnum]
public enum PenaltyActionTypeEnum
{
    /// <summary>
    /// 封禁：阻断登录 + 票据（登录阻断集合成员）。
    /// </summary>
    [Description("封禁")] Ban = 1,

    /// <summary>
    /// 限制登录：阻断登录 + 票据（登录阻断集合成员）。
    /// </summary>
    [Description("限制登录")] RestrictLogin = 2,

    /// <summary>
    /// 禁言 / 限制发言：仅落账，不阻断登录 / 支付。
    /// </summary>
    [Description("禁言")] Mute = 3,

    /// <summary>
    /// 限制交易：仅落账。
    /// </summary>
    [Description("限制交易")] RestrictTrade = 4,

    /// <summary>
    /// 限制匹配：仅落账。
    /// </summary>
    [Description("限制匹配")] RestrictMatch = 5,
}