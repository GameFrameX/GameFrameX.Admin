using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Client.Api.Services;
using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using GameFrameX.Entity.Tenant.Game.App;
using SqlSugar;

namespace GameFrameX.Core.Tests.ClientApi;

/// <summary>
/// GFX-86 / C89 回归测试：GameAppPackageResolver 运行时继承语义与缓存刷新信号。
/// 覆盖 InheritanceMode 三态（Inherit/Override/DisableAsync）回退行为、禁用分包不参与回退、
/// 以及缓存信号对子包新增 / 父子关系重指 / 母包改名 / 继承模式切换的响应。
/// </summary>
public sealed class GameAppPackageResolverInheritanceTests : IDisposable
{
    private readonly List<string> _dbPaths = new();

    [Fact]
    public void Resolve_InheritChild_ReturnsMasterName()
    {
        var fixture = BuildFixture(2001);
        fixture.SeedMaster(9201, "com.test.master");
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Inherit);

        Assert.Equal("com.test.master", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void Resolve_OverrideChild_ReturnsOwnName_NoFallback()
    {
        var fixture = BuildFixture(2002);
        fixture.SeedMaster(9201, "com.test.master");
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Override);

        // Override：子包自洽配置，不回退母包。
        Assert.Equal("com.test.child", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void Resolve_DisableChild_ReturnsOwnName_NoFallback()
    {
        var fixture = BuildFixture(2003);
        fixture.SeedMaster(9201, "com.test.master");
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Disable);

        // DisableAsync：显式禁用，不回退母包。
        Assert.Equal("com.test.child", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void Resolve_DisabledChild_ReturnsOwnName_EvenIfInherit()
    {
        var fixture = BuildFixture(2004);
        fixture.SeedMaster(9201, "com.test.master");
        // Inherit 子包但被禁用，不参与回退。
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Inherit, false);

        Assert.Equal("com.test.child", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void Resolve_DisabledMaster_InheritChildReturnsOwnName()
    {
        var fixture = BuildFixture(2005);
        fixture.SeedMaster(9201, "com.test.master", false);
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Inherit);

        // 母包被禁用，子包无可用母包名映射，回退不生效。
        Assert.Equal("com.test.child", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void CacheRefresh_AddingChild_PicksUpNewMapping()
    {
        var fixture = BuildFixture(2006);
        fixture.SeedMaster(9201, "com.test.master", updateTime: 1000);
        // 首次解析：尚无子包，命中失败。
        Assert.Equal("com.test.child", fixture.Resolver.Resolve("com.test.child"));

        // 新增 Inherit 子包，count + maxUpdateTime 双变化触发刷新。
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Inherit, updateTime: 2000);

        Assert.Equal("com.test.master", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void CacheRefresh_MasterRename_PropagatesToResolve()
    {
        var fixture = BuildFixture(2007);
        fixture.SeedMaster(9201, "com.test.master", updateTime: 1000);
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Inherit, updateTime: 1000);
        Assert.Equal("com.test.master", fixture.Resolver.Resolve("com.test.child"));

        // 母包改名（UpdateTime bump），MaxUpdateTime 变化触发刷新。
        fixture.UpdatePackage(9201, "com.test.master.v2", updateTime: 3000);

        Assert.Equal("com.test.master.v2", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void CacheRefresh_InheritanceModeToggle_DropsMapping()
    {
        var fixture = BuildFixture(2008);
        fixture.SeedMaster(9201, "com.test.master", updateTime: 1000);
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Inherit, updateTime: 1000);
        Assert.Equal("com.test.master", fixture.Resolver.Resolve("com.test.child"));

        // 继承模式切到 Override（count 不变，UpdateTime bump 触发刷新），子包不再映射母包。
        fixture.UpdatePackage(9202, mode: InheritanceModeEnum.Override, updateTime: 4000);

        Assert.Equal("com.test.child", fixture.Resolver.Resolve("com.test.child"));
    }

    [Fact]
    public void CacheRefresh_ParentReassign_SwitchesMaster()
    {
        var fixture = BuildFixture(2009);
        fixture.SeedMaster(9201, "com.test.master.a", updateTime: 1000);
        fixture.SeedMaster(9203, "com.test.master.b", updateTime: 1000);
        fixture.SeedChild(9202, "com.test.child", 9201, InheritanceModeEnum.Inherit, updateTime: 1000);
        Assert.Equal("com.test.master.a", fixture.Resolver.Resolve("com.test.child"));

        // 父子关系重指到另一个母包（count 不变，UpdateTime bump 触发刷新）。
        fixture.UpdatePackage(9202, parentPackageId: 9203, updateTime: 5000);

        Assert.Equal("com.test.master.b", fixture.Resolver.Resolve("com.test.child"));
    }

    private ResolverFixture BuildFixture(long tenantId)
    {
        var mainConfig = CreateConfig("main");
        var tenantConfig = CreateConfig(tenantId.ToString());
        var scope = new SqlSugarScope(mainConfig);
        scope.CodeFirst.InitTables(typeof(SystemTenantEntity));
        scope.Insertable(CreateTenant(tenantId, tenantConfig.ConnectionString)).ExecuteCommand();

        var tenantIdResolver = new DelegateSqlSugarTenantIdResolver(() => tenantId);
        var provider = new ClientApiTenantDbProvider(scope, tenantIdResolver);
        var tenantDb = provider.GetTenantDb(tenantId);
        tenantDb.CodeFirst.InitTables(typeof(TenantGameAppPackageEntity));

        return new ResolverFixture(scope, provider, tenantIdResolver, tenantDb, tenantId);
    }

    private ConnectionConfig CreateConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_pkg_resolver_{configId}_{Guid.NewGuid():N}.db");
        _dbPaths.Add(dbPath);
        var config = new ConnectionConfig
        {
            ConfigId = configId,
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={dbPath}",
            IsAutoCloseConnection = true,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        return config;
    }

    private static SystemTenantEntity CreateTenant(long tenantId, string connectionString)
    {
        return new SystemTenantEntity
        {
            Id = tenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = connectionString,
            ConfigId = tenantId.ToString(),
            IsEnabled = true,
        };
    }

    public void Dispose()
    {
        foreach (var dbPath in _dbPaths)
        {
            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
            }
        }
    }

    private sealed class ResolverFixture
    {
        private readonly SqlSugarScope _scope;
        private readonly ISqlSugarClient _tenantDb;
        private readonly long _tenantId;

        public ResolverFixture(SqlSugarScope scope, ClientApiTenantDbProvider provider, ISqlSugarTenantIdResolver tenantIdResolver, ISqlSugarClient tenantDb, long tenantId)
        {
            _scope = scope;
            _tenantDb = tenantDb;
            _tenantId = tenantId;
            Resolver = new GameAppPackageResolver(provider, tenantIdResolver);
        }

        public GameAppPackageResolver Resolver { get; }

        public void SeedMaster(long id, string packageName, bool isEnabled = true, long updateTime = 0)
        {
            _tenantDb.Insertable(BuildEntity(id, packageName, null, PackageKindEnum.Master, InheritanceModeEnum.Override, isEnabled, updateTime))
                     .ExecuteCommand();
        }

        public void SeedChild(long id, string packageName, long parentPackageId, InheritanceModeEnum mode, bool isEnabled = true, long updateTime = 0)
        {
            _tenantDb.Insertable(BuildEntity(id, packageName, parentPackageId, PackageKindEnum.Derived, mode, isEnabled, updateTime))
                     .ExecuteCommand();
        }

        public void UpdatePackage(long id, string? packageName = null, long? parentPackageId = null, InheritanceModeEnum? mode = null, long updateTime = 0)
        {
            var existing = _tenantDb.Queryable<TenantGameAppPackageEntity>().First(p => p.Id == id);
            if (existing == null)
            {
                return;
            }

            if (packageName != null)
            {
                existing.PackageName = packageName;
            }

            if (parentPackageId.HasValue)
            {
                existing.ParentPackageId = parentPackageId;
            }

            if (mode.HasValue)
            {
                existing.PlatformInheritanceMode = mode.Value;
            }

            existing.UpdateTime = updateTime;
            _tenantDb.Updateable(existing).ExecuteCommand();
        }

        private TenantGameAppPackageEntity BuildEntity(long id, string packageName, long? parentPackageId, PackageKindEnum kind, InheritanceModeEnum mode, bool isEnabled, long updateTime)
        {
            return new TenantGameAppPackageEntity
            {
                Id = id,
                TenantId = _tenantId,
                Name = $"pkg-{id}",
                GameAppId = 1,
                PackageName = packageName,
                PlatformCode = "ios",
                PlatformName = "iOS",
                ChannelCode = "default",
                ChannelName = "Default",
                ParentPackageId = parentPackageId,
                PackageKind = kind,
                PlatformInheritanceMode = mode,
                IsEnabled = isEnabled,
                IsDeleted = false,
                CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                UpdateTime = updateTime,
                RowVersion = 0,
            };
        }
    }
}