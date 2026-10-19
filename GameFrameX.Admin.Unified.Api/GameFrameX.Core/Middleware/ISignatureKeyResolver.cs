namespace GameFrameX.Core.Middleware;

/// <summary>
/// App 传输签名密钥环解析器（change C2 sig-keyring-server）。
/// <para>v2 请求按 (AppId, Kid) 精确择 key；v1（无 Kid）过渡期取 App 域全部生效 key 遍历，
/// 由中间件逐把尝试 + C1 常量回退。租约（NotBefore/Expire）与启用判定在每次请求时以当前时刻重新计算。</para>
/// </summary>
public interface ISignatureKeyResolver
{
    /// <summary>
    /// 按 (AppId, Kid) 精确查找密钥，返回含租约/启用判定结果的查找结果（区分 60102 未知与 60103 禁用/租约外）。
    /// </summary>
    /// <param name="appId">应用 ID（密钥域）。</param>
    /// <param name="kid">v2 签名头中的 keyId。</param>
    Task<SignatureKeyLookup> ResolveAsync(long appId, string kid);

    /// <summary>
    /// 取 App 域全部生效密钥（启用 + 租约内），按创建时间倒序（最新优先），供 v1 无 Kid 请求遍历。
    /// </summary>
    /// <param name="appId">应用 ID（密钥域）。</param>
    Task<IReadOnlyList<SignatureKeyMaterial>> GetActiveKeysAsync(long appId);
}