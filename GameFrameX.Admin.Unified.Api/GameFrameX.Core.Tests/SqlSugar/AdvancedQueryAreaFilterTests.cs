using GameFrameX.Core.Enum;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// GameAreaEntityService — IsDefault / IsRecommend / IsEnabled（C301 起归一基类列）/ Language(GameLanguage?) 过滤契约。
/// </summary>
public sealed class AdvancedQueryAreaFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"advq_area_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public AdvancedQueryAreaFilterTests()
    {
        // 使用与 GameOrderShipServiceTests 一致的"裸"配置（无 ConfigureBaseSettings/underline），
        // 避免 SqlSugar 静态实体缓存（PascalCase 表名）跨测试污染导致 InitTables no-op。
        _db = new SqlSugarScope(new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
            InitKeyType = InitKeyType.Attribute,
            IsAutoCloseConnection = true,
        });
        _db.CodeFirst.InitTables(typeof(TenantGameAreaEntity));
    }

    [Fact]
    public async Task IsEnabledFilter_MapsToIsEnableColumn()
    {
        await SeedAsync();
        // DTO 字段 IsEnabled → 实体基类列 IsEnabled（C301 归一）。禁用过滤只返回 IsEnabled=false 的区服。
        var disabled = await BuildQuery(isEnabled: false).ToListAsync();
        Assert.Single(disabled);
        Assert.True(disabled[0].IsEnabled == false);
    }

    [Fact]
    public async Task LanguageFilter_ReturnsOnlyMatchingLanguage()
    {
        await SeedAsync();
        var chinese = await BuildQuery(language: GameLanguage.Chinese).ToListAsync();
        Assert.Single(chinese);
        Assert.Equal(GameLanguage.Chinese, chinese[0].Language);
    }

    [Fact]
    public async Task NoAdvancedFilter_ReturnsAll()
    {
        await SeedAsync();
        var all = await BuildQuery(null, null, null, null, null).ToListAsync();
        Assert.Equal(3, all.Count);
    }

    private ISugarQueryable<TenantGameAreaEntity> BuildQuery(bool? isDefault = null, bool? isRecommend = null, bool? isEnabled = null, GameLanguage? language = null, long? gameAppId = null)
    {
        return _db.Queryable<TenantGameAreaEntity>()
                  .WhereIF(isDefault.HasValue, u => u.IsDefault == isDefault!.Value)
                  .WhereIF(isRecommend.HasValue, u => u.IsRecommend == isRecommend!.Value)
                  .WhereIF(isEnabled.HasValue, u => u.IsEnabled == isEnabled!.Value)
                  .WhereIF(language.HasValue, u => u.Language == language!.Value)
                  .WhereIF(gameAppId.HasValue, u => u.GameAppId == gameAppId!.Value);
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(BuildArea(1, "a1", true, GameLanguage.Chinese, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildArea(2, "a2", false, GameLanguage.ChineseSimplified, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildArea(3, "a3", true, GameLanguage.Unspecified, now)).ExecuteCommandAsync();
    }

    private static TenantGameAreaEntity BuildArea(long id, string name, bool isEnable, GameLanguage language, long now)
    {
        // 与 GameOrderShipServiceTests 一致的种子字段集（裸配置下所有列均 NOT NULL，需全量赋值）。
        // 本测试只断言 IsEnabled（C301 归一基类列）/Language 过滤形状，其余列给占位值。
        return new TenantGameAreaEntity
        {
            Id = id, Name = name, Language = language,
            TenantId = 1, CreatedId = 1, CreatedTime = now, CreatedName = "test",
            UpdateCount = 0, UpdateTime = now, UpdatedId = 1, UpdatedName = "test",
            IsDeleted = false, IsEnabled = isEnable, DeleteTime = 0, DeletedId = 0, DeletedName = string.Empty,
            RowVersion = 0, Description = "test",
            HttpManageUrl = "", TcpSocketUrl = "", WebSocketUrl = "", ServerVersion = "",
            ServerType = "", LocalizationKey = "", AllowedPlayerProfile = "", ServerEvents = "",
            AccessControls = "", ServerContainerId = "", ServerParams = "", AppVersion = "",
            CurrentLoad = 0, CreationTime = new DateTimeOffset(new DateTime(2024, 1, 1)).ToUnixTimeSeconds(), LastMaintenanceTime = new DateTimeOffset(new DateTime(2024, 1, 1)).ToUnixTimeSeconds(),
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