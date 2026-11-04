using GameFrameX.Entity.Enum;

namespace GameFrameX.Application.PlayerCenter.GamePenalty;

/// <summary>
/// 处罚事实源登录阻断查询实现。
/// <para>查询以主库处罚状态和区服范围为准：</para>
/// <para>TargetType = @targetType AND TargetId = @targetId AND Status = Active</para>
/// <para>AND (ExpireTime IS NULL OR ExpireTime &gt; now) AND ActionType IN (BanAsync, RestrictLogin)</para>
/// <para>AND (AreaId IS NULL OR AreaId = @areaId)</para>
/// <para>ORDER BY Id，取第一条。</para>
/// </summary>
public class TenantGamePenaltyEnforcementService : ITenantGamePenaltyEnforcementService
{
    private readonly SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyEntity> _penaltyRepository;

    public TenantGamePenaltyEnforcementService(SqlSugarRepository<Entity.Tenant.Game.TenantGamePenaltyEntity> penaltyRepository)
    {
        _penaltyRepository = penaltyRepository;
    }

    /// <inheritdoc />
    public async Task<Entity.Tenant.Game.TenantGamePenaltyEntity> GetLoginBlockingPenaltyAsync(
        PenaltyTargetTypeEnum targetType, string targetId, long? areaId = null)
    {
        if (string.IsNullOrEmpty(targetId))
        {
#pragma warning disable CS8603 // 接口契约文档允许返回 null（未命中返回 null），跨项目共享接口签名不可改
            return null;
#pragma warning restore CS8603
        }

        var now = TimerHelper.UnixTimeSeconds();
        return await _penaltyRepository.AsQueryable()
                                       .Where(p => p.TargetType == targetType)
                                       .Where(p => p.TargetId == targetId)
                                       .Where(p => p.Status == PenaltyStatusEnum.Active)
                                       .Where(p => p.ExpireTime == null || p.ExpireTime > now)
                                       .Where(p => p.ActionType == PenaltyActionTypeEnum.Ban
                                                   || p.ActionType == PenaltyActionTypeEnum.RestrictLogin)
                                       .Where(p => p.AreaId == null || p.AreaId == areaId)
                                       .OrderBy(p => p.Id)
                                       .FirstAsync();
    }
}