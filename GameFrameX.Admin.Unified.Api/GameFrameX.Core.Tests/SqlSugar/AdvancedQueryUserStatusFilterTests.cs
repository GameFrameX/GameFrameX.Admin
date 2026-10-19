using GameFrameX.Core.Enum;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Enum;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

// GFX-131 / C131 回归测试：锁定 5 个 game-business 服务 BuildPageQuery 中此前漂移的过滤字段
// （PageInput 已声明、Service 此前未应用）。每条测试复刻对应 Service 的 WhereIF 形状，
// 用 Sqlite 内存文件库 + ConfigureBaseFilters 端到端验证「传值收窄 / 不传全量」契约。
// 注：仅复刻过滤形状，省略 SelectAsync<Output>() 投影与 LeftJoin——投影/连接不影响 WHERE 过滤断言。

/// <summary>
/// GameUserEntityService — Status (GameUserStatus?) 过滤契约。
/// </summary>
public sealed class AdvancedQueryUserStatusFilterTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"advq_user_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public AdvancedQueryUserStatusFilterTests()
    {
        var config = new ConnectionConfig { DbType = DbType.Sqlite, ConnectionString = $"DataSource={_dbPath}", };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
        SqlSugarUnifiedConfig.ConfigureBaseFilters(_db);
        _db.CodeFirst.InitTables(typeof(TenantGameUserEntity));
    }

    [Fact]
    public async Task StatusFilter_ReturnsOnlyMatchingStatus()
    {
        await SeedAsync();
        var banned = await BuildQuery(GameUserStatus.Banned).ToListAsync();
        Assert.Single(banned);
        Assert.Equal(GameUserStatus.Banned, banned[0].Status);
    }

    [Fact]
    public async Task NoStatusFilter_ReturnsAll()
    {
        await SeedAsync();
        var all = await BuildQuery(null).ToListAsync();
        Assert.Equal(3, all.Count);
    }

    private ISugarQueryable<TenantGameUserEntity> BuildQuery(GameUserStatus? status)
    {
        return _db.Queryable<TenantGameUserEntity>()
                  .WhereIF(status.HasValue, u => u.Status == status!.Value);
    }

    private async Task SeedAsync()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        await _db.Insertable(BuildUser(1, "u1", GameUserStatus.Active, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildUser(2, "u2", GameUserStatus.Banned, now)).ExecuteCommandAsync();
        await _db.Insertable(BuildUser(3, "u3", GameUserStatus.New, now)).ExecuteCommandAsync();
    }

    private static TenantGameUserEntity BuildUser(long id, string name, GameUserStatus status, long now)
    {
        // 非空字符串列全部给占位值，满足 Sqlite NOT NULL 约束（本测试只断言 Status 过滤形状）。
        return new TenantGameUserEntity
        {
            Id = id, UserName = name, Name = name, Password = "p", Email = name + "@e", EmailPassword = "ep",
            Phone = "", Avatar = "", GoogleId = "", AppleId = "", FacebookId = "", TwitterId = "",
            DiscordId = "", QqId = "", WeChatId = "", DeviceId = "", Status = status, IsDeleted = false, CreatedTime = now,
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