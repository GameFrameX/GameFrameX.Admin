using GameFrameX.Core.Service.User;

namespace GameFrameX.Application.Game.ServerManager.Online;

/// <summary>
/// <see cref="OnlineScopeContext"/> 唯一构造入口（来源 change C339 / vault:C2 S1.7）。
/// <para>
/// 作用域注入与越界拒绝的业务语义（VC-1.6 / VC-1.7 Admin 半边）：
/// <list type="number">
/// <item><description><c>TenantId</c> 强制取 <see cref="CurrentUserContext"/> 鉴权上下文——方法签名不接收租户参数，
/// 调用方（包括请求体字段）没有任何途径覆盖它；租户上下文缺失（如平台级调用未落租户）抛 <see cref="OnlineErrorCode.ScopeMissing"/>。</description></item>
/// <item><description><c>AppId / ServerId</c> 必填（&gt;0），缺失抛 <see cref="OnlineErrorCode.ScopeMissing"/>；
/// 经多租户隔离仓储查 <c>TenantGameAppEntity / TenantGameAreaEntity</c>，查不到即越界（跨租户 / 跨 App 的 AppId，
/// 租户隔离下与不存在不可区分，统一语义为作用域拒绝）抛 <see cref="OnlineErrorCode.ScopeDenied"/>；
/// 区服存在但不归属该 App（<c>GameAppId</c> 不匹配）抛 <see cref="OnlineErrorCode.ServerScopeDenied"/>。</description></item>
/// <item><description>拒绝均为 <c>BusinessError</c>（3xxx 镜像码 + 中文文案经 ErrorLocalizer），
/// 发生在 Admin action 调用链内时由既有 MonitorLogging 自动落租户操作日志（TenantLogOperationEntity）留痕。</description></item>
/// </list>
/// </para>
/// </summary>
public sealed class OnlineScopeContextFactory
{
    private readonly CurrentUserContext _currentUserContext;
    private readonly SqlSugarRepository<TenantGameAppEntity> _gameAppRepository;
    private readonly SqlSugarRepository<TenantGameAreaEntity> _gameAreaRepository;

    public OnlineScopeContextFactory(CurrentUserContext currentUserContext,
        SqlSugarRepository<TenantGameAppEntity> gameAppRepository,
        SqlSugarRepository<TenantGameAreaEntity> gameAreaRepository)
    {
        _currentUserContext = currentUserContext;
        _gameAppRepository = gameAppRepository;
        _gameAreaRepository = gameAreaRepository;
    }

    /// <summary>
    /// 构造 Admin→Online 调用作用域：TenantId 鉴权强制注入 + AppId/ServerId 租户归属校验，越界/缺失即抛 3xxx 镜像码。
    /// </summary>
    /// <param name="appId">游戏应用 Id（必须归属当前租户）。</param>
    /// <param name="serverId">区服 Id（必须归属当前租户且属于 <paramref name="appId"/> 应用）。</param>
    /// <param name="requestSummary">请求摘要（action / 用途描述，审计辅助信息，可选）。</param>
    /// <exception cref="BusinessException">作用域缺失（<see cref="OnlineErrorCode.ScopeMissing"/>）、
    /// 越界（<see cref="OnlineErrorCode.ScopeDenied"/> / <see cref="OnlineErrorCode.ServerScopeDenied"/>）。</exception>
    public async Task<OnlineScopeContext> CreateAsync(long appId, long serverId, string requestSummary = "")
    {
        // TenantId 只取 Admin 鉴权上下文（强制注入点）：方法签名无租户参数，请求字段不可能覆盖。
        var tenantId = _currentUserContext.TenantId;
        if (tenantId <= 0)
        {
            throw BusinessError.Oh(OnlineErrorCode.ScopeMissing);
        }

        if (appId <= 0 || serverId <= 0)
        {
            throw BusinessError.Oh(OnlineErrorCode.ScopeMissing);
        }

        var gameApp = await _gameAppRepository.GetFirstAsync(u => u.Id == appId);
        if (gameApp == null)
        {
            // 多租户隔离下「不属于本租户」与「不存在」不可区分，统一按作用域越界拒绝。
            throw BusinessError.Oh(OnlineErrorCode.ScopeDenied, new { appId, });
        }

        var gameArea = await _gameAreaRepository.GetFirstAsync(u => u.Id == serverId);
        if (gameArea == null)
        {
            throw BusinessError.Oh(OnlineErrorCode.ScopeDenied, new { serverId, });
        }

        if (gameArea.GameAppId != appId)
        {
            // 区服主归属 = TenantId + GameAppId：区服存在但不属于目标应用，按区服作用域非法拒绝。
            throw BusinessError.Oh(OnlineErrorCode.ServerScopeDenied, new { appId, serverId, });
        }

        return new OnlineScopeContext(tenantId, appId, serverId, requestSummary);
    }
}
