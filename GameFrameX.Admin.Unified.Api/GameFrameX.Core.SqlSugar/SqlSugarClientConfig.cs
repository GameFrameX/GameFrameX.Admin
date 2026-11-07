using GameFrameX.Foundation.Orm.Entity;
using GameFrameX.Foundation.Orm.Entity.Filter;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 两 host（Client / Hub，Game 已于 C268 并入 Client）共享的 SqlSugar 主库客户端构建入口：
/// 连接配置、基础审计（雪花 ID / CreatedTime / UpdateTime）、
/// 软删除 / 启用禁用三组过滤器均复用 <see cref="SqlSugarUnifiedConfig"/> 统一配置。
/// </summary>
public static class SqlSugarClientConfig
{
    /// <summary>
    /// 添加 SqlSugarClient 服务
    /// </summary>
    /// <param name="mainConfig">主库连接配置（由 HostMainDbConnectionResolver.Resolve 解析，含启动参数/环境变量覆盖与错配校验）。</param>
    /// <param name="userAuditHandler">
    /// 可选的操作人审计挂点，在基础审计之后逐事件回调。
    /// 承载依赖 Core 层类型（如 ClaimConst 操作人 claims）的 host 差异审计，例如 Hub 的
    /// CreatedId / CreatedName / UpdatedId / UpdatedName 填充；不需要的 host 传 null。
    /// </param>
    /// <returns></returns>
    public static SqlSugarScope AddSqlSugarClientService(ConnectionConfig mainConfig, Action<DataFilterModel>? userAuditHandler = null)
    {
        //Scoped用SqlSugarClient
        var dbConfig = new ConnectionConfig
        {
            ConfigId = mainConfig.ConfigId,
            DbType = mainConfig.DbType,
            ConnectionString = mainConfig.ConnectionString,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(dbConfig);

        var sqlSugarClient = new SqlSugarScope(dbConfig,
                                               db =>
                                               {
                                                   db.Aop.DataExecuting = (oldValue, entityInfo) =>
                                                   {
                                                       SqlSugarAopHelper.HandleBaseAudit(oldValue, entityInfo);

                                                       // host 特有的操作人审计（如 Hub 的 CreatedId / UpdatedId claims 填充）
                                                       userAuditHandler?.Invoke(entityInfo);
                                                   };
                                                   // 配置软删除过滤器
                                                   SqlSugarUnifiedConfig.ConfigureBaseFilters(db);
                                                   // 配置启用禁用过滤器
                                                   SqlSugarUnifiedConfig.ConfigureClientFilters(db);
                                               });
        return sqlSugarClient;
    }
}