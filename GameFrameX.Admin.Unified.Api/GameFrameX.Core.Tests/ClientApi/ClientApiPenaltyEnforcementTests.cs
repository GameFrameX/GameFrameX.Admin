using GameFrameX.Application.PlayerCenter.GamePenalty;
using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Business.Game;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// 处罚事实源登录阻断查询规则验收（change C115 §3.3 / §3.5 验收）。
/// 铁律：仅命中 ActionType ∈ {BanAsync, RestrictLogin} 的 Active + 未过期处罚；
/// Mute / RestrictTrade / RestrictMatch 永不阻断登录。
/// </summary>
public sealed class ClientApiPenaltyEnforcementTests
{
    [Fact]
    public async Task Ban_ActiveUnexpired_ReturnsPenalty()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GamePenaltyEntity));
        SeedPenalty(db, 1, PenaltyActionTypeEnum.Ban, PenaltyStatusEnum.Active, null, db.AppId);

        var service = new GamePenaltyEnforcementService(db.Repository<GamePenaltyEntity>());
        var hit = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, "user-1001");

        Assert.NotNull(hit);
        Assert.Equal(1, hit.Id);
    }

    [Fact]
    public async Task RestrictLogin_Active_BlocksLogin()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GamePenaltyEntity));
        SeedPenalty(db, 2, PenaltyActionTypeEnum.RestrictLogin, PenaltyStatusEnum.Active, null, db.AppId);

        var service = new GamePenaltyEnforcementService(db.Repository<GamePenaltyEntity>());
        var hit = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, "user-1001");

        Assert.NotNull(hit);
    }

    [Fact]
    public async Task Mute_Active_NeverBlocksLogin()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GamePenaltyEntity));
        SeedPenalty(db, 3, PenaltyActionTypeEnum.Mute, PenaltyStatusEnum.Active, null, db.AppId);

        var service = new GamePenaltyEnforcementService(db.Repository<GamePenaltyEntity>());
        var hit = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, "user-1001");

        Assert.Null(hit);
    }

    [Fact]
    public async Task Ban_Revoked_NeverBlocksLogin()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GamePenaltyEntity));
        SeedPenalty(db, 4, PenaltyActionTypeEnum.Ban, PenaltyStatusEnum.Revoked, null, db.AppId);

        var service = new GamePenaltyEnforcementService(db.Repository<GamePenaltyEntity>());
        var hit = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, "user-1001");

        Assert.Null(hit);
    }

    [Fact]
    public async Task Ban_Expired_NeverBlocksLogin()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GamePenaltyEntity));
        SeedPenalty(db, 5, PenaltyActionTypeEnum.Ban, PenaltyStatusEnum.Active, DateTimeOffset.UtcNow.AddMinutes(-1), db.AppId);

        var service = new GamePenaltyEnforcementService(db.Repository<GamePenaltyEntity>());
        var hit = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, "user-1001");

        Assert.Null(hit);
    }

    [Fact]
    public async Task Ban_MainDatabaseDefault_BlocksAccount()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GamePenaltyEntity));
        SeedPenalty(db, 6, PenaltyActionTypeEnum.Ban, PenaltyStatusEnum.Active, null, 0);

        var service = new GamePenaltyEnforcementService(db.Repository<GamePenaltyEntity>());
        var hit = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Account, "user-1001");

        Assert.NotNull(hit);
    }

    [Fact]
    public async Task Ban_AreaScoped_OnlyMatchesSameArea()
    {
        using var db = new ClientApiTestDb();
        db.InitBusinessTables(typeof(GamePenaltyEntity));
        SeedPenalty(db, 7, PenaltyActionTypeEnum.Ban, PenaltyStatusEnum.Active, null, db.AppId, 2001);

        var service = new GamePenaltyEnforcementService(db.Repository<GamePenaltyEntity>());

        // 同区服命中
        var hitSame = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Role, "role-3001", 2001);
        Assert.NotNull(hitSame);

        // 异区服不命中（AreaId IS NULL OR AreaId = @areaId）
        var hitOther = await service.GetLoginBlockingPenaltyAsync(PenaltyTargetTypeEnum.Role, "role-3001", 9999);
        Assert.Null(hitOther);
    }

    private static void SeedPenalty(
        ClientApiTestDb db,
        long id,
        PenaltyActionTypeEnum actionType,
        PenaltyStatusEnum status,
        DateTimeOffset? expiresAt,
        long appId,
        long? areaId = null)
    {
        var entity = new GamePenaltyEntity
        {
            Id = id,
            AreaId = areaId,
            TargetType = areaId.HasValue ? PenaltyTargetTypeEnum.Role : PenaltyTargetTypeEnum.Account,
            TargetId = areaId.HasValue ? "role-3001" : "user-1001",
            ActionType = actionType,
            Status = status,
            Reason = "test penalty",
            ExpireTime = expiresAt?.ToUnixTimeSeconds(),
            OperatorId = 1,
        };
        db.TenantDb.Insertable(entity).ExecuteCommand();
    }
}