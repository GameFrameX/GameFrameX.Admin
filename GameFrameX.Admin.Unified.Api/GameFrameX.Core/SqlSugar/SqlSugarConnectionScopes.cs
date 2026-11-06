using GameFrameX.Core.Extension.Const;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar 多库连接与跨库事务的唯一收口。
/// </summary>
/// <remarks>
/// SqlSugar 的多库客户端 API 沿用其多租户多库模型的命名；本平台是单游戏单主库架构，业务语义上不存在租户分库。
/// 业务代码禁止直接调用该 API 族，统一经本类转发：全仓只有本文件出现该调用形态，后续如需更换 ORM 或调整多库策略也只改这一处。
/// </remarks>
public static class SqlSugarConnectionScopes
{
    /// <summary>
    /// 获取主库（连接标识 main）的当前作用域连接。
    /// </summary>
    /// <param name="client">当前 SqlSugar 客户端。</param>
    /// <returns>主库连接作用域。</returns>
    public static SqlSugarScopeProvider GetMainDatabaseScope(this ISqlSugarClient client)
    {
        return client.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
    }

    /// <summary>
    /// 按连接标识获取指定库的当前作用域连接（数据库运维工具按配置连接任意库）。
    /// </summary>
    /// <param name="client">当前 SqlSugar 客户端。</param>
    /// <param name="configId">目标连接标识。</param>
    /// <returns>目标库连接作用域。</returns>
    public static SqlSugarScopeProvider GetDatabaseScope(this ISqlSugarClient client, string configId)
    {
        return client.AsTenant().GetConnectionScope(configId);
    }

    /// <summary>
    /// 获取跨库事务客户端：其事务操作会同步纳入当前已注册的全部连接。
    /// </summary>
    /// <param name="client">当前 SqlSugar 客户端。</param>
    /// <returns>SqlSugar 多库事务客户端。</returns>
    public static ITenant GetMultiDatabaseClient(this ISqlSugarClient client)
    {
        return client.AsTenant();
    }
}
