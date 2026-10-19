using GameFrameX.Application.Game.App.FeatureSwitch;
using GameFrameX.Application.Game.App.FeatureSwitch.Dto;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.Extension;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.FeatureSwitch;
using SqlSugar;

namespace GameFrameX.Core.Tests.GameBusiness;

/// <summary>
/// C345 活动/功能开关管理服务测试。
/// <para>覆盖：写入侧事实固化（作用域维度归一为 Default 哨兵、消费方标记收小写、时间窗不倒置、
/// ValueJson 合法性、灰度与白名单形状）、同作用域组合查重与自身排除、应用作用域锁定、
/// 租户默认层不得携带具体包名（C80）、分页按应用作用域收口，以及物理表名落 tenant_ 前缀。</para>
/// </summary>
public sealed class TenantGameAppFeatureSwitchServiceTests : IDisposable
{
    private const long App100 = 100;
    private const long App200 = 200;

    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"feature_switch_service_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public TenantGameAppFeatureSwitchServiceTests()
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
        _db.CodeFirst.InitTables(typeof(TenantGameAppPackageEntity));
    }

    /// <summary>
    /// 实体落库物理表名带 tenant_ 前缀（写后读一致性前提，避免租户表名分叉）。
    /// </summary>
    [Fact]
    public async Task PhysicalTableName_CarriesTenantPrefix()
    {
        var service = CreateService();
        await service.AddAsync(NewRequest("switch.table"));

        var tableNames = await _db.Ado.SqlQueryAsync<string>("SELECT name FROM sqlite_master WHERE type = 'table'");
        Assert.Contains("tenant_game_app_feature_switch_entity", tableNames);
    }

    /// <summary>
    /// 作用域维度留空 / 空白统一落 Default 哨兵，库里不并存 null / 空串 / "default" 三种写法。
    /// </summary>
    [Fact]
    public async Task Add_NormalizesBlankScopeDimensionsToDefaultSentinels()
    {
        var service = CreateService();
        var request = NewRequest("switch.dimensions");
        request.Platform = "   ";
        request.Package = null;
        request.Language = string.Empty;
        request.Channel = null;

        var row = await service.AddAsync(request);

        Assert.Equal("default", row.Platform);
        Assert.Equal("default", row.Package);
        Assert.Equal("default", row.Language);
        Assert.Equal("default", row.Channel);
    }

    /// <summary>
    /// 消费方标记收小写后校验取值域：SERVER 归一为 server，未知取值被拒。
    /// </summary>
    [Fact]
    public async Task Add_NormalizesAudienceCase_AndRejectsUnknownValue()
    {
        var service = CreateService();
        var request = NewRequest("switch.audience");
        request.Audience = "SERVER";

        var row = await service.AddAsync(request);
        Assert.Equal("server", row.Audience);

        var invalid = NewRequest("switch.audience.bad");
        invalid.Audience = "everyone";
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(invalid));
    }

    /// <summary>
    /// 同应用 + 同开关键 + 同作用域维度组合重复被拒。
    /// </summary>
    [Fact]
    public async Task Add_RejectsDuplicateScopeCombination()
    {
        var service = CreateService();
        await service.AddAsync(NewRequest("switch.duplicate"));

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(NewRequest("switch.duplicate")));
    }

    /// <summary>
    /// 同开关键不同作用域维度允许共存（同一开关的多条作用域行）。
    /// </summary>
    [Fact]
    public async Task Add_AllowsSameKeyWithDifferentScopeDimension()
    {
        var service = CreateService();
        await InsertAsync("switch.scoped", App100);

        var android = NewRequest("switch.scoped");
        android.Platform = "android";
        var row = await service.AddAsync(android);

        Assert.Equal("android", row.Platform);
    }

    /// <summary>
    /// 租户默认层与应用层是两个独立作用域空间，同键同行互不冲突。
    /// </summary>
    [Fact]
    public async Task Add_AllowsSameKeyAcrossAppAndTenantDefaultLayer()
    {
        var service = CreateService();
        await InsertAsync("switch.layered", App100);

        var tenantDefault = NewRequest("switch.layered");
        tenantDefault.AppId = null;
        var row = await service.AddAsync(tenantDefault);

        Assert.Null(row.AppId);
    }

    /// <summary>
    /// 租户默认层（AppId &lt;= 0）不得携带具体包名（C80 规则，由 PackageScopeValidator 收口）。
    /// </summary>
    [Fact]
    public async Task Add_RejectsConcretePackageOnTenantDefaultLayer()
    {
        var service = CreateService();
        var request = NewRequest("switch.package");
        request.AppId = null;
        request.Package = "com.gameframe.demo";

        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(request));
    }

    /// <summary>
    /// 具体包名必须命中当前应用下启用分包：未登记包名被拒，登记后放行。
    /// </summary>
    [Fact]
    public async Task Add_ValidatesConcretePackageAgainstEnabledAppPackages()
    {
        var service = CreateService();

        var unknown = NewRequest("switch.package.unknown");
        unknown.Package = "com.gameframe.unknown";
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(unknown));

        await InsertPackageAsync(App100, "com.gameframe.known");
        var known = NewRequest("switch.package.known");
        known.Package = "com.gameframe.known";
        var row = await service.AddAsync(known);

        Assert.Equal("com.gameframe.known", row.Package);
    }

    /// <summary>
    /// 时间窗两端均填时不允许倒置（等长窗同样拒绝）。
    /// </summary>
    [Fact]
    public async Task Add_RejectsInvertedTimeWindow()
    {
        var service = CreateService();

        var inverted = NewRequest("switch.window.inverted");
        inverted.StartTime = 2000;
        inverted.EndTime = 1000;
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(inverted));

        var equal = NewRequest("switch.window.equal");
        equal.StartTime = 1000;
        equal.EndTime = 1000;
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(equal));
    }

    /// <summary>
    /// 开关参数必须是合法 JSON，且按原文落库不下发态重写。
    /// </summary>
    [Fact]
    public async Task Add_RejectsInvalidJson_AndKeepsJsonVerbatim()
    {
        var service = CreateService();

        var broken = NewRequest("switch.json.broken");
        broken.ValueJson = "not-a-json{";
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(broken));

        var blank = NewRequest("switch.json.blank");
        blank.ValueJson = "   ";
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(blank));

        var request = NewRequest("switch.json.verbatim");
        request.ValueJson = "{ \"a\" : 1 }";
        var row = await service.AddAsync(request);

        var stored = await _db.Queryable<TenantGameAppFeatureSwitchEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.Equal("{ \"a\" : 1 }", stored.ValueJson);
    }

    /// <summary>
    /// 开关键与显示名去空白后不得为空。
    /// </summary>
    [Fact]
    public async Task Add_RejectsBlankSwitchKeyAndDisplayName()
    {
        var service = CreateService();

        var blankKey = NewRequest("switch.blank.key");
        blankKey.SwitchKey = "   ";
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(blankKey));

        var blankDisplayName = NewRequest("switch.blank.name");
        blankDisplayName.DisplayName = "  ";
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(blankDisplayName));
    }

    /// <summary>
    /// 灰度比例限 0-100；白名单必须为 JSON 数组；空白白名单归一为 null（空串会让投放侧解析走异常分支）。
    /// </summary>
    [Fact]
    public async Task Add_ValidatesRolloutShape()
    {
        var service = CreateService();

        var over = NewRequest("switch.rollout.over");
        over.RolloutPercent = 101;
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(over));

        var negative = NewRequest("switch.rollout.negative");
        negative.RolloutPercent = -1;
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(negative));

        var notArray = NewRequest("switch.rollout.notarray");
        notArray.DeviceWhitelistJson = "{\"device\":\"a\"}";
        await Assert.ThrowsAsync<BusinessException>(() => service.AddAsync(notArray));

        var blankWhitelist = NewRequest("switch.rollout.blank");
        blankWhitelist.DeviceWhitelistJson = "   ";
        var row = await service.AddAsync(blankWhitelist);
        Assert.Null(row.DeviceWhitelistJson);
    }

    /// <summary>
    /// 更新锁定应用作用域：调用方传 AppId 也不允许把应用行改挂到租户默认层。
    /// </summary>
    [Fact]
    public async Task Update_LocksAppScope()
    {
        var service = CreateService();
        var row = await service.AddAsync(NewRequest("switch.locked"));

        await service.UpdateAsync(new UpdateGameAppFeatureSwitchRequest
        {
            Id = row.Id,
            RowVersion = row.RowVersion,
            AppId = null,
            SwitchKey = row.SwitchKey,
            DisplayName = row.DisplayName,
            Audience = row.Audience,
            ValueJson = "{\"v\":2}",
        });

        var stored = await _db.Queryable<TenantGameAppFeatureSwitchEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.Equal(App100, stored.AppId);
    }

    /// <summary>
    /// 更新沿用自身作用域组合不算重复（查重须排除自身）。
    /// </summary>
    [Fact]
    public async Task Update_ExcludesSelfFromDuplicateCheck()
    {
        var service = CreateService();
        var row = await service.AddAsync(NewRequest("switch.self"));

        await service.UpdateAsync(new UpdateGameAppFeatureSwitchRequest
        {
            Id = row.Id,
            RowVersion = row.RowVersion,
            AppId = App100,
            SwitchKey = row.SwitchKey,
            DisplayName = "改名后",
            Audience = row.Audience,
            ValueJson = "{\"v\":2}",
        });

        var stored = await _db.Queryable<TenantGameAppFeatureSwitchEntity>().FirstAsync(u => u.Id == row.Id);
        Assert.Equal("改名后", stored.DisplayName);
    }

    /// <summary>
    /// 更新不存在的记录返回「记录不存在」而不是静默成功。
    /// </summary>
    [Fact]
    public async Task Update_UnknownId_IsRejected()
    {
        var service = CreateService();

        await Assert.ThrowsAsync<BusinessException>(() => service.UpdateAsync(new UpdateGameAppFeatureSwitchRequest
        {
            Id = 99999999,
            SwitchKey = "switch.missing",
            DisplayName = "不存在",
            Audience = "both",
            ValueJson = "{}",
        }));
    }

    /// <summary>
    /// 分页按应用作用域收口：正值只看该应用行，留空只看租户默认层行，两层不混列。
    /// </summary>
    [Fact]
    public async Task Page_ScopesToAppOrTenantDefaultLayer()
    {
        await InsertAsync("switch.app100", App100);
        await InsertAsync("switch.app200", App200);
        await InsertAsync("switch.default", null);

        var service = CreateService();

        var appPage = await service.PageAsync(new PageGameAppFeatureSwitchRequest { AppId = App100, });
        var appRow = Assert.Single(appPage.Items);
        Assert.Equal("switch.app100", appRow.SwitchKey);

        var defaultPage = await service.PageAsync(new PageGameAppFeatureSwitchRequest());
        var defaultRow = Assert.Single(defaultPage.Items);
        Assert.Equal("switch.default", defaultRow.SwitchKey);
    }

    private TenantGameAppFeatureSwitchService CreateService()
    {
        return new TenantGameAppFeatureSwitchService(
            new SqlSugarRepository<TenantGameAppFeatureSwitchEntity> { Context = _db, },
            new SqlSugarRepository<TenantGameAppPackageEntity> { Context = _db, });
    }

    private static AddGameAppFeatureSwitchRequest NewRequest(string switchKey)
    {
        return new AddGameAppFeatureSwitchRequest
        {
            AppId = App100,
            SwitchKey = switchKey,
            DisplayName = switchKey,
            Audience = FeatureSwitchAudienceValues.Both,
            ValueJson = "{}",
        };
    }

    // 前置数据一律用显式主键的 InsertAsync 铺（主键确定性可控，断言不依赖取号时序）。
    // 注：测试进程的 SqlSugar 取号已由 SqlSugarSnowflakeFallbackInitializer 进程级接管
    // （紧凑布局、同毫秒有序列），旧「同毫秒两次插入同主键」退化不再成立（C361 / GFX-804）。
    private long _nextSeedId = 900000;

    private async Task InsertAsync(string switchKey, long? appId)
    {
        await _db.Insertable(new TenantGameAppFeatureSwitchEntity
        {
            Id = _nextSeedId++,
            AppId = appId,
            SwitchKey = switchKey,
            DisplayName = switchKey,
            Audience = FeatureSwitchAudienceValues.Both,
            ValueJson = "{}",
            Platform = "default",
            Package = "default",
            Language = "default",
            Channel = "default",
            RolloutPercent = 100,
            IsEnabled = true,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsDeleted = false,
        }).ExecuteCommandAsync();
    }

    private async Task InsertPackageAsync(long gameAppId, string packageName)
    {
        await _db.Insertable(new TenantGameAppPackageEntity
        {
            Id = _nextSeedId++,
            Name = packageName,
            GameAppId = gameAppId,
            PackageName = packageName,
            PlatformCode = "android",
            PlatformName = "安卓",
            ChannelCode = "official",
            ChannelName = "官方",
            IsEnabled = true,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            IsDeleted = false,
        }).ExecuteCommandAsync();
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
