using System.Reflection;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game;
using SqlSugar;

namespace GameFrameX.Core.Tests.SqlSugar;

/// <summary>
/// 角色广告租户实体索引回归测试。
/// 守护目标：GFX-92 启动报错 —— TenantGameRoleAdvertisementEntity 的
/// [SugarIndex(... "TenantId" ... nameof(AppId) ...)] 必须能被 SqlSugar
/// CodeFirst 在建表阶段解析，不能抛 "no SugarIndex[ TenantId ] found"。
/// 反射断言不足以守此失败模式（基类被改回非租户 EntityBase 时反射仍能找到
/// 间接成员，但 SqlSugar 不会把它当列），故必须真正调用 InitTables。
/// </summary>
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
    public void TenantGameRoleAdvertisement_HasTenantIdForTenantAppIndex()
    {
        // 基线：实体经继承链（EntityTenantBase）暴露 TenantId 属性。
        var tenantId = typeof(TenantGameRoleAdvertisementEntity).GetProperty(
            "TenantId",
            BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        Assert.NotNull(tenantId);
    }

    [Fact]
    public void InitTables_AcceptsTenantIdSugarIndex()
    {
        // 真实失败模式：SqlSugar CodeFirst.InitTables 校验 [SugarIndex] 引用的列
        // 必须存在于实体列集合。若实体基类被改回非租户 EntityBase（无 TenantId 列），
        // 这里会抛 SqlSugarException "no SugarIndex[ TenantId ] found"。
        _db.CodeFirst.InitTables(typeof(TenantGameRoleAdvertisementEntity));

        // 建表成功后断言 TenantId 被识别为列，防止“建表通过但 TenantId 被静默丢弃”的退化。
        var columns = _db.EntityMaintenance.GetEntityInfo<TenantGameRoleAdvertisementEntity>().Columns;
        Assert.Contains(columns, c => c.PropertyName == "TenantId");
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