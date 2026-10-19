using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.Announcement;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GameBannerAnnouncementEntityService — IsActive 过滤契约。
/// </summary>
public sealed class AdvancedQueryBannerActiveFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"advq_banner_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public AdvancedQueryBannerActiveFilterTests()
    {
        var config = new ConnectionConfig { DbType = DbType.Sqlite, ConnectionString = $"DataSource={_dbPath}", };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameBannerAnnouncementEntity));
    }

    [Fact]
    public async Task IsActiveFilter_ReturnsOnlyActiveBanners()
    {
        await SeedAsync();
        var active = await BuildQuery(true).ToListAsync();
        Assert.Equal(2, active.Count);
        Assert.All(active, b => Assert.True(b.IsActive));
    }

    private ISugarQueryable<TenantGameBannerAnnouncementEntity> BuildQuery(bool? isActive)
    {
        return _db.Queryable<TenantGameBannerAnnouncementEntity>()
                  .WhereIF(isActive.HasValue, u => u.IsActive == isActive!.Value);
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(new TenantGameBannerAnnouncementEntity { Id = 1, Content = "b1", ContentLocalizationKey = "", IsActive = true, IsDeleted = false, CreatedTime = now, }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameBannerAnnouncementEntity { Id = 2, Content = "b2", ContentLocalizationKey = "", IsActive = false, IsDeleted = false, CreatedTime = now, }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameBannerAnnouncementEntity { Id = 3, Content = "b3", ContentLocalizationKey = "", IsActive = true, IsDeleted = false, CreatedTime = now, }).ExecuteCommandAsync();
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