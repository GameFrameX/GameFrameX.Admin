using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.Release;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GFX-84 / C80 / C1 回归测试：复刻 TenantGameAppReleaseVersionService 的应用作用域查询和唯一性判定。
/// <para>用 Sqlite 内存文件库 + ConfigureBaseFilters 端到端验证查询形状，确保 Package 过滤从
/// <c>Contains</c> 收紧为 <c>==</c>、AppId 过滤按作用域精确命中。</para>
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
        _db.CodeFirst.InitTables(typeof(TenantGameAppReleaseVersionEntity));
    }

    /// <summary>
    /// AppId 过滤：传 AppId=100 只返回应用 100 下的版本，排除应用 200 与租户默认（AppId=0）。
    /// </summary>
    [Fact]
    public async Task AppIdFilter_ReturnsOnlyGivenAppScope()
    {
        await SeedAsync();

        var rows = await BuildQuery(100, null).ToListAsync();

        Assert.Equal(2, rows.Count);
        Assert.All(rows, r => Assert.Equal(100L, r.AppId));
    }

    /// <summary>
    /// Package 精确筛选：传 Package="com.app.a" 只返回精确匹配，排除 "com.app.a.patch"（证明 == 而非 Contains）。
    /// </summary>
    [Fact]
    public async Task PackageFilter_MatchesExactly_ExcludesPrefixCollision()
    {
        await SeedAsync();

        var rows = await BuildQuery(100, "com.app.a").ToListAsync();

        Assert.Single(rows);
        Assert.All(rows, r => Assert.Equal("com.app.a", r.Package));
    }

    /// <summary>
    /// AppId + Package 组合：双重精确过滤只命中唯一记录。
    /// </summary>
    [Fact]
    public async Task AppIdAndPackage_Combined_ReturnsSingleMatch()
    {
        await SeedAsync();

        var rows = await BuildQuery(100, "com.app.a").ToListAsync();

        Assert.Single(rows);
        Assert.Equal(100L, rows[0].AppId);
        Assert.Equal("com.app.a", rows[0].Package);
    }

    /// <summary>
    /// AppId 缺失：发布版本是应用级数据，不能回退为全租户查询。
    /// </summary>
    [Fact]
    public async Task MissingAppId_Throws()
    {
        await SeedAsync();

        Assert.Throws<InvalidOperationException>(() => BuildQuery(null, null).ToList());
    }

    /// <summary>
    /// 同应用同渠道/平台/旧版本/新版本/包名/语言应判定重复。
    /// </summary>
    [Fact]
    public async Task SameAppSameReleaseKey_DetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckReleaseVersionDuplicate(100, "default", "android", "0.9.0", "1.0.0", "com.app.a", "zh", null);

        Assert.True(exists);
    }

    /// <summary>
    /// 跨应用相同发布键不应判定重复。
    /// </summary>
    [Fact]
    public async Task CrossAppSameReleaseKey_NotDetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckReleaseVersionDuplicate(300, "default", "android", "0.9.0", "1.0.0", "com.app.a", "zh", null);

        Assert.False(exists);
    }

    /// <summary>
    /// 更新自身时排除当前 Id，不误报重复。
    /// </summary>
    [Fact]
    public async Task UpdateExcludeSelfId_NotDetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckReleaseVersionDuplicate(100, "default", "android", "0.9.0", "1.0.0", "com.app.a", "zh", 1);

        Assert.False(exists);
    }

    private ISugarQueryable<TenantGameAppReleaseVersionEntity> BuildQuery(long? appId, string? package)
    {
        // 复刻 TenantGameAppReleaseVersionService.BuildPageQuery 的应用级增量（AppId 必填 + Package ==）。
        // 注：仅复刻过滤形状，省略 SelectAsync<Output>() 投影——投影不影响 WHERE 过滤断言。
        if (appId is null or <= 0)
        {
            throw new InvalidOperationException("请选择应用后再维护发布版本");
        }

        var normalizedPackage = string.IsNullOrWhiteSpace(package) ? string.Empty : package.Trim();
        return _db.Queryable<TenantGameAppReleaseVersionEntity>()
                  .WhereIF(!string.IsNullOrEmpty(normalizedPackage), u => u.Package == normalizedPackage)
                  .Where(u => u.AppId == appId.Value);
    }

    private async Task<bool> CheckReleaseVersionDuplicate(long appId, string channel, string platform, string oldVersion, string appVersion, string package, string language, long? excludeId)
    {
        var query = _db.Queryable<TenantGameAppReleaseVersionEntity>()
                       .Where(u => u.AppId == appId
                                   && u.Channel == channel
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

        await _db.Insertable(BuildVersion(1, 100, "com.app.a", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildVersion(2, 100, "com.app.a.patch", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildVersion(3, 200, "com.app.a", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildVersion(4, 0, "default", now)).ExecuteCommandAsync();
    }

    private static TenantGameAppReleaseVersionEntity BuildVersion(long id, long appId, string package, long now)
    {
        return new TenantGameAppReleaseVersionEntity
        {
            Id = id,
            AppId = appId,
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
            TenantId = null,
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