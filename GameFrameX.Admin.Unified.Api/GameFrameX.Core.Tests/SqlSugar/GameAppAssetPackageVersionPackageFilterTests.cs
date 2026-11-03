using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.Release;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// 验证主库的分包精确筛选与业务键唯一性。
/// </summary>
public sealed class GameAppAssetPackageVersionPackageFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"asset_pkg_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public GameAppAssetPackageVersionPackageFilterTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameAppAssetPackageVersionEntity));
    }


    /// <summary>
    /// Package 精确筛选：传 Package="com.app.a" 只返回精确匹配，排除 "com.app.a.patch"（证明 == 而非 Contains）。
    /// </summary>
    [Fact]
    public async Task PackageFilter_MatchesExactly_ExcludesPrefixCollision()
    {
        await SeedAsync();

        var rows = await BuildQuery("com.app.a").ToListAsync();

        Assert.Single(rows);
        Assert.All(rows, r => Assert.Equal("com.app.a", r.Package));
    }



    /// <summary>
    /// 同应用同渠道/平台/应用版本/包名/语言/资源包名称应判定重复。
    /// </summary>
    [Fact]
    public async Task SameAppSameAssetPackageVersionKey_DetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckAssetPackageVersionDuplicate("default", "android", "1.0.0", "com.app.a", "zh", "base");

        Assert.True(exists);
    }

    /// <summary>
    /// 不同渠道的资源包版本键不应判定重复。
    /// </summary>
    [Fact]
    public async Task DifferentPackageKey_NotDetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckAssetPackageVersionDuplicate("other", "android", "1.0.0", "com.app.a", "zh", "base");

        Assert.False(exists);
    }

    private ISugarQueryable<TenantGameAppAssetPackageVersionEntity> BuildQuery(string? package)
    {
        var normalizedPackage = string.IsNullOrWhiteSpace(package) ? string.Empty : package.Trim();
        return _db.Queryable<TenantGameAppAssetPackageVersionEntity>()
                  .WhereIF(!string.IsNullOrEmpty(normalizedPackage), u => u.Package == normalizedPackage);
    }

    private async Task<bool> CheckAssetPackageVersionDuplicate(string channel, string platform, string appVersion, string package, string language, string assetPackageName)
    {
        return await _db.Queryable<TenantGameAppAssetPackageVersionEntity>()
                        .AnyAsync(u => u.Channel == channel
                                       && u.Platform == platform
                                       && u.AppVersion == appVersion
                                       && u.Package == package
                                       && u.Language == language
                                       && u.AssetPackageName == assetPackageName);
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(BuildAssetVersion(1, "com.app.a", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildAssetVersion(2, "com.app.a.patch", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildAssetVersion(4, "default", now)).ExecuteCommandAsync();
    }

    private static TenantGameAppAssetPackageVersionEntity BuildAssetVersion(long id, string package, long now)
    {
        return new TenantGameAppAssetPackageVersionEntity
        {
            Id = id,
            Channel = "default",
            Platform = "android",
            AppVersion = "1.0.0",
            Package = package,
            Language = "zh",
            AssetPackageName = "base",
            RootPath = "/res/",
            ResourceVersion = "100",
            Description = "d",
            CreatedTime = now,
            IsDeleted = false,
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