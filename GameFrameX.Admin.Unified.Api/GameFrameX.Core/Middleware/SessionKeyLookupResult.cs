namespace GameFrameX.Core.Middleware;

/// <summary>
/// 会话密钥查找结果（不含解密结论，仅密钥状态判定）。
/// <para>失败状态与信封中间件拒收信封码的映射：NotFound → 60104（kid 未知）；
/// Disabled / NotYetValid / Expired → 60105（key 禁用或租约到期）。</para>
/// </summary>
public enum SessionKeyLookupResult
{
    /// <summary>
    /// 生效中：启用且租约内（NotBefore ≤ now &lt; Expire），可用于解密。
    /// </summary>
    Active = 0,

    /// <summary>
    /// 密钥域内不存在该 Kid 的 key（含 AppId 不匹配），拒收 60104。
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// key 已被禁用（IsEnabled = false），拒收 60105。
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// 租约未生效（now &lt; NotBefore），拒收 60105；防止密钥提前泄出被使用。
    /// </summary>
    NotYetValid = 3,

    /// <summary>
    /// 租约已到期（now ≥ Expire），拒收 60105；到期退场是 Rotate 轮换的主要路径。
    /// </summary>
    Expired = 4,
}