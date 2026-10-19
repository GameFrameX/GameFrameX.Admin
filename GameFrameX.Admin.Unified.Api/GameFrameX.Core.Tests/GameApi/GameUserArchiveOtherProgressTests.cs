using GameFrameX.Client.Api.GameUserArchive.Models;
using GameFrameX.Client.Api.GameUserArchive;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Client.Api.GameUserArchive.Dto;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Client.Api.Services;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C8 T3 RED：GetOtherProgress 契约（有档摘要投影、无档 Rcode1）
/// <para>契约来源：C8 notes T3b 客户端调研（ApiInterface.lua:1515-1653，14 字段全必填；
/// rcode 0=有数据/1=无云端数据）。客户端调用方已在 C7 删除，服务端先建能力。</para>
/// </summary>
public sealed class GameUserArchiveOtherProgressTests
{
    [Fact]
    public async Task OtherProgress_ReturnsSummary_WhenArchiveExists()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        await controller.Upload(new GameUserArchiveUploadRequest
        {
            UserId = 5001,
            Version = "1.0.0",
            SyncList = new List<string> { "item", },
            User = new List<GameUserArchiveUserBranch>
            {
                new() { Gold = "999", Gem = "88", SkipProp = "7", Level = "66", Progress = 500, Energy = "30", Day = 3, },
            },
            Item = new List<string> { "item_1,1", },
            CurrentTask = "task_9",
            CurrentChapter = 5,
            OrderChapterId = 2,
            OrderGroupId = 8,
            Name = "target_player",
            Badge = "badge_1",
            BadgeCount = 1,
            Frame = "frame_1",
            FrameCount = 2,
        });

        var result = await controller.GetOtherProgress(new GameUserArchiveOtherProgressRequest { OtherId = 5001, Version = "1.0.0", });
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(result));
        Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
        Assert.True(data.GetProperty("SyncTime").GetInt64() > 0);
        Assert.Equal("999", data.GetProperty("Gold").GetString());
        Assert.Equal("88", data.GetProperty("Gem").GetString());
        Assert.Equal("66", data.GetProperty("Level").GetString());
        Assert.Equal("task_9", data.GetProperty("CurrentTask").GetString());
        Assert.Equal(5, data.GetProperty("CurrentChapter").GetInt64());
        Assert.Equal(8, data.GetProperty("OrderGroupId").GetInt64());
        Assert.Equal("badge_1", data.GetProperty("Badge").GetString());
        Assert.Equal(2, data.GetProperty("FrameCount").GetInt64());
    }

    [Fact]
    public async Task OtherProgress_ReturnsRcodeOne_WhenNoArchive()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var result = await controller.GetOtherProgress(new GameUserArchiveOtherProgressRequest { OtherId = 5999, Version = "1.0.0", });
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(result));
        Assert.Equal(1, data.GetProperty("Rcode").GetInt64());
    }

    private static GameUserArchiveController CreateController(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameUserArchiveEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        var repository = new Repository<TenantGameUserArchiveEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
        return new GameUserArchiveController(repository);
    }
}