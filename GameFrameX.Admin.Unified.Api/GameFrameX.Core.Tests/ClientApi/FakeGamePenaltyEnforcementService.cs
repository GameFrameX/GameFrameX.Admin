using GameFrameX.Application.PlayerCenter.GamePenalty;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// 处罚阻断查询测试替身。默认不阻断（返回 null）；传入 <paramref name="blocking"/> 可模拟命中封禁。
/// </summary>
internal sealed class FakeGamePenaltyEnforcementService : IGamePenaltyEnforcementService
{
    private readonly GamePenaltyEntity _blocking = null!;

    public FakeGamePenaltyEnforcementService(GamePenaltyEntity? blocking = null)
    {
#pragma warning disable CS8601 // 测试替身：blocking 显式允许 null（"无阻断"语义），与 _blocking = null! 初始化器配合
        _blocking = blocking;
#pragma warning restore CS8601
    }

    public Task<GamePenaltyEntity> GetLoginBlockingPenaltyAsync(
        PenaltyTargetTypeEnum targetType, string targetId, long? areaId = null)
    {
        return Task.FromResult(_blocking);
    }
}