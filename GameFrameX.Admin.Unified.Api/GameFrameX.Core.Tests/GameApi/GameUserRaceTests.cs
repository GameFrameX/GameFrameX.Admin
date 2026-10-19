using GameFrameX.Client.Api.GameUserArchive;
using GameFrameX.Client.Api.GameUserRace.Models;
using GameFrameX.Client.Api.GameUserRace;
using System.Reflection;
using System.Text.Json;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Client.Api.GameUserArchive.Dto;
using GameFrameX.Client.Api.GameUserRace.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C9 T3 RED：比赛域契约（Entry 六赛制成绩形态、数量严格凑满、exclude 生效、Day 无 Round）
/// <para>契约来源：C9 任务书（旧 BL 比赛 6 op 收编；客户端严格断言 #robots == RobotCount）。</para>
/// </summary>
public sealed class GameUserRaceTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(6)]
    public async Task Entry_ScoringRace_FillsScoreStringNotSecond(int raceType)
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Entry(
                                                                                 MakeEntry(raceType, 2))));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            var robots = data.GetProperty("Robots");
            Assert.Equal(2, robots.GetArrayLength());
            for (var i = 0; i < robots.GetArrayLength(); i++)
            {
                // 计分制：Score 为可解析十进制字符串，Second 空
                var score = robots[i].GetProperty("Score");
                Assert.Equal(JsonValueKind.String, score.ValueKind);
                Assert.True(long.Parse(score.GetString()!) >= 0);
                Assert.True(IsAbsentOrNull(robots[i], "Second"));
                Assert.True(robots[i].GetProperty("UserId").GetInt64() > 0);
                Assert.False(string.IsNullOrEmpty(robots[i].GetProperty("Name").GetString()));
                Assert.False(string.IsNullOrEmpty(robots[i].GetProperty("Icon").GetString()));
                Assert.Equal("5", robots[i].GetProperty("GroupType").GetString());
                Assert.True(robots[i].GetProperty("Ability").GetInt64() > 0);
            }
        }
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public async Task Entry_TimerRace_FillsSecondNotScore(int raceType)
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Entry(
                                                                                 MakeEntry(raceType, 3, "2"))));
            var robots = data.GetProperty("Robots");
            Assert.Equal(3, robots.GetArrayLength());
            for (var i = 0; i < robots.GetArrayLength(); i++)
            {
                // 计时制：Second 为正数，Score 空
                Assert.True(robots[i].GetProperty("Second").GetInt64() > 0);
                Assert.True(IsAbsentOrNull(robots[i], "Score"));
            }
        }
    }

    [Fact]
    public async Task Entry_GeneratesPerTierCounts_FromGroupsNum()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Entry(
                                                                                 MakeEntry(1, 4, groups: new List<RaceGroupNum>
                                                                                 {
                                                                                     new() { GroupType = "2", GroupNum = 2, },
                                                                                     new() { GroupType = "3", GroupNum = 2, },
                                                                                 }))));
            var robots = data.GetProperty("Robots");
            Assert.Equal(4, robots.GetArrayLength());
            var tier2 = 0;
            var tier3 = 0;
            for (var i = 0; i < robots.GetArrayLength(); i++)
            {
                var groupType = robots[i].GetProperty("GroupType").GetString();
                if (groupType == "2")
                {
                    tier2++;
                }
                else if (groupType == "3")
                {
                    tier3++;
                }
            }

            Assert.Equal(2, tier2);
            Assert.Equal(2, tier3);
        }
    }

    [Fact]
    public async Task Entry_PadsSyntheticRobots_WhenGroupsSumBelowRobotCount()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Entry(
                                                                                 MakeEntry(1, 5, groups: new List<RaceGroupNum> { new() { GroupType = "1", GroupNum = 2, }, }))));
            var robots = data.GetProperty("Robots");
            // 客户端严格断言数量 == RobotCount，不足必须凑满
            Assert.Equal(5, robots.GetArrayLength());
        }
    }

    [Fact]
    public async Task Entry_DayRace_AcceptsNullRound()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var request = MakeEntry(6, 2, "1");
            request.Round = null;
            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Entry(request)));
            var robots = data.GetProperty("Robots");
            Assert.Equal(2, robots.GetArrayLength());
            Assert.Equal(JsonValueKind.String, robots[0].GetProperty("Score").ValueKind);
        }
    }

    [Fact]
    public async Task Entry_RespectsExclusions_AndSamplesRealArchives()
    {
        using (var db = new ClientApiTestDb())
        {
            db.InitTenantTables(typeof(TenantGameUserArchiveEntity));
            var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
            var archiveRepository = new Repository<TenantGameUserArchiveEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
            var archiveController = new GameUserArchiveController(archiveRepository);
            await archiveController.Upload(MakeUpload(9101, "real_a", "icon_a"));
            await archiveController.Upload(MakeUpload(9102, "real_b", "icon_b"));
            var controller = CreateController(db);

            var request = MakeEntry(1, 2, "1");
            request.UserId = 9100;
            request.ExcludeUserIds = new List<long> { 9101, };
            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Entry(request)));
            var robots = data.GetProperty("Robots");
            Assert.Equal(2, robots.GetArrayLength());
            // 真实档抽样 + 合成凑满，exclude 生效且 id 唯一
            var ids = new HashSet<long>();
            for (var i = 0; i < robots.GetArrayLength(); i++)
            {
                var id = robots[i].GetProperty("UserId").GetInt64();
                Assert.NotEqual(9101, id);
                Assert.DoesNotContain(id, ids);
                ids.Add(id);
            }

            Assert.Contains(9102, ids);
        }
    }

    [Fact]
    public void Controllers_HaveExplicitRouteAndHttpPostVerbs()
    {
        Assert.Equal("api/[controller]/[action]",
                     typeof(GameUserRaceController).GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(typeof(GameUserRaceController).GetMethod("Entry")!.GetCustomAttribute<HttpPostAttribute>());
    }

    private static GameUserRaceController CreateController(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameUserArchiveEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        var archiveRepository = new Repository<TenantGameUserArchiveEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
        return new GameUserRaceController(archiveRepository);
    }

    private static GameUserRaceEntryRequest MakeEntry(int raceType, long robotCount, string groupType = "5",
        List<RaceGroupNum>? groups = null)
    {
        groups ??= new List<RaceGroupNum> { new() { GroupType = groupType, GroupNum = robotCount, }, };
        return new GameUserRaceEntryRequest
        {
            RaceType = raceType,
            UserId = 6001,
            ExcludeUserIds = new List<long>(),
            Round = 3,
            RobotCount = robotCount,
            GroupsNum = groups,
        };
    }

    private static GameUserArchiveUploadRequest MakeUpload(long userId, string name, string icon)
    {
        return new GameUserArchiveUploadRequest
        {
            UserId = userId,
            Version = "1.0.0",
            SyncList = new List<string> { "item", },
            Item = new List<string> { "item_1,1", },
            Name = name,
            Icon = icon,
        };
    }

    private static bool IsAbsentOrNull(JsonElement element, string name)
    {
        return !element.TryGetProperty(name, out var value) || value.ValueKind == JsonValueKind.Null;
    }
}