using GameFrameX.Core.Service.Tenant;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Tenant.Game.ServerConfig;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
/// 游戏运营配置表 表种子数据（AppId=null 出厂基线行；登录下发时被应用行按 Key 覆盖）。
/// 数据构建逻辑在 GameServerConfigBaselineSeed（Core 嵌入资源单一事实源）；
/// 自动推导的租户库连接 EnableInitSeed=false 时本渠道不生效，由 DefaultTenantDatabaseSeeder 走同一工厂补种
/// </summary>
public class GameAppServerConfigSeedData : ISqlSugarEntitySeedData<TenantGameAppServerConfigEntity>
{
    /// <summary>
    /// 出厂基线行（原厂 10 键 + 原厂 md5，行 Id 固定基数跨环境稳定）
    /// </summary>
    public IEnumerable<TenantGameAppServerConfigEntity> HasData()
    {
        return GameServerConfigBaselineSeed.BuildRows();
    }
}
