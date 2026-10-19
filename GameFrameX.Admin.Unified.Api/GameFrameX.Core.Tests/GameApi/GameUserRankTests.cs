using GameFrameX.Client.Api.GameUserRank;
using System.Reflection;
using System.Text.Json;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Client.Api.GameUserRank.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.SqlSugar;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C9 T3 RED：排行域契约（GetAsync 报名建组/机器人凑满、Query 组内回榜重算、HeartBeat 四榜分数与名次）
/// <para>契约来源：C9 任务书（旧 RS 通道 9 op 收编：GetAsync/Query/HeartBeat）。</para>
/// </summary>
public sealed class GameUserRankTests
{
    [Fact]
    public async Task Get_EnrollsPlayerIntoNewGroup_PadsRobotsToBoardSize()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var result = await controller.JoinRank(MakeJoin(5001, 100));
            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(result));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            var groupId = data.GetProperty("GroupId").GetString();
            Assert.False(string.IsNullOrEmpty(groupId));
            Assert.StartsWith("g", groupId);
            Assert.Equal(0, data.GetProperty("IsFinished").GetInt64());

            var ranks = data.GetProperty("Ranks");
            Assert.Equal(GameUserRankController.BoardSize, ranks.GetArrayLength());
            var previousScore = long.MaxValue;
            JsonElement own = default;
            for (var i = 0; i < ranks.GetArrayLength(); i++)
            {
                var row = ranks[i];
                // 严格按名次 1..N 排序返回
                Assert.Equal(i + 1, row.GetProperty("Rank").GetInt64());
                // 组内按 Score 降序
                var score = row.GetProperty("Score").GetInt64();
                Assert.True(score <= previousScore);
                previousScore = score;
                if (row.GetProperty("UserId").GetInt64() == 5001)
                {
                    own = row;
                }
            }

            // 本人四展示字段落库 + Score=Token
            Assert.Equal(JsonValueKind.Object, own.ValueKind);
            Assert.Equal("user5001", own.GetProperty("Name").GetString());
            Assert.Equal("icon5001", own.GetProperty("Icon").GetString());
            Assert.Equal("frame5001", own.GetProperty("IconFrame").GetString());
            Assert.Equal("badge5001", own.GetProperty("Badge").GetString());
            Assert.Equal(100, own.GetProperty("Score").GetInt64());

            var rows = db.TenantDb.Queryable<TenantGameRankEntryEntity>().Where(x => x.GroupId == groupId).ToList();
            Assert.Equal(GameUserRankController.BoardSize, rows.Count);
            var ownEntity = rows.Single(x => x.UserId == 5001);
            Assert.Equal(1, ownEntity.RankType);
            Assert.Equal("bakeout", ownEntity.ConfigKey);
            Assert.Equal(101, ownEntity.ConfigId);
            Assert.Equal(100, ownEntity.Score);
            Assert.Equal(100, ownEntity.RoundToken);
        }
    }

    [Fact]
    public async Task Get_SameTokenSharesGroup_DifferentTokenOpensNewGroup()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var first = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.JoinRank(MakeJoin(5001, 100))));
            var second = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.JoinRank(MakeJoin(5002, 100))));
            Assert.Equal(first.GetProperty("GroupId").GetString(), second.GetProperty("GroupId").GetString());
            // 同组第二人后不再补机器人（已到榜单规模）
            var sharedGroupId = first.GetProperty("GroupId").GetString();
            var sharedRows = db.TenantDb.Queryable<TenantGameRankEntryEntity>().Where(x => x.GroupId == sharedGroupId).ToList();
            Assert.Equal(GameUserRankController.BoardSize + 1, sharedRows.Count);

            // token 变化（活动代币增长）= 新一轮次令牌 → 开新组
            var third = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.JoinRank(MakeJoin(5003, 200))));
            var newGroupId = third.GetProperty("GroupId").GetString();
            Assert.NotEqual(sharedGroupId, newGroupId);
            var ownEntity = db.TenantDb.Queryable<TenantGameRankEntryEntity>().Single(x => x.UserId == 5003);
            Assert.Equal(200, ownEntity.RoundToken);
            Assert.Equal(200, ownEntity.Score);
        }
    }

    [Fact]
    public async Task Get_FullGroup_OpensNewGroup()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRankRepository(db);
            for (var i = 0; i < GameUserRankController.GroupCapacity; i++)
            {
                await repository.InsertAsync(MakeEntity(2, "g_seed", "multi", 202, 90000 + i, 300));
            }

            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.JoinRank(MakeJoin(5201, 300, 2, "multi", 202))));
            var groupId = data.GetProperty("GroupId").GetString();
            Assert.NotEqual("g_seed", groupId);
            // 新组 = 本人 + 机器人凑满榜单规模
            var rows = db.TenantDb.Queryable<TenantGameRankEntryEntity>().Where(x => x.GroupId == groupId).ToList();
            Assert.Equal(GameUserRankController.BoardSize, rows.Count);
            Assert.Contains(rows, x => x.UserId == 5201);
        }
    }

    [Fact]
    public async Task Query_ReturnsWholeGroup_WithRecomputedRanks()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRankRepository(db);
            await repository.InsertAsync(MakeEntity(3, "gq", "weekly1", 303, 7001, 100));
            await repository.InsertAsync(MakeEntity(3, "gq", "weekly1", 303, 7002, 300));
            await repository.InsertAsync(MakeEntity(3, "gq", "weekly1", 303, 7003, 200));
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Query(
                                                                                 new GameUserRankQueryRequest { RankType = 3, UserId = 7001, GroupId = "gq", })));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            var ranks = data.GetProperty("Ranks");
            Assert.Equal(3, ranks.GetArrayLength());
            Assert.Equal(7002, ranks[0].GetProperty("UserId").GetInt64());
            Assert.Equal(1, ranks[0].GetProperty("Rank").GetInt64());
            Assert.Equal(7003, ranks[1].GetProperty("UserId").GetInt64());
            Assert.Equal(2, ranks[1].GetProperty("Rank").GetInt64());
            Assert.Equal(7001, ranks[2].GetProperty("UserId").GetInt64());
            Assert.Equal(3, ranks[2].GetProperty("Rank").GetInt64());

            // 分数变化后重算（心跳推进 7001 反超）
            var row7001 = db.TenantDb.Queryable<TenantGameRankEntryEntity>().Single(x => x.UserId == 7001);
            row7001.Score = 999;
            await repository.UpdateAsync(row7001);
            var replay = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Query(
                                                                                   new GameUserRankQueryRequest { RankType = 3, UserId = 7001, GroupId = "gq", })));
            var replayRanks = replay.GetProperty("Ranks");
            Assert.Equal(7001, replayRanks[0].GetProperty("UserId").GetInt64());
            Assert.Equal(1, replayRanks[0].GetProperty("Rank").GetInt64());
            Assert.Equal(999, replayRanks[0].GetProperty("Score").GetInt64());
        }
    }

    [Fact]
    public async Task HeartBeat_UpdatesFourBoardScores_MissingBoardRankZero()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRankRepository(db);
            await repository.InsertAsync(MakeEntity(1, "gh1", "bakeout", 111, 8001, 100));
            await repository.InsertAsync(MakeEntity(1, "gh1", "bakeout", 111, 99001, 50));
            await repository.InsertAsync(MakeEntity(1, "gh1", "bakeout", 111, 99002, 150));
            await repository.InsertAsync(MakeEntity(3, "gh3", "weekly1", 333, 8001, 10));
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.HeartBeat(
                                                                                 new GameUserRankHeartBeatRequest
                                                                                 {
                                                                                     UserId = 8001,
                                                                                     Session = "legacy-session-passthrough",
                                                                                     BakeoutToken = 120,
                                                                                     MultiToken = 9,
                                                                                     Weekly1Token = 5,
                                                                                     Weekly2Token = 7,
                                                                                 })));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            // bakeout 组内 150 > 120 > 50 → 第 2 名
            Assert.Equal(2, data.GetProperty("BakeoutRank").GetInt64());
            Assert.Equal(1, data.GetProperty("Weekly1Rank").GetInt64());
            // 无行回 0（透传不校验 Session）
            Assert.Equal(0, data.GetProperty("MultiRank").GetInt64());
            Assert.Equal(0, data.GetProperty("Weekly2Rank").GetInt64());

            var bakeout = db.TenantDb.Queryable<TenantGameRankEntryEntity>().Single(x => x.UserId == 8001 && x.RankType == 1);
            Assert.Equal(120, bakeout.Score);
            Assert.Equal(2, bakeout.Rank);
            var weekly1 = db.TenantDb.Queryable<TenantGameRankEntryEntity>().Single(x => x.UserId == 8001 && x.RankType == 3);
            Assert.Equal(5, weekly1.Score);
        }
    }

    [Fact]
    public void Controller_HasExplicitRouteAndHttpPostVerbs()
    {
        Assert.Equal("api/[controller]/[action]",
                     typeof(GameUserRankController).GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(typeof(GameUserRankController).GetMethod("JoinRank")!.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(typeof(GameUserRankController).GetMethod("Query")!.GetCustomAttribute<HttpPostAttribute>());
        Assert.NotNull(typeof(GameUserRankController).GetMethod("HeartBeat")!.GetCustomAttribute<HttpPostAttribute>());
    }

    private static GameUserRankController CreateController(ClientApiTestDb db)
    {
        return new GameUserRankController(CreateRankRepository(db), CreateArchiveRepository(db));
    }

    private static Repository<TenantGameRankEntryEntity> CreateRankRepository(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameRankEntryEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        return new Repository<TenantGameRankEntryEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
    }

    private static Repository<TenantGameUserArchiveEntity> CreateArchiveRepository(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameUserArchiveEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        return new Repository<TenantGameUserArchiveEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
    }

    private static GameUserRankJoinRequest MakeJoin(long userId, long token, int rankType = 1, string configKey = "bakeout", long configId = 101)
    {
        return new GameUserRankJoinRequest
        {
            RankType = rankType,
            UserId = userId,
            ConfigKey = configKey,
            ConfigId = configId,
            GroupType = 1,
            Name = "user" + userId,
            Icon = "icon" + userId,
            IconFrame = "frame" + userId,
            Badge = "badge" + userId,
            Token = token,
        };
    }

    private static TenantGameRankEntryEntity MakeEntity(int rankType, string groupId, string configKey, long configId, long userId, long score)
    {
        return new TenantGameRankEntryEntity
        {
            RankType = rankType,
            GroupId = groupId,
            ConfigKey = configKey,
            ConfigId = configId,
            UserId = userId,
            Name = "seed" + userId,
            Score = score,
            RoundToken = score,
        };
    }
}