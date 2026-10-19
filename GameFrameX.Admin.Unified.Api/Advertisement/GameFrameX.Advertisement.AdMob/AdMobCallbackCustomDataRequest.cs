namespace GameFrameX.Advertisement.AdMob;

/// <summary>
/// Google AdMob自定义数据
/// </summary>
public sealed class AdMobCallbackCustomDataRequest
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// 角色ID
    /// </summary>
    public string PlayerId { get; set; } = string.Empty;

    /// <summary>
    /// 是否沙盒
    /// </summary>
    public string IsSandbox { get; set; } = string.Empty;

    /// <summary>
    /// 广告插槽ID
    /// </summary>
    public string SlotId { get; set; } = string.Empty;
}