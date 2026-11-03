using GameFrameX.Application.Game.App.GameAppAssetPackageVersion;
using GameFrameX.Application.Game.App.GameAppAssetPackageVersion.Dto;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.Exceptions;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.App;
using GameFrameX.Entity.Tenant.Game.Release;
using GameFrameX.Entity.Tenant.Game.Resource;
using GameFrameX.Foundation.Orm.Entity;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// 资源包版本选择器回归测试：验证 AppPackageId 仅返回当前分包下的去重资源版本。
/// </summary>
public sealed class GameAppAssetPackageVersionSelectTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"asset_pkg_select_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public GameAppAssetPackageVersionSelectTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameAppPackageEntity), typeof(TenantGameAppAssetPackageVersionEntity));
    }

    [Fact]
    public async Task Select_ReturnsDistinctLatestVersions_ForCurrentAppPackage()
    {
        await SeedAsync();

        var rows = await BuildSelectAsync(1);

        Assert.Equal(new[] { "2.0.0", "1.0.0", }, rows.Select(u => u.Value).ToArray());
        Assert.Equal(rows.Select(u => u.Value), rows.Select(u => u.Label));
    }

    [Fact]
    public async Task Select_RejectsMissingPackageId()
    {
        await SeedAsync();

        await Assert.ThrowsAsync<BusinessException>(() => BuildSelectAsync(999));
    }

    [Fact]
    public async Task Select_MissingPackageId_ReturnsEmptyList()
    {
        await SeedAsync();

        var rows = await BuildSelectAsync(null);

        Assert.Empty(rows);
    }

    private async Task<List<BaseSelectPageResponse>> BuildSelectAsync(long? appPackageId, string? searchKey = null)
    {
        var service = new TenantGameAppAssetPackageVersionService(
            Repository<TenantGameAppAssetPackageVersionEntity>(),
            Repository<TenantGameAppAssetPackageVersionHistoryEntity>(),
            Repository<TenantGameAppPackageEntity>(),
            Repository<TenantGameAppAssetPackageEntity>());

        return await service.SelectAsync(new PageGameAppAssetPackageVersionSelectRequest
        {
            AppPackageId = appPackageId,
            SearchKey = searchKey,
        });
    }

    private SqlSugarRepository<T> Repository<T>() where T : EntityBase, new()
    {
        return new SqlSugarRepository<T> { Context = _db, };
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(new TenantGameAppPackageEntity
        {
            Id = 1,
            Name = "pkg-a",
            PackageName = "com.app.a",
            PlatformCode = "android",
            PlatformName = "Android",
            ChannelCode = "default",
            ChannelName = "default",
            IsEnabled = true,
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppPackageEntity
        {
            Id = 2,
            Name = "pkg-b",
            PackageName = "com.app.b",
            PlatformCode = "android",
            PlatformName = "Android",
            ChannelCode = "default",
            ChannelName = "default",
            IsEnabled = true,
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppPackageEntity
        {
            Id = 3,
            Name = "pkg-c",
            PackageName = "com.app.a",
            PlatformCode = "android",
            PlatformName = "Android",
            ChannelCode = "default",
            ChannelName = "default",
            IsEnabled = true,
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppAssetPackageVersionEntity
        {
            Id = 11,
            Channel = "default",
            Platform = "android",
            AppVersion = "1.0.0",
            Package = "com.app.a",
            Language = "zh",
            AssetPackageName = "base",
            RootPath = "/a",
            ResourceVersion = "1.0.0",
            Description = "first",
            CreatedTime = now - 20,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppAssetPackageVersionEntity
        {
            Id = 12,
            Channel = "default",
            Platform = "android",
            AppVersion = "1.0.0",
            Package = "com.app.a",
            Language = "zh",
            AssetPackageName = "base",
            RootPath = "/a",
            ResourceVersion = "1.0.0",
            Description = "duplicate",
            CreatedTime = now - 10,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppAssetPackageVersionEntity
        {
            Id = 13,
            Channel = "default",
            Platform = "android",
            AppVersion = "1.0.0",
            Package = "com.app.a",
            Language = "zh",
            AssetPackageName = "base",
            RootPath = "/a",
            ResourceVersion = "2.0.0",
            Description = "latest",
            CreatedTime = now,
            IsDeleted = false,
        }).ExecuteCommandAsync();

        await _db.Insertable(new TenantGameAppAssetPackageVersionEntity
        {
            Id = 21,
            Channel = "default",
            Platform = "android",
            AppVersion = "1.0.0",
            Package = "com.app.b",
            Language = "zh",
            AssetPackageName = "base",
            RootPath = "/b",
            ResourceVersion = "9.9.9",
            Description = "other package",
            CreatedTime = now,
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