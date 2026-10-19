namespace GameFrameX.Application.Game.ServerManager.Online;

/// <summary>
/// Admin→Online 调用作用域上下文（来源 change C339 / vault:C2 S1.7）。
/// <para>
/// 作用域三元组 <c>TenantId / AppId / ServerId</c>：Admin 调用 Online Runtime 的每一次请求都必须携带，
/// 以鉴权与服务端路由为准，客户端字段不可覆盖（vault C2 公共请求契约）。
/// 本类型只由 <see cref="OnlineScopeContextFactory"/> 构造：
/// <c>TenantId</c> 强制取 Admin 鉴权上下文（调用方没有任何入参可以覆盖它）；
/// <c>AppId / ServerId</c> 必填且经 <c>TenantGameAppEntity / TenantGameAreaEntity</c> 租户归属校验，
/// 越界（跨租户 / 跨 App / 区服不属于该 App）在构造阶段即被拒绝并抛 3xxx 镜像码，越界请求成功次数恒为 0。
/// </para>
/// <para><c>RequestSummary</c> 为请求摘要（调用 action / 用途描述），供审计与幂等键上下文使用，不参与归属判定。</para>
/// </summary>
public sealed class OnlineScopeContext
{
    /// <summary>
    /// 租户 Id（来源：Admin 鉴权上下文强制注入，调用方不可覆盖）。
    /// </summary>
    public long TenantId { get; }

    /// <summary>
    /// 游戏应用 Id（经 TenantGameAppEntity 租户归属校验）。
    /// </summary>
    public long AppId { get; }

    /// <summary>
    /// 区服 Id（经 TenantGameAreaEntity 租户归属校验，且区服必须归属该 AppId）。
    /// </summary>
    public long ServerId { get; }

    /// <summary>
    /// 请求摘要（action / 用途描述，审计辅助信息）。
    /// </summary>
    public string RequestSummary { get; }

    internal OnlineScopeContext(long tenantId, long appId, long serverId, string requestSummary)
    {
        TenantId = tenantId;
        AppId = appId;
        ServerId = serverId;
        RequestSummary = requestSummary ?? string.Empty;
    }
}
