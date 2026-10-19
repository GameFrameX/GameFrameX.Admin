using GameFrameX.Core.Service.Tenant;
using GameFrameX.Entity.System;
using SqlSugar;

namespace GameFrameX.Hub.Api.Services;

/// <summary>
/// Hub API 租户库连接提供器：实时查库解析租户独立数据库。
/// <para>按用户决策丢弃 Hub 内存缓存，每次解析实时查询 SystemTenantEntity（校验 IsEnabled 与连接配置）并
/// 创建独立租户库连接；实体元数据、审计、软删除过滤器复用 Core.SqlSugar 统一配置。
/// 逻辑承接自原 Hub.Api Repository&lt;T&gt;.ResolveTenantDb（三 host 仓储收敛 C259）。</para>
/// </summary>
public sealed class HubTenantDbProvider : ITenantDbProvider
{
    private readonly ISqlSugarClient _mainDb;

    /// <summary>
    /// 构造函数，注入平台主库连接（读取 SystemTenantEntity 租户注册信息）。
    /// </summary>
    /// <param name="mainDb">主库（连 SystemTenantEntity 等平台表）</param>
    public HubTenantDbProvider(ISqlSugarClient mainDb)
    {
        _mainDb = mainDb;
    }

    /// <summary>
    /// 获取指定租户的业务库连接（实时查库，无内存缓存）。
    /// <para>租户不存在、被禁用或未配置连接时直接失败，禁止回落主库。</para>
    /// </summary>
    /// <param name="tenantId"></param>
    /// <returns></returns>
    public ISqlSugarClient GetTenantDb(long tenantId)
    {
        var tenant = _mainDb.Queryable<SystemTenantEntity>()
                            .ClearFilter()
                            .Where(t => t.Id == tenantId && t.IsEnabled == true)
                            .Select(t => new SystemTenantEntity
                            {
                                Id = t.Id,
                                Connection = t.Connection,
                                DbType = t.DbType,
                            })
                            .First();

        if (tenant == null || string.IsNullOrWhiteSpace(tenant.Connection))
        {
            throw new InvalidOperationException($"租户 {tenantId} 不存在、已被禁用或未配置数据库连接。");
        }

        // 创建租户库连接（实体元数据、审计、软删除过滤器均复用 Core.SqlSugar 统一配置）；
        // Connection 落库为 enc:v1: 密文（无前缀明文兼容），建连前统一经 TenantConnectionCrypto 解密
        var dbConfig = new ConnectionConfig
        {
            DbType = tenant.DbType,
            ConnectionString = TenantConnectionCrypto.Decrypt(tenant.Connection)!,
        };
        SqlSugarUnifiedConfig.ConfigureBaseSettings(dbConfig);

        var tenantDb = new SqlSugarClient(dbConfig);
        tenantDb.Aop.DataExecuting = (oldValue, entityInfo) => { SqlSugarAopHelper.HandleBaseAudit(oldValue, entityInfo); };
        SqlSugarUnifiedConfig.ConfigureBaseFilters(tenantDb);
        return tenantDb;
    }
}