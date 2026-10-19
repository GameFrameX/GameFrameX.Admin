namespace GameFrameX.Application.System.Tenant.Device.AppScope;

/// <summary>
/// 游戏应用归属租户校验扩展（设备域专用）。
/// <para>逻辑与 <c>GameFrameX.Application.Game.App.AppScope.GameAppOwnershipValidator</c> 逐行对齐：
/// <c>appId &lt;= 0</c> 抛业务异常；仓储租户隔离下查不到应用即视为不属于当前租户。</para>
/// <para>存在跨模块副本的依据：C194 已确立"跨 Application 边界各自实现应用归属校验"先例（<c>Application.Client</c> 的
/// <c>TenantGameAppVersionDefinitionService</c> 同样保留 private 实现）。<c>Application.System</c> 与 <c>Application.Game</c> 为并列模块、
/// 禁止横向引用（原 C211 对 <c>Application.Tenant</c> 的同款约束随该项目裁撤），故设备域在 <c>Application.System.Tenant.Device</c> 内提供等价入口。</para>
/// <para>后续可将 <c>AppScopeRules</c> / <c>GameAppOwnershipValidator</c> 下沉到 Core 消除跨模块副本（独立基建项）。</para>
/// </summary>
public static class TenantGameAppOwnershipValidator
{
    /// <summary>
    /// 校验 <paramref name="appId"/> 大于 0，且对应的游戏应用存在于当前租户（依赖仓储租户隔离过滤）。
    /// </summary>
    /// <param name="gameAppRepository">游戏应用仓储。</param>
    /// <param name="appId">应用 ID。</param>
    /// <param name="emptyMessage"><paramref name="appId"/> 为空时的业务异常消息（默认「应用Id不能为空」）。</param>
    /// <param name="notFoundMessage">应用不存在时的业务异常消息（默认「游戏应用不存在」）。</param>
    public static async Task ValidateGameAppBelongsToTenantAsync(
        this SqlSugarRepository<GameFrameX.Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository,
        long appId,
        string emptyMessage = "应用Id不能为空",
        string notFoundMessage = "游戏应用不存在")
    {
        if (gameAppRepository == null)
        {
            throw new ArgumentNullException(nameof(gameAppRepository));
        }

        if (appId <= 0)
        {
            throw BusinessError.Oh(emptyMessage);
        }

        var gameApp = await gameAppRepository.GetFirstAsync(u => u.Id == appId);
        if (gameApp == null)
        {
            throw BusinessError.Oh(notFoundMessage);
        }
    }
}