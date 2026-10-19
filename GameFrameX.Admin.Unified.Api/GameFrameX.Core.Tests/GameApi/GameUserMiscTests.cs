using GameFrameX.Client.Api.GameUserArchive;
using GameFrameX.Client.Api.GameUserMisc;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Client.Api.GameUserArchive.Dto;
using GameFrameX.Client.Api.GameUserMisc.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Client.Api.Services;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C8 T3 RED：杂项域契约（MarkLogout 落库、RandomRobots 数量凑满 + exclude 过滤）
/// <para>契约来源：C8 notes T3b 客户端调研（AccountManager.lua:446、LineRaceActivityModel.lua:277
/// 严格断言 #robots == 请求数）。</para>
/// </summary>
public sealed class GameUserMiscTests
{
    [Fact]
    public async Task MarkLogout_PersistsRecord()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var result = await controller.MarkLogout(new MarkLogoutRequest { UserId = 4001, Version = "1.0.0", });
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(result));
        Assert.Equal(0, data.GetProperty("Rcode").GetInt64());

        var records = db.TenantDb.Queryable<TenantGameUserLogoutRecordEntity>().Where(x => x.UserId == 4001).ToList();
        Assert.Single(records);
        Assert.Equal("1.0.0", records[0].ClientVersion);
        Assert.True(records[0].LogoutTime > 0);
    }

    [Fact]
    public async Task RandomRobots_FillsRequestedCount_AndRespectsExclusions()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameUserArchiveEntity));
        var archiveController = CreateArchiveController(db);
        // 真实档 2 个（4101/4102），exclude 4101 → 请 3 个：真实 1 + 合成 2 凑满
        await archiveController.Upload(MakeUpload(4101, "real_one"));
        await archiveController.Upload(MakeUpload(4102, "real_two"));

        var controller = CreateController(db);
        var result = await controller.RandomRobots(new RandomRobotsRequest
        {
            UserId = 4002,
            ExcludeUserIds = new List<long> { 4101, },
            RobotCount = 3,
        });
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(result));
        Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
        var robots = data.GetProperty("Robots");
        Assert.Equal(3, robots.GetArrayLength());
        var ids = new HashSet<long>();
        for (var i = 0; i < robots.GetArrayLength(); i++)
        {
            var id = robots[i].GetProperty("UserId").GetInt64();
            Assert.NotEqual(4101, id);
            Assert.DoesNotContain(id, ids);
            Assert.True(robots[i].GetProperty("UserId").GetInt64() > 0);
            Assert.False(string.IsNullOrEmpty(robots[i].GetProperty("Name").GetString()));
            Assert.False(string.IsNullOrEmpty(robots[i].GetProperty("Icon").GetString()));
        }
    }

    private static GameUserMiscController CreateController(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameUserLogoutRecordEntity));
        db.InitTenantTables(typeof(TenantGameUserArchiveEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        var logoutRepository = new Repository<TenantGameUserLogoutRecordEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
        var archiveRepository = new Repository<TenantGameUserArchiveEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
        return new GameUserMiscController(logoutRepository, archiveRepository);
    }

    private static GameUserArchiveController CreateArchiveController(ClientApiTestDb db)
    {
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        var archiveRepository = new Repository<TenantGameUserArchiveEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
        return new GameUserArchiveController(archiveRepository);
    }

    private static GameUserArchiveUploadRequest MakeUpload(long userId, string name)
    {
        return new GameUserArchiveUploadRequest
        {
            UserId = userId,
            Version = "1.0.0",
            SyncList = new List<string> { "item", },
            Item = new List<string> { "item_1,1", },
            Name = name,
        };
    }
}