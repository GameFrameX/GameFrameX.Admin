using GameFrameX.Application.Game.App.FeatureSwitch;
using GameFrameX.Client.Api.GameFeatureSwitch;
using GameFrameX.Client.Api.GameFeatureSwitch.Dto;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Core.Const;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// C347 活动/功能开关客户端通道（GameFeatureSwitchController）端点级测试。
/// <para>覆盖：版本协议三态（首拉全量 / 同版本 Unchanged 短路 / 变更后全量）、消费方过滤（server 行不下发）、
/// 时间窗外不下发、设备灰度三分支（未命中 / 白名单旁路 / 缺 DeviceId 仅全量命中）、
/// 缺租户 / 缺应用上下文 fail-fast（C314 语义）与 PascalCase wire 契约。</para>
/// </summary>
public sealed class ClientApiFeatureSwitchEndpointTests
{
    private const string PackageName = "com.gameframex.c108";

    private static FeatureSwitchRequest Request(string? version = null, string? deviceId = null)
    {
        return new FeatureSwitchRequest
        {
            Language = "zh-CN",
            PackageName = PackageName,
            Platform = "ios",
            Channel = "official",
            AppVersion = "1.0.0",
            DeviceId = deviceId,
            Version = version,
        };
    }

    private static TenantGameAppFeatureSwitchEntity Row(
        long id,
        string switchKey,
        string valueJson = "{}",
        string audience = FeatureSwitchAudienceValues.Both,
        long? startTime = null,
        long? endTime = null,
        int rolloutPercent = 100,
        string? deviceWhitelistJson = null)
    {
        return new TenantGameAppFeatureSwitchEntity
        {
            Id = id,
            AppId = null,
            SwitchKey = switchKey,
            DisplayName = switchKey,
            Audience = audience,
            ValueJson = valueJson,
            StartTime = startTime,
            EndTime = endTime,
            Platform = GameDimensionConst.PlatformDefault,
            Package = GameDimensionConst.PackageNameDefault,
            Language = GameDimensionConst.LanguageDefault,
            Channel = GameDimensionConst.ChannelNameDefault,
            Priority = 0,
            RolloutPercent = rolloutPercent,
            DeviceWhitelistJson = deviceWhitelistJson,
        };
    }

    private static GameFeatureSwitchController CreateController(ClientApiTestDb db)
    {
        var resolver = new FeatureSwitchDeliveryResolver(db.Repository<TenantGameAppFeatureSwitchEntity>());
        var packageResolver = new GameAppPackageResolver(db.TenantDbProvider, db.TenantIdResolver);
        return new GameFeatureSwitchController(resolver, packageResolver);
    }

    /// <summary>
    /// 首拉（无 Version）返回全量命中开关：版本号为 32 位小写 hex、列表按开关键排序、ServerTime 下发、PascalCase wire。
    /// </summary>
    [Fact]
    public async Task GetInfo_FirstPull_ReturnsFullSwitchesWithVersion()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        db.SeedFeatureSwitch(Row(1, "activity.summer", "{\"enabled\":true}", audience: FeatureSwitchAudienceValues.Client));
        db.SeedFeatureSwitch(Row(2, "login.banner", "{\"v\":1}", audience: FeatureSwitchAudienceValues.Both));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var result = await controller.GetInfo(Request());

        var root = ClientApiResultReader.Read(result);
        Assert.Equal(0, ClientApiResultReader.Code(root));
        var data = ClientApiResultReader.Data(root);
        Assert.False(data.GetProperty("Unchanged").GetBoolean());
        Assert.Matches("^[0-9a-f]{32}$", data.GetProperty("Version").GetString());
        Assert.True(data.GetProperty("ServerTime").GetInt64() > 0);
        var switches = data.GetProperty("Switches");
        Assert.Equal(2, switches.GetArrayLength());
        // 按开关键 Ordinal 排序：activity.summer < login.banner
        Assert.Equal("activity.summer", switches[0].GetProperty("SwitchKey").GetString());
        Assert.Equal("{\"enabled\":true}", switches[0].GetProperty("ValueJson").GetString());
        Assert.Equal("login.banner", switches[1].GetProperty("SwitchKey").GetString());
        // 命中开关无时间窗时 StartTime / EndTime 为 null
        Assert.Null(switches[1].GetProperty("StartTime").GetString());
    }

    /// <summary>
    /// 携带与当前相同的 Version → Unchanged=true 且 Switches 为空数组，Version 仍返回。
    /// </summary>
    [Fact]
    public async Task GetInfo_SameVersion_ReturnsUnchangedWithEmptySwitches()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        db.SeedFeatureSwitch(Row(1, "login.banner", "{\"v\":1}"));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var first = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request())));
        var version = first.GetProperty("Version").GetString();

        var second = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request(version: version))));

        Assert.True(second.GetProperty("Unchanged").GetBoolean());
        Assert.Equal(version, second.GetProperty("Version").GetString());
        Assert.Equal(0, second.GetProperty("Switches").GetArrayLength());
    }

    /// <summary>
    /// 配置变更（改 ValueJson）后 Version 变化：携带旧 Version 的请求拿到全量新列表。
    /// </summary>
    [Fact]
    public async Task GetInfo_AfterConfigChange_VersionChangesAndReturnsFullList()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        var row = db.SeedFeatureSwitch(Row(1, "login.banner", "{\"v\":1}"));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var first = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request())));
        var oldVersion = first.GetProperty("Version").GetString();

        row.ValueJson = "{\"v\":2}";
        db.TenantDb.Updateable(row).ExecuteCommand();

        var second = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request(version: oldVersion))));

        Assert.False(second.GetProperty("Unchanged").GetBoolean());
        Assert.NotEqual(oldVersion, second.GetProperty("Version").GetString());
        var switches = second.GetProperty("Switches");
        Assert.Equal(1, switches.GetArrayLength());
        Assert.Equal("{\"v\":2}", switches[0].GetProperty("ValueJson").GetString());
    }

    /// <summary>
    /// 时间窗在服务端按 UTC 过滤：已结束（EndTime &lt;= now）与未开始（StartTime &gt; now）的开关均不下发。
    /// </summary>
    [Fact]
    public async Task GetInfo_OutOfTimeWindow_NotDelivered()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        var now = DateTimeOffset.UtcNow;
        db.SeedFeatureSwitch(Row(1, "activity.ended", endTime: now.AddSeconds(-10).ToUnixTimeSeconds()));
        db.SeedFeatureSwitch(Row(2, "activity.future", startTime: now.AddSeconds(3600).ToUnixTimeSeconds()));
        db.SeedFeatureSwitch(Row(3, "activity.live", startTime: now.AddSeconds(-3600).ToUnixTimeSeconds(), endTime: now.AddSeconds(3600).ToUnixTimeSeconds()));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request())));

        var switches = data.GetProperty("Switches");
        Assert.Equal(1, switches.GetArrayLength());
        Assert.Equal("activity.live", switches[0].GetProperty("SwitchKey").GetString());
        // 时间窗以 Unix 秒（数字）下发给客户端
        Assert.True(switches[0].GetProperty("StartTime").GetInt64() > 0);
        Assert.True(switches[0].GetProperty("EndTime").GetInt64() > 0);
    }

    /// <summary>
    /// 客户端通道消费方过滤：Audience=server 的开关不下发（client|both 命中）。
    /// </summary>
    [Fact]
    public async Task GetInfo_AudienceServerRow_NotDeliveredOnClientChannel()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        db.SeedFeatureSwitch(Row(1, "economy.drop_rate", "{\"rate\":2}", audience: FeatureSwitchAudienceValues.Server));
        db.SeedFeatureSwitch(Row(2, "login.banner", "{\"v\":1}", audience: FeatureSwitchAudienceValues.Both));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request())));

        var switches = data.GetProperty("Switches");
        Assert.Equal(1, switches.GetArrayLength());
        Assert.Equal("login.banner", switches[0].GetProperty("SwitchKey").GetString());
    }

    /// <summary>
    /// 设备灰度：灰度比例 0 的行即使携带 DeviceId 也不命中；白名单旁路绕过灰度比例直接命中。
    /// </summary>
    [Fact]
    public async Task GetInfo_RolloutZero_HiddenUnlessWhitelisted()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        db.SeedFeatureSwitch(Row(1, "activity.gray", "{\"v\":1}", rolloutPercent: 0));
        db.SeedFeatureSwitch(Row(2, "activity.whitelist", "{\"v\":2}", rolloutPercent: 0, deviceWhitelistJson: "[\"Device-Alpha\"]"));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request(deviceId: "Device-Alpha"))));

        var switches = data.GetProperty("Switches");
        Assert.Equal(1, switches.GetArrayLength());
        Assert.Equal("activity.whitelist", switches[0].GetProperty("SwitchKey").GetString());
    }

    /// <summary>
    /// 缺 DeviceId 仅命中全量投放（RolloutPercent=100）行，部分灰度行不下发——与远程配置投放同语义。
    /// </summary>
    [Fact]
    public async Task GetInfo_MissingDeviceId_OnlyFullRolloutRowsDelivered()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        db.SeedFeatureSwitch(Row(1, "activity.full", "{\"v\":1}", rolloutPercent: 100));
        db.SeedFeatureSwitch(Row(2, "activity.partial", "{\"v\":2}", rolloutPercent: 50));
        var controller = CreateController(db);
        db.AttachContext(controller);

        var data = ClientApiResultReader.Data(ClientApiResultReader.Read(await controller.GetInfo(Request())));

        var switches = data.GetProperty("Switches");
        Assert.Equal(1, switches.GetArrayLength());
        Assert.Equal("activity.full", switches[0].GetProperty("SwitchKey").GetString());
    }

    /// <summary>
    /// 缺租户上下文 fail-fast：抛 MissingTenantContextException（C314 语义，不回落默认租户）。
    /// </summary>
    [Fact]
    public async Task GetInfo_MissingTenant_ThrowsMissingTenantContext()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        var controller = CreateController(db);
        db.AttachContext(controller, includeTenant: false);

        await Assert.ThrowsAsync<MissingTenantContextException>(() => controller.GetInfo(Request()));
    }

    /// <summary>
    /// 缺应用上下文 fail-fast：抛 UnauthorizedAccessException（ASP.NET 管道转换为 401）。
    /// </summary>
    [Fact]
    public async Task GetInfo_MissingApp_ThrowsUnauthorized()
    {
        using var db = new ClientApiTestDb();
        db.InitTenantTables(typeof(TenantGameAppFeatureSwitchEntity), typeof(TenantGameAppPackageEntity));
        var controller = CreateController(db);
        db.AttachContext(controller, includeApp: false);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => controller.GetInfo(Request()));
    }
}
