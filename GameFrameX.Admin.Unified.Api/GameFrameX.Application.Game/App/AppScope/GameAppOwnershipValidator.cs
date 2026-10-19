namespace GameFrameX.Application.Game.App.AppScope;

/// <summary>
/// 游戏应用归属租户校验扩展。
/// <para>把原散落在 20+ 个 Service 中的 <c>private ValidateGameAppBelongsToTenantAsync</c> 收敛为仓储扩展方法：
/// 调用方直接 <c>await _gameAppRepository.ValidateGameAppBelongsToTenantAsync(appId)</c>，无需各自重复实现。</para>
/// <para>归属判定依赖多租户仓储的租户隔离：查询不到即表示该应用不属于当前租户（与原各 Service 实现语义一致）。</para>
/// </summary>
public static class GameAppOwnershipValidator
{
    /// <summary>
    /// 校验 <paramref name="appId"/> 大于 0，且对应的游戏应用存在于当前租户（依赖仓储租户隔离过滤）。
    /// </summary>
    /// <param name="gameAppRepository">游戏应用仓储。</param>
    /// <param name="appId">应用 ID。</param>
    /// <param name="emptyMessage"><paramref name="appId"/> 为空时的业务异常消息（默认「游戏应用Id不能为空」）。</param>
    /// <param name="notFoundMessage">应用不存在时的业务异常消息（默认「游戏应用不存在」）。</param>
    /// <exception cref="ArgumentNullException"><paramref name="gameAppRepository"/> 为 null。</exception>
    public static async Task ValidateGameAppBelongsToTenantAsync(
        this SqlSugarRepository<GameFrameX.Entity.Tenant.Game.TenantGameAppEntity> gameAppRepository,
        long appId,
        string emptyMessage = "游戏应用Id不能为空",
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