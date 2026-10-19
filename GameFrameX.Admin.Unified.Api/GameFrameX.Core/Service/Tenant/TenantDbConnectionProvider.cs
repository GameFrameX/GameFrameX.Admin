using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Cache;
using GameFrameX.Entity.System;

namespace GameFrameX.Core.Service.Tenant;

public sealed class TenantDbConnectionProvider : ITenantDbConnectionProvider
{
    private readonly ISqlSugarClient _sqlSugarClient;
    private readonly SystemCacheService _systemCacheService;
    private readonly ISqlSugarTenantIdResolver _tenantIdResolver;
    private readonly DbConnectionOptions _dbOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantDbConnectionProvider(
        ISqlSugarClient sqlSugarClient,
        SystemCacheService systemCacheService,
        ISqlSugarTenantIdResolver tenantIdResolver,
        IOptions<DbConnectionOptions> dbOptions,
        IHttpContextAccessor httpContextAccessor)
    {
        _sqlSugarClient = sqlSugarClient;
        _systemCacheService = systemCacheService;
        _tenantIdResolver = tenantIdResolver;
        _dbOptions = dbOptions.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public SqlSugarScopeProvider GetTenantDbConnectionScope(long tenantId)
    {
        var iTenant = _sqlSugarClient.AsTenant();
        var configId = tenantId.ToString();

        if (iTenant.IsAnyConnection(configId))
        {
            return iTenant.GetConnectionScope(configId);
        }

        var tenant = ResolveTenant(tenantId);
        if (tenant == null)
        {
#pragma warning disable CS8603 // 租户不存在返回 null；返回类型跨项目调用方依赖非空签名，调用方已做 null 检查
            return null;
#pragma warning restore CS8603
        }

        var tenantConnConfig = new DbConnectionConfig
        {
            ConfigId = configId,
            DbType = tenant.DbType,
            IsAutoCloseConnection = true,
            // Connection 落库为 enc:v1: 密文（无前缀明文兼容），建连前统一经 TenantConnectionCrypto 解密
            ConnectionString = TenantConnectionCrypto.Decrypt(tenant.Connection)!,
            DbSettings = new DbSettings(),
        };

        iTenant.AddConnection(tenantConnConfig);

        var sqlSugarScopeProvider = iTenant.GetConnectionScope(configId);
        SqlSugarSetup.SetDbConfig(tenantConnConfig);
        SqlSugarSetup.SetDbAop(sqlSugarScopeProvider, _dbOptions.EnableConsoleSql, _tenantIdResolver, _httpContextAccessor);

        return sqlSugarScopeProvider;
    }

    private SystemTenantEntity ResolveTenant(long tenantId)
    {
        var tenantList = _systemCacheService.Get<List<SystemTenantEntity>>(CacheConst.KeyTenant) ?? new List<SystemTenantEntity>();
        var tenant = tenantList.FirstOrDefault(u => u.Id == tenantId);
        if (tenant != null)
        {
            return tenant;
        }

        return _sqlSugarClient.AsTenant()
                              .GetConnectionScope(SqlSugarConst.MainConfigId)
                              .Queryable<SystemTenantEntity>()
                              .Filter(null, true)
                              .First(u => u.Id == tenantId);
    }
}