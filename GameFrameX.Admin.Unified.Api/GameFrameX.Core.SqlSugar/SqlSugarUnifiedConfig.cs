using GameFrameX.Foundation.Orm.Entity.Filter;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar 统一配置服务 — 过滤器、连接配置的单一入口
/// </summary>
public static class SqlSugarUnifiedConfig
{
    /// <summary>
    /// 配置数据库连接基础属性
    /// </summary>
    /// <param name="config">连接配置</param>
    /// <param name="cacheService">二级缓存服务，null 时不设置缓存</param>
    public static void ConfigureBaseSettings(ConnectionConfig config, ICacheService? cacheService = null)
    {
        var configureExternalServices = new ConfigureExternalServices();
        SqlSugarEntityConfig.ConfigureEntityServices(configureExternalServices);
        if (cacheService != null)
        {
            configureExternalServices.DataInfoCacheService = cacheService;
        }

        config.ConfigureExternalServices = configureExternalServices;
        config.InitKeyType = InitKeyType.Attribute;
        config.IsAutoCloseConnection = true;
        config.MoreSettings = new ConnMoreSettings
        {
            IsAutoRemoveDataCache = true,
            IsAutoDeleteQueryFilter = true,
            IsAutoUpdateQueryFilter = true,
            SqlServerCodeFirstNvarchar = true,
        };
    }

    /// <summary>
    /// 配置基础过滤器（两端共享）: ISafeDeletedFilter
    /// </summary>
    public static void ConfigureBaseFilters(ISqlSugarClient db)
    {
        db.QueryFilter.AddTableFilter<ISafeDeletedFilter>(u => u.IsDeleted == false);
    }

    /// <summary>
    /// 配置 Admin 端专用过滤器: ITenantIdFilter（需要外部调用 SqlSugarFilter 完成机构和自定义过滤器）
    /// </summary>
    /// <param name="db">SqlSugar 作用域提供者</param>
    /// <param name="tenantId">租户ID，由调用方获取</param>
    public static void ConfigureAdminFilters(ISqlSugarClient db, long? tenantId = null)
    {
        if (tenantId.HasValue)
        {
            db.QueryFilter.AddTableFilter<ITenantIdFilter>(u => u.TenantId == null || u.TenantId == tenantId.Value);
        }
    }

    /// <summary>
    /// 配置 启用/禁用过滤器: ISafeEnabledFilter（Admin/Hub/Client 通用）
    /// </summary>
    public static void ConfigureClientFilters(ISqlSugarClient db)
    {
        db.QueryFilter.AddTableFilter<ISafeEnabledFilter>(u => u.IsEnabled == null || u.IsEnabled == true);
    }
}