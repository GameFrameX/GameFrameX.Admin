namespace GameFrameX.Application.ServerCenter.Online;

/// <summary>
/// 验证目标区服存在并构造 Online 调用上下文。
/// </summary>
public sealed class OnlineScopeContextFactory
{
    // S3/C401 断链修复点
    private readonly SqlSugarRepository<GameAreaEntity> _gameAreaRepository;

    public OnlineScopeContextFactory(SqlSugarRepository<GameAreaEntity> gameAreaRepository)
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
    /// <summary>开服登记专用：允许未启用的区服，但仍拒绝已软删区服。</summary>
    public async Task<OnlineScopeContext> CreateForProvisioningAsync(long serverId, string requestSummary = "")
    {
        if (serverId <= 0)
        {
            throw BusinessError.Oh(OnlineErrorCode.ScopeMissing);
        }

        var gameArea = await _gameAreaRepository.AsQueryable().Filter(null, true)
            .Where(area => area.IsDeleted == false).FirstAsync(area => area.Id == serverId);
        if (gameArea == null)
        {
            throw BusinessError.Oh(OnlineErrorCode.ScopeDenied, new { serverId });
        }

        return new OnlineScopeContext(serverId, requestSummary, true);
    }
}
