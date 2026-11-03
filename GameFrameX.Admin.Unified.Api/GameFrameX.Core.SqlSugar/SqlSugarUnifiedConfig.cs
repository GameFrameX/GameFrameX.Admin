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
    /// 配置 启用/禁用过滤器: ISafeEnabledFilter（Admin/Hub/Client 通用）
    /// </summary>
    public static void ConfigureClientFilters(ISqlSugarClient db)
    {
        db.QueryFilter.AddTableFilter<ISafeEnabledFilter>(u => u.IsEnabled == null || u.IsEnabled == true);
    }
}