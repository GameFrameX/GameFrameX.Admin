namespace GameFrameX.Core.Middleware;

/// <summary>
/// 签名密钥查找结果（不含验签结论，仅密钥状态判定）。
/// <para>失败状态与中间件拒签信封码的映射：NotFound → 60102（keyId 未知）；
/// Disabled / NotYetValid / Expired → 60103（key 禁用或租约到期）。</para>
/// </summary>
public enum SignatureKeyLookupResult
{
    /// <summary>
    /// 生效中：启用且租约内（NotBefore ≤ now &lt; Expire），可用于验签。
    /// </summary>
    Active = 0,

    /// <summary>
    /// 密钥域内不存在该 Kid 的 key（含 AppId 不匹配），拒签 60102。
    /// </summary>
    NotFound = 1,

    /// <summary>
    /// key 已被禁用（IsEnabled = false），拒签 60103。
    /// </summary>
    Disabled = 2,

    /// <summary>
    /// 租约未生效（now &lt; NotBefore），拒签 60103；防止密钥提前泄出被使用。
    /// </summary>
    NotYetValid = 3,

    /// <summary>
    /// 租约已到期（now ≥ Expire），拒签 60103；到期退场是 Rotate 轮换的主要路径。
    /// </summary>
    Expired = 4,
}