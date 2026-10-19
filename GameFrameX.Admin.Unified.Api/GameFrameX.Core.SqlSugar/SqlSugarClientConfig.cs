using GameFrameX.Foundation.Orm.Entity;
using GameFrameX.Foundation.Orm.Entity.Filter;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 两 host（Client / Hub，Game 已于 C268 并入 Client）共享的 SqlSugar 主库客户端构建入口：
/// 连接配置、基础审计（雪花 ID / CreatedTime / UpdateTime）、插入时 TenantId 自动填充、
/// 软删除 / 租户 / 启用禁用三组过滤器均复用 <see cref="SqlSugarUnifiedConfig"/> 统一配置。
/// </summary>
public static class SqlSugarClientConfig
{
    /// <summary>
    /// 添加 SqlSugarClient 服务
    /// </summary>
    /// <param name="mainConfig">主库连接配置（由 HostMainDbConnectionResolver.Resolve 解析，含 GFX_MAIN_DB_* 覆盖与错配校验）。</param>
    /// <param name="tenantIdResolver">当前请求租户 Id 解析器。</param>
    /// <param name="userAuditHandler">
    /// 可选的操作人审计挂点，在基础审计与 TenantId 填充之后逐事件回调。
    /// 承载依赖 Core 层类型（如 ClaimConst 操作人 claims）的 host 差异审计，例如 Hub 的
    /// CreatedId / CreatedName / UpdatedId / UpdatedName 填充；不需要的 host 传 null。
    /// </param>
    /// <returns></returns>
    public static SqlSugarScope AddSqlSugarClientService(ConnectionConfig mainConfig, ISqlSugarTenantIdResolver tenantIdResolver, Action<DataFilterModel>? userAuditHandler = null)
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

                                                       // 插入时自动填充 TenantId
                                                       if (entityInfo.OperationType == DataFilterType.InsertByObject &&
                                                           entityInfo.PropertyName == nameof(EntityTenantBase.TenantId) &&
                                                           entityInfo.EntityValue is ITenantIdFilter)
                                                       {
                                                           var currentTenantId = (long)entityInfo.EntityValue.GetType()
                                                                                                 .GetProperty(nameof(EntityTenantBase.TenantId))!
                                                                                                 .GetValue(entityInfo.EntityValue)!;
                                                           if (currentTenantId == 0)
                                                           {
                                                               var tenantId = tenantIdResolver.GetTenantId();
                                                               if (tenantId.HasValue)
                                                               {
                                                                   entityInfo.SetValue(tenantId.Value);
                                                               }
                                                           }
                                                       }

                                                       // host 特有的操作人审计（如 Hub 的 CreatedId / UpdatedId claims 填充）
                                                       userAuditHandler?.Invoke(entityInfo);
                                                   };
                                                   // 配置软删除过滤器
                                                   SqlSugarUnifiedConfig.ConfigureBaseFilters(db);
                                                   // 配置租户过滤器
                                                   SqlSugarUnifiedConfig.ConfigureAdminFilters(db, tenantIdResolver.GetTenantId());
                                                   // 配置启用禁用过滤器
                                                   SqlSugarUnifiedConfig.ConfigureClientFilters(db);
                                               });
        return sqlSugarClient;
    }
}