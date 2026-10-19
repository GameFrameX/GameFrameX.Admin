using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GameRedeemCodeEntityService — IsActive 过滤契约。
/// </summary>
public sealed class AdvancedQueryRedeemCodeActiveFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"advq_rc_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public AdvancedQueryRedeemCodeActiveFilterTests()
    {
        var config = new ConnectionConfig { DbType = DbType.Sqlite, ConnectionString = $"DataSource={_dbPath}", };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameRedeemCodeEntity));
    }

    [Fact]
    public async Task IsActiveFilter_ReturnsOnlyInactiveCodes()
    {
        await SeedAsync();
        var inactive = await BuildQuery(false).ToListAsync();
        Assert.Single(inactive);
        Assert.False(inactive[0].IsActive);
    }

    [Fact]
    public async Task NoActiveFilter_ReturnsAll()
    {
        await SeedAsync();
        var all = await BuildQuery(null).ToListAsync();
        Assert.Equal(3, all.Count);
    }

    private ISugarQueryable<TenantGameRedeemCodeEntity> BuildQuery(bool? isActive)
    {
        return _db.Queryable<TenantGameRedeemCodeEntity>()
                  .WhereIF(isActive.HasValue, u => u.IsActive == isActive!.Value);
    }

    private async Task SeedAsync()
    {
        var nowDt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(new TenantGameRedeemCodeEntity { Id = 1, Code = "C1", IsActive = true, UsageLimit = 10, UsedCount = 0, StartTime = nowDt, EndTime = nowDt, Description = "d", Contents = "[]", IsDeleted = false, CreatedTime = now, }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeEntity { Id = 2, Code = "C2", IsActive = false, UsageLimit = 10, UsedCount = 0, StartTime = nowDt, EndTime = nowDt, Description = "d", Contents = "[]", IsDeleted = false, CreatedTime = now, }).ExecuteCommandAsync();
        await _db.Insertable(new TenantGameRedeemCodeEntity { Id = 3, Code = "C3", IsActive = true, UsageLimit = 10, UsedCount = 0, StartTime = nowDt, EndTime = nowDt, Description = "d", Contents = "[]", IsDeleted = false, CreatedTime = now, }).ExecuteCommandAsync();
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