using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.Release;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GFX-95 / C102 回归测试：复刻版本定义应用作用域查询和唯一性判定。
/// </summary>
public sealed class GameAppVersionDefinitionAppScopeTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), string.Format("version_def_{0:N}.db", Guid.NewGuid()));
    private readonly SqlSugarScope _db;

    public GameAppVersionDefinitionAppScopeTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = string.Format("DataSource={0}", _dbPath),
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameAppVersionDefinitionEntity));
    }

    /// <summary>
    /// PageAsync AppId 过滤只返回当前应用版本定义。
    /// </summary>
    [Fact]
    public async Task PageFilterByApp_ReturnsOnlyCurrentAppDefinitions()
    {
        await SeedAsync();

        var rows = await BuildPageQuery(1, null).ToListAsync();

        Assert.Equal(2, rows.Count);
        Assert.All(rows, row => Assert.Equal(1L, row.AppId));
    }

    /// <summary>
    /// null AppId 保持后台全局视图兼容。
    /// </summary>
    [Fact]
    public async Task PageWithoutAppFilter_ReturnsAll()
    {
        await SeedAsync();

        var rows = await BuildPageQuery(null, null).ToListAsync();

        Assert.Equal(3, rows.Count);
    }

    /// <summary>
    /// SelectAsync AppId 过滤只返回当前应用版本定义。
    /// </summary>
    [Fact]
    public async Task SelectFilterByApp_ReturnsOnlyCurrentAppDefinitions()
    {
        await SeedAsync();

        var rows = await BuildSelectQuery(2, null).ToListAsync();

        Assert.Single(rows);
        Assert.Equal(2L, rows[0].AppId);
    }

    /// <summary>
    /// 同应用同名应判定重复。
    /// </summary>
    [Fact]
    public async Task SameAppSameName_DetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckNameUniqueInApp(1, "1.0.0", null);

        Assert.True(exists);
    }

    /// <summary>
    /// 跨应用同名不应判定重复。
    /// </summary>
    [Fact]
    public async Task CrossAppSameName_NotDetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckNameUniqueInApp(3, "1.0.0", null);

        Assert.False(exists);
    }

    /// <summary>
    /// 更新自身时排除当前 Id，不误报同名重复。
    /// </summary>
    [Fact]
    public async Task UpdateExcludeSelfId_NotDetectedAsDuplicate()
    {
        await SeedAsync();

        var exists = await CheckNameUniqueInApp(1, "1.0.0", 1);

        Assert.False(exists);
    }

    private ISugarQueryable<TenantGameAppVersionDefinitionEntity> BuildPageQuery(long? appId, string? searchKey)
    {
        var normalizedSearchKey = searchKey == null ? string.Empty : searchKey.Trim();
        return _db.Queryable<TenantGameAppVersionDefinitionEntity>()
                  .Where(u => u.IsDeleted == false)
                  .WhereIF(!string.IsNullOrWhiteSpace(normalizedSearchKey), u => u.Name.Contains(normalizedSearchKey))
                  .WhereIF(appId.HasValue, u => u.AppId == appId.GetValueOrDefault());
    }

    private ISugarQueryable<TenantGameAppVersionDefinitionEntity> BuildSelectQuery(long? appId, string? searchKey)
    {
        var normalizedSearchKey = searchKey == null ? string.Empty : searchKey.Trim();
        return _db.Queryable<TenantGameAppVersionDefinitionEntity>()
                  .Where(u => u.IsDeleted == false)
                  .WhereIF(!string.IsNullOrWhiteSpace(normalizedSearchKey), u => u.Name.Contains(normalizedSearchKey))
                  .WhereIF(appId.HasValue, u => u.AppId == appId.GetValueOrDefault())
                  .Take(100);
    }

    private async Task<bool> CheckNameUniqueInApp(long appId, string name, long? excludeId)
    {
        return excludeId.HasValue
                   ? await _db.Queryable<TenantGameAppVersionDefinitionEntity>().AnyAsync(x => x.AppId == appId && x.Name == name && x.Id != excludeId.Value)
                   : await _db.Queryable<TenantGameAppVersionDefinitionEntity>().AnyAsync(x => x.AppId == appId && x.Name == name);
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        await _db.Insertable(BuildVersionDefinition(1, 1, "1.0.0", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildVersionDefinition(2, 1, "1.1.0", now)).ExecuteCommandAsync();
        await _db.Insertable(BuildVersionDefinition(3, 2, "1.0.0", now)).ExecuteCommandAsync();
    }

    private static TenantGameAppVersionDefinitionEntity BuildVersionDefinition(long id, long appId, string name, long now)
    {
        return new TenantGameAppVersionDefinitionEntity
        {
            Id = id,
            AppId = appId,
            Name = name,
            Description = "版本定义",
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