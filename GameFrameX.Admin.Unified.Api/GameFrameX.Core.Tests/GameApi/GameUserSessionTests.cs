using GameFrameX.Client.Api.GameUserSession.Models;
using GameFrameX.Client.Api.GameUserSession;
using System.Reflection;
using System.Text.Json;
using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.ClientApi;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.ServerConfig;
using GameFrameX.Client.Api.GameUserSession.Dto;
using GameFrameX.Client.Api.Services;
using GameFrameX.Entity.Enum;
using Microsoft.AspNetCore.Mvc;

namespace GameFrameX.Core.Tests.GameApi;

/// <summary>
/// C13 T2：登录控制面契约（正常/首启无档/换端顶号/累计充值/注销 rcode=2/版本纯函数/路由守护）
/// <para>契约来源：C13 change.md §1 28 字段四态去向 + C280 config 差异下发（C315 数据源切租户库
/// tenant_game_app_server_config 表，含基线/应用行合并与 Enabled 过滤语义）+ P0-2 存档行会话列。</para>
/// </summary>
public sealed class GameUserSessionTests
{
    [Fact]
    public async Task Login_WithArchive_EchoesSyncFields_WritesSessionColumns()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameUserArchiveEntity>(db);
            await repository.InsertAsync(MakeArchive(8001, "uuid-a", "iPhone13", 123456));
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(MakeLogin(8001))));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            // 存档三回显：SyncTime/InstallationId/DeviceModel 原样下发
            Assert.Equal(123456, data.GetProperty("LastSyncTime").GetInt64());
            Assert.Equal("uuid-a", data.GetProperty("InstallationId").GetString());
            Assert.Equal("iPhone13", data.GetProperty("UpdateDeviceModel").GetString());
            // P0-2：会话列成对写回存档行（SessionId == 下发 Session）
            var row = db.TenantDb.Queryable<TenantGameUserArchiveEntity>().Single(x => x.UserId == 8001);
            Assert.Equal(data.GetProperty("Session").GetInt64(), row.SessionId);
            Assert.Equal("SM-G991B", row.SessionDeviceModel);
            // 常量回显与请求回显
            Assert.Equal(60, data.GetProperty("HeartBeatInterval").GetInt64());
            Assert.Equal(300, data.GetProperty("HeartBeatIntervalIdle").GetInt64());
            Assert.Equal(1724115000, data.GetProperty("RequestId").GetInt64());
            Assert.True(data.GetProperty("ServerTime").GetInt64() > 0);
        }
    }

    [Fact]
    public async Task Login_FirstLogin_NoArchive_DoesNotCreateRow()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(MakeLogin(8002))));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
            // 首启无档：lastSyncTime=0 / 换端两键空串（客户端走本地存档兜底）
            Assert.Equal(0, data.GetProperty("LastSyncTime").GetInt64());
            Assert.Equal(string.Empty, data.GetProperty("InstallationId").GetString());
            Assert.Equal(string.Empty, data.GetProperty("UpdateDeviceModel").GetString());
            Assert.True(data.GetProperty("Session").GetInt64() > 0);
            // 存档行由 C8 存档链路负责建行，登录不代建
            Assert.Equal(0, db.TenantDb.Queryable<TenantGameUserArchiveEntity>().Count(x => x.UserId == 8002));
        }
    }

    [Fact]
    public async Task Login_SecondDevice_SessionOverwritten()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameUserArchiveEntity>(db);
            await repository.InsertAsync(MakeArchive(8003));
            var controller = CreateController(db);

            var first = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(MakeLogin(8003, "device-A"))));
            var second = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(MakeLogin(8003, "device-B"))));
            Assert.NotEqual(first.GetProperty("Session").GetInt64(), second.GetProperty("Session").GetInt64());

            // ponytail: 单副本起步——后登录覆盖（顶号），不保留历史会话
            var row = db.TenantDb.Queryable<TenantGameUserArchiveEntity>().Single(x => x.UserId == 8003);
            Assert.Equal(second.GetProperty("Session").GetInt64(), row.SessionId);
            Assert.Equal("device-B", row.SessionDeviceModel);
        }
    }

    [Fact]
    public async Task Login_TotalPay_SumsShippedOrdersOnly()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameOrderEntity>(db);
            await repository.InsertAsync(MakeOrder(8004, "pay_t1", (int)OrderStatusEnum.Shipped, 660));
            await repository.InsertAsync(MakeOrder(8004, "pay_t2", (int)OrderStatusEnum.Unpaid, 990));
            await repository.InsertAsync(MakeOrder(8005, "pay_t3", (int)OrderStatusEnum.Shipped, 500));
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(MakeLogin(8004))));
            // 已到账（Shipped 及以后）计入，未付款不计入；他人订单不串
            Assert.Equal(660, data.GetProperty("TotalPay").GetInt64());
        }
    }

    [Fact]
    public async Task Login_DeletedUser_ReturnsRcode2_WithLogoutTime()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameUserEntity>(db);
            await repository.InsertAsync(MakeUser(8006, GameUserStatus.Deleted, 888));
            var controller = CreateController(db);

            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(MakeLogin(8006))));
            Assert.Equal(2, data.GetProperty("Rcode").GetInt64());
            Assert.Equal(888, data.GetProperty("LogoutTime").GetInt64());
        }
    }

    [Fact]
    public async Task Login_UserRowMissing_PassesThrough()
    {
        using (var db = new ClientApiTestDb())
        {
            var controller = CreateController(db);

            // 游客链路 Game.Api 不持有账号行：查无行 = 放行
            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(MakeLogin(8007))));
            Assert.Equal(0, data.GetProperty("Rcode").GetInt64());
        }
    }

    [Fact]
    public async Task Login_UnknownConfigKeys_NotEchoed_ServesDbRows()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameAppServerConfigEntity>(db);
            await repository.InsertAsync(MakeServerConfig(319200000000101, "frame", "[1,2,3]", Md5Frame));
            await repository.InsertAsync(MakeServerConfig(319200000000102, "badge", "[4,5]", Md5Badge));
            var controller = CreateController(db);

            var request = MakeLogin(8008);
            request.ConfigEntries = new List<GameUserSessionConfigEntry>
            {
                new() { ConfigKey = "ServerConfigKey1", ConfigMd5 = "md5-a", },
                new() { ConfigKey = "ActivityType2", ConfigMd5 = "md5-b", },
            };
            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(request)));
            var config = JsonDocument.Parse(data.GetProperty("Config").GetString()!).RootElement;
            // C315：库里没有的键不回显（客户端删除本地该键，与原厂「无内容不下发」一致）
            Assert.False(config.TryGetProperty("ServerConfigKey1", out _));
            Assert.False(config.TryGetProperty("ActivityType2", out _));
            // 库内键上行 md5 不等 → 下发内容（数据源为租户库行，不再依赖嵌入种子）
            Assert.Equal(3, config.GetProperty("frame").GetProperty("config").GetArrayLength());
            Assert.True(config.GetProperty("badge").TryGetProperty("config", out _));
            Assert.Equal(Md5Frame, config.GetProperty("frame").GetProperty("md5").GetString());
        }
    }

    [Fact]
    public async Task Login_MatchingMd5_ServesMd5Only()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameAppServerConfigEntity>(db);
            await repository.InsertAsync(MakeServerConfig(319200000000101, "frame", "[1,2,3]", Md5Frame));
            await repository.InsertAsync(MakeServerConfig(319200000000102, "badge", "[4,5]", Md5Badge));
            var controller = CreateController(db);

            var request = MakeLogin(8009);
            request.ConfigEntries = new List<GameUserSessionConfigEntry>
            {
                new() { ConfigKey = "frame", ConfigMd5 = Md5Frame, },
            };
            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(request)));
            var config = JsonDocument.Parse(data.GetProperty("Config").GetString()!).RootElement;
            // md5 相等 → 只回 {md5} 不带 config（客户端不写不删不重启，省流量增量形态）
            var frame = config.GetProperty("frame");
            Assert.Equal(Md5Frame, frame.GetProperty("md5").GetString());
            Assert.False(frame.TryGetProperty("config", out _));
            // 其余库内键上行缺失 → 照常下发内容
            Assert.True(config.GetProperty("badge").TryGetProperty("config", out _));
        }
    }

    [Fact]
    public async Task Login_NoConfigEntries_ServesAllEnabledRows()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameAppServerConfigEntity>(db);
            await repository.InsertAsync(MakeServerConfig(319200000000101, "frame", "[1,2,3]", Md5Frame));
            await repository.InsertAsync(MakeServerConfig(319200000000102, "badge", "[4,5]", Md5Badge));
            await repository.InsertAsync(MakeServerConfig(319200000000103, "notify", "{}", Md5Badge, enabled: false));
            var controller = CreateController(db);

            var request = MakeLogin(8010);
            request.ConfigEntries = new List<GameUserSessionConfigEntry>();
            var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.Login(request)));
            var config = JsonDocument.Parse(data.GetProperty("Config").GetString()!).RootElement;
            // 首登无上行条目 = 全部生效键视为 md5 不等 → 全量下发；Enabled=false 键不下发（客户端删本地该键保持空态）
            Assert.Equal(2, config.GetPropertyCount());
            Assert.True(config.GetProperty("frame").TryGetProperty("config", out _));
            Assert.True(config.GetProperty("badge").TryGetProperty("config", out _));
            Assert.False(config.TryGetProperty("notify", out _));
        }
    }

    [Fact]
    public async Task GameServerConfigProvider_MergesAppRowOverBaseline_FiltersDisabled()
    {
        using (var db = new ClientApiTestDb())
        {
            var repository = CreateRepository<TenantGameAppServerConfigEntity>(db);
            await repository.InsertAsync(MakeServerConfig(319200000000101, "frame", "[1,2,3]", Md5Frame));
            await repository.InsertAsync(MakeServerConfig(319200000000102, "badge", "[4,5]", Md5Badge));
            await repository.InsertAsync(MakeServerConfig(319200000000103, "notify", "{}", Md5Badge, enabled: false));
            await repository.InsertAsync(MakeServerConfig(319200000000201, "frame", "[9]", Md5FrameApp, appId: 901));

            // 应用视角（appId=901）：应用行按 Key 覆盖基线行，badge 落基线，Enabled=false 键剔除
            var appView = await GameServerConfigProvider.LoadEffectiveAsync(repository, 901);
            Assert.Equal(2, appView.Count);
            Assert.Equal(Md5FrameApp, appView["frame"].Md5);
            Assert.Equal("[9]", appView["frame"].Config);
            Assert.Equal(Md5Badge, appView["badge"].Md5);
            Assert.False(appView.ContainsKey("notify"));

            // 无 App 上下文（appId=0）：仅基线行生效，应用行不外漏
            var baselineView = await GameServerConfigProvider.LoadEffectiveAsync(repository, 0);
            Assert.Equal(2, baselineView.Count);
            Assert.Equal(Md5Frame, baselineView["frame"].Md5);
        }
    }

    [Fact]
    public void IsClientVersionTooLow_SemanticCompare_LenientFallback()
    {
        Assert.True(GameUserSessionController.IsClientVersionTooLow("0.9.0", "1.0.0"));
        Assert.False(GameUserSessionController.IsClientVersionTooLow("1.0.0", "1.0.0"));
        Assert.False(GameUserSessionController.IsClientVersionTooLow("1.47.1", "1.47.0"));
        // 解析失败放行：异形版本号不拦游客 / 非法 min 同放行
        Assert.False(GameUserSessionController.IsClientVersionTooLow("abc", "1.0.0"));
        Assert.False(GameUserSessionController.IsClientVersionTooLow("1.0.0", "not-a-version"));
        // 起步常量恒放行（管理面配置化留 P1）
        Assert.False(GameUserSessionController.IsClientVersionTooLow("0.0.0", GameUserSessionController.MinClientVersion));
    }

    [Fact]
    public void Controller_HasExplicitRouteAndHttpPostVerbs()
    {
        Assert.Equal("api/[controller]/[action]",
                     typeof(GameUserSessionController).GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.NotNull(typeof(GameUserSessionController).GetMethod("Login")!.GetCustomAttribute<HttpPostAttribute>());
    }

    private static GameUserSessionController CreateController(ClientApiTestDb db)
    {
        return new GameUserSessionController(
            CreateRepository<TenantGameUserArchiveEntity>(db),
            CreateRepository<TenantGameUserEntity>(db),
            CreateRepository<TenantGameOrderEntity>(db),
            CreateRepository<TenantGameAppServerConfigEntity>(db));
    }

    private static Repository<T> CreateRepository<T>(ClientApiTestDb db) where T : GameFrameX.Foundation.Orm.Entity.EntityBase, new()
    {
        db.InitTenantTables(typeof(T));
        var tenantDbProvider = new ClientApiTenantDbProvider(db.MainDb, db.TenantIdResolver);
        return new Repository<T>(db.MainDb, tenantDbProvider, db.TenantIdResolver);
    }

    private static GameUserSessionLoginRequest MakeLogin(long userId, string deviceModel = "SM-G991B")
    {
        return new GameUserSessionLoginRequest
        {
            UserId = userId,
            Version = "1.47.1",
            Channel = 60,
            RequestId = 1724115000,
            InstallId = "install-uuid-1",
            DeviceModel = deviceModel,
        };
    }

    /// <summary>
    /// 测试用 md5 版本号（登录链路只比较相等性，不校验格式；值任意但区分行）
    /// </summary>
    private const string Md5Frame = "a67d42b17043bc244f73d8557a871b0a";
    private const string Md5Badge = "0f1e2d3c4b5a69788796a5b4c3d2e1f0";
    private const string Md5FrameApp = "1a2b3c4d5e6f708192a3b4c5d6e7f809";

    private static TenantGameAppServerConfigEntity MakeServerConfig(long id, string key, string config, string md5, bool enabled = true, long? appId = null)
    {
        return new TenantGameAppServerConfigEntity
        {
            Id = id,
            AppId = appId,
            Key = key,
            Config = config,
            Md5 = md5,
            Enabled = enabled,
        };
    }

    private static TenantGameUserArchiveEntity MakeArchive(long userId, string installationId = "uuid-a", string deviceModel = "iPhone13", long syncTime = 123456)
    {
        return new TenantGameUserArchiveEntity
        {
            Name = "archive-" + userId,
            UserId = userId,
            SyncTime = syncTime,
            BranchesJson = "{}",
            InstallationId = installationId,
            DeviceModel = deviceModel,
            ClientVersion = "1.47.0",
        };
    }

    private static TenantGameUserEntity MakeUser(long id, GameUserStatus status, long deletedTime)
    {
        return new TenantGameUserEntity
        {
            Id = id,
            Name = "user-" + id,
            UserName = "u" + id,
            Password = "p",
            Email = string.Empty,
            EmailPassword = string.Empty,
            GoogleId = string.Empty,
            AppleId = string.Empty,
            FacebookId = string.Empty,
            TwitterId = string.Empty,
            DiscordId = string.Empty,
            QqId = string.Empty,
            WeChatId = string.Empty,
            DeviceId = string.Empty,
            Phone = string.Empty,
            Avatar = string.Empty,
            Status = status,
            DeletedTime = deletedTime,
        };
    }

    private static TenantGameOrderEntity MakeOrder(long userId, string orderId, int status, long amount)
    {
        return new TenantGameOrderEntity
        {
            AccountId = userId.ToString(),
            RoleId = userId.ToString(),
            AreaId = 0,
            OrderId = orderId,
            Sku = 0,
            ProductId = "sku.gold",
            Status = (OrderStatusEnum)status,
            Amount = (uint)amount,
            PayChannel = "100",
            Platform = "1",
            Channel = string.Empty,
            SubChannel = string.Empty,
            Country = "US",
            Content = string.Empty,
            AdventureSuffix = string.Empty,
        };
    }
}