using GameFrameX.Client.Api.Services.PackageVersion;
using GameFrameX.Client.Api.Middleware;
using GameFrameX.Core.Middleware;
using GameFrameX.Core.Security;
using GameFrameX.Client.Api.Services;
using GameFrameX.Client.Api.Services.ClientApiTenantOptions;
using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.ClientApiConfig;
using GameFrameX.Entity.Tenant.Game;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.System;
using Microsoft.AspNetCore.Http;
using SqlSugar;

namespace GameFrameX.Core.Tests.ClientApi;

public class ClientApiTenantOptionsProviderTests : IDisposable
{
    private readonly List<string> _dbPaths = new();

    [Fact]
    public void ResolveContext_ThrowsWhenTenantContextMissing()
    {
        // 缺少租户上下文时必须抛 MissingTenantContextException，禁止回落主库或进程级业务参数
        var accessor = new HttpContextAccessor { HttpContext = new DefaultHttpContext(), };
        var provider = new ClientApiTenantOptionsProvider(null!, accessor);

        Assert.Throws<MissingTenantContextException>(() => provider.ResolveContext());
    }

    [Fact]
    public void ResolveContext_ReturnsTenantIdAndAppIdFromMiddleware()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TenantContextAccessor.TenantIdKey] = 77L;
        httpContext.Items[AppResolutionMiddleware.AppIdKey] = 88L;
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        var provider = new ClientApiTenantOptionsProvider(null!, accessor);

        var resolution = provider.ResolveContext();

        Assert.Equal(77, resolution.TenantId);
        Assert.Equal(88, resolution.AppId);
    }

    [Fact]
    public void ResolveContext_AppIdNullWhenAppContextMissing()
    {
        // 无应用上下文时只能读取租户默认配置（AppId == null）
        var httpContext = new DefaultHttpContext();
        httpContext.Items[TenantContextAccessor.TenantIdKey] = 77L;
        var accessor = new HttpContextAccessor { HttpContext = httpContext, };
        var provider = new ClientApiTenantOptionsProvider(null!, accessor);

        var resolution = provider.ResolveContext();

        Assert.Equal(77, resolution.TenantId);
        Assert.Null(resolution.AppId);
    }

    [Theory]
    [InlineData("", 0)]
    [InlineData("k1=base64a", 1)]
    [InlineData("k1=base64a,k2=base64b", 2)]
    [InlineData("k1=base64a,,k2=base64b", 2)]
    [InlineData("k1=, =base64b", 0)]
    public void ParseAdMobKeys_HandlesFormats(string raw, int expectedCount)
    {
        var keys = ClientApiTenantOptionsProvider.ParseAdMobKeys(raw);

        Assert.Equal(expectedCount, keys.Count);
    }

    [Fact]
    public void ParseAdMobKeys_PreservesKeyIdAndValue()
    {
        var keys = ClientApiTenantOptionsProvider.ParseAdMobKeys("123=abc==,456=xyz");

        Assert.Equal("abc==", keys["123"]);
        Assert.Equal("xyz", keys["456"]);
    }

    [Fact]
    public void TenantDbProvider_UsesTenantConnectionForClientApiConfig()
    {
        var tenantId = 1001L;
        var mainConfig = CreateConfig("main");
        var tenantConfig = CreateConfig(tenantId.ToString());
        using var scope = new SqlSugarScope(mainConfig);

        scope.CodeFirst.InitTables(typeof(SystemTenantEntity));
        scope.Insertable(new SystemTenantEntity
        {
            Id = tenantId,
            TenantType = TenantTypeEnum.Db,
            DbType = DbType.Sqlite,
            Connection = tenantConfig.ConnectionString,
            ConfigId = tenantId.ToString(),
            IsEnabled = true,
        }).ExecuteCommand();

        var provider = new ClientApiTenantDbProvider(scope, new DelegateSqlSugarTenantIdResolver(() => tenantId));
        var tenantDb = provider.GetTenantDb(tenantId);
        tenantDb.CodeFirst.InitTables(typeof(TenantAdvertisementCallbackConfigEntity));

        Assert.True(TableExists(tenantDb, typeof(TenantAdvertisementCallbackConfigEntity)));
        Assert.False(TableExists(scope, typeof(TenantAdvertisementCallbackConfigEntity)));
    }

    [Fact]
    public void ClientApiRepository_UsesTenantDatabaseForTenantEntities()
    {
        var tenantId = 1002L;
        var mainConfig = CreateConfig("main");
        var tenantConfig = CreateConfig(tenantId.ToString());
        using var scope = new SqlSugarScope(mainConfig);
        scope.CodeFirst.InitTables(typeof(SystemTenantEntity));
        scope.Insertable(CreateTenant(tenantId, tenantConfig.ConnectionString)).ExecuteCommand();

        var resolver = new DelegateSqlSugarTenantIdResolver(() => tenantId);
        var provider = new ClientApiTenantDbProvider(scope, resolver);
        var tenantDb = provider.GetTenantDb(tenantId);
        tenantDb.CodeFirst.InitTables(typeof(TenantGameAppEntity));

        var repository = new Repository<TenantGameAppEntity>(scope, provider, resolver);

        Assert.True(TableExists(repository.AsSugarClient(), typeof(TenantGameAppEntity)));
        Assert.False(TableExists(scope, typeof(TenantGameAppEntity)));
    }

    [Fact]
    public void AppCacheService_ReadsGameAppsFromTenantDatabase()
    {
        var tenantId = 1003L;
        var mainConfig = CreateConfig("main");
        var tenantConfig = CreateConfig(tenantId.ToString());
        using var scope = new SqlSugarScope(mainConfig);
        scope.CodeFirst.InitTables(typeof(SystemTenantEntity));
        scope.Insertable(CreateTenant(tenantId, tenantConfig.ConnectionString)).ExecuteCommand();

        var resolver = new DelegateSqlSugarTenantIdResolver(() => tenantId);
        var provider = new ClientApiTenantDbProvider(scope, resolver);
        var tenantDb = provider.GetTenantDb(tenantId);
        tenantDb.CodeFirst.InitTables(typeof(TenantGameAppEntity));
        tenantDb.Insertable(new TenantGameAppEntity
        {
            Id = 9001,
            TenantId = tenantId,
            Name = "test-app",
            // C371（GFX-838）：明文比对分支已移除，验证仅走 hash 常量时间比对（AppSecret 列恒空）
            AppSecretHash = AppSecretHasher.Hash("secret"),
            AppSecretHashAlgorithm = AppSecretHasher.Algorithm,
            IsEnabled = true,
            IsDeleted = false,
            UpdateTime = 1,
        }).ExecuteCommand();

        var cache = new AppCacheService(provider, resolver);

        Assert.True(cache.ValidateApp(9001, "secret", out var app));
        Assert.Equal(tenantId, app.TenantId);
        Assert.False(TableExists(scope, typeof(TenantGameAppEntity)));
    }

    [Fact]
    public void GameAppPackageResolver_ReadsPackageMappingFromTenantDatabase()
    {
        var tenantId = 1004L;
        var mainConfig = CreateConfig("main");
        var tenantConfig = CreateConfig(tenantId.ToString());
        using var scope = new SqlSugarScope(mainConfig);
        scope.CodeFirst.InitTables(typeof(SystemTenantEntity));
        scope.Insertable(CreateTenant(tenantId, tenantConfig.ConnectionString)).ExecuteCommand();

        var resolver = new DelegateSqlSugarTenantIdResolver(() => tenantId);
        var provider = new ClientApiTenantDbProvider(scope, resolver);
        var tenantDb = provider.GetTenantDb(tenantId);
        tenantDb.CodeFirst.InitTables(typeof(TenantGameAppPackageEntity));
        tenantDb.Insertable(new[]
        {
            new TenantGameAppPackageEntity
            {
                Id = 9101,
                TenantId = tenantId,
                GameAppId = 1,
                Name = "master",
                PackageName = "com.test.master",
                PlatformCode = "ios",
                PlatformName = "iOS",
                ChannelCode = "default",
                ChannelName = "Default",
                IsEnabled = true,
                IsDeleted = false,
            },
            new TenantGameAppPackageEntity
            {
                Id = 9102,
                TenantId = tenantId,
                GameAppId = 1,
                ParentPackageId = 9101,
                Name = "child",
                PackageName = "com.test.child",
                PlatformCode = "ios",
                PlatformName = "iOS",
                ChannelCode = "default",
                ChannelName = "Default",
                // GFX-86/C89：仅 Inherit 子包参与母包回退；Override/DisableAsync 不回退。
                PlatformInheritanceMode = InheritanceModeEnum.Inherit,
                IsEnabled = true,
                IsDeleted = false,
            },
        }).ExecuteCommand();

        var packageResolver = new GameAppPackageResolver(provider, resolver);

        Assert.Equal("com.test.master", packageResolver.Resolve("com.test.child"));
        Assert.False(TableExists(scope, typeof(TenantGameAppPackageEntity)));
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

    private ConnectionConfig CreateConfig(string configId)
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"gfx_client_api_{configId}_{Guid.NewGuid():N}.db");
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

    private static bool TableExists(ISqlSugarClient db, Type entityType)
    {
        var tableName = db.EntityMaintenance.GetEntityInfo(entityType).DbTableName;
        return db.DbMaintenance.IsAnyTable(tableName, false);
    }
}