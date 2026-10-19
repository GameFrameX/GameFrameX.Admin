using GameFrameX.Core.Service.Tenant;
using GameFrameX.Foundation.Orm.Entity.Filter;
using SqlSugar;

namespace GameFrameX.Client.Api.Services;

/// <summary>
/// Client API 租户库连接提供器。
/// <para>Client API 启动连接只指向平台主库；请求期游戏业务和租户配置必须根据当前 TenantId 切到 SystemTenantEntity.Connection 指向的租户库。</para>
/// </summary>
public sealed class ClientApiTenantDbProvider : ITenantDbProvider
{
    private readonly ISqlSugarClient _db;
    private readonly ISqlSugarTenantIdResolver _tenantIdResolver;
    private readonly object _connectionLock = new();

    public ClientApiTenantDbProvider(ISqlSugarClient db, ISqlSugarTenantIdResolver tenantIdResolver)
    {
        _db = db;
        _tenantIdResolver = tenantIdResolver;
    }

    /// <summary>
    /// 获取指定租户的业务库连接。
    /// <para>缺少租户库连接串时直接失败，避免把租户配置表误读到平台主库。</para>
    /// </summary>
    public ISqlSugarClient GetTenantDb(long tenantId)
    {
        var configId = tenantId.ToString();
        var tenantClient = _db.AsTenant();
        if (tenantClient.IsAnyConnection(configId))
        {
            return tenantClient.GetConnectionScope(configId);
        }

        lock (_connectionLock)
        {
            if (tenantClient.IsAnyConnection(configId))
            {
                return tenantClient.GetConnectionScope(configId);
            }

            var tenant = ResolveTenant(tenantId);
            if (tenant == null || string.IsNullOrWhiteSpace(tenant.Connection))
            {
                throw new InvalidOperationException($"租户库连接配置缺失，无法读取 Client API 租户配置。TenantId={tenantId}");
            }

            var config = new ConnectionConfig
            {
                ConfigId = configId,
                DbType = tenant.DbType,
                // Connection 落库为 enc:v1: 密文（无前缀明文兼容），建连前统一经 TenantConnectionCrypto 解密
                ConnectionString = TenantConnectionCrypto.Decrypt(tenant.Connection)!,
                IsAutoCloseConnection = true,
            };
            SqlSugarUnifiedConfig.ConfigureBaseSettings(config);
            tenantClient.AddConnection(config);

            var tenantDb = tenantClient.GetConnectionScope(configId);
            ConfigureTenantConnection(tenantDb);
            return tenantDb;
        }
    }

    private SystemTenantEntity ResolveTenant(long tenantId)
    {
        return _db.Queryable<SystemTenantEntity>()
                  .Filter(null, true)
                  .First(u => u.Id == tenantId);
    }

    private void ConfigureTenantConnection(SqlSugarScopeProvider db)
    {
        db.Aop.DataExecuting = (oldValue, entityInfo) =>
        {
            SqlSugarAopHelper.HandleBaseAudit(oldValue, entityInfo);

            if (entityInfo.OperationType == DataFilterType.InsertByObject &&
                entityInfo.PropertyName == nameof(EntityTenantBase.TenantId) &&
                entityInfo.EntityValue is ITenantIdFilter)
            {
                var currentTenantIdValue = entityInfo.EntityValue.GetType()
                                                     .GetProperty(nameof(EntityTenantBase.TenantId))
                                                     ?.GetValue(entityInfo.EntityValue);
                var currentTenantId = currentTenantIdValue is long value ? value : 0L;
                if (currentTenantId == 0)
                {
                    var tenantId = _tenantIdResolver.GetTenantId();
                    if (tenantId.HasValue)
                    {
                        entityInfo.SetValue(tenantId.Value);
                    }
                }
            }
        };

        SqlSugarUnifiedConfig.ConfigureBaseFilters(db);
        SqlSugarUnifiedConfig.ConfigureAdminFilters(db, _tenantIdResolver.GetTenantId());
        SqlSugarUnifiedConfig.ConfigureClientFilters(db);
    }
}