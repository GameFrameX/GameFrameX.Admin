using System.Reflection;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>角色广告索引必须在无作用域列的真实表上成功创建。</summary>
public sealed class TenantGameRoleAdvertisementEntityTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"role_ad_index_{Guid.NewGuid():N}.db");
    private readonly SqlSugarScope _db;

    public TenantGameRoleAdvertisementEntityTests()
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
    public void TenantGameRoleAdvertisement_HasNoTenantScope()
    {
        var tenantId = typeof(TenantGameRoleAdvertisementEntity).GetProperty(
            "TenantId",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        Assert.Null(tenantId);
    }

    [Fact]
    public void InitTables_AcceptsBusinessIndexesWithoutTenantScope()
    {
        _db.CodeFirst.InitTables(typeof(TenantGameRoleAdvertisementEntity));

        var columns = _db.EntityMaintenance.GetEntityInfo<TenantGameRoleAdvertisementEntity>().Columns;
        Assert.DoesNotContain(columns, c => c.PropertyName is "TenantId" or "AppId");
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
