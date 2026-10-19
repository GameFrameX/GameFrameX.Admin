using System.Linq.Expressions;
using System.Reflection;
using GameFrameX.Core.BaseInputOutput;
using GameFrameX.Core.SqlSugar;
using GameFrameX.Foundation.Orm.Entity;
using Mapster;
using MapsterMapper;
using SqlSugar;

namespace GameFrameX.Core.Extension;

public static class RepositoryExtension
{
    /*/// <summary>
    /// 实体假删除 _repository.FakeDelete(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static int FakeDelete<T>(this ISugarRepository repository, T entity) where T : EntityBase, new()
    {
        return repository.Context.FakeDelete(entity);
    }

    /// <summary>
    /// 实体假删除 db.FakeDelete(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>

    public static int FakeDelete<T>(this ISqlSugarClient db, T entity) where T : EntityBase, new()
    {
        return db.Updateable(entity).AS().ReSetValue(x => { x.IsDeleted = true; })
                 .IgnoreColumns(true)
                 .EnableDiffLogEvent() // 记录差异日志
                 .UpdateColumns(x => new { x.IsDeleted, x.UpdateTime, x.UpdatedId, }) // 允许更新的字段-AOP拦截自动设置UpdateTime、UpdatedId
                 .ExecuteCommandWithOptLock(true);
    }


    /// <summary>
    /// 实体集合批量假删除 _repository.FakeDelete(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static int FakeDelete<T>(this ISugarRepository repository, List<T> entity) where T : EntityBase, new()
    {
        return repository.Context.FakeDelete(entity);
    }

    /// <summary>
    /// 实体集合批量假删除 db.FakeDelete(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static int FakeDelete<T>(this ISqlSugarClient db, List<T> entity) where T : EntityBase, new()
    {
        return db.Updateable(entity).AS().ReSetValue(x => { x.IsDeleted = true; })
                 .IgnoreColumns(true)
                 .EnableDiffLogEvent() // 记录差异日志
                 .UpdateColumns(x => new { x.IsDeleted, x.UpdateTime, x.UpdatedId, }) // 允许更新的字段-AOP拦截自动设置UpdateTime、UpdatedId
                 .ExecuteCommandWithOptLock();
    }*/

    /// <summary>
    /// 实体假删除异步 _repository.FakeDeleteAsync(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> FakeDeleteAsync<T>(this ISugarRepository repository, T entity) where T : EntityBase, new()
    {
        return repository.Context.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 实体假删除 db.FakeDelete(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> FakeDeleteAsync<T>(this ISqlSugarClient db, T entity) where T : EntityBase, new()
    {
        return db.Updateable(entity).AS().ReSetValue(x => { x.IsDeleted = true; })
                 .IgnoreColumns(true)
                 .EnableDiffLogEvent() // 记录差异日志
                 .UpdateColumns(x => new { x.IsDeleted, x.UpdateTime, x.UpdatedId, }) // 允许更新的字段-AOP拦截自动设置UpdateTime、UpdatedId
                 .ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 实体集合批量假删除异步 _repository.FakeDeleteAsync(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> FakeDeleteAsync<T>(this ISugarRepository repository, List<T> entity) where T : EntityBase, new()
    {
        return repository.Context.FakeDeleteAsync(entity);
    }

    /// <summary>
    /// 实体集合批量假删除 db.FakeDelete(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> FakeDeleteAsync<T>(this ISqlSugarClient db, List<T> entity) where T : EntityBase, new()
    {
        return db.Updateable(entity).AS().ReSetValue(x => { x.IsDeleted = true; })
                 .IgnoreColumns(true)
                 .EnableDiffLogEvent() // 记录差异日志
                 .UpdateColumns(x => new { x.IsDeleted, x.UpdateTime, x.UpdatedId, }) // 允许更新的字段-AOP拦截自动设置UpdateTime、UpdatedId
                 .ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 排序方式(默认降序) - 使用 Lambda 表达式指定默认排序字段
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="pageInput"></param>
    /// <param name="defaultSortField"> 默认排序字段 Lambda 表达式 </param>
    /// <param name="prefix"> </param>
    /// <param name="descSort"> 是否降序 </param>
    /// <returns> </returns>
    public static ISugarQueryable<T> OrderBuilder<T>(this ISugarQueryable<T> queryable, BasePageRequest pageInput, Expression<Func<T, object>> defaultSortField, string prefix = "", bool descSort = true)
    {
        var propertyName = GetPropertyName(defaultSortField);
        var col = queryable.Context.EntityMaintenance.GetEntityInfo<T>().Columns.FirstOrDefault(u => u.PropertyName == propertyName);
        var dbColumnName = col?.DbColumnName ?? UtilMethods.ToUnderLine(propertyName);
        return queryable.OrderBuilder(pageInput, prefix, dbColumnName, descSort);
    }

    private static string GetPropertyName<T>(Expression<Func<T, object>> expression)
    {
        var body = expression.Body;
        if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        {
            body = unary.Operand;
        }

        return body is MemberExpression member ? member.Member.Name : throw new ArgumentException("Expression must be a simple property access, e.g. x => x.CreatedTime");
    }

    /// <summary>
    /// 排序方式(默认降序)
    /// </summary>
    /// <param name="queryable"></param>
    /// <param name="pageInput"> </param>
    /// <param name="prefix"> </param>
    /// <param name="defaultSortField"> 默认排序字段 </param>
    /// <param name="descSort"> 是否降序 </param>
    /// <returns> </returns>
    public static ISugarQueryable<T> OrderBuilder<T>(this ISugarQueryable<T> queryable, BasePageRequest pageInput, string prefix = "", string defaultSortField = "Id", bool descSort = true)
    {
        // 约定默认每张表都有Id排序
        var orderStr = string.IsNullOrWhiteSpace(defaultSortField) ? "" : $"{prefix}{defaultSortField}" + (descSort ? " Desc" : " Asc");

        var typeAdapterConfig = new TypeAdapterConfig();
        typeAdapterConfig.ForType<T, BasePageRequest>().IgnoreNullValues(true);
        var mapper = new Mapper(typeAdapterConfig); // 务必将mapper设为单实例
        var nowPagerInput = mapper.Map<BasePageRequest>(pageInput);
        // 排序是否可用-排序字段非空才启用排序
        if (!string.IsNullOrEmpty(nowPagerInput.OrderBy))
        {
            var col = queryable.Context.EntityMaintenance.GetEntityInfo<T>().Columns.FirstOrDefault(u => u.PropertyName.Equals(nowPagerInput.OrderBy, StringComparison.CurrentCultureIgnoreCase));
            orderStr = col != null
                           ? $"{prefix}{col.DbColumnName} {(nowPagerInput.IsDescending ? "Desc" : "Asc")}"
                           : $"{prefix}{nowPagerInput.OrderBy} {(nowPagerInput.IsDescending ? "Desc" : "Asc")}";
        }

        return queryable.OrderByIF(!string.IsNullOrWhiteSpace(orderStr), orderStr);
    }

    /*/// <summary>
    /// 更新实体并记录差异日志 _repository.UpdateWithDiffLog(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <param name="ignoreAllNullColumns"></param>
    /// <returns></returns>
    public static int UpdateWithDiffLog<T>(this ISugarRepository repository, T entity, bool ignoreAllNullColumns = true) where T : EntityBase, new()
    {
        return repository.Context.UpdateWithDiffLog(entity, ignoreAllNullColumns);
    }

    /// <summary>
    /// 更新实体并记录差异日志 _repository.UpdateWithDiffLog(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <param name="ignoreAllNullColumns"></param>
    /// <returns></returns>
    public static int UpdateWithDiffLog<T>(this ISqlSugarClient db, T entity, bool ignoreAllNullColumns = true) where T : EntityBase, new()
    {
        return db.Updateable(entity).AS()
                 .IgnoreColumns(ignoreAllNullColumns)
                 .EnableDiffLogEvent()
                 .ExecuteCommand();
    }
    */

    /// <summary>
    /// 更新实体并记录差异日志 _repository.UpdateWithDiffLogAsync(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <param name="ignoreAllNullColumns"></param>
    /// <returns></returns>
    public static Task<int> UpdateWithDiffLogAsync<T>(this ISugarRepository repository, T entity, bool ignoreAllNullColumns = true) where T : EntityBase, new()
    {
        return repository.Context.UpdateWithDiffLogAsync(entity, ignoreAllNullColumns);
    }

    /// <summary>
    /// 更新实体并记录差异日志 _repository.UpdateWithDiffLogAsync(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <param name="ignoreAllNullColumns"></param>
    /// <returns></returns>
    public static Task<int> UpdateWithDiffLogAsync<T>(this ISqlSugarClient db, T entity, bool ignoreAllNullColumns = true) where T : EntityBase, new()
    {
        return db.Updateable(entity)
                 .IgnoreColumns(ignoreAllNullColumns)
                 .EnableDiffLogEvent()
                 .ExecuteCommandAsync();
    }

    /// <summary>
    /// 使用实体当前版本执行原子更新，版本冲突时抛出 <see cref="VersionExceptions"/>。
    /// </summary>
    /// <typeparam name="T">带版本字段的实体类型。</typeparam>
    /// <param name="db">SqlSugar 客户端。</param>
    /// <param name="entity">包含主键和旧版本的实体快照。</param>
    /// <param name="ignoreAllNullColumns">是否忽略值为 null 的列。</param>
    /// <param name="enableDiffLog">是否记录差异日志。</param>
    /// <returns>受影响行数。</returns>
    public static Task<int> UpdateWithOptimisticLockAsync<T>(this ISqlSugarClient db, T entity, bool ignoreAllNullColumns = true, bool enableDiffLog = false) where T : EntityBase, new()
    {
        var updateable = db.Updateable(entity).AS().IgnoreColumns(ignoreAllNullColumns);
        if (enableDiffLog)
        {
            updateable.EnableDiffLogEvent();
        }

        return updateable.ExecuteCommandWithOptLockAsync(true);
    }

    /// <summary>
    /// 使用实体当前版本执行原子更新，版本冲突时抛出 <see cref="VersionExceptions"/>。
    /// </summary>
    public static Task<int> UpdateWithOptimisticLockAsync<T>(this ISugarRepository repository, T entity, bool ignoreAllNullColumns = true, bool enableDiffLog = false) where T : EntityBase, new()
    {
        return repository.Context.UpdateWithOptimisticLockAsync(entity, ignoreAllNullColumns, enableDiffLog);
    }

    /*
    /// <summary>
    /// 新增实体并记录差异日志 _repository.InsertWithDiffLog(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static int InsertWithDiffLog<T>(this ISugarRepository repository, T entity) where T : EntityBase, new()
    {
        return repository.Context.InsertWithDiffLog(entity);
    }

    /// <summary>
    /// 新增实体并记录差异日志 _repository.InsertWithDiffLog(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static int InsertWithDiffLog<T>(this ISqlSugarClient db, T entity) where T : EntityBase, new()
    {
        return db.Insertable(entity).AS().EnableDiffLogEvent().ExecuteCommand();
    }*/

    /// <summary>
    /// 新增实体并记录差异日志 _repository.InsertWithDiffLogAsync(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="repository"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> InsertWithDiffLogAsync<T>(this ISugarRepository repository, T entity) where T : EntityBase, new()
    {
        return repository.Context.InsertWithDiffLogAsync(entity);
    }

    /// <summary>
    /// 新增实体并记录差异日志 _repository.InsertWithDiffLog(entity)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="db"></param>
    /// <param name="entity"></param>
    /// <returns></returns>
    public static Task<int> InsertWithDiffLogAsync<T>(this ISqlSugarClient db, T entity) where T : EntityBase, new()
    {
        return db.Insertable(entity).AS().EnableDiffLogEvent().ExecuteCommandAsync();
    }

    /// <summary>
    /// 多库查询
    /// </summary>
    /// <param name="queryable"></param>
    /// <returns> </returns>
    public static ISugarQueryable<T> AS<T>(this ISugarQueryable<T> queryable)
    {
        return queryable.AS<T>(GetQualifiedTableName<T>());
    }

    /// <summary>
    /// 多库查询
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="queryable"></param>
    /// <returns></returns>
    public static ISugarQueryable<T, T2> AS<T, T2>(this ISugarQueryable<T, T2> queryable)
    {
        return queryable.AS<T2>(GetQualifiedTableName<T2>());
    }

    /// <summary>
    /// 多库更新
    /// </summary>
    /// <param name="updateable"></param>
    /// <returns> </returns>
    public static IUpdateable<T> AS<T>(this IUpdateable<T> updateable) where T : EntityBase, new()
    {
        return updateable.AS(GetQualifiedTableName<T>());
    }

    /// <summary>
    /// 多库新增
    /// </summary>
    /// <param name="insertable"></param>
    /// <returns> </returns>
    public static IInsertable<T> AS<T>(this IInsertable<T> insertable) where T : EntityBase, new()
    {
        return insertable.AS(GetQualifiedTableName<T>());
    }

    /// <summary>
    /// 多库删除
    /// </summary>
    /// <param name="deleteable"></param>
    /// <returns> </returns>
    public static IDeleteable<T> AS<T>(this IDeleteable<T> deleteable) where T : EntityBase, new()
    {
        return deleteable.AS(GetQualifiedTableName<T>());
    }

    /// <summary>
    /// 根据实体类型获取表信息
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    private static string GetQualifiedTableName<T>()
    {
        // 与 SqlSugarEntityBoundary.GetPhysicalTableName（建表/默认 CRUD 路径）保持单一来源，
        // 确保 .AS() 多库读写用的表名与 CodeFirst 建表名恒等（C30 对齐）。
        var entityType = typeof(T);
        var physicalName = SqlSugarEntityBoundary.GetPhysicalTableName(entityType, entityType.Name);
        var attr = entityType.GetCustomAttribute<TenantAttribute>();
        if (attr == null)
        {
            return physicalName;
        }

        var configId = attr.configId.ToString() ?? string.Empty;
        return string.IsNullOrWhiteSpace(configId) ? physicalName : $"{configId}.{physicalName}";
    }

    /// <summary>
    /// 禁用过滤器-适用于更新和删除操作（只对当前请求有效，禁止使用异步）
    /// </summary>
    /// <param name="repository"></param>
    /// <param name="action">禁止异步</param>
    /// <returns></returns>
    public static void RunWithoutFilter(this ISugarRepository repository, Action action)
    {
        repository.Context.QueryFilter.ClearAndBackup(); // 清空并备份过滤器
        action.Invoke();
        repository.Context.QueryFilter.Restore(); // 还原过滤器
    }
}