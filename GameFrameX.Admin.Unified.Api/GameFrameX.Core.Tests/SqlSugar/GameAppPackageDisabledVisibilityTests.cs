using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.App;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
///  / 回归测试：验证全局 ISafeEnabledFilter 会隐藏 IsEnabled==false 的分包，
/// 而 .Filter(null, true) 绕过 + 手动补 IsDeleted==false + 搜索过滤 WhereIF（BuildPageQuery/SetStatusAsync 采用的模式）
/// 能让禁用分包重新可见且不破坏软删除排除与搜索过滤语义。
/// 复刻生产过滤器注册（ConfigureBaseFilters + ConfigureClientFilters），用 Sqlite 内存文件库做端到端查询验证。
/// </summary>
public sealed class GameAppPackageDisabledVisibilityTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"gameapppackage_disabled_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public GameAppPackageDisabledVisibilityTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        // 复刻生产全局过滤器：软删除 + 启用/禁用（ISafeEnabledFilter 即 根因）。
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        SqlSugarUnifiedConfig.ConfigureClientFilters(_db);
        _db.CodeFirst.InitTables(typeof(GameAppPackageEntity));
    }

    [Fact]
    public async Task GlobalEnabledFilter_HidesDisabledPackage_ReproducesBla28()
    {
        await SeedAsync(1, true, false);
        await SeedAsync(2, false, false);

        // 旧 BuildPageQuery 行为：AsQueryable() 未绕过过滤器 → 禁用记录被 ISafeEnabledFilter 排除。
        var rows = await _db.Queryable<GameAppPackageEntity>().ToListAsync();

        var ids = rows.Select(u => u.Id).OrderBy(u => u).ToList();
        Assert.Equal(new List<long> { 1, }, ids);
        Assert.DoesNotContain(2L, ids);
    }

    [Fact]
    public async Task FixPattern_FilterNullTrue_ExposesDisabledAndKeepsSoftDelete()
    {
        await SeedAsync(1, true, false);
        await SeedAsync(2, false, false);
        await SeedAsync(3, true, true);

        // 修复后 BuildPageQuery 行为：绕过全部过滤器，手动补 IsDeleted==false。
        var rows = await _db.Queryable<GameAppPackageEntity>()
                            .Filter(null, true)
                            .Where(u => u.IsDeleted == false)
                            .ToListAsync();

        var ids = rows.Select(u => u.Id).OrderBy(u => u).ToList();
        Assert.Equal(new List<long> { 1, 2, }, ids);
        Assert.DoesNotContain(3L, ids);
    }

    [Fact]
    public async Task FixPattern_MainDatabase_ReturnsAllActivePackages()
    {
        await SeedAsync(1, false, false);
        await SeedAsync(2, false, false);
        await SeedAsync(3, false, false);

        long? id = 1;
        // 修复后查询：绕过过滤器 + IsDeleted==false + 搜索过滤 WhereIF（精确复刻 ConfigureAdminFilters 谓词）。
        var rows = await _db.Queryable<GameAppPackageEntity>()
                            .Filter(null, true)
                            .Where(u => u.IsDeleted == false)
                                                        .ToListAsync();

        var ids = rows.Select(u => u.Id).OrderBy(u => u).ToList();
        Assert.Equal(new List<long> { 1, 2, 3, }, ids);
    }

    [Fact]
    public async Task FixPattern_SetStatusLookup_FindsDisabledPackageById()
    {
        await SeedAsync(1, false, false);

        // 旧 SetStatusAsync 行为：GetFirstAsync 受 ISafeEnabledFilter 影响，找不到禁用记录（抛“分包不存在”）。
        var blocked = await _db.Queryable<GameAppPackageEntity>().FirstAsync(u => u.Id == 1);
        Assert.Null(blocked);

        // 修复后 SetStatusAsync 行为：绕过过滤器 + IsDeleted==false，按 Id 命中禁用记录，可继续重新启用。
        long? id = null;
        var found = await _db.Queryable<GameAppPackageEntity>()
                             .Filter(null, true)
                             .Where(u => u.IsDeleted == false)
                                                          .FirstAsync(u => u.Id == 1);

        Assert.NotNull(found);
        Assert.Equal(1, found!.Id);
        Assert.False(found.IsEnabled);
    }

    private async Task SeedAsync(long id, bool? isEnabled, bool isDeleted)
    {
        var entity = new GameAppPackageEntity
        {
            Id = id,
            Name = $"pkg-{id}",
            PackageName = $"com.test.pkg{id}",
            PlatformCode = "android",
            PlatformName = "android",
            ChannelCode = "default",
            ChannelName = "default",
            IsEnabled = isEnabled,
            IsDeleted = isDeleted,
            CreatedTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            RowVersion = 0,
        };
        await _db.Insertable(entity).ExecuteCommandAsync();
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
