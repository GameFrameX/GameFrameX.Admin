using GameFrameX.Core.Service.Business;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Entity.Business.Game.ServerConfig;

namespace GameFrameX.Application.SeedData.Client;

/// <summary>
/// 游戏运营配置表 表种子数据（AppId=null 出厂基线行；登录下发时被应用行按 Key 覆盖）。
/// 数据构建逻辑在 GameServerConfigBaselineSeed（Core 嵌入资源单一事实源）；
/// 自动推导连接的库 EnableInitSeed=false 时本渠道不生效，由 DefaultDatabaseSeeder 走同一工厂补种
/// </summary>
public class GameAppServerConfigSeedData : ISqlSugarEntitySeedData<GameAppServerConfigEntity>
{
    /// <summary>
    /// 出厂基线行（原厂 10 键 + 原厂 md5，行 Id 固定基数跨环境稳定）
    /// </summary>
    public IEnumerable<GameAppServerConfigEntity> HasData()
    {
        return GameServerConfigBaselineSeed.BuildRows();
    }
}
