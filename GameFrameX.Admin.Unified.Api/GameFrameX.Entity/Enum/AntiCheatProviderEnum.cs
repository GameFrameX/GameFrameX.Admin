namespace GameFrameX.Entity.Enum;

/// <summary>
/// 反作弊 / 广告奖励 Provider 来源（反作弊事件账本 TenantGameAntiCheatEventEntity.Provider）。
/// <para>严格锁定 4 值：与广告回调 Provider（C11-B / C114）对齐，避免任意 Provider 字符串；非枚举内的来源统一用 Custom 并在 ProviderName 字段记录实际名称。</para>
/// </summary>
[Description("反作弊Provider")]
[AdminEnum]
public enum AntiCheatProviderEnum
{
    /// <summary>
    /// TopOn 聚合反作弊 / 广告奖励。
    /// </summary>
    [Description("TopOn")] TopOn = 1,

    /// <summary>
    /// Google AdMob。
    /// </summary>
    [Description("GoogleAdMob")] GoogleAdMob = 2,

    /// <summary>
    /// Unity Ads。
    /// </summary>
    [Description("UnityAds")] UnityAds = 3,

    /// <summary>
    /// 自定义 Provider（名称记录在实体 ProviderName 字段）。
    /// </summary>
    [Description("自定义")] Custom = 99,
}