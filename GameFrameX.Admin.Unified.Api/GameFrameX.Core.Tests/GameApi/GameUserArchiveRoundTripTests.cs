using GameFrameX.Client.Api.GameUserArchive.Models;
using GameFrameX.Client.Api.GameUserArchive;
using System.Text.Json;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Client.Api.GameUserArchive.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.SqlSugar;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C8 T2 RED：存档三件套 round-trip 契约（上行 → 服务端透传存储 → 下行逐元素字节等价）。
/// <para>T3 实现前全红（NotImplementedException）；转绿标准 = 本文件 5 用例全过。</para>
/// <para>分表行内容为客户端 DBTable 逗号拼接格式原样字符串（服务端不解析）。</para>
/// </summary>
public sealed class GameUserArchiveRoundTripTests
{
    [Fact]
    public async Task UploadThenDownload_RoundTripsEveryBranchByteEquivalent()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var upload = await controller.Upload(CreateFullUpload(2001, 0));
        var uploadRoot = ClientApiResultReader.Read(upload);
        Assert.Equal(0, ClientApiResultReader.Code(uploadRoot));
        var newSyncTime = ClientApiResultReader.Data(uploadRoot).GetProperty("NewSyncTime").GetInt64();
        Assert.True(newSyncTime > 0, "成功上传必须下发新版本戳");

        var download = await controller.Download(new GameUserArchiveDownloadRequest { UserId = 2001, Version = "1.0.0", });
        var downloadRoot = ClientApiResultReader.Read(download);
        Assert.Equal(0, ClientApiResultReader.Code(downloadRoot));
        var data = ClientApiResultReader.Data(downloadRoot);
        Assert.Equal(newSyncTime, data.GetProperty("SyncTime").GetInt64());

        foreach (var pair in BuildBranchData())
        {
            AssertBranchEqual(pair.Key, data.GetProperty(pair.Key), pair.Value);
        }

        Assert.Equal(string.Empty, data.GetProperty("Slot").GetString());
        Assert.Equal(string.Empty, data.GetProperty("AdStatus").GetString());

        var user = data.GetProperty("User")[0];
        Assert.Equal("12345", user.GetProperty("Gold").GetString());
        Assert.Equal("67", user.GetProperty("Gem").GetString());
        Assert.Equal(850, user.GetProperty("Progress").GetInt64());
        Assert.Equal(3, user.GetProperty("Day").GetInt64());

        Assert.Equal("task_1001", data.GetProperty("CurrentTask").GetString());
        Assert.Equal(3, data.GetProperty("CurrentChapter").GetInt64());
        Assert.Equal(7, data.GetProperty("OrderGroupId").GetInt64());
        Assert.Equal("cola_player", data.GetProperty("Name").GetString());
        Assert.Equal("badge_9", data.GetProperty("Badge").GetString());
        Assert.Equal(12, data.GetProperty("FrameCount").GetInt64());
    }

    [Fact]
    public async Task Upload_PartialSyncList_DoesNotClearUntouchedBranches()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var first = await controller.Upload(CreateFullUpload(2002, 0));
        var firstSync = ClientApiResultReader.Data(ClientApiResultReader.Read(first)).GetProperty("NewSyncTime").GetInt64();

        // 只提交 item（syncList=["item"]），其余表全部空数组且不在 syncList —— 不得视为清空
        var partial = new GameUserArchiveUploadRequest
        {
            UserId = 2002,
            Version = "1.0.0",
            SyncTime = firstSync,
            SyncList = new List<string> { "item", },
            User = new List<GameUserArchiveUserBranch> { CreateUserBranch(), },
            Item = new List<string> { "item_99,5", },
            CurrentTask = "task_1002",
        };
        var second = await controller.Upload(partial);
        Assert.Equal(0, ClientApiResultReader.Code(ClientApiResultReader.Read(second)));

        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(
                                                  await controller.Download(new GameUserArchiveDownloadRequest { UserId = 2002, Version = "1.0.0", })));
        AssertBranchEqual("Item", data.GetProperty("Item"), new List<string> { "item_99,5", });
        AssertBranchEqual("Energy", data.GetProperty("Energy"), BuildBranchData()["Energy"]);
        AssertBranchEqual("Board", data.GetProperty("Board"), BuildBranchData()["Board"]);
        Assert.Equal("task_1002", data.GetProperty("CurrentTask").GetString());
    }

    [Fact]
    public async Task Upload_SyncListWithEmptyArray_ClearsBranch()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var first = await controller.Upload(CreateFullUpload(2003, 0));
        var firstSync = ClientApiResultReader.Data(ClientApiResultReader.Read(first)).GetProperty("NewSyncTime").GetInt64();

        // board 在 syncList 且数组为空 —— 清空该表（arrDiscardSyncKey 语义）
        var clearRequest = new GameUserArchiveUploadRequest
        {
            UserId = 2003,
            Version = "1.0.0",
            SyncTime = firstSync,
            SyncList = new List<string> { "board", },
            User = new List<GameUserArchiveUserBranch> { CreateUserBranch(), },
        };
        Assert.Equal(0, ClientApiResultReader.Code(ClientApiResultReader.Read(await controller.Upload(clearRequest))));

        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(
                                                  await controller.Download(new GameUserArchiveDownloadRequest { UserId = 2003, Version = "1.0.0", })));
        Assert.Equal(0, data.GetProperty("Board").GetArrayLength());
        AssertBranchEqual("Item", data.GetProperty("Item"), BuildBranchData()["Item"]);
    }

    [Fact]
    public async Task Upload_SyncTimeMismatch_ReturnsBodyRcode3_KeepsServerArchive()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var firstSync = ClientApiResultReader.Data(ClientApiResultReader.Read(
                                                       await controller.Upload(CreateFullUpload(2004, 0)))).GetProperty("NewSyncTime").GetInt64();

        // 期望值与服务端当前版本戳不符 → 乐观锁冲突：信封 code=0 + body Rcode=3
        // （客户端 SyncModel 上传 rcode==3 分支：强制 Download 后重放；信封错误码会被
        // HandleResponse 折叠成传输失败，永远到不了那个分支——2026-09-07 修复）
        var stale = CreateFullUpload(2004, 999999);
        stale.CurrentTask = "task_should_not_land";
        var result = ClientApiResultReader.Read(await controller.Upload(stale));
        Assert.Equal(0, ClientApiResultReader.Code(result));
        Assert.Equal(3, ClientApiResultReader.Data(result).GetProperty("Rcode").GetInt64());

        // 冲突上传不得改写服务端档（版本戳与内容均保持首传状态）
        var download = ClientApiResultReader.Data(ClientApiResultReader.Read(
                                                      await controller.Download(new GameUserArchiveDownloadRequest { UserId = 2004, Version = "1.0.0", })));
        Assert.Equal(firstSync, download.GetProperty("SyncTime").GetInt64());
        Assert.Equal("task_1001", download.GetProperty("CurrentTask").GetString());
    }

    [Fact]
    public async Task Upload_SuccessiveUploads_AdvanceNewSyncTime()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        var firstSync = ClientApiResultReader.Data(ClientApiResultReader.Read(
                                                       await controller.Upload(CreateFullUpload(2005, 0)))).GetProperty("NewSyncTime").GetInt64();
        var secondSync = ClientApiResultReader.Data(ClientApiResultReader.Read(
                                                        await controller.Upload(CreateFullUpload(2005, firstSync)))).GetProperty("NewSyncTime").GetInt64();

        Assert.True(secondSync > firstSync, "连续成功上传的版本戳必须单调递增（乐观锁防重放）");
    }

    [Fact]
    public async Task Upload_SyncListNull_DoesNotThrow()
    {
        using var db = new ClientApiTestDb();
        var controller = CreateController(db);

        // 客户端发 "syncList": null 会覆盖 property initializer = new()，InSyncList 必须 null 兜底
        // 否则 22 个 InSyncList 调用首个就 NRE（C8 线上崩溃复现）
        var request = CreateFullUpload(2006, 0);
        request.SyncList = null!;

        var result = ClientApiResultReader.Read(await controller.Upload(request));
        Assert.Equal(0, ClientApiResultReader.Code(result));

        // 兜底语义：SyncList 为 null 等价于「未命中任何表」，22 张分表下行均为空数组
        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(
                                                  await controller.Download(new GameUserArchiveDownloadRequest { UserId = 2006, Version = "1.0.0", })));
        Assert.Equal(0, data.GetProperty("Board").GetArrayLength());
        Assert.Equal(0, data.GetProperty("Item").GetArrayLength());
        Assert.Equal(0, data.GetProperty("Energy").GetArrayLength());
    }

    private static GameUserArchiveController CreateController(ClientApiTestDb db)
    {
        db.InitTenantTables(typeof(TenantGameUserArchiveEntity));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        var repository = new Repository<TenantGameUserArchiveEntity>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
        return new GameUserArchiveController(repository);
    }

    private static GameUserArchiveUserBranch CreateUserBranch()
    {
        return new GameUserArchiveUserBranch
        {
            Gold = "12345",
            Gem = "67",
            SkipProp = "8",
            Level = "9",
            Exp = "1000",
            Progress = 850,
            Energy = "30",
            Day = 3,
        };
    }

    private static GameUserArchiveUploadRequest CreateFullUpload(long userId, long syncTime)
    {
        var branches = BuildBranchData();
        return new GameUserArchiveUploadRequest
        {
            UserId = userId,
            Version = "1.0.0",
            SyncTime = syncTime,
            SyncList = new List<string>(branches.Keys),
            User = new List<GameUserArchiveUserBranch> { CreateUserBranch(), },
            Energy = branches["Energy"],
            Board = branches["Board"],
            CacheItem = branches["CacheItem"],
            MainTask = branches["MainTask"],
            ItemUnlock = branches["ItemUnlock"],
            ShopItem = branches["ShopItem"],
            Shop = branches["Shop"],
            Inventory = branches["Inventory"],
            Item = branches["Item"],
            Tutorial = branches["Tutorial"],
            Orders = branches["Orders"],
            OrderMeta = branches["OrderMeta"],
            Misc = branches["Misc"],
            Bundle = branches["Bundle"],
            BundleMeta = branches["BundleMeta"],
            ActivityData = branches["ActivityData"],
            Profile = branches["Profile"],
            ReturnUser = branches["ReturnUser"],
            MainTaskMeta = branches["MainTaskMeta"],
            LocalRewards = branches["LocalRewards"],
            BiSync = branches["BiSync"],
            Notice = branches["Notice"],
            CurrentTask = "task_1001",
            CurrentChapter = 3,
            OrderChapterId = 2,
            OrderGroupId = 7,
            InstallationId = "install-uuid-1",
            DeviceModel = "iPhone17,2",
            Icon = "icon_5",
            Name = "cola_player",
            Badge = "badge_9",
            BadgeCount = 4,
            Frame = "frame_2",
            FrameCount = 12,
        };
    }

    /// <summary>
    /// 22 张 String[] 分表的期望内容（客户端 DBTable 行格式：列值逗号拼接）
    /// </summary>
    private static Dictionary<string, List<string>> BuildBranchData()
    {
        return new Dictionary<string, List<string>>
        {
            ["Energy"] = new() { "energy_1,1,100", "energy_2,2,200", },
            ["Board"] = new() { "board_1,1,0,15", "board_2,2,1,20", },
            ["CacheItem"] = new() { "cache_1,3,1", },
            ["MainTask"] = new() { "task_1,1", "task_2,1", "task_3,1", },
            ["ItemUnlock"] = new() { "unlock_1,1", },
            ["ShopItem"] = new() { "shopitem_1,2,99", },
            ["Shop"] = new() { "shop_1,1", },
            ["Inventory"] = new() { "inv_1,10", },
            ["Item"] = new() { "item_1,5", "item_2,3", },
            ["Tutorial"] = new() { "tut_step_1,1", "tut_step_2,1", },
            ["Orders"] = new() { "order_1,done,100", "order_2,pending,0", },
            ["OrderMeta"] = new() { "meta_1,2,3", },
            ["Misc"] = new() { "misc_key,misc_value", "misc_key2,42", },
            ["Bundle"] = new() { "bundle_1,owned", },
            ["BundleMeta"] = new() { "bmeta_1,1", },
            ["ActivityData"] = new() { "act_1,{\"p\":1}", "act_2,done", },
            ["Profile"] = new() { "profile_1,avatar", },
            ["ReturnUser"] = new() { "return_1,3", },
            ["MainTaskMeta"] = new() { "mtmeta_1,1", },
            ["LocalRewards"] = new() { "account_bind,1", },
            ["BiSync"] = new() { "bi_1,1680000000", },
            ["Notice"] = new() { "notice_1,1", },
        };
    }

    private static void AssertBranchEqual(string branchName, JsonElement actual, IReadOnlyList<string> expected)
    {
        Assert.True(actual.ValueKind == JsonValueKind.Array, $"分表 {branchName} 下行必须是数组");
        Assert.Equal(expected.Count, actual.GetArrayLength());
        for (var i = 0; i < expected.Count; i++)
        {
            Assert.Equal(expected[i], actual[i].GetString());
        }
    }
}