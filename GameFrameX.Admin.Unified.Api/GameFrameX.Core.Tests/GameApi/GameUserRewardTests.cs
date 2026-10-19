using GameFrameX.Client.Api.GameUserReward;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Client.Api.GameUserReward.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Client.Api.Services;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C8 T3 RED：奖励域契约（token 幂等同重放同结果、已领 rcode1、check_exist=0 创建、查询只返回存量 key）
/// <para>契约来源：C8 notes T3b 客户端调研（RewardModel.lua:64-85、ApiInterface.lua:1655-1781）。</para>
/// </summary>
public sealed class GameUserRewardTests
{
    [Fact]
    public async Task Receive_CreatesRecordAndReturnsZero_WhenMissingWithCheckExistZero()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var result = await controller.ReceiveRewards(new ReceiveRewardsRequest
        {
            UserId = 3001, Key = "survey_1001", Token = "tok-a", CheckExist = 0,
        });
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(result));
        Assert.Equal(0, data.GetProperty("Rcode").GetInt64());

        // 存量查询能看到已创建记录（status=1 已领）
        var query = await controller.GetUserRewards(new GetUserRewardsRequest
        {
            UserId = 3001, RewardKeys = new List<string> { "survey_1001", "never_created", },
        });
        var rewards = ClientApiResultReader.Data(ClientApiResultReader.Read(query)).GetProperty("Rewards");
        Assert.Equal(1, rewards.GetArrayLength());
        Assert.Equal("survey_1001", rewards[0].GetProperty("Key").GetString());
        Assert.Equal(1, rewards[0].GetProperty("Status").GetInt64());
    }

    [Fact]
    public async Task Receive_TokenReplay_ReturnsSameZeroResult()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        await controller.ReceiveRewards(new ReceiveRewardsRequest { UserId = 3002, Key = "mail_9", Token = "tok-replay", CheckExist = 0, });
        var replay = await controller.ReceiveRewards(new ReceiveRewardsRequest { UserId = 3002, Key = "mail_9", Token = "tok-replay", CheckExist = 0, });
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(replay));
        // 同 token 重放必须同结果（客户端响应丢失重试补发本地奖励，返回 1 会丢奖）
        Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
    }

    [Fact]
    public async Task Receive_DifferentTokenAfterReceived_ReturnsOne()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        await controller.ReceiveRewards(new ReceiveRewardsRequest { UserId = 3003, Key = "op_cp_1_2", Token = "tok-first", CheckExist = 1, });
        var second = await controller.ReceiveRewards(new ReceiveRewardsRequest { UserId = 3003, Key = "op_cp_1_2", Token = "tok-second", CheckExist = 1, });
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(second));
        Assert.Equal(1, data.GetProperty("Rcode").GetInt64());
    }

    private static GameUserRewardController CreateController(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameUserRewardEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        var repository = new Repository<TenantGameUserRewardEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
        return new GameUserRewardController(repository);
    }
}