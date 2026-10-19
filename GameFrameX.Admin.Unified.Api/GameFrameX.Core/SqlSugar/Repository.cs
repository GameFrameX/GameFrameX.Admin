using GameFrameX.Foundation.Orm.Entity;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 三 host（Client / Game / Hub）共享的通用仓储薄壳：按当前请求的租户上下文路由数据库。
/// <para>非租户表（平台主库表等）→ 主库连接；租户表 → 经 <see cref="ITenantDbProvider"/> 解析租户独立数据库，
/// 缺少租户上下文时禁止回落主库。CRUD 辅助方法与乐观锁更新由基类 <see cref="SqlSugarRepository{T}"/> 提供。</para>
/// </summary>
/// <typeparam name="T">实体类型，必须继承自 EntityBase</typeparam>
public class Repository<T> : SqlSugarRepository<T> where T : EntityBase, new()
{
    /// <summary>
    /// 构造函数，按实体边界解析数据库上下文。
    /// </summary>
    /// <param name="db">主库 SqlSugar 数据库客户端。</param>
    /// <param name="tenantDbProvider">租户库连接提供器（连接缓存型或实时查库型，由各 host 注册）。</param>
    /// <param name="tenantIdResolver">当前请求租户解析器。</param>
    public Repository(ISqlSugarClient db, ITenantDbProvider tenantDbProvider, ISqlSugarTenantIdResolver tenantIdResolver)
        : base(true)
    {
        var entityType = typeof(T);
        SqlSugarEntityBoundary.EnsureSingleBoundary(entityType);

        // 平台主库表等非租户表 → 主库
        if (!SqlSugarEntityBoundary.IsTenantTable(entityType))
        {
            Context = db;
            return;
        }

        // 租户表必须有租户上下文，禁止回落主库
        var tenantId = tenantIdResolver.GetTenantId();
        if (!tenantId.HasValue || tenantId.Value <= 0)
        {
            throw new InvalidOperationException($"缺少租户上下文，租户实体 {entityType.Name} 禁止回落到主库。");
        }

        Context = tenantDbProvider.GetTenantDb(tenantId.Value);
    }
}