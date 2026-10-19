using GameFrameX.Application.Game.App.FeatureSwitch;
using GameFrameX.Core.Const;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;
using SqlSugar;

namespace GameFrameX.Core.Tests.GameBusiness;

/// <summary>
/// C345 活动/功能开关投放解析器测试。
/// <para>覆盖：winner 作用域评分与 tie-break、时间窗边界（左闭右开）、消费方过滤、设备灰度与白名单旁路、
/// 租户默认层与应用覆盖同键合并、版本号决定性（行序无关 / 字段敏感 / 灰度无关），
/// 以及候选查询的应用作用域收口（跨应用不泄漏）。</para>
/// </summary>
public sealed class FeatureSwitchDeliveryResolverTests : IDisposable
{
    private const long TenantId = 1001;
    private const long App100 = 100;
    private const long App200 = 200;

    private static readonly DateTime NowUtc = new DateTime(2026, 9, 14, 12, 0, 0, DateTimeKind.Utc);

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"feature_switch_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public FeatureSwitchDeliveryResolverTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameAppFeatureSwitchEntity));
    }

    /// <summary>
    /// 应用覆盖行与租户默认行同键时，应用行胜出（作用域评分 App 分 64 压过默认层）。
    /// </summary>
    [Fact]
    public void Resolve_AppRowOverridesTenantDefaultRowForSameSwitchKey()
    {
        var tenantDefault = Row("login.banner", id: 1, appId: null, valueJson: "{\"v\":\"tenant\"}");
        var appRow = Row("login.banner", id: 2, appId: App100, valueJson: "{\"v\":\"app\"}");

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { tenantDefault, appRow, }, Query());

        var item = Assert.Single(result.Switches);
        Assert.Equal("{\"v\":\"app\"}", item.ValueJson);
    }

    /// <summary>
    /// 同应用同开关键同作用域（存量数据可能出现）时按投放优先级降序决胜。
    /// </summary>
    [Fact]
    public void Resolve_PriorityBreaksScopeScoreTie()
    {
        var lowPriority = Row("login.banner", id: 1, priority: 0, valueJson: "{\"v\":\"low\"}");
        var highPriority = Row("login.banner", id: 2, priority: 9, valueJson: "{\"v\":\"high\"}");

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { lowPriority, highPriority, }, Query());

        var item = Assert.Single(result.Switches);
        Assert.Equal("{\"v\":\"high\"}", item.ValueJson);
    }

    /// <summary>
    /// 作用域评分与优先级均相同时按主键降序决胜（新行优先），且结论与候选行顺序无关。
    /// </summary>
    [Fact]
    public void Resolve_NewerRowWinsWhenScoreAndPriorityTie()
    {
        var older = Row("login.banner", id: 10, valueJson: "{\"v\":\"older\"}");
        var newer = Row("login.banner", id: 11, valueJson: "{\"v\":\"newer\"}");

        var forward = FeatureSwitchDeliveryResolver.Resolve(new[] { older, newer, }, Query());
        var backward = FeatureSwitchDeliveryResolver.Resolve(new[] { newer, older, }, Query());

        Assert.Equal("{\"v\":\"newer\"}", Assert.Single(forward.Switches).ValueJson);
        Assert.Equal("{\"v\":\"newer\"}", Assert.Single(backward.Switches).ValueJson);
    }

    /// <summary>
    /// 调用方携带平台上下文时，平台具体值行以 32 分压过 Default 哨兵行。
    /// </summary>
    [Fact]
    public void Resolve_ScopeSpecificRowOutranksDefaultSentinelRow()
    {
        var defaultRow = Row("login.banner", id: 1, valueJson: "{\"v\":\"default\"}");
        var androidRow = Row("login.banner", id: 2, platform: "android", valueJson: "{\"v\":\"android\"}");

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { defaultRow, androidRow, }, Query(platform: "android"));

        var item = Assert.Single(result.Switches);
        Assert.Equal("{\"v\":\"android\"}", item.ValueJson);
    }

    /// <summary>
    /// 调用方无平台上下文时只匹配 Default 哨兵行，具体值行不参与投放。
    /// </summary>
    [Fact]
    public void Resolve_SpecificScopeRowIsSkippedWhenCallerHasNoDimension()
    {
        var androidRow = Row("login.banner", id: 2, platform: "android", valueJson: "{\"v\":\"android\"}");

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { androidRow, }, Query());

        Assert.Empty(result.Switches);
    }

    /// <summary>
    /// 未到开始时间的开关不下发（左闭右开区间的左端由下一条测试覆盖）。
    /// </summary>
    [Fact]
    public void Resolve_NotStartedSwitchIsNotDelivered()
    {
        var future = ToUnixSeconds(NowUtc.AddMinutes(1));
        var row = Row("login.banner", id: 1, startTime: future);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query());

        Assert.Empty(result.Switches);
    }

    /// <summary>
    /// 已过结束时间的开关不下发。
    /// </summary>
    [Fact]
    public void Resolve_EndedSwitchIsNotDelivered()
    {
        var past = ToUnixSeconds(NowUtc.AddMinutes(-1));
        var row = Row("login.banner", id: 1, endTime: past);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query());

        Assert.Empty(result.Switches);
    }

    /// <summary>
    /// 时间窗为左闭右开：nowUtc == StartTime 视为已开始而下发，nowUtc == EndTime 视为已结束而不下发。
    /// </summary>
    [Fact]
    public void Resolve_TimeWindowIsStartInclusiveEndExclusive()
    {
        var nowSeconds = ToUnixSeconds(NowUtc);
        var starting = Row("switch.starting", id: 1, startTime: nowSeconds);
        var ending = Row("switch.ending", id: 2, endTime: nowSeconds);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { starting, ending, }, Query());

        var item = Assert.Single(result.Switches);
        Assert.Equal("switch.starting", item.SwitchKey);
    }

    /// <summary>
    /// 时间窗两端均可空，null 表示该端不限。
    /// </summary>
    [Fact]
    public void Resolve_NullTimeWindowBoundsAreUnlimited()
    {
        var row = Row("login.banner", id: 1, startTime: null, endTime: null);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query());

        var item = Assert.Single(result.Switches);
        Assert.Null(item.StartTime);
        Assert.Null(item.EndTime);
    }

    /// <summary>
    /// 时间窗输出沿用 UTC 时刻（实体列为 Unix 秒），保证消费方无需再猜时区。
    /// </summary>
    [Fact]
    public void Resolve_TimeWindowIsProjectedAsUtc()
    {
        var startSeconds = ToUnixSeconds(NowUtc.AddMinutes(-5));
        var endSeconds = ToUnixSeconds(NowUtc.AddMinutes(5));
        var row = Row("login.banner", id: 1, startTime: startSeconds, endTime: endSeconds);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query());

        var item = Assert.Single(result.Switches);
        Assert.Equal(NowUtc.AddMinutes(-5), item.StartTime);
        Assert.Equal(NowUtc.AddMinutes(5), item.EndTime);
    }

    /// <summary>
    /// 客户端通道看不到 server-only 开关；both 对两个通道均下发。
    /// </summary>
    [Fact]
    public void Resolve_ClientChannelHidesServerOnlySwitch()
    {
        var serverOnly = Row("server.only", id: 1, audience: FeatureSwitchAudienceValues.Server);
        var both = Row("both.switch", id: 2, audience: FeatureSwitchAudienceValues.Both);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { serverOnly, both, }, Query(FeatureSwitchAudience.Client));

        var item = Assert.Single(result.Switches);
        Assert.Equal("both.switch", item.SwitchKey);
    }

    /// <summary>
    /// 服务器通道看不到 client-only 开关。
    /// </summary>
    [Fact]
    public void Resolve_ServerChannelHidesClientOnlySwitch()
    {
        var clientOnly = Row("client.only", id: 1, audience: FeatureSwitchAudienceValues.Client);
        var both = Row("both.switch", id: 2, audience: FeatureSwitchAudienceValues.Both);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { clientOnly, both, }, Query(FeatureSwitchAudience.Server));

        var item = Assert.Single(result.Switches);
        Assert.Equal("both.switch", item.SwitchKey);
    }

    /// <summary>
    /// 服务器通道无设备身份，不做设备灰度：比例 0 且无白名单也照常下发。
    /// </summary>
    [Fact]
    public void Resolve_ServerChannelSkipsDeviceRollout()
    {
        var row = Row("server.only", id: 1, audience: FeatureSwitchAudienceValues.Server, rolloutPercent: 0);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query(FeatureSwitchAudience.Server));

        Assert.Single(result.Switches);
    }

    /// <summary>
    /// 客户端通道灰度：比例 0 不下发，白名单设备绕过比例直接命中。
    /// </summary>
    [Fact]
    public void Resolve_ClientChannelHonoursRolloutAndWhitelistBypass()
    {
        var zeroRollout = Row("switch.a", id: 1, rolloutPercent: 0);
        var whitelisted = Row("switch.b", id: 2, rolloutPercent: 0, deviceWhitelistJson: "[\"device-a\"]");

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { zeroRollout, whitelisted, }, Query(FeatureSwitchAudience.Client, deviceId: "DEVICE-A"));

        var item = Assert.Single(result.Switches);
        Assert.Equal("switch.b", item.SwitchKey);
    }

    /// <summary>
    /// 客户端缺少设备标识时只允许命中全量投放记录。
    /// </summary>
    [Fact]
    public void Resolve_MissingDeviceIdOnlyMatchesFullRollout()
    {
        var partial = Row("switch.partial", id: 1, rolloutPercent: 50);
        var full = Row("switch.full", id: 2, rolloutPercent: 100);

        var result = FeatureSwitchDeliveryResolver.Resolve(new[] { partial, full, }, Query(FeatureSwitchAudience.Client, deviceId: null));

        var item = Assert.Single(result.Switches);
        Assert.Equal("switch.full", item.SwitchKey);
    }

    /// <summary>
    /// 被停用的行不参与投放，也不进入版本号输入集合。
    /// </summary>
    [Fact]
    public void Resolve_DisabledRowIsNotDelivered()
    {
        var enabled = Row("switch.enabled", id: 1, valueJson: "{\"v\":1}");
        var disabled = Row("switch.disabled", id: 2, valueJson: "{\"v\":2}", isEnabled: false);

        var onlyEnabled = FeatureSwitchDeliveryResolver.Resolve(new[] { enabled, }, Query());
        var withDisabled = FeatureSwitchDeliveryResolver.Resolve(new[] { enabled, disabled, }, Query());

        Assert.Single(withDisabled.Switches);
        Assert.Equal(onlyEnabled.Version, withDisabled.Version);
    }

    /// <summary>
    /// 版本号是行序无关的：同一集合打乱顺序得到同一版本号。
    /// </summary>
    [Fact]
    public void Resolve_VersionIsIndependentOfRowOrder()
    {
        var first = Row("switch.a", id: 1);
        var second = Row("switch.b", id: 2);
        var third = Row("switch.c", id: 3);

        var ordered = FeatureSwitchDeliveryResolver.Resolve(new[] { first, second, third, }, Query());
        var shuffled = FeatureSwitchDeliveryResolver.Resolve(new[] { third, first, second, }, Query());

        Assert.Equal(ordered.Version, shuffled.Version);
    }

    /// <summary>
    /// 任一业务字段变化必变版本号（构造保证「内容变版本必变」）。
    /// </summary>
    [Fact]
    public void Resolve_VersionChangesWhenBusinessFieldChanges()
    {
        var before = FeatureSwitchDeliveryResolver.Resolve(new[] { Row("switch.a", id: 1, valueJson: "{\"v\":1}"), }, Query());
        var after = FeatureSwitchDeliveryResolver.Resolve(new[] { Row("switch.a", id: 1, valueJson: "{\"v\":2}"), }, Query());

        Assert.NotEqual(before.Version, after.Version);
    }

    /// <summary>
    /// 候选行集合增删必变版本号。
    /// </summary>
    [Fact]
    public void Resolve_VersionChangesWhenRowSetChanges()
    {
        var single = FeatureSwitchDeliveryResolver.Resolve(new[] { Row("switch.a", id: 1), }, Query());
        var doubled = FeatureSwitchDeliveryResolver.Resolve(new[] { Row("switch.a", id: 1), Row("switch.b", id: 2), }, Query());

        Assert.NotEqual(single.Version, doubled.Version);
    }

    /// <summary>
    /// 活动进出时间窗时版本号自然变化（消费方短轮询即可感知）。
    /// </summary>
    [Fact]
    public void Resolve_VersionChangesWhenSwitchEntersTimeWindow()
    {
        var startSeconds = ToUnixSeconds(NowUtc.AddMinutes(30));
        var row = Row("switch.scheduled", id: 1, startTime: startSeconds);

        var beforeWindow = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query(nowUtc: NowUtc));
        var afterWindow = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query(nowUtc: NowUtc.AddMinutes(31)));

        Assert.Empty(beforeWindow.Switches);
        Assert.Single(afterWindow.Switches);
        Assert.NotEqual(beforeWindow.Version, afterWindow.Version);
    }

    /// <summary>
    /// 版本号按通道视角计算：另一通道专属的行不参与本通道版本号。
    /// </summary>
    [Fact]
    public void Resolve_VersionIsPerAudienceView()
    {
        var shared = Row("both.switch", id: 1, audience: FeatureSwitchAudienceValues.Both);
        var clientOnly = Row("client.only", id: 2, audience: FeatureSwitchAudienceValues.Client);

        var clientView = FeatureSwitchDeliveryResolver.Resolve(new[] { shared, clientOnly, }, Query(FeatureSwitchAudience.Client));
        var serverView = FeatureSwitchDeliveryResolver.Resolve(new[] { shared, clientOnly, }, Query(FeatureSwitchAudience.Server));

        Assert.NotEqual(clientView.Version, serverView.Version);
    }

    /// <summary>
    /// 设备灰度结论不参与版本号：换设备只影响下发集合，不影响版本号。
    /// </summary>
    [Fact]
    public void Resolve_VersionIgnoresDeviceRolloutOutcome()
    {
        var row = Row("switch.rolled", id: 1, valueJson: "{\"v\":1}", rolloutPercent: 1);

        var hit = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query(FeatureSwitchAudience.Client, deviceId: "device-a", nowUtc: NowUtc));
        var other = FeatureSwitchDeliveryResolver.Resolve(new[] { row, }, Query(FeatureSwitchAudience.Client, deviceId: "device-b", nowUtc: NowUtc));

        Assert.Equal(hit.Version, other.Version);
    }

    /// <summary>
    /// 候选查询收口在应用作用域：应用行与租户默认层行同时进入候选，其他应用的行被排除。
    /// </summary>
    [Fact]
    public async Task ResolveAsync_ScopesCandidatesToAppAndTenantDefaultLayer()
    {
        await InsertAsync(Row("switch.app", id: 900001, appId: App100));
        await InsertAsync(Row("switch.default", id: 900002, appId: null));
        await InsertAsync(Row("switch.other", id: 900003, appId: App200));

        var result = await CreateResolver().ResolveAsync(Query());

        var keys = result.Switches.Select(item => item.SwitchKey).OrderBy(item => item, StringComparer.Ordinal).ToArray();
        Assert.Equal(new[] { "switch.app", "switch.default", }, keys);
    }

    private FeatureSwitchDeliveryResolver CreateResolver()
    {
        return new FeatureSwitchDeliveryResolver(new SqlSugarRepository<TenantGameAppFeatureSwitchEntity> { Context = _db, });
    }

    private async Task InsertAsync(TenantGameAppFeatureSwitchEntity row)
    {
        row.CreatedTime = ToUnixSeconds(NowUtc);
        row.IsDeleted = false;
        await _db.Insertable(row).ExecuteCommandAsync();
    }

    private static long ToUnixSeconds(DateTime utc)
    {
        return new DateTimeOffset(utc, TimeSpan.Zero).ToUnixTimeSeconds();
    }

    private static FeatureSwitchDeliveryQuery Query(
        FeatureSwitchAudience audience = FeatureSwitchAudience.Client,
        long? appId = App100,
        string? platform = null,
        IReadOnlyList<string>? packageCandidates = null,
        string? language = null,
        string? channel = null,
        string? deviceId = null,
        DateTime? nowUtc = null)
    {
        return new FeatureSwitchDeliveryQuery
        {
            TenantId = TenantId,
            AppId = appId,
            Audience = audience,
            Platform = platform,
            PackageCandidates = packageCandidates,
            Language = language,
            Channel = channel,
            DeviceId = deviceId,
            NowUtc = nowUtc ?? NowUtc,
        };
    }

    private static TenantGameAppFeatureSwitchEntity Row(
        string switchKey,
        long id,
        long? appId = App100,
        string audience = FeatureSwitchAudienceValues.Both,
        string valueJson = "{}",
        long? startTime = null,
        long? endTime = null,
        string platform = GameDimensionConst.PlatformDefault,
        string package = GameDimensionConst.PackageNameDefault,
        string language = GameDimensionConst.LanguageDefault,
        string channel = GameDimensionConst.ChannelNameDefault,
        int priority = 0,
        int rolloutPercent = 100,
        string? deviceWhitelistJson = null,
        bool? isEnabled = true)
    {
        return new TenantGameAppFeatureSwitchEntity
        {
            Id = id,
            AppId = appId,
            SwitchKey = switchKey,
            DisplayName = switchKey,
            Audience = audience,
            ValueJson = valueJson,
            StartTime = startTime,
            EndTime = endTime,
            Platform = platform,
            Package = package,
            Language = language,
            Channel = channel,
            Priority = priority,
            RolloutPercent = rolloutPercent,
            DeviceWhitelistJson = deviceWhitelistJson,
            IsEnabled = isEnabled,
        };
    }

    public void Dispose()
    {
        _db.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
