using GameFrameX.Core.Cache;
using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Option;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Core.Service.Config;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.Service.Tenant.Dto;
using GameFrameX.Core.Service.TenantMenuSync;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Core.Tests.System;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SqlSugar;

namespace GameFrameX.Core.Tests.TenantMenuSync;

/// <summary>
/// C358 T3：<see cref="SystemTenantService.PageAsync"/> 按本页租户 Id 集合批量 hydrate
/// 主库 <c>system_tenant_menu_sync_state_entity</c> 的展示字段（<c>MenuSyncStatus</c> /
/// <c>MenuSyncLastSyncTime</c> / <c>MenuSyncLastError</c> / <c>MenuSyncNextRetryTime</c>）。
/// 状态行缺失 ⇒ Pending + 三字段 null；failed 行携带 LastError/NextRetryTime；
/// succeeded 行 LastError 为空；分页只查本页 Id 集合（无跨页串数据）。
/// </summary>
[Collection("MenuSeedCatalog")]
public class SystemTenantServicePageMenuSyncHydrateTests : IDisposable
{
    private readonly TenantMenuSyncTestHost _host = new();

    [Fact]
    public async Task PageAsync_MissingStateRow_ReturnsPendingAndNulls()
    {
        await _host.SeedTenantAsync(TenantMenuSyncTestHost.PackageId);

        var page = await BuildService().PageAsync(new PageTenantRequest
        {
            Page = 1,
            PageSize = 10,
        });

        var item = Assert.Single(page.Items);
        Assert.Equal(TenantMenuSyncTestHost.TenantId, item.Id);
        Assert.Equal(TenantMenuSyncStatusEnum.Pending, item.MenuSyncStatus);
        Assert.Null(item.MenuSyncLastSyncTime);
        Assert.Null(item.MenuSyncLastError);
        Assert.Null(item.MenuSyncNextRetryTime);
    }

    [Fact]
    public async Task PageAsync_FailedStateRow_CarriesLastErrorAndNextRetryTime()
    {
        await _host.SeedTenantAsync(TenantMenuSyncTestHost.PackageId);
        await _host.MainDb.Insertable(new SystemTenantMenuSyncStateEntity
        {
            Id = TenantMenuSyncTestHost.TenantId,
            Status = TenantMenuSyncStatusEnum.Failed,
            LastCatalogVersion = "v-1",
            LastSyncTime = 1715800000L,
            LastError = "dangling-package",
            NextRetryTime = 1715900000L,
        }).ExecuteCommandAsync();

        var page = await BuildService().PageAsync(new PageTenantRequest
        {
            Page = 1,
            PageSize = 10,
        });

        var item = Assert.Single(page.Items);
        Assert.Equal(TenantMenuSyncStatusEnum.Failed, item.MenuSyncStatus);
        Assert.Equal(1715800000L, item.MenuSyncLastSyncTime);
        Assert.Equal("dangling-package", item.MenuSyncLastError);
        Assert.Equal(1715900000L, item.MenuSyncNextRetryTime);
    }

    [Fact]
    public async Task PageAsync_SucceededStateRow_LastErrorIsEmpty()
    {
        await _host.SeedTenantAsync(TenantMenuSyncTestHost.PackageId);
        await _host.MainDb.Insertable(new SystemTenantMenuSyncStateEntity
        {
            Id = TenantMenuSyncTestHost.TenantId,
            Status = TenantMenuSyncStatusEnum.Succeeded,
            LastCatalogVersion = "v-1",
            LastSyncTime = 1715800000L,
            LastError = string.Empty,
            NextRetryTime = null,
        }).ExecuteCommandAsync();

        var page = await BuildService().PageAsync(new PageTenantRequest
        {
            Page = 1,
            PageSize = 10,
        });

        var item = Assert.Single(page.Items);
        Assert.Equal(TenantMenuSyncStatusEnum.Succeeded, item.MenuSyncStatus);
        Assert.Equal(1715800000L, item.MenuSyncLastSyncTime);
        Assert.Equal(string.Empty, item.MenuSyncLastError);
        Assert.Null(item.MenuSyncNextRetryTime);
    }

    [Fact]
    public async Task PageAsync_QueriesOnlyPageIds_NotOtherTenants()
    {
        // 同页 1 个租户 + 页外 1 个租户：只回填本页租户的状态行；页外租户状态行不应参与本次查询。
        const long pageTenantId = 3300000000001;
        const long offPageTenantId = 3300000000002;

        await _host.MainDb.Insertable(new[]
        {
            new SystemTenantEntity
            {
                Id = pageTenantId,
                TenantType = TenantTypeEnum.Db,
                DbType = DbType.Sqlite,
                Connection = string.Empty,
                ConfigId = pageTenantId.ToString(),
                IsEnabled = true,
                PackageId = TenantMenuSyncTestHost.PackageId,
            },
            new SystemTenantEntity
            {
                Id = offPageTenantId,
                TenantType = TenantTypeEnum.Db,
                DbType = DbType.Sqlite,
                Connection = string.Empty,
                ConfigId = offPageTenantId.ToString(),
                IsEnabled = true,
                PackageId = TenantMenuSyncTestHost.PackageId,
            },
        }).ExecuteCommandAsync();

        await _host.MainDb.Insertable(new[]
        {
            new SystemTenantMenuSyncStateEntity
            {
                Id = pageTenantId,
                Status = TenantMenuSyncStatusEnum.Succeeded,
                LastCatalogVersion = "v-1",
                LastSyncTime = 1715800001L,
                LastError = string.Empty,
            },
            new SystemTenantMenuSyncStateEntity
            {
                Id = offPageTenantId,
                Status = TenantMenuSyncStatusEnum.Failed,
                LastCatalogVersion = "v-2",
                LastSyncTime = 1715800002L,
                LastError = "off-page-error",
                NextRetryTime = 1715900002L,
            },
        }).ExecuteCommandAsync();

        var page = await BuildService().PageAsync(new PageTenantRequest
        {
            Page = 1,
            PageSize = 1,
        });

        var item = Assert.Single(page.Items);
        Assert.Equal(pageTenantId, item.Id);
        Assert.Equal(TenantMenuSyncStatusEnum.Succeeded, item.MenuSyncStatus);
        Assert.Equal(1715800001L, item.MenuSyncLastSyncTime);
        Assert.Equal(string.Empty, item.MenuSyncLastError);
    }

    [Fact]
    public async Task PageAsync_EmptyPage_DoesNotQueryStateTable()
    {
        // 无租户：PageAsync 不触发状态批查（防御性早返回）。
        var page = await BuildService().PageAsync(new PageTenantRequest
        {
            Page = 1,
            PageSize = 10,
        });

        Assert.Empty(page.Items);
    }

    private SystemTenantService BuildService()
    {
        SqlSugarCache.Configure(new FakeCache());
        var cacheService = new SystemCacheService(new FakeCache(), Options.Create(new CacheOptions()));
        // 仅注册主库 configId；租户 configId 由 TenantDbConnectionProvider 在 GetTenantDbConnectionScope 内按需 AddConnection。
        var mainConfig = new DbConnectionConfig
        {
            ConfigId = SqlSugarConst.MainConfigId,
            DbType = DbType.Sqlite,
            ConnectionString = _host.MainDb.Ado.Connection.ConnectionString,
            IsAutoCloseConnection = true,
        };
        return new SystemTenantService(
            new SqlSugarRepository<SystemTenantEntity> { Context = _host.MainDb, },
            new SqlSugarRepository<SystemTenantMenuSyncStateEntity> { Context = _host.MainDb, },
            new SystemConfigService(
                new SqlSugarRepository<Entity.System.SystemConfigEntity> { Context = _host.MainDb, },
                cacheService),
            cacheService,
            new TenantDbConnectionProvider(
                _host.Client,
                cacheService,
                new DelegateSqlSugarTenantIdResolver(() => null),
                Options.Create(new DbConnectionOptions { ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, }, }),
                new HttpContextAccessor()),
            new PostgreSqlTenantDatabaseProvisioner(),
            NullLogger<SystemTenantService>.Instance,
            Options.Create(new DbConnectionOptions { ConnectionConfigs = new List<DbConnectionConfig> { mainConfig, }, }),
            _host.Client,
            new EmptyServiceProvider(),
            new TenantMenuSyncQueue());
    }

    public void Dispose()
    {
        _host.Dispose();
    }

    /// <summary>
    /// PageAsync 不解析 <see cref="IServiceProvider"/>；保留扩展点用。
    /// </summary>
    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType)
        {
            return null;
        }
    }
}
