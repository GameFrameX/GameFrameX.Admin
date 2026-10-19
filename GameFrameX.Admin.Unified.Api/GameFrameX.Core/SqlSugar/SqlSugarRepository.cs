using GameFrameX.Core.Extension.Const;
using GameFrameX.Core.Service.Tenant;
using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// SqlSugar 实体仓储
/// </summary>
/// <typeparam name="T"></typeparam>
public class SqlSugarRepository<T> : SimpleClient<T> where T : EntityBase, new()
{
    public SqlSugarRepository()
    {
    }

    public SqlSugarRepository(ISqlSugarClient sqlSugarClient, TenantDbConnectionProvider tenantDbConnectionProvider)
    {
        var tenant = sqlSugarClient.AsTenant();
        base.Context = tenant.GetConnectionScope(SqlSugarConst.MainConfigId);

        var entityType = typeof(T);
        SqlSugarEntityBoundary.EnsureSingleBoundary(entityType);

        // 若实体贴有多库特性，则返回指定库连接
        if (SqlSugarEntityBoundary.IsExternalSpecificTable(entityType))
        {
            base.Context = tenant.GetConnectionScopeWithAttr<T>();
            return;
        }

        // 平台主库表始终返回默认库连接
        if (SqlSugarEntityBoundary.IsPlatformTable(entityType))
        {
            base.Context = tenant.GetConnectionScope(SqlSugarConst.MainConfigId);
            return;
        }

        if (!SqlSugarEntityBoundary.IsTenantTable(entityType))
        {
            return;
        }

        var parsedTenantId = TenantOperationContext.GetEffectiveTenantId();
        if (!parsedTenantId.HasValue)
        {
            throw BusinessError.Oh(SystemUserErrorCode.NoPermission);
        }

        // 根据租户Id切换库连接, 为空则返回默认库连接
        var sqlSugarScopeProvider = tenantDbConnectionProvider.GetTenantDbConnectionScope(parsedTenantId.Value);
        if (sqlSugarScopeProvider == null)
        {
            if (SqlSugarEntityBoundary.RequiresTenantContext(entityType))
            {
                throw BusinessError.Oh(SystemUserErrorCode.NoPermission);
            }

            return;
        }

        base.Context = sqlSugarScopeProvider;
    }

    /// <summary>
    /// 供派生仓储使用的构造函数。派生类自行负责租户路由与 <see cref="SimpleClient{T}.Context"/> 绑定，
    /// 跳过本类默认的实体边界校验与租户库路由。
    /// </summary>
    /// <param name="skipRouting">固定传 <c>true</c>，仅用于与公共无参构造函数区分签名。</param>
    protected SqlSugarRepository(bool skipRouting)
    {
        // 派生仓储自行解析数据库连接，基类不做任何路由。
    }

    /// <summary>
    /// 根据条件异步获取单个实体（排除已删除的记录）
    /// </summary>
    /// <param name="filter">查询条件表达式</param>
    /// <returns>符合条件的实体对象</returns>
    public async Task<T> GetAsync(Expression<Func<T, bool>> filter)
    {
        return await AsQueryable().Where(m => m.IsDeleted == false).FirstAsync(filter);
    }

    /// <summary>
    /// 内部更新方法，忽略空值列进行更新
    /// </summary>
    /// <param name="entity">基础更新输入对象</param>
    /// <param name="cancellationToken"></param>
    /// <returns>更新后的实体对象</returns>
    public async Task<T> InnerUpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Context.Updateable(entity)
                     .IgnoreColumns(m => new { m.CreatedTime, m.CreatedId, m.CreatedName, m.IsDeleted, })
                     .IgnoreColumns(true)
                     .ExecuteCommandWithOptLockAsync(true);
        return entity;
    }

    /// <summary>
    /// 使用已读取实体的旧版本执行原子更新。
    /// </summary>
    public Task<int> UpdateWithOptimisticLockAsync(T entity)
    {
        return Context.UpdateWithOptimisticLockAsync(entity);
    }

}