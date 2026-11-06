using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>角色广告索引必须在无作用域列的真实表上成功创建。</summary>
public sealed class GameRoleAdvertisementEntityTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"role_ad_index_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public GameRoleAdvertisementEntityTests()
    {
        var config = new ConnectionConfig
        {
            DbType = DbType.Sqlite,
            ConnectionString = $"DataSource={_dbPath}",
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
        _db = new SqlSugarScope(config);
    }

    [Fact]
    public void InitTables_AcceptsBusinessIndexesWithoutTenantScope()
    {
        _db.CodeFirst.InitTables(typeof(GameRoleAdvertisementEntity));

        var columns = _db.EntityMaintenance.GetEntityInfo<GameRoleAdvertisementEntity>().Columns;
        Assert.DoesNotContain(columns, c => c.PropertyName is "AppId");
    }

    public void Dispose()
    {
        _db?.Dispose();
        if (File.Exists(_dbPath))
        {
            File.Delete(_dbPath);
        }
    }
}
