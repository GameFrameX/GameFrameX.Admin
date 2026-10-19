namespace GameFrameX.Core.Middleware;

/// <summary>
/// App 会话密钥环解析器（change C5 server-envelope-middleware）。
/// <para>加密信封按信封自报 kid 精确择 key（双活轮换窗口内新旧 key 共存，信封自报即所依）；
/// 租约（NotBefore/Expire）与启用判定在每次请求时以当前时刻重新计算。</para>
/// </summary>
public interface ISessionKeyResolver
{
    /// <summary>
    /// 按 (AppId, Kid) 精确查找会话密钥，返回含租约/启用判定结果的查找结果
    /// （区分 60104 未知与 60105 禁用/租约外）。
    /// </summary>
    /// <param name="appId">应用 ID（密钥域）。</param>
    /// <param name="kid">信封 JSON kid 字段。</param>
    Task<SessionKeyLookup> ResolveAsync(long appId, string kid);
}