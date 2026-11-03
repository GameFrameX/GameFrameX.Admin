using GameFrameX.Core.Extension.Const;
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

    public SqlSugarRepository(ISqlSugarClient sqlSugarClient)
    {
        ArgumentNullException.ThrowIfNull(sqlSugarClient);
        Context = sqlSugarClient.AsTenant().GetConnectionScope(SqlSugarConst.MainConfigId);
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