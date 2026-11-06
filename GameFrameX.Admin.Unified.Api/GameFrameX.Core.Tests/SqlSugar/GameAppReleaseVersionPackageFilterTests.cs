using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.Release;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// 验证主库的分包精确筛选与业务键唯一性。
/// </summary>
public sealed class GameAppReleaseVersionPackageFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"release_pkg_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public GameAppReleaseVersionPackageFilterTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(GameAppReleaseVersionEntity));
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
    /// 同应用同渠道/平台/旧版本/新版本/包名/语言应判定重复。
    /// </summary>
    [Fact]
    public async Task SameAppSameReleaseKey_DetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckReleaseVersionDuplicate("default", "android", "0.9.0", "1.0.0", "com.app.a", "zh", null);

        Assert.True(exists);
    }

    /// <summary>
    /// 不同渠道的发布键不应判定重复。
    /// </summary>
    [Fact]
    public async Task DifferentPackageKey_NotDetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckReleaseVersionDuplicate("other", "android", "0.9.0", "1.0.0", "com.app.a", "zh", null);

        Assert.False(exists);
    }

    /// <summary>
    /// 更新自身时排除当前 Id，不误报重复。
    /// </summary>
    [Fact]
    public async Task UpdateExcludeSelfId_NotDetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckReleaseVersionDuplicate("default", "android", "0.9.0", "1.0.0", "com.app.a", "zh", 1);

        Assert.False(exists);
    }

    private ISugarQueryable<GameAppReleaseVersionEntity> BuildQuery(string? package)
    {
        var normalizedPackage = string.IsNullOrWhiteSpace(package) ? string.Empty : package.Trim();
        return _db.Queryable<GameAppReleaseVersionEntity>()
                  .WhereIF(!string.IsNullOrEmpty(normalizedPackage), u => u.Package == normalizedPackage);
    }

    private async Task<bool> CheckReleaseVersionDuplicate(string channel, string platform, string oldVersion, string appVersion, string package, string language, long? excludeId)
    {
        var query = _db.Queryable<GameAppReleaseVersionEntity>()
                       .Where(u => u.Channel == channel
                                   && u.Platform == platform
                                   && u.OldVersion == oldVersion
                                   && u.AppVersion == appVersion
                                   && u.Package == package
                                   && u.Language == language);
        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(BuildVersion(1, "com.app.a", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildVersion(2, "com.app.a.patch", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildVersion(4, "default", now)).ExecuteCommandAsync();
    }

    private static GameAppReleaseVersionEntity BuildVersion(long id, string package, long now)
    {
        return new GameAppReleaseVersionEntity
        {
            Id = id,
            Channel = "default",
            Platform = "android",
            AppVersion = "1.0.0",
            Package = package,
            Language = "zh",
            OldVersion = "0.9.0",
            DownloadUrl = "https://example.com/d.apk",
            IsForce = false,
            Title = "t",
            Announcement = "a",
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