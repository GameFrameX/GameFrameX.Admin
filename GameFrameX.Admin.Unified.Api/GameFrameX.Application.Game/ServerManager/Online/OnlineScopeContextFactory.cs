namespace GameFrameX.Application.Game.ServerManager.Online;

/// <summary>
/// 验证目标区服存在并构造 Online 调用上下文。
/// </summary>
public sealed class OnlineScopeContextFactory
{
    private readonly SqlSugarRepository<TenantGameAreaEntity> _gameAreaRepository;

    public OnlineScopeContextFactory(SqlSugarRepository<TenantGameAreaEntity> gameAreaRepository)
    {
        _gameAreaRepository = gameAreaRepository;
    }

    /// <summary>区服标识必须为正值，且主库中存在该区服。</summary>
    public async Task<OnlineScopeContext> CreateAsync(long serverId, string requestSummary = "")
    {
        if (serverId <= 0)
        {
            throw BusinessError.Oh(OnlineErrorCode.ScopeMissing);
        }

        var gameArea = await _gameAreaRepository.GetFirstAsync(u => u.Id == serverId);
        if (gameArea == null)
        {
            throw BusinessError.Oh(OnlineErrorCode.ScopeDenied, new { serverId, });
        }

        return new OnlineScopeContext(serverId, requestSummary);
    }
}
