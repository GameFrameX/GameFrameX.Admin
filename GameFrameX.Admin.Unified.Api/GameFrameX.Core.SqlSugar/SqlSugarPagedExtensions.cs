using System.Linq.Expressions;
using SqlSugar;

namespace GameFrameX.Core.SqlSugar;

/// <summary>
/// 分页拓展类
/// </summary>
public static class SqlSugarPagedExtensions
{
    /// <summary>
    /// 默认最大页容量，避免一次分页读取过多数据。
    /// </summary>
    public const int MaxPageSize = 1000;

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{T}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <param name="expression">查询结果 Select 表达式</param>
    /// <returns></returns>
    public static SqlSugarPagedList<TResult> ToPagedList<TEntity, TResult>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize,
        Expression<Func<TEntity, TResult>> expression)
    {
        ValidatePaging(pageIndex, pageSize);
        var total = 0;
        var items = query.ToPageList(pageIndex, pageSize, ref total, expression);
        return CreateSqlSugarPagedList(items, total, pageIndex, pageSize);
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{T}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <returns></returns>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize)
    {
        ValidatePaging(pageIndex, pageSize);
        var total = 0;
        var items = query.ToPageList(pageIndex, pageSize, ref total);
        return CreateSqlSugarPagedList(items, total, pageIndex, pageSize);
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{T}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <param name="expression">查询结果 Select 表达式</param>
    /// <returns></returns>
    public static async Task<SqlSugarPagedList<TResult>> ToPagedListAsync<TEntity, TResult>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize,
        Expression<Func<TEntity, TResult>> expression)
    {
        ValidatePaging(pageIndex, pageSize);
        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, total, expression);
        return CreateSqlSugarPagedList(items, total, pageIndex, pageSize);
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="query"><see cref="ISugarQueryable{T}"/>对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <returns></returns>
    public static async Task<SqlSugarPagedList<TEntity>> ToPagedListAsync<TEntity>(this ISugarQueryable<TEntity> query, int pageIndex, int pageSize)
    {
        ValidatePaging(pageIndex, pageSize);
        RefAsync<int> total = 0;
        var items = await query.ToPageListAsync(pageIndex, pageSize, total);
        return CreateSqlSugarPagedList(items, total, pageIndex, pageSize);
    }

    /// <summary>
    /// 分页拓展
    /// </summary>
    /// <param name="list">集合对象</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <returns></returns>
    public static SqlSugarPagedList<TEntity> ToPagedList<TEntity>(this IEnumerable<TEntity> list, int pageIndex, int pageSize)
    {
        ValidatePaging(pageIndex, pageSize);
        var total = list.Count();
        var items = list.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToList();
        return CreateSqlSugarPagedList(items, total, pageIndex, pageSize);
    }

    private static void ValidatePaging(int pageIndex, int pageSize)
    {
        if (pageIndex < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageIndex), pageIndex, "Page index must be greater than or equal to 1.");
        }

        if (pageSize < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, "Page size must be greater than 0.");
        }

        if (pageSize > MaxPageSize)
        {
            throw new ArgumentOutOfRangeException(nameof(pageSize), pageSize, $"Page size must be less than or equal to {MaxPageSize}.");
        }
    }

    /// <summary>
    /// 创建 <see cref="SqlSugarPagedList{TEntity}"/> 对象
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="items">分页内容的对象集合</param>
    /// <param name="total">总条数</param>
    /// <param name="pageIndex">当前页码，从1开始</param>
    /// <param name="pageSize">页码容量</param>
    /// <returns></returns>
    private static SqlSugarPagedList<TEntity> CreateSqlSugarPagedList<TEntity>(IEnumerable<TEntity> items, int total, int pageIndex, int pageSize)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling(total / (double)pageSize) : 0;
        return new SqlSugarPagedList<TEntity>
        {
            Page = pageIndex,
            PageSize = pageSize,
            Items = items,
            Total = total,
            TotalPages = totalPages,
            HasNextPage = pageIndex < totalPages,
            HasPrevPage = pageIndex - 1 > 0,
        };
    }
}